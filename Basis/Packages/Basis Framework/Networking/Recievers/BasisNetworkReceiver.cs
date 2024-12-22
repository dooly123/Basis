using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Profiler;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static SerializableBasis;


namespace Basis.Scripts.Networking.Recievers
{
    [DefaultExecutionOrder(15001)]
    [System.Serializable]
    public partial class BasisNetworkReceiver : BasisNetworkSendBase
    {
        public float[] silentData;
        [SerializeField]
        public BasisAudioReceiver AudioReceiverModule = new BasisAudioReceiver();
        [Header("Interpolation Settings")]
        public double delayTime = 0.1f; // How far behind real-time we want to stay, hopefully double is good.
        [SerializeField]
        public Queue<AvatarBuffer> PayloadQueue = new Queue<AvatarBuffer>();
        public BasisRemotePlayer RemotePlayer;
        public bool HasEvents = false;
        private NativeArray<float3> OuputVectors;      // Merged positions and scales
        private NativeArray<float3> TargetVectors; // Merged target positions and scales
        private NativeArray<float> muscles;
        private NativeArray<float> targetMuscles;
        public JobHandle musclesHandle;
        public JobHandle AvatarHandle;
        public UpdateAvatarMusclesJob musclesJob = new UpdateAvatarMusclesJob();
        public UpdateAvatarJob AvatarJob = new UpdateAvatarJob();
        public float[] MuscleFinalStageOutput = new float[90];
        public quaternion OutputRotation;
        public AvatarBuffer First;
        public AvatarBuffer Last;
        //  public int PayloadCount = 0;
        public static int BufferCapacityBeforeCleanup = 3;
        public float interpolationTime;
        public double TimeBeforeCompletion;
        public double TimeInThePast;
        public bool HasAvatarInitalized;
        /// <summary>
        /// Perform computations to interpolate and update avatar state.
        /// </summary>
        public void Compute(double TimeAsDouble)
        {
            if (Ready)
            {
                if (HasAvatarInitalized)
                {
                    // Calculate interpolation time
                    interpolationTime = Mathf.Clamp01((float)((TimeAsDouble - TimeInThePast) / TimeBeforeCompletion));

                    // PayloadCount = PayloadQueue.Count;
                    TargetVectors[0] = Last.Position; // Target position at index 0
                    OuputVectors[0] = First.Position; // Position at index 0

                    OuputVectors[1] = First.Scale;    // Scale at index 1
                    TargetVectors[1] = Last.Scale;    // Target scale at index 1
                    muscles.CopyFrom(First.Muscles);
                    targetMuscles.CopyFrom(Last.Muscles);
                    AvatarJob.Time = interpolationTime;

                    AvatarHandle = AvatarJob.Schedule();

                    // Muscle interpolation job
                    musclesJob.Time = interpolationTime;
                    musclesHandle = musclesJob.Schedule(muscles.Length, 64, AvatarHandle);
                }
            }
        }
        public void Apply(double TimeAsDouble, float DeltaTime)
        {
            if (HasAvatarInitalized)
            {
                OutputRotation = math.slerp(First.rotation, Last.rotation, interpolationTime);
                // Complete the jobs and apply the results
                musclesHandle.Complete();

                ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, OuputVectors[1], OuputVectors[0], OutputRotation, muscles);
                PoseHandler.SetHumanPose(ref HumanPose);

                RemotePlayer.RemoteBoneDriver.SimulateAndApply(TimeAsDouble, DeltaTime);

                //come back to this later!  RemotePlayer.Avatar.FaceVisemeMesh.transform.position = RemotePlayer.MouthControl.OutgoingWorldData.position;
            }
            if (interpolationTime >= 1 && PayloadQueue.TryDequeue(out AvatarBuffer result))
            {
                First = Last;
                Last = result;

                TimeBeforeCompletion = Last.SecondsInterval;
                TimeInThePast = TimeAsDouble;
            }
        }
        public void EnQueueAvatarBuffer(ref AvatarBuffer avatarBuffer)
        {
            if(avatarBuffer == null)
            {
                Debug.LogError("Missing Avatar Buffer!");
                return;
            }
            if (Ready)
            {
                if (HasAvatarInitalized)
                {
                    PayloadQueue.Enqueue(avatarBuffer);
                    while (PayloadQueue.Count > BufferCapacityBeforeCleanup)
                    {
                        PayloadQueue.TryDequeue(out AvatarBuffer Buffer);
                    }
                }
                else
                {
                    First = avatarBuffer;
                    Last = avatarBuffer;
                    HasAvatarInitalized = true;
                }
            }
            else
            {
                Debug.LogError("trying to apply Avatar Buffer before ready!");
            }
        }
        public void ApplyPoseData(Animator animator, float3 Scale, float3 Position, quaternion Rotation, NativeArray<float> Muscles)
        {
            // Directly adjust scaling by applying the inverse of the AvatarHumanScale
            Vector3 Scaling = Vector3.one / animator.humanScale;  // Initial scaling with human scale inverse

            // Now adjust scaling with the output scaling vector
            Scaling = Divide(Scaling, Scale);  // Apply custom scaling logic

            // Apply scaling to position
            Vector3 ScaledPosition = Vector3.Scale(Position, Scaling);  // Apply the scaling

            // Apply pose data
            HumanPose.bodyPosition = ScaledPosition;
            HumanPose.bodyRotation = Rotation;

            // Copy from job to MuscleFinalStageOutput
            Muscles.CopyTo(MuscleFinalStageOutput);
            // First, copy the first 14 elements directly
            Array.Copy(MuscleFinalStageOutput, 0, HumanPose.muscles, 0, FirstBuffer);

            // Then, copy the remaining elements from index 15 onwards into the pose.muscles array, starting from index 21
            Array.Copy(MuscleFinalStageOutput, FirstBuffer, HumanPose.muscles, SecondBuffer, SizeAfterGap);

            // Adjust the local scale of the animator's transform
            animator.transform.localScale = Scale;  // Directly adjust scale with output scaling
        }
        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            // Define a small epsilon to avoid division by zero, using a flexible value based on magnitude
            const float epsilon = 0.00001f;

            return new Vector3(
                Mathf.Abs(b.x) > epsilon ? a.x / b.x : a.x,  // Avoid scaling if b is too small
                Mathf.Abs(b.y) > epsilon ? a.y / b.y : a.y,  // Same for y-axis
                Mathf.Abs(b.z) > epsilon ? a.z / b.z : a.z   // Same for z-axis
            );
        }
        public void ReceiveNetworkAudio(ServerAudioSegmentMessage audioSegment)
        {
            if (AudioReceiverModule.decoder != null)
            {
                BasisNetworkProfiler.ServerAudioSegmentMessageCounter.Sample(audioSegment.audioSegmentData.LengthUsed);
                AudioReceiverModule.decoder.OnDecode(audioSegment.audioSegmentData.buffer, audioSegment.audioSegmentData.LengthUsed);
                NetworkedPlayer.Player.AudioReceived?.Invoke(true);
            }
        }
        public void ReceiveSilentNetworkAudio(ServerAudioSegmentMessage audioSilentSegment)
        {
            if (AudioReceiverModule.decoder != null)
            {
                if (silentData == null || silentData.Length != AudioReceiverModule.decoder.FakepcmLength)
                {
                    silentData = new float[AudioReceiverModule.decoder.FakepcmLength];
                    Array.Fill(silentData, 0f);
                }
                BasisNetworkProfiler.ServerAudioSegmentMessageCounter.Sample(1);
                AudioReceiverModule.OnDecoded(silentData, AudioReceiverModule.decoder.FakepcmLength);
                NetworkedPlayer.Player.AudioReceived?.Invoke(false);
            }
        }
        public void ReceiveAvatarChangeRequest(ServerAvatarChangeMessage ServerAvatarChangeMessage)
        {
            RemotePlayer.CACM = ServerAvatarChangeMessage.clientAvatarChangeMessage;
            BasisLoadableBundle BasisLoadableBundle = BasisBundleConversionNetwork.ConvertNetworkBytesToBasisLoadableBundle(ServerAvatarChangeMessage.clientAvatarChangeMessage.byteArray);

            RemotePlayer.CreateAvatar(ServerAvatarChangeMessage.clientAvatarChangeMessage.loadMode, BasisLoadableBundle);
        }
        public override void Initialize(BasisNetworkedPlayer networkedPlayer)
        {
            if (!Ready)
            {
                if (HumanPose.muscles == null || HumanPose.muscles.Length == 0)
                {
                    HumanPose.muscles = new float[95];
                }
                OuputVectors = new NativeArray<float3>(2, Allocator.Persistent); // Index 0 = position, Index 1 = scale
                TargetVectors = new NativeArray<float3>(2, Allocator.Persistent); // Index 0 = target position, Index 1 = target scale
                muscles = new NativeArray<float>(90, Allocator.Persistent);
                targetMuscles = new NativeArray<float>(90, Allocator.Persistent);
                musclesJob = new UpdateAvatarMusclesJob();
                AvatarJob = new UpdateAvatarJob();
                musclesJob.Outputmuscles = muscles;
                musclesJob.targetMuscles = targetMuscles;
                AvatarJob.OutputVector = OuputVectors;
                AvatarJob.TargetVector = TargetVectors;
                ComputeHumanPose();
                NetworkedPlayer = networkedPlayer;
                RemotePlayer = (BasisRemotePlayer)NetworkedPlayer.Player;
                AudioReceiverModule.OnEnable(networkedPlayer);
                OnAvatarCalibration();
                if (HasEvents == false)
                {
                    RemotePlayer.RemoteAvatarDriver.CalibrationComplete += OnCalibration;
                    HasEvents = true;
                }
                Ready = true;
            }
        }
        public void OnCalibration()
        {
            AudioReceiverModule.OnCalibration(NetworkedPlayer);
        }
        public override void DeInitialize()
        {
            if (OuputVectors.IsCreated) OuputVectors.Dispose();
            if (TargetVectors.IsCreated) TargetVectors.Dispose();
            if (muscles.IsCreated) muscles.Dispose();
            if (targetMuscles.IsCreated) targetMuscles.Dispose();
            if (HasEvents && RemotePlayer != null && RemotePlayer.RemoteAvatarDriver != null)
            {
                RemotePlayer.RemoteAvatarDriver.CalibrationComplete -= OnCalibration;
                HasEvents = false;
            }
            if (AudioReceiverModule != null)
            {
                AudioReceiverModule.OnDestroy();
            }
        }
    }
}
