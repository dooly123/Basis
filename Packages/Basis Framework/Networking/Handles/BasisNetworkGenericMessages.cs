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
        ushort PlayerID = ServerAvatarChangeMessage.playerIdMessage.playerID;
        SceneDataMessage SceneDataMessage = ServerAvatarChangeMessage.sceneDataMessage;
        BasisScene.Instance.OnNetworkMessageReceived?.Invoke(PlayerID, SceneDataMessage.messageIndex, SceneDataMessage.buffer);
    }
    public static void HandleServerAvatarDataMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarDataMessage ServerAvatarDataMessage);
        ushort AvatarLinkID = ServerAvatarDataMessage.avatarDataMessage.assignedAvatarPlayer;//destination
        if (BasisNetworkManagement.Instance.Players.TryGetValue(AvatarLinkID, out BasisNetworkedPlayer Player))
        {
            if (Player.Player == null)
            {
                Debug.LogError("Missing Player! " + AvatarLinkID);
                return;
            }
            if (Player.Player.Avatar != null)
            {
                AvatarDataMessage output = ServerAvatarDataMessage.avatarDataMessage;
                Player.Player.Avatar.OnNetworkMessageReceived?.Invoke(ServerAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, output.recipients);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    public static void OnNetworkMessageSend(ushort MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable, ushort[] Recipients = null)
    {
        if (Recipients != null && Recipients.Length == 0)
        {
            Recipients = null;
        }
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            SceneDataMessage AvatarDataMessage = new SceneDataMessage
            {
                messageIndex = MessageIndex,
                buffer = buffer,
                recipients = Recipients,
            };
            writer.Write(AvatarDataMessage);
            using (var msg = Message.Create(BasisTags.SceneGenericMessage, writer))
            {
                BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.SceneChannel, DeliveryMethod);
            }
        }
    }
}