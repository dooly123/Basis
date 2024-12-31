using Basis.Network.Core;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Profiler;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using static BasisNetworkPrimitiveCompression;
using static SerializableBasis;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    /// <summary>
    /// the goal of this script is to be the glue of consistent data between remote and local
    /// </summary>
    [System.Serializable]
    public abstract class BasisNetworkSendBase
    {
        public bool Ready;
        [SerializeField]
        public BasisNetworkedPlayer NetworkedPlayer;
        private readonly object _lock = new object(); // Lock object for thread-safety
        private bool _hasReasonToSendAudio;
        public int Offset = 0;
        public bool HasReasonToSendAudio
        {
            get
            {
                lock (_lock)
                {
                    return _hasReasonToSendAudio;
                }
            }
            set
            {
                lock (_lock)
                {
                    _hasReasonToSendAudio = value;
                }
            }
        }
        public static BasisRangedUshortFloatData RotationCompression = new BasisRangedUshortFloatData(-1f, 1f, 0.001f);
        [SerializeField]
        public HumanPose HumanPose = new HumanPose();
        [SerializeField]
        public HumanPoseHandler PoseHandler;
        public const int SizeAfterGap = 95 - SecondBuffer;
        public const int FirstBuffer = 15;
        public const int SecondBuffer = 21;
        public abstract void Initialize(BasisNetworkedPlayer NetworkedPlayer);
        public abstract void DeInitialize();
        public void OnAvatarCalibration()
        {
            if (BasisNetworkManagement.MainThreadContext == null)
            {
                BasisDebug.LogError("Main thread context is not set. Ensure this script is started on the main thread.");
                return;
            }

            // Post the task to the main thread
            BasisNetworkManagement.MainThreadContext.Post(_ =>
            {
                if (NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.BasisAvatar != null)
                {
                    ComputeHumanPose();
                    if (!NetworkedPlayer.Player.BasisAvatar.HasSendEvent)
                    {
                        NetworkedPlayer.Player.BasisAvatar.OnNetworkMessageSend += OnNetworkMessageSend;
                        NetworkedPlayer.Player.BasisAvatar.HasSendEvent = true;
                    }

                    NetworkedPlayer.Player.BasisAvatar.LinkedPlayerID = NetworkedPlayer.NetId;
                    NetworkedPlayer.Player.BasisAvatar.OnAvatarNetworkReady?.Invoke(NetworkedPlayer.Player.IsLocal);
                }
            }, null);
        }
        public void ComputeHumanPose()
        {
            if (NetworkedPlayer == null)
            {
                BasisDebug.LogError("NetworkedPlayer is null! Cannot compute HumanPose.");
                return;
            }

            if (NetworkedPlayer.Player == null)
            {
                BasisDebug.LogError("NetworkedPlayer.Player is null! Cannot compute HumanPose.");
                return;
            }

            if (NetworkedPlayer.Player.BasisAvatar == null)
            {
                BasisDebug.LogError("BasisAvatar is null! Cannot compute HumanPose.");
                return;
            }
            // All checks passed
            PoseHandler = new HumanPoseHandler(
                NetworkedPlayer.Player.BasisAvatar.Animator.avatar,
                NetworkedPlayer.Player.BasisAvatar.transform
            );
            PoseHandler.GetHumanPose(ref HumanPose);
        }
        private void OnNetworkMessageSend(byte MessageIndex, byte[] buffer = null, DeliveryMethod DeliveryMethod = DeliveryMethod.Sequenced, ushort[] Recipients = null)
        {
            // Handle cases based on presence of Recipients and buffer
            AvatarDataMessage AvatarDataMessage = new AvatarDataMessage
            {
                messageIndex = MessageIndex,
                payload = buffer,
                recipients = Recipients,
                PlayerIdMessage = new PlayerIdMessage() { playerID = NetworkedPlayer.NetId },
            };
            NetDataWriter netDataWriter = new NetDataWriter();
            if (DeliveryMethod == DeliveryMethod.Unreliable)
            {
                netDataWriter.Put(BasisNetworkCommons.AvatarChannel);
                AvatarDataMessage.Serialize(netDataWriter);
                BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.FallChannel, DeliveryMethod);
            }
            else
            {
                AvatarDataMessage.Serialize(netDataWriter);
                BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.AvatarChannel, DeliveryMethod);
            }
            BasisNetworkProfiler.AvatarDataMessageCounter.Sample(netDataWriter.Length);
        }
        public static float[] MinMuscle;
        public static float[] MaxMuscle;
        public static float[] RangeMuscle;
        public static void SetupData()
        {
            MinMuscle = new float[LocalAvatarSyncMessage.StoredBones];
            MaxMuscle = new float[LocalAvatarSyncMessage.StoredBones];
            RangeMuscle = new float[LocalAvatarSyncMessage.StoredBones];
            for (int i = 0, j = 0; i < LocalAvatarSyncMessage.StoredBones; i++)
            {
                if (i < FirstBuffer || i > SecondBuffer)
                {
                    MinMuscle[j] = HumanTrait.GetMuscleDefaultMin(i);
                    MaxMuscle[j] = HumanTrait.GetMuscleDefaultMax(i);
                    j++;
                }
            }
            for (int Index = 0; Index < LocalAvatarSyncMessage.StoredBones; Index++)
            {
                RangeMuscle[Index] = MaxMuscle[Index] - MinMuscle[Index];
            }
        }
    }
}
