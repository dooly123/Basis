using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;
using static SerializableBasis;
namespace Basis.Scripts.Networking.NetworkedPlayer
{
    public partial class BasisNetworkedPlayer : MonoBehaviour
    {
        public BasisNetworkSendBase NetworkSend;
        public BasisBoneControl MouthBone;
        public BasisPlayer Player;
        [SerializeField]
        public PlayerIdMessage NetworkNetID = new PlayerIdMessage();
        public ushort NetId => NetworkNetID.playerID;
        public void OnDestroy()
        {
            if (Player != null)
            {
                Destroy(Player.gameObject);

                if (Player.Avatar != null)
                {
                    Destroy(Player.Avatar.gameObject);
                }
            }
        }
        /// <summary>
        /// only use this method if NetworkSend.NetId is assigned
        /// and only use this method if Player is assigned
        /// </summary>
        public void CalibrationComplete()
        {
            if (NetworkSend != null)
            {
                NetworkSend.OnAvatarCalibration();
            }
        }
        public void RemoteInitalization(BasisRemotePlayer RemotePlayer)
        {
            this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Player = RemotePlayer;
            if (RemotePlayer.RemoteAvatarDriver != null)
            {
                if (RemotePlayer.RemoteAvatarDriver.HasEvents == false)
                {
                    RemotePlayer.RemoteAvatarDriver.CalibrationComplete += CalibrationComplete;
                    RemotePlayer.RemoteAvatarDriver.HasEvents = true;
                }
                RemotePlayer.RemoteBoneDriver.FindBone(out MouthBone, BasisBoneTrackedRole.Mouth);
            }
            else
            {
                Debug.LogError("Missing CharacterIKCalibration");
            }
            if (RemotePlayer.RemoteAvatarDriver != null)
            {
            }
            else
            {
                Debug.LogError("Missing CharacterIKCalibration");
            }
            NetworkSend = GetOrCreateNetworkComponent<BasisNetworkReceiver>();
        }
        public void InitalizeNetwork()
        {
            NetworkSend.Initialize(this);
            CalibrationComplete();
        }
        public void ProvideNetworkKey(ushort PlayerID)
        {
            NetworkNetID.playerID = PlayerID;
        }
        public void LocalInitalize(BasisLocalPlayer BasisLocalPlayer)
        {
            this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Player = BasisLocalPlayer;
            if (BasisLocalPlayer.AvatarDriver != null)
            {
                if (BasisLocalPlayer.AvatarDriver.HasEvents == false)
                {
                    BasisLocalPlayer.AvatarDriver.CalibrationComplete += CalibrationComplete;
                    BasisLocalPlayer.AvatarDriver.HasEvents = true;
                }
                BasisLocalPlayer.LocalBoneDriver.FindBone(out MouthBone, BasisBoneTrackedRole.Mouth);
            }
            else
            {
                Debug.LogError("Missing CharacterIKCalibration");
            }
            NetworkSend = GetOrCreateNetworkComponent<BasisNetworkTransmitter>();
        }
        private T GetOrCreateNetworkComponent<T>() where T : BasisNetworkSendBase
        {
            if (NetworkSend != null && NetworkSend.GetType() == typeof(T))
                return NetworkSend as T;

            if (NetworkSend != null)
            {
                NetworkSend.DeInitialize();
                Destroy(NetworkSend.gameObject);
            }

            NetworkSend = gameObject.AddComponent<T>();
            return NetworkSend as T;
        }
    }
}