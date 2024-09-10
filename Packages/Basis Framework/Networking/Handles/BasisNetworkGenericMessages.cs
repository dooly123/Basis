using Basis.Scripts.BasisSdk;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
using static SerializableDarkRift;

public static class BasisNetworkGenericMessages
{
    // Handler for server scene data messages
    public static void HandleServerSceneDataMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerSceneDataMessage serverSceneDataMessage);
        ushort playerID = serverSceneDataMessage.playerIdMessage.playerID;
        SceneDataMessage sceneDataMessage = serverSceneDataMessage.sceneDataMessage;
        BasisScene.Instance.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, sceneDataMessage.payload);
    }

    // Handler for server avatar data messages
    public static void HandleServerAvatarDataMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarDataMessage serverAvatarDataMessage);
        ushort avatarLinkID = serverAvatarDataMessage.avatarDataMessage.assignedAvatarPlayer; // destination
        if (BasisNetworkManagement.Instance.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                AvatarDataMessage output = serverAvatarDataMessage.avatarDataMessage;
                if (output.payload == null)
                {
                    Debug.LogError("Missing Payload");
                }
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, output.recipients);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    // Handler for server scene data messages with no recipients
    public static void HandleServerSceneDataMessage_NoRecipients(DarkRiftReader reader)
    {
        reader.Read(out ServerSceneDataMessage_NoRecipients serverSceneDataMessage_NoRecipients);
        ushort playerID = serverSceneDataMessage_NoRecipients.playerIdMessage.playerID;
        var sceneDataMessage = serverSceneDataMessage_NoRecipients.sceneDataMessage;
        BasisScene.Instance.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, sceneDataMessage.payload);
    }

    // Handler for server scene data messages with no recipients and no payload
    public static void HandleServerSceneDataMessage_NoRecipients_NoPayload(DarkRiftReader reader)
    {
        reader.Read(out ServerSceneDataMessage_NoRecipients_NoPayload serverSceneDataMessage_NoRecipients_NoPayload);
        ushort playerID = serverSceneDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID;
        var sceneDataMessage = serverSceneDataMessage_NoRecipients_NoPayload.sceneDataMessage;
        BasisScene.Instance.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, null); // Pass null payload
    }

    // Handler for server avatar data messages with no recipients
    public static void HandleServerAvatarDataMessage_NoRecipients(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarDataMessage_NoRecipients serverAvatarDataMessage_NoRecipients);
        ushort avatarLinkID = serverAvatarDataMessage_NoRecipients.avatarDataMessage.assignedAvatarPlayer; // destination
        if (BasisNetworkManagement.Instance.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                var output = serverAvatarDataMessage_NoRecipients.avatarDataMessage;
                if (output.payload == null)
                {
                    Debug.LogError("Missing Payload");
                }
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage_NoRecipients.playerIdMessage.playerID, output.messageIndex, output.payload, null);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage_NoRecipients.playerIdMessage.playerID);
        }
    }

    // Handler for server avatar data messages with no recipients and no payload
    public static void HandleServerAvatarDataMessage_NoRecipients_NoPayload(DarkRiftReader reader)
    {
        Debug.LogError("interpreting HandleServerAvatarDataMessage_NoRecipients_NoPayload");
        reader.Read(out ServerAvatarDataMessage_NoRecipients_NoPayload serverAvatarDataMessage_NoRecipients_NoPayload);
        ushort avatarLinkID = serverAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage.assignedAvatarPlayer; // destination
        if (BasisNetworkManagement.Instance.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                var output = serverAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage;
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID, output.messageIndex, null,null); // Pass null payload
            }
            else
            {
                Debug.LogError("cant accept payload no avatar!");
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID);
        }
    }
    // Sending message with different conditions
    public static void OnNetworkMessageSend(ushort messageIndex, byte[] buffer, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, ushort[] recipients = null)
    {
        if (recipients != null && recipients.Length == 0)
        {
            recipients = null;
        }

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            SceneDataMessage sceneDataMessage = new SceneDataMessage
            {
                messageIndex = messageIndex,
                payload = buffer,
                recipients = recipients,
            };
            writer.Write(sceneDataMessage);
            using (var msg = Message.Create(BasisTags.SceneGenericMessage, writer))
            {
                BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.SceneChannel, deliveryMethod);
            }
        }
    }
}