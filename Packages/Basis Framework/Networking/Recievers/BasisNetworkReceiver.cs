using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static SerializableDarkRift;

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
        public List<AvatarBuffer> AvatarDataBuffer = new List<AvatarBuffer>();
        public BasisRemotePlayer RemotePlayer;
        public bool HasEvents = false;
        private NativeArray<float3> OuputVectors;      // Merged positions and scales
        private NativeArray<float3> TargetVectors; // Merged target positions and scales
        private NativeArray<float> muscles;
        private NativeArray<float> targetMuscles;
        public JobHandle musclesHandle;
        public JobHandle AvatarHandle;
        public UpdateAvatarMusclesJob musclesJob = new UpdateAvatarMusclesJob();
        public float DeltaTime;
        public UpdateAvatarJob AvatarJob = new UpdateAvatarJob();
        public float[] MuscleFinalStageOutput = new float[90];
        public quaternion OutputRotation;
        public void Initialize()
        {
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
        }
        /// <summary>
        /// Clean up resources used in the compute process.
        /// </summary>
        public void Destroy()
        {
            if (OuputVectors.IsCreated) OuputVectors.Dispose();
            if (TargetVectors.IsCreated) TargetVectors.Dispose();
            if (muscles.IsCreated) muscles.Dispose();
            if (targetMuscles.IsCreated) targetMuscles.Dispose();
        }

        /// <summary>
        /// Perform computations to interpolate and update avatar state.
        /// </summary>
        public override void Compute()
        {
            if (!IsAbleToUpdate())
            {
                return;
            }

            double currentTime = Time.realtimeSinceStartupAsDouble;

            // Remove outdated rotations, keeping at least 2 data points
            while (AvatarDataBuffer.Count > 1 && currentTime - delayTime > AvatarDataBuffer[1].timestamp)
            {
                BasisAvatarBufferPool.Return(AvatarDataBuffer[0]);
                AvatarDataBuffer.RemoveAt(0);
            }
            // Ensure there are enough data points
            if (AvatarDataBuffer.Count >= 2)
            {
                AvatarBuffer Inital = AvatarDataBuffer[0];
                AvatarBuffer Target = AvatarDataBuffer[1];
                double startTime = AvatarDataBuffer[0].timestamp;
                double endTime = AvatarDataBuffer[1].timestamp;
                double targetTime = currentTime - delayTime;

                // Calculate normalized interpolation factor t
                float normalizedTime = (float)((targetTime - startTime) / (endTime - startTime));
                normalizedTime = Mathf.Clamp01(normalizedTime);

                TargetVectors[0] = Target.Position; // Target position at index 0
                OuputVectors[0] = Inital.Position; // Position at index 0

                OuputVectors[1] = Inital.Scale;    // Scale at index 1
                TargetVectors[1] = Target.Scale;    // Target scale at index 1

                muscles.CopyFrom(Inital.Muscles);
                targetMuscles.CopyFrom(Target.Muscles);
                AvatarJob.Time = normalizedTime;

                AvatarHandle = AvatarJob.Schedule();

                // Muscle interpolation job
                musclesJob.Time = normalizedTime;
                musclesHandle = musclesJob.Schedule(muscles.Length, 64, AvatarHandle);
                OutputRotation = math.slerp(Inital.rotation, Target.rotation, normalizedTime);
                // Complete the jobs and apply the results
                musclesHandle.Complete();

                ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, OuputVectors[1], OuputVectors[0], OutputRotation, muscles);
                PoseHandler.SetHumanPose(ref HumanPose);

                RemotePlayer.RemoteBoneDriver.SimulateAndApply();
                RemotePlayer.UpdateTransform(RemotePlayer.MouthControl.OutgoingWorldData.position, RemotePlayer.MouthControl.OutgoingWorldData.rotation);
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
        public void LateUpdate()
        {
            if (Ready)
            {
                Compute();
                AudioReceiverModule.LateUpdate();
            }
        }
        public bool IsAbleToUpdate()
        {
            return NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.Avatar != null;
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
        public void ReceiveNetworkAudio(AudioSegmentMessage audioSegment)
        {
            if (AudioReceiverModule.decoder != null)
            {
                AudioReceiverModule.decoder.OnEncoded(audioSegment.audioSegmentData.buffer);
                NetworkedPlayer.Player.AudioReceived?.Invoke(true);
            }
        }
        public void ReceiveSilentNetworkAudio(AudioSilentSegmentDataMessage audioSilentSegment)
        {
            if (AudioReceiverModule.decoder != null)
            {
                if (silentData == null || silentData.Length != AudioReceiverModule.SegmentSize)
                {
                    silentData = new float[AudioReceiverModule.SegmentSize];
                    Array.Fill(silentData, 0f);
                }
                AudioReceiverModule.OnDecoded(silentData);
                NetworkedPlayer.Player.AudioReceived?.Invoke(false);
            }
        }
        public void ReceiveNetworkAvatarData(ServerSideSyncPlayerMessage serverSideSyncPlayerMessage)
        {
            BasisNetworkAvatarDecompressor.DecompressAndProcessAvatar(this, serverSideSyncPlayerMessage);
        }
        public void ReceiveAvatarChangeRequest(ServerAvatarChangeMessage ServerAvatarChangeMessage)
        {
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
                Initialize();
                Ready = true;
                NetworkedPlayer = networkedPlayer;
                RemotePlayer = (BasisRemotePlayer)NetworkedPlayer.Player;
                AudioReceiverModule.OnEnable(networkedPlayer, gameObject);
                OnAvatarCalibration();
                if (HasEvents == false)
                {
                    RemotePlayer.RemoteAvatarDriver.CalibrationComplete += OnCalibration;
                    HasEvents = true;
                }
            }
        }
        public void OnDestroy()
        {
            Destroy();
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
        public void OnCalibration()
        {
            AudioReceiverModule.OnCalibration(NetworkedPlayer);
        }
        public override void DeInitialize()
        {
            AudioReceiverModule.OnDisable();
        }
    }
}