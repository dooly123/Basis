using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ReadyMessage
    {
        public PlayerMetaDataMessage playerMetaDataMessage;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        public void Deserialize(NetDataReader Writer)
        {
            playerMetaDataMessage.Deserialize(Writer);
            clientAvatarChangeMessage.Deserialize(Writer);
            localAvatarSyncMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerMetaDataMessage.Dispose();
            clientAvatarChangeMessage.Dispose();
            localAvatarSyncMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerMetaDataMessage.Serialize(Writer);
            clientAvatarChangeMessage.Serialize(Writer);
            localAvatarSyncMessage.Serialize(Writer);
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
