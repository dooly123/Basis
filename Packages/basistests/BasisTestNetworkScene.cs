using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using UnityEngine;

public class BasisTestNetworkScene : MonoBehaviour
{
    public BasisScene scene;
    public byte[] SendingData;
    public void Awake()
    {
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
    }
    /// <summary>
    /// this runs after a remote user connects and passes all there local checks and balances with the server
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    private void OnRemotePlayerJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {

    }
    /// <summary>
    /// this is called once
    /// level is loaded
    /// network is connected
    /// player is created
    /// player is authenticated
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    public void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        scene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        scene.OnNetworkMessageSend.Invoke(1, SendingData, DarkRift.DeliveryMethod.ReliableOrdered);
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer,ushort[] Recipients)
    {

    }
}
