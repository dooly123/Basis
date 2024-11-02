using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.Transmitters
{
    [DefaultExecutionOrder(15001)]
    public partial class BasisNetworkTransmitter : BasisNetworkSendBase
    {
        public float timer = 0f;
        public float interval = 0.05f;
        [SerializeField]
        public BasisAudioTransmission AudioTransmission = new BasisAudioTransmission();
        public override void Compute()
        {
            if (Ready)
            {
                if (NetworkedPlayer.Player.Avatar != null)
                {
                    BasisNetworkAvatarCompressor.Compress(this, NetworkedPlayer.Player.Avatar.Animator);
                }
            }
        }
        void OnRenderer()
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                Compute();
                timer = 0f;
            }
        }
        public void OnDestroy()
        {
            DeInitialize();
        }
        public bool HasEvents = false;
        public override void Initialize(BasisNetworkedPlayer networkedPlayer)
        {
            if (Ready == false)
            {
                InitalizeDataJobs();
                InitalizeAvatarStoredData(ref Target);
                InitalizeAvatarStoredData(ref Output);
                NetworkedPlayer = networkedPlayer;
                AudioTransmission.OnEnable(networkedPlayer);
                OnAvatarCalibration();
                if (HasEvents == false)
                {
                    NetworkedPlayer.Player.OnAvatarSwitchedFallBack += OnAvatarCalibration;
                    NetworkedPlayer.Player.OnAvatarSwitched += OnAvatarCalibration;
                    NetworkedPlayer.Player.OnAvatarSwitched += SendOutLatestAvatar;
                    if (NetworkedPlayer.Player.IsLocal)
                    {
                        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead.AddAction(102, OnRenderer);
                    }
                    HasEvents = true;
                }
                Ready = true;
            }
            else
            {
                Debug.Log("Already Ready");
            }
        }
        public override void DeInitialize()
        {
            if (Ready)
            {
                AudioTransmission.OnDisable();
            }
            if (HasEvents)
            {
                NetworkedPlayer.Player.OnAvatarSwitchedFallBack -= OnAvatarCalibration;
                NetworkedPlayer.Player.OnAvatarSwitched -= OnAvatarCalibration;
                NetworkedPlayer.Player.OnAvatarSwitched -= SendOutLatestAvatar;
                if (NetworkedPlayer.Player.IsLocal)
                {
                    BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead.RemoveAction(102, OnRenderer);
                }
                HasEvents = false;
            }
        }
        public void SendOutLatestAvatar()
        {

            byte[] LAI = BasisBundleConversionNetwork.ConvertBasisLoadableBundleToBytes(NetworkedPlayer.Player.AvatarMetaData);
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                ClientAvatarChangeMessage ClientAvatarChangeMessage = new ClientAvatarChangeMessage
                {
                     byteArray = LAI,
                    loadMode = NetworkedPlayer.Player.AvatarLoadMode,
                };
                writer.Write(ClientAvatarChangeMessage);
                using (var msg = Message.Create(BasisTags.AvatarChangeMessage, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.EventsChannel, DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}