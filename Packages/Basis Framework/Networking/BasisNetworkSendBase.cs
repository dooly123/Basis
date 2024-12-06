using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift.Server.Plugins.Commands;
using DarkRift;
using UnityEngine;
using static SerializableDarkRift;
namespace Basis.Scripts.Networking.NetworkedAvatar
{
    /// <summary>
    /// the goal of this script is to be the glue of consistent data between remote and local
    /// </summary>
    public abstract partial class BasisNetworkSendBase : MonoBehaviour
    {
        public bool Ready;
        public BasisNetworkedPlayer NetworkedPlayer;
        private readonly object _lock = new object(); // Lock object for thread-safety
        private bool _hasReasonToSendAudio;
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
        public struct AvatarBuffer
        {
            public Unity.Mathematics.quaternion rotation;
            public Unity.Mathematics.float3 Scale;
            public Unity.Mathematics.float3 Position;
            public float[] Muscles;
            public double SecondsInterval;
        }
        [SerializeField]
        public HumanPose HumanPose = new HumanPose();
        [SerializeField]
        public PlayerIdMessage NetworkNetID = new PlayerIdMessage();
        [SerializeField]
        public HumanPoseHandler PoseHandler;
        [SerializeField]
        public static BasisRangedUshortFloatData PositionRanged = new BasisRangedUshortFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
        [SerializeField]
        public static BasisRangedUshortFloatData ScaleRanged = new BasisRangedUshortFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);

        public const int SizeAfterGap = 95 - SecondBuffer;
        public const int FirstBuffer = 15;
        public const int SecondBuffer = 21;
        public abstract void Initialize(BasisNetworkedPlayer NetworkedPlayer);
        public abstract void DeInitialize();
        public void OnAvatarCalibration()
        {
            if (NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.Avatar != null)
            {
                PoseHandler = new HumanPoseHandler(NetworkedPlayer.Player.Avatar.Animator.avatar, NetworkedPlayer.Player.Avatar.transform.transform);
                PoseHandler.GetHumanPose(ref HumanPose);
                if (NetworkedPlayer.Player.Avatar.HasSendEvent == false)
                {
                    NetworkedPlayer.Player.Avatar.OnNetworkMessageSend += OnNetworkMessageSend;
                    NetworkedPlayer.Player.Avatar.HasSendEvent = true;
                }
                NetworkedPlayer.Player.Avatar.LinkedPlayerID = NetworkedPlayer.NetId;
                NetworkedPlayer.Player.Avatar.OnAvatarNetworkReady?.Invoke();

            }
        }
        private void OnNetworkMessageSend(byte MessageIndex, byte[] buffer = null, DeliveryMethod DeliveryMethod = DeliveryMethod.Sequenced, ushort[] Recipients = null)
        {
            // Check if Recipients or buffer arrays are valid or not
            if (Recipients != null && Recipients.Length == 0) Recipients = null;

            if (buffer != null && buffer.Length == 0) buffer = null;

            ushort NetId = NetworkedPlayer.NetId;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                // Handle cases based on presence of Recipients and buffer
                if (Recipients == null)
                {
                    if (buffer == null)
                    {
                        AvatarDataMessage_NoRecipients_NoPayload AvatarDataMessage_NoRecipients_NoPayload = new AvatarDataMessage_NoRecipients_NoPayload
                        {
                             playerIdMessage = new PlayerIdMessage() { playerID = NetId },
                            messageIndex = MessageIndex
                        };
                        writer.Write(AvatarDataMessage_NoRecipients_NoPayload);
                        // No recipients and no payload
                        WriteAndSendMessage(BasisTags.AvatarGenericMessage_NoRecipients_NoPayload,writer, DeliveryMethod);
                    }
                    else
                    {
                        AvatarDataMessage_NoRecipients AvatarDataMessage_NoRecipients = new AvatarDataMessage_NoRecipients
                        {
                             playerIdMessage = new PlayerIdMessage() { playerID = NetId },
                            messageIndex = MessageIndex,
                            payload = buffer
                        };

                        writer.Write(AvatarDataMessage_NoRecipients);
                        // No recipients but has payload
                        WriteAndSendMessage(BasisTags.AvatarGenericMessage_NoRecipients,writer, DeliveryMethod);
                    }
                }
                else
                {
                    if (buffer == null)
                    {
                        AvatarDataMessage_Recipients_NoPayload AvatarDataMessage = new AvatarDataMessage_Recipients_NoPayload();
                        AvatarDataMessage.playerIdMessage = new PlayerIdMessage() { playerID = NetId };
                        AvatarDataMessage.messageIndex = MessageIndex;
                        AvatarDataMessage.recipients = Recipients;
                        // Recipients present, payload may or may not be present
                        writer.Write(AvatarDataMessage);
                        WriteAndSendMessage(BasisTags.AvatarGenericMessage_Recipients_NoPayload, writer, DeliveryMethod);
                    }
                    else
                    {
                        AvatarDataMessage AvatarDataMessage = new AvatarDataMessage
                        {
                             playerIdMessage = new PlayerIdMessage() { playerID = NetId },
                            messageIndex = MessageIndex,
                            payload = buffer,
                            recipients = Recipients
                        };
                        // Recipients present, payload may or may not be present
                        writer.Write(AvatarDataMessage);
                        WriteAndSendMessage(BasisTags.AvatarGenericMessage, writer, DeliveryMethod);
                    }
                }
            }
        }

        // Helper method to avoid code duplication
        private void WriteAndSendMessage(ushort tag, DarkRiftWriter writer, DeliveryMethod deliveryMethod)
        {
            using (var msg = Message.Create(tag, writer))
            {
                BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.AvatarChannel, deliveryMethod);
            }
        }

    }
}