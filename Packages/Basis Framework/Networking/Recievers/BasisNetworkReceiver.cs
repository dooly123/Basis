using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Smoothing;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.Recievers
{
    [DefaultExecutionOrder(15001)]
    [System.Serializable]
    public partial class BasisNetworkReceiver : BasisNetworkSendBase
    {
        public float[] silentData;
        public BasisAvatarLerpDataSettings Settings;

        [SerializeField]
        public BasisAudioReceiver AudioReceiverModule = new BasisAudioReceiver();

        public BasisRemotePlayer RemotePlayer;
        public bool HasEvents = false;
        /// <summary>
        /// CurrentData equals final
        /// TargetData is the networks most recent info
        /// </summary>
        public override void Compute()
        {
            if (!IsAbleToUpdate())
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            BasisAvatarLerp.UpdateAvatar(NetworkAvatarSyncDelta, ref CurrentData, LastData, TargetData, AvatarJobs, TimeAsDoubleWhenLastSync, Settings.LerpSpeedMovement, deltaTime, deltaTime * Settings.LerpSpeedMuscles, Settings.TeleportDistanceSquared);

            ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, CurrentData, ref HumanPose);
            PoseHandler.SetHumanPose(ref HumanPose);

            RemotePlayer.RemoteBoneDriver.SimulateAndApply();
            RemotePlayer.UpdateTransform(RemotePlayer.MouthControl.OutgoingWorldData.position, RemotePlayer.MouthControl.OutgoingWorldData.rotation);
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
        public void ApplyPoseData(Animator animator, BasisAvatarData output, ref HumanPose pose)
        {
            float AvatarHumanScale = animator.humanScale;

            // Directly adjust scaling by applying the inverse of the AvatarHumanScale
            Vector3 Scaling = Vector3.one / AvatarHumanScale;  // Initial scaling with human scale inverse
         //   Debug.Log("Initial Scaling: " + Scaling);

            // Now adjust scaling with the output scaling vector
            Scaling = Divide(Scaling, output.Vectors[0]);  // Apply custom scaling logic
         //   Debug.Log("Adjusted Scaling: " + Scaling);

            // Apply scaling to position
            Vector3 ScaledPosition = Vector3.Scale(output.Vectors[1], Scaling);  // Apply the scaling

            // Apply pose data
            pose.bodyPosition = ScaledPosition;
            pose.bodyRotation = output.Rotation;

            // Ensure muscles array is correctly sized
            if (pose.muscles == null || pose.muscles.Length != output.Muscles.Length)
            {
                pose.muscles = output.Muscles.ToArray();
            }
            else
            {
                output.Muscles.CopyTo(pose.muscles);
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
            if(CompressionArraysRangedUshort == null)
            {
                CompressionArraysRangedUshort = new CompressionArraysRangedUshort(95, -180, 180, BasisNetworkConstants.MusclePrecision, true);
            }
            BasisNetworkAvatarDecompressor.DeCompress(this, serverSideSyncPlayerMessage);
        }
        public void ReceiveAvatarChangeRequest(ServerAvatarChangeMessage ServerAvatarChangeMessage)
        {
            BasisLoadableBundle BasisLoadableBundle = BasisBundleConversionNetwork.ConvertNetworkBytesToBasisLoadableBundle(ServerAvatarChangeMessage.clientAvatarChangeMessage.byteArray);

            RemotePlayer.CreateAvatar(ServerAvatarChangeMessage.clientAvatarChangeMessage.loadMode, BasisLoadableBundle);
        }
        public override async void Initialize(BasisNetworkedPlayer networkedPlayer)
        {
            if (!Ready)
            {
                InitalizeDataJobs(ref AvatarJobs);
                InitalizeAvatarStoredData(ref TargetData);
                InitalizeAvatarStoredData(ref CurrentData);
                InitalizeAvatarStoredData(ref LastData);
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BasisAvatarLerpDataSettings> handle = Addressables.LoadAssetAsync<BasisAvatarLerpDataSettings>(BasisAvatarLerp.Settings);
                await handle.Task;
                Settings = handle.Result;
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
            LastData.Vectors.Dispose();
            LastData.Muscles.Dispose();

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
            if (CompressionArraysRangedUshort != null)
            {
                CompressionArraysRangedUshort.Dispose();
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