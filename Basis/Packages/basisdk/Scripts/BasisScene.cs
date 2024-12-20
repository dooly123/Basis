using LiteNetLib;
using UnityEngine;
using UnityEngine.Events;

namespace Basis.Scripts.BasisSdk
{
    public class BasisScene : BasisContentBase
    {
        public Transform SpawnPoint;
        public float RespawnHeight = -100;
        public float RespawnCheckTimer = 0.1f;
        public UnityEngine.Audio.AudioMixerGroup Group;
        public static BasisScene Instance;
        public static UnityEvent<BasisScene> Ready = new UnityEvent<BasisScene>();
        public Camera MainCamera;
        public void Awake()
        {
            Instance = this;
            Ready?.Invoke(this);
        }
        public static SceneNetworkMessageReceiveEvent OnNetworkMessageReceived;
        public static SceneNetworkMessageSendEvent OnNetworkMessageSend;
        /// <summary>
        /// this is used for sending Network Messages
        /// very much a data sync that can be used more like a traditional sync method
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        /// <param name="DeliveryMethod"></param>
        /// <param name="Recipients">if null everyone but self, you can include yourself to make it loop back over the network</param>
        public static void NetworkMessageSend(ushort MessageIndex, byte[] buffer = null, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable, ushort[] Recipients = null)
        {
            OnNetworkMessageSend?.Invoke(MessageIndex, buffer, DeliveryMethod, Recipients);
        }
        /// <summary>
        /// this is used for sending Network Messages,
        /// more like a RPC then a data sync
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="DeliveryMethod"></param>
        /// <param name="Recipients">if null everyone but self, you can include yourself to make it loop back over the network</param>
        public static void NetworkMessageSend(ushort MessageIndex, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable)
        {
            OnNetworkMessageSend?.Invoke(MessageIndex, null, DeliveryMethod);
        }
        /// <summary>
        /// this is used for Receiving Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        public delegate void SceneNetworkMessageReceiveEvent(ushort PlayerID, ushort MessageIndex, byte[] buffer, LiteNetLib.DeliveryMethod deliveryMethod);


        /// <summary>
        /// this is used for sending Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        /// <param name="DeliveryMethod"></param>

        public delegate void SceneNetworkMessageSendEvent(ushort MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable, ushort[] Recipients = null);
    }
}
