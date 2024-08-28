using DarkRift;
using System.Collections.Generic;
using UnityEngine;

namespace Basis.Scripts.BasisSdk
{
    public class BasisAvatar : BasisContentBase
    {
        public Animator Animator;
        public SkinnedMeshRenderer FaceVisemeMesh;
        public SkinnedMeshRenderer FaceBlinkMesh;
        public Vector2 AvatarEyePosition;
        public Vector2 AvatarMouthPosition;
        public int[] FaceVisemeMovement = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        public int[] BlinkViseme = new int[] { -1 };
        public int laughterBlendTarget = -1;
        [SerializeField]
        public List<Renderer> Renders = new List<Renderer>();
        [SerializeField]
        public List<BasisJiggleStrain> JiggleStrains = new List<BasisJiggleStrain>();

        public bool HasSendEvent;
        public AvatarNetworkMessageReceiveEvent OnNetworkMessageReceived;
        public AvatarNetworkMessageSendEvent OnNetworkMessageSend;

        /// <summary>
        /// this is used for Receiving Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        public delegate void AvatarNetworkMessageReceiveEvent(byte MessageIndex, byte[] buffer);


        /// <summary>
        /// this is used for sending Network Messages
        /// </summary>
        /// <param name="MessageIndex"></param>
        /// <param name="buffer"></param>
        /// <param name="DeliveryMethod"></param>

        public delegate void AvatarNetworkMessageSendEvent(byte MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable);
    }
}