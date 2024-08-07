using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts.Tests
{
public class BasisConnectorGUI : MonoBehaviour
{
    public BasisNetworkConnector NetworkConnector;
    public string IP = "192.168.1.7";
    public ushort Port = 4296;
    void Start()
    {
        //  NetworkConnector.Host(25565);
        NetworkConnector.Connect(Port, IP);
    }
}
}