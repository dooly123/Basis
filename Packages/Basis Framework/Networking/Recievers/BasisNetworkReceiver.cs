using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using System;
using Unity.Collections;
using Unity.Jobs;
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

        public BasisRemotePlayer RemotePlayer;
        public bool HasEvents = false;
        /// <summary>
        /// represents the final position that we are goign to
        /// </summary>
        [SerializeField]
        public BasisAvatarData TargetData = new BasisAvatarData();
        /// <summary>
        /// represents the most recently applied data
        /// </summary>
        [SerializeField]
        public BasisAvatarData CurrentData = new BasisAvatarData();
        /// <summary>
        /// CurrentData equals final
        /// TargetData is the networks most recent info
        /// continue tomorrow -LD
        /// </summary>
        public override void Compute()
        {
            if (!IsAbleToUpdate())
            {
                return;
            }

            double currentTime = Time.realtimeSinceStartupAsDouble;

            // Remove outdated rotations
            while (AvatarDataBuffer.Count > 1 && currentTime - delayTime > AvatarDataBuffer[1].timestamp)
            {
                AvatarDataBuffer.RemoveAt(0);
            }

            // Only run job if there are enough data points
            if (AvatarDataBuffer.Count >= 2)
            {
                double startTime = AvatarDataBuffer[0].timestamp;
                double endTime = AvatarDataBuffer[1].timestamp;
                double targetTime = currentTime - delayTime;

                // Calculate normalized interpolation factor t
                float t = (float)((targetTime - startTime) / (endTime - startTime));
                t = Mathf.Clamp01(t);

                NativeArray<Quaternion> rotations = new NativeArray<Quaternion>(1, Allocator.TempJob);
                NativeArray<Quaternion> targetRotations = new NativeArray<Quaternion>(1, Allocator.TempJob);
                NativeArray<Vector3> positions = new NativeArray<Vector3>(1, Allocator.TempJob);
                NativeArray<Vector3> targetPositions = new NativeArray<Vector3>(1, Allocator.TempJob);
                NativeArray<Vector3> scales = new NativeArray<Vector3>(1, Allocator.TempJob);
                NativeArray<Vector3> targetScales = new NativeArray<Vector3>(1, Allocator.TempJob);

                // Copy data from AvatarDataBuffer into the NativeArrays
                rotations[0] = AvatarDataBuffer[0].rotation;
                targetRotations[0] = AvatarDataBuffer[1].rotation;
                positions[0] = AvatarDataBuffer[0].Position;
                targetPositions[0] = AvatarDataBuffer[1].Position;
                scales[0] = AvatarDataBuffer[0].Scale;
                targetScales[0] = AvatarDataBuffer[1].Scale;

                // Schedule the job to interpolate positions, rotations, and scales
                AvatarJobs.AvatarJob = new UpdateAvatarRotationJob
                {
                    rotations = rotations,
                    targetRotations = targetRotations,
                    positions = positions,
                    targetPositions = targetPositions,
                    scales = scales,
                    targetScales = targetScales,
                    t = t
                };

                AvatarJobs.AvatarHandle = AvatarJobs.AvatarJob.Schedule();

                // Muscle interpolation job
                NativeArray<float> muscles = new NativeArray<float>(AvatarDataBuffer[0].Muscles, Allocator.TempJob);
                NativeArray<float> targetMuscles = new NativeArray<float>(AvatarDataBuffer[1].Muscles, Allocator.TempJob);

                UpdateAvatarMusclesJob musclesJob = new UpdateAvatarMusclesJob
                {
                    muscles = muscles,
                    targetMuscles = targetMuscles,
                    t = t
                };

                JobHandle musclesHandle = musclesJob.Schedule(muscles.Length, 64, AvatarJobs.AvatarHandle);

                // Complete the jobs and apply the results
                musclesHandle.Complete();

                // After jobs are done, apply the resulting values
                CurrentData.Rotation = rotations[0];
                CurrentData.Vectors[1] = positions[0];
                CurrentData.Vectors[0] = scales[0];

                // Apply muscle data
                for (int Index = 0; Index < 95; Index++)
                {
                    CurrentData.Muscles[Index] = muscles[Index];
                }

                // Dispose of NativeArrays after use
                rotations.Dispose();
                targetRotations.Dispose();
                positions.Dispose();
                targetPositions.Dispose();
                scales.Dispose();
                targetScales.Dispose();
                muscles.Dispose();
                targetMuscles.Dispose();

                ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, CurrentData, ref HumanPose);
                PoseHandler.SetHumanPose(ref HumanPose);

                RemotePlayer.RemoteBoneDriver.SimulateAndApply();
                RemotePlayer.UpdateTransform(RemotePlayer.MouthControl.OutgoingWorldData.position, RemotePlayer.MouthControl.OutgoingWorldData.rotation);
            }
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
        public float[] MuscleFinalStageOutput = new float[90];
        public void ApplyPoseData(Animator animator, BasisAvatarData output, ref HumanPose pose)
        {
            float AvatarHumanScale = animator.humanScale;

            // Directly adjust scaling by applying the inverse of the AvatarHumanScale
            Vector3 Scaling = Vector3.one / AvatarHumanScale;  // Initial scaling with human scale inverse

            // Now adjust scaling with the output scaling vector
            Scaling = Divide(Scaling, output.Vectors[0]);  // Apply custom scaling logic

            // Apply scaling to position
            Vector3 ScaledPosition = Vector3.Scale(output.Vectors[1], Scaling);  // Apply the scaling

            // Apply pose data
            pose.bodyPosition = ScaledPosition;
            pose.bodyRotation = output.Rotation;

            // Ensure muscles array is initialized properly
            if (pose.muscles == null || pose.muscles.Length == 0)
            {
                pose.muscles = new float[95];
            }

            // Check the size of MuscleFinalStageOutput array
            if (MuscleFinalStageOutput.Length < 90)
            {
                Debug.LogError("MuscleFinalStageOutput array size is smaller than expected.");
                return; // Exit if the size is incorrect
            }

            // Copy from job to MuscleFinalStageOutput
            output.Muscles.CopyFrom(MuscleFinalStageOutput);

            // Ensure proper array sizes for the blocks being copied
            if (BasisNetworkSendBase.FirstBufferBytes > MuscleFinalStageOutput.Length || BasisNetworkSendBase.FirstBufferBytes > pose.muscles.Length)
            {
                Debug.LogError("FirstBuffer size exceeds array bounds.");
                return;
            }

            // Copy muscles [0..14]
            Buffer.BlockCopy(MuscleFinalStageOutput, 0, pose.muscles, 0, BasisNetworkSendBase.FirstBufferBytes);


            // Assuming MuscleFinalStageOutput and pose.muscles are arrays and BasisNetworkSendBase contains the proper indexes and size
            for (int i = 0; i < BasisNetworkSendBase.SizeAfterGap; i++)
            {
                // Copy each element from MuscleFinalStageOutput to pose.muscles manually
                pose.muscles[BasisNetworkSendBase.SecondBuffer + i] = MuscleFinalStageOutput[BasisNetworkSendBase.SecondBuffer + i];
            }

            // Adjust the local scale of the animator's transform
            animator.transform.localScale = output.Vectors[0];  // Directly adjust scale with output scaling
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
            BasisNetworkAvatarDecompressor.DeCompress(this, serverSideSyncPlayerMessage);
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
                if (TargetData.Muscles.IsCreated == false)
                {
                    TargetData.Muscles.ResizeArray(90);
                    TargetData.floatArray = new float[90];
                }
                if (CurrentData.Muscles.IsCreated == false)
                {
                    CurrentData.floatArray = new float[90];
                    CurrentData.Muscles.ResizeArray(90);
                }
                InitalizeDataJobs(ref AvatarJobs);
                InitalizeAvatarStoredData(ref TargetData);
                InitalizeAvatarStoredData(ref CurrentData);
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
            TargetData.Vectors.Dispose();
            TargetData.Muscles.Dispose();

            CurrentData.Vectors.Dispose();
            CurrentData.Muscles.Dispose();

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