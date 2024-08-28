using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Tests;
using DarkRift.Server.Plugins.Commands;
using DarkRift;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    /// <summary>
    /// the goal of this script is to be the glue of consistent data between remote and local
    /// </summary>
    public abstract class BasisNetworkSendBase : MonoBehaviour
    {
        public bool Ready;
        public BasisNetworkedPlayer NetworkedPlayer;
        [SerializeField]
        public BasisAvatarData Target = new BasisAvatarData();
        [SerializeField]
        public BasisAvatarData Output = new BasisAvatarData();
        public HumanPose HumanPose = new HumanPose();
        public LocalAvatarSyncMessage LASM = new LocalAvatarSyncMessage();
        public PlayerIdMessage NetworkNetID = new PlayerIdMessage();
        public BasisDataJobs AvatarJobs;
        public HumanPoseHandler PoseHandler;
        [SerializeField]
        public BasisRangedUshortFloatData PositionRanged;
        [SerializeField]
        public BasisRangedUshortFloatData ScaleRanged;
        protected BasisNetworkSendBase()
        {
            LASM = new LocalAvatarSyncMessage()
            {
                array = new byte[224],
            };
            if (Target.Muscles.IsCreated == false)
            {
                Target.Muscles.ResizeArray(95);
                Target.floatArray = new float[95];
            }
            if (Output.Muscles.IsCreated == false)
            {
                Output.floatArray = new float[95];
                Output.Muscles.ResizeArray(95);
            }
            PositionRanged = new BasisRangedUshortFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
            ScaleRanged = new BasisRangedUshortFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
        }
        public void InitalizeAvatarStoredData(ref BasisAvatarData data, int VectorCount = 3, int QuaternionCount = 1, int MuscleCount = 95)
        {
            data.Vectors = new NativeArray<Vector3>(VectorCount, Allocator.Persistent);
            data.Quaternions = new NativeArray<Quaternion>(QuaternionCount, Allocator.Persistent);
            data.Muscles = new NativeArray<float>(MuscleCount, Allocator.Persistent);
        }
        public void InitalizeDataJobs()
        {
            //jobs
            AvatarJobs.rotationJob = new UpdateAvatarRotationJob();
            AvatarJobs.positionJob = new UpdateAvatarPositionJob();
            AvatarJobs.muscleJob = new UpdateAvatarMusclesJob();
        }
        public abstract void Compute();
        public abstract void Initialize(BasisNetworkedPlayer NetworkedPlayer);
        public abstract void DeInitialize();
        public void OnAvatarCalibration()
        {
            if (NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.Avatar != null)
            {
                PoseHandler = new HumanPoseHandler(NetworkedPlayer.Player.Avatar.Animator.avatar, NetworkedPlayer.Player.Avatar.transform.transform);
            }
            if (NetworkedPlayer.Player.Avatar != null)
            {
                if (NetworkedPlayer.Player.Avatar.HasSendEvent == false)
                {
                    NetworkedPlayer.Player.Avatar.OnNetworkMessageSend += OnNetworkMessageSend;
                    NetworkedPlayer.Player.Avatar.HasSendEvent = true;
                }
            }
        }

        private void OnNetworkMessageSend(byte MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                AvatarDataMessage AvatarDataMessage = new AvatarDataMessage
                {
                    MessageIndex = MessageIndex,
                    buffer = buffer
                };
                writer.Write(AvatarDataMessage);
                using (var msg = Message.Create(BasisTags.AvatarGenericMessage, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.AvatarChannel, DeliveryMethod);
                }
            }
        }
    }
}