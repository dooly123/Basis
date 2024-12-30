using Basis.Scripts.BasisSdk;
using Basis.Scripts.Networking;
using LiteNetLib;
using UnityEngine;

public class BasisTestRoundTrip : MonoBehaviour
{
    public BasisAvatar Avatar;
    public byte RemoteMessageIndex = 32;
    public byte LocalMessageIndex = 64;
    public LiteNetLib.DeliveryMethod DeliveryMethod;
    void Awake()
    {
        Avatar.OnAvatarReady += OnAvatarReady;
        Avatar.OnAvatarNetworkReady += OnAvatarNetworkReady;
        Avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }
    public void OnDestroy()
    {
        Avatar.OnAvatarReady -= OnAvatarReady;
        Avatar.OnAvatarNetworkReady -= OnAvatarNetworkReady;
        Avatar.OnNetworkMessageReceived -= OnNetworkMessageReceived;
    }

    private void OnAvatarNetworkReady(bool IsOwner)
    {
        if (IsOwner == false)
        {
            Debug.Log("I was not the owner");
            Avatar.NetworkMessageSend(RemoteMessageIndex, DeliveryMethod);
        }
        else
        {
            Avatar.NetworkMessageSend(LocalMessageIndex, DeliveryMethod);
            Debug.Log("I was the owner of this avatar");
        }
    }
    private void OnAvatarReady(bool IsOwner)
    {
        Debug.Log("OnAvatarReady " + IsOwner);
    }
    /// <summary>
    /// Handles network message reception.
    /// </summary>
    /// <param name="PlayerID">The ID of the player sending the message.</param>
    /// <param name="MessageIndex">The index of the message type.</param>
    /// <param name="buffer">The data buffer received.</param>
    /// <param name="Recipients">The array of recipient player IDs.</param>
    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer, DeliveryMethod Method = DeliveryMethod.ReliableSequenced)
    {
        // Null checks for buffer and Recipients arrays
        if (buffer == null)
        {
            if (BasisNetworkManagement.Players.TryGetValue(PlayerID, out Basis.Scripts.Networking.NetworkedPlayer.BasisNetworkedPlayer netPlayer))
            {
                if (netPlayer.Player.IsLocal)
                {
                    Debug.LogError($"Buffer is null. MessageIndex: {MessageIndex}, Was a loop back: {PlayerID}");
                }
                else
                {
                    Debug.LogError($"Buffer is null. MessageIndex: {MessageIndex}, Was from remote player: {PlayerID}");
                }
            }
            else
            {
                Debug.LogError($"Buffer is null. MessageIndex: {MessageIndex}, Was from unknown remote player: {PlayerID}");
            }
            return;
        }

        // Try to get the player from the network management system
        if (BasisNetworkManagement.Players.TryGetValue(PlayerID, out Basis.Scripts.Networking.NetworkedPlayer.BasisNetworkedPlayer Player))
        {
            Debug.Log($"Rec Avatar Message from player {Player.Player.DisplayName}, MessageIndex: {MessageIndex}, Buffer Length: {buffer.Length}, Recipients Count: {Method}");
        }
        else
        {
            Debug.Log($"Player ID {PlayerID} not found. MessageIndex: {MessageIndex}, Buffer Length: {buffer.Length}, Recipients Count: {Method}");
        }
    }
}
