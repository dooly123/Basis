using Basis.Scripts.BasisSdk;
using System;
using UnityEngine;

public class BasisTestNetworkScene : MonoBehaviour
{
    public BasisScene scene;
    public byte[] SendingData;
    public void Awake()
    {
        scene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        scene.OnNetworkMessageSend.Invoke(1, SendingData, DarkRift.DeliveryMethod.ReliableOrdered);
    }

    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer)
    {

    }
}
