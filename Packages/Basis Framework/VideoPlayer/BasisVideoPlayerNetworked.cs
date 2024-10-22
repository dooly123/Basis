using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using BasisSerializer.OdinSerializer;
using DarkRift;
using UnityEngine;
public class BasisVideoPlayerNetworked : MonoBehaviour
{
    public BasisVideoPlayer VideoPlayer;
    public ushort MyCurrentOwner;
    public const ushort UrlSyncId = 420;
    public string MyUniqueString = "testing this damn string";
    public void Awake()
    {
        if (VideoPlayer == null)
        {
            Debug.LogError("Missing the video player!");
            this.enabled = false;
        }
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft += OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft += OnRemotePlayerLeft;
        if (BasisNetworkManagement.HasSentOnLocalPlayerJoin)
        {
            BasisNetworkManagement.OnOwnershipTransfer += OnOwnershipTransfer;
            BasisNetworkManagement.RequestCurrentOwnership(MyUniqueString);
        }
    }
    public void OnDestroy()
    {
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined -= OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined -= OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft -= OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft -= OnRemotePlayerLeft;
    }
    public void Play(string Url)
    {
        byte[] CompressedString = SerializationUtility.SerializeValue(Url, DataFormat.Binary);
        BasisScene.NetworkMessageSend(UrlSyncId, CompressedString, DeliveryMethod.ReliableSequenced);
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        if (MessageIndex != UrlSyncId) return;
        string url = SerializationUtility.DeserializeValue<string>(buffer, DataFormat.Binary);
        VideoPlayer.Play(url);
    }
    public void OnRemotePlayerLeft(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
    }
    public void OnLocalPlayerLeft(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
    }
    public void OnRemotePlayerJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
    }
    public void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        BasisNetworkManagement.OnOwnershipTransfer += OnOwnershipTransfer;
        BasisNetworkManagement.RequestCurrentOwnership(MyUniqueString);
    }
    private void OnOwnershipTransfer(string UniqueEntityID, ushort NetIdNewOwner, bool IsOwner)
    {
        if (UniqueEntityID == MyUniqueString)
        {
            MyCurrentOwner = NetIdNewOwner;
            Debug.Log("Updated my Owner To " + UniqueEntityID + " | " + NetIdNewOwner);
        }
        else
        {
            Debug.Log("Recieving someone elses " + UniqueEntityID + " | " + NetIdNewOwner);
        }
    }
}