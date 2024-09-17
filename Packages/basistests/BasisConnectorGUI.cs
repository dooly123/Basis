using Basis.Scripts.Networking;
using UnityEngine;

namespace Basis.Scripts.Tests
{
    public class BasisConnectorGUI : MonoBehaviour
    {
        public BasisNetworkManagement NetworkConnector;
        public string IP = "192.168.1.7";
        public ushort Port = 4296;
        public bool IsServer = false;
        void Start()
        {
            //  NetworkConnector.Host(25565);
            NetworkConnector.Connect(Port, IP, IsServer);
        }
    }
}