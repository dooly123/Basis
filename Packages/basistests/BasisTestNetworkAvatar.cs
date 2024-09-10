using Basis.Scripts.BasisSdk;
using UnityEngine;

public class BasisTestNetworkAvatar : MonoBehaviour
{
    public BasisAvatar avatar;
    public byte MessageIndexTest;
    public byte[] SubmittingData;
    public void Awake()
    {
        avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
        InvokeRepeating(nameof(LoopSend),0,1);
    }
    public void LoopSend()
    {
        avatar.OnNetworkMessageSend?.Invoke(MessageIndexTest, SubmittingData, DarkRift.DeliveryMethod.Unreliable);
    }

    private void OnNetworkMessageReceived(byte MessageIndex, byte[] buffer)
    {
        if(MessageIndexTest == MessageIndex)
        {

        }
    }
}
