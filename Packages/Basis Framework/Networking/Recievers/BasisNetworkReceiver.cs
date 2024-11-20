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
        private NativeArray<float3> transforms;      // Merged positions and scales
        private NativeArray<float3> targetTransforms; // Merged target positions and scales
        private NativeArray<float> muscles;
        private NativeArray<float> targetMuscles;
        public void Initialize()
        {
            transforms = new NativeArray<float3>(2, Allocator.Persistent); // Index 0 = position, Index 1 = scale
            targetTransforms = new NativeArray<float3>(2, Allocator.Persistent); // Index 0 = target position, Index 1 = target scale
            muscles = new NativeArray<float>(90, Allocator.Persistent);
            targetMuscles = new NativeArray<float>(90, Allocator.Persistent);
        }
        /// <summary>
        /// Clean up resources used in the compute process.
        /// </summary>
        public void Destroy()
        {
            if (transforms.IsCreated) transforms.Dispose();
            if (targetTransforms.IsCreated) targetTransforms.Dispose();
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
                transforms[0] = AvatarDataBuffer[0].Position; // Position at index 0
                transforms[1] = AvatarDataBuffer[0].Scale;    // Scale at index 1
                targetTransforms[0] = AvatarDataBuffer[1].Position; // Target position at index 0
                targetTransforms[1] = AvatarDataBuffer[1].Scale;    // Target scale at index 1

                for (int i = 0; i < 90; i++)
                {
                    muscles[i] = AvatarDataBuffer[0].Muscles[i];
                    targetMuscles[i] = AvatarDataBuffer[1].Muscles[i];
                }

                // Schedule the job to interpolate positions, rotations, and scales
                AvatarJobs.AvatarJob = new UpdateAvatarRotationJob
                {
                    rotations = AvatarDataBuffer[0].rotation,
                    targetRotations = AvatarDataBuffer[1].rotation,
                    TransformationalOutput = transforms,         // Merged positions
                    TransformationalInput = targetTransforms, // Merged target positions
                    t = t
                };

                AvatarJobs.AvatarHandle = AvatarJobs.AvatarJob.Schedule();

                // Muscle interpolation job
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
                CurrentData.Rotation = AvatarJobs.AvatarJob.rotations;
                CurrentData.Vectors[1] = transforms[0]; // Position
                CurrentData.Vectors[0] = transforms[1]; // Scale

                // Apply muscle data
                for (int i = 0; i < 90; i++)
                {
                    CurrentData.Muscles[i] = muscles[i];
                }

                ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, CurrentData);
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
        public void ApplyPoseData(Animator animator, BasisAvatarData output)
        {
            float AvatarHumanScale = animator.humanScale;

            // Directly adjust scaling by applying the inverse of the AvatarHumanScale
            Vector3 Scaling = Vector3.one / AvatarHumanScale;  // Initial scaling with human scale inverse

            // Now adjust scaling with the output scaling vector
            Scaling = Divide(Scaling, output.Vectors[0]);  // Apply custom scaling logic

            // Apply scaling to position
            Vector3 ScaledPosition = Vector3.Scale(output.Vectors[1], Scaling);  // Apply the scaling

            // Apply pose data
            HumanPose.bodyPosition = ScaledPosition;
            HumanPose.bodyRotation = output.Rotation;

            // Copy from job to MuscleFinalStageOutput
            output.Muscles.CopyTo(MuscleFinalStageOutput);
            // First, copy the first 14 elements directly
            Array.Copy(MuscleFinalStageOutput, 0, HumanPose.muscles, 0, FirstBuffer);

            // Then, copy the remaining elements from index 15 onwards into the pose.muscles array, starting from index 21
            Array.Copy(MuscleFinalStageOutput, FirstBuffer, HumanPose.muscles, SecondBuffer, SizeAfterGap);

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
                InitalizeDataJobs(ref AvatarJobs);
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