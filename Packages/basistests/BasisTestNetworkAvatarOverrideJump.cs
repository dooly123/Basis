using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using UnityEngine;
using UnityEngine.InputSystem;
public class BasisTestNetworkAvatarOverrideJump : MonoBehaviour
{
    [Header("Assign Ahead Of Time")]
    public BasisAvatar avatar;
    public byte MessageIndexTest = 2;
    public ushort[] Recipients = null;
    public BasisPlayer BasisPlayer;
    public bool Isready;
    public DeliveryMethod Method = DeliveryMethod.Unreliable;
    public void Awake()
    {
        avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined += SignalReadyTosend;
    }

    private void SignalReadyTosend(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        if (BasisNetworkManagement.AvatarToPlayer(avatar, out BasisPlayer))
        {
            Isready = true;
        }
    }

    public void Update()
    {
        if (Isready)
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                avatar.NetworkMessageSend(MessageIndexTest, Method);
            }
        }
    }
    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer = null, ushort[] Recipients = null)
    {
        Debug.Log("got message " + MessageIndex);
        if (MessageIndex == MessageIndexTest)
        {
            BasisLocalPlayer.Instance.Move.HandleJump();
        }
    }
}