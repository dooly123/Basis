using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ReadyMessage
    {
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public PlayerMetaDataMessage playerMetaDataMessage;
        public void Deserialize(NetDataReader Writer)
        {
            localAvatarSyncMessage.Deserialize(Writer);
            clientAvatarChangeMessage.Deserialize(Writer);
            playerMetaDataMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            localAvatarSyncMessage.Dispose();
            clientAvatarChangeMessage.Dispose();
            playerMetaDataMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            localAvatarSyncMessage.Serialize(Writer);
            clientAvatarChangeMessage.Serialize(Writer);
            playerMetaDataMessage.Serialize(Writer);
        }
    }
    public struct ServerReadyMessage
    {
        public PlayerIdMessage playerIdMessage;
        public ReadyMessage localReadyMessage;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
             localReadyMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            localReadyMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            localReadyMessage.Serialize(Writer);
        }
    }
}
