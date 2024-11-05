using Basis.Scripts.BasisSdk.Players;
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
        private float lerpTimeSpeedRotation = 0;
        private float lerpTimeSpeedMuscles = 0;
        public float[] silentData;
        public BasisAvatarLerpDataSettings Settings;

        [SerializeField]
        public BasisAudioReceiver AudioReceiverModule = new BasisAudioReceiver();

        public BasisRemotePlayer RemotePlayer;
        public bool HasEvents = false;
        public override void Compute()
        {
            if (!IsAbleToUpdate())
                return;

            float deltaTime = Time.deltaTime;
            lerpTimeSpeedRotation = deltaTime * Settings.LerpSpeedRotation;
            lerpTimeSpeedMuscles = deltaTime * Settings.LerpSpeedMuscles;

            BasisAvatarLerp.UpdateAvatar(ref Output, Target, AvatarJobs, Settings.LerpSpeedMovement, deltaTime, lerpTimeSpeedRotation, lerpTimeSpeedMuscles, Settings.TeleportDistance);

            ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, Output, ref HumanPose);
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
            Vector3 scale = Vector3.one * animator.humanScale;
            Vector3 Scaling = Divide(Vector3.one, scale);
            Scaling = Divide(Scaling, Output.Vectors[0]);
            Vector3 ScaledPosition = output.Vectors[1];
            ScaledPosition.Scale(Scaling);

            pose.bodyPosition = ScaledPosition;
            pose.bodyRotation = output.Rotation;
            if (pose.muscles == null || pose.muscles.Length != output.Muscles.Length)
            {
                pose.muscles = output.Muscles.ToArray();
            }
            else
            {
                output.Muscles.CopyTo(pose.muscles);
            }
            animator.transform.localScale = Output.Vectors[0];//scale
        }
        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            // Define a small epsilon to avoid division by zero
            const float epsilon = 1e-5f;

            return new Vector3(
                b.x != 0 ? a.x / b.x : a.x / epsilon,
                b.y != 0 ? a.y / b.y : a.y / epsilon,
                b.z != 0 ? a.z / b.z : a.z / epsilon
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
        public override async void Initialize(BasisNetworkedPlayer networkedPlayer)
        {
            if (!Ready)
            {
                InitalizeDataJobs(ref AvatarJobs);
                InitalizeAvatarStoredData(ref Target);
                InitalizeAvatarStoredData(ref Output);
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
            Target.Vectors.Dispose();
            Target.Muscles.Dispose();

            Output.Vectors.Dispose();
            Output.Muscles.Dispose();

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