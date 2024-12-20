using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using LiteNetLib;
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
    public byte[] Buffer;
    public DeliveryMethod Method = DeliveryMethod.Unreliable;
    public void Awake()
    {
        avatar.OnAvatarReady += OnAvatarReady;
        avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }
    public void OnDestroy()
    {
        avatar.OnAvatarReady -= OnAvatarReady;
        avatar.OnNetworkMessageReceived -= OnNetworkMessageReceived;
    }
    private void OnAvatarReady(bool IsOwner)
    {
        Debug.Log("OnAvatarReady");
        if (IsOwner)
        {
            if (BasisNetworkManagement.AvatarToPlayer(avatar, out BasisPlayer))
            {
                Isready = true;
                Debug.Log("Actually ran!");
            }
        }
    }

    public void Update()
    {
        if (Isready)
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                //(byte MessageIndex, byte[] buffer = null, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable, ushort[] Recipients = null)
                avatar.NetworkMessageSend(MessageIndexTest, Buffer, Method, Recipients);
               // BasisNetworkManagement.RequestOwnership();
            }
        }
    }
    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer = null, DeliveryMethod Method = DeliveryMethod.ReliableSequenced)
    {
        Debug.Log("got message " + MessageIndex);
        if (MessageIndex == MessageIndexTest)
        {
            BasisLocalPlayer.Instance.Move.HandleJump();
        }
    }
}
