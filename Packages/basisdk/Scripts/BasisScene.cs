using DarkRift;
using UnityEngine;
using UnityEngine.Events;

namespace Basis.Scripts.BasisSdk
{
    public class BasisScene : MonoBehaviour
    {
        public Transform SpawnPoint;
        public float RespawnHeight = -100;
        public float RespawnCheckTimer = 0.1f;
        public UnityEngine.Audio.AudioMixerGroup Group;
        public static BasisScene Instance;
        public static UnityEvent<BasisScene> Ready = new UnityEvent<BasisScene>();
        public void Awake()
        {
            Instance = this;
            Ready?.Invoke(this);
        }
        public bool HasSendEvent;
        public SceneNetworkMessageReceiveEvent OnNetworkMessageReceived;
        public SceneNetworkMessageSendEvent OnNetworkMessageSend;
        /// <summary>
        /// this is used for Receiving Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        public delegate void SceneNetworkMessageReceiveEvent(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients = null);


        /// <summary>
        /// this is used for sending Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        /// <param name="DeliveryMethod"></param>

        public delegate void SceneNetworkMessageSendEvent(ushort MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable, ushort[] Recipients = null);
    }
}