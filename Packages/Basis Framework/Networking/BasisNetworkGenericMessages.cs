using Basis.Scripts.BasisSdk;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
using static SerializableDarkRift;

public static class BasisNetworkGenericMessages
{
    public static void HandleServerSceneDataMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerSceneDataMessage ServerAvatarChangeMessage);
        ushort PlayerID = ServerAvatarChangeMessage.PlayerIdMessage.playerID;
        SceneDataMessage SceneDataMessage = ServerAvatarChangeMessage.SceneDataMessage;
        BasisScene.Instance.OnNetworkMessageReceived?.Invoke(PlayerID, SceneDataMessage.messageIndex, SceneDataMessage.buffer);
    }
    public static void HandleServerAvatarDataMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarDataMessage ServerAvatarDataMessage);
        ushort PlayerID = ServerAvatarDataMessage.playerIdMessage.playerID;
        if (BasisNetworkManagement.Instance.Players.TryGetValue(PlayerID, out BasisNetworkedPlayer Player))
        {
            if (Player.Player.Avatar != null)
            {
                AvatarDataMessage output = ServerAvatarDataMessage.avatarDataMessage;
                Player.Player.Avatar.OnNetworkMessageReceived?.Invoke(output.messageIndex, output.buffer);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    public static void OnNetworkMessageSend(ushort MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            SceneDataMessage AvatarDataMessage = new SceneDataMessage
            {
                messageIndex = MessageIndex,
                buffer = buffer
            };
            writer.Write(AvatarDataMessage);
            using (var msg = Message.Create(BasisTags.SceneGenericMessage, writer))
            {
                BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.SceneChannel, DeliveryMethod);
            }
        }
    }
}
