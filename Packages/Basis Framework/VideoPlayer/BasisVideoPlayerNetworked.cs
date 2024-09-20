using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using UnityEngine;

public class BasisVideoPlayerNetworked : MonoBehaviour
{
    public BasisVideoPlayer VideoPlayer;
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

    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {

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
    }
}
