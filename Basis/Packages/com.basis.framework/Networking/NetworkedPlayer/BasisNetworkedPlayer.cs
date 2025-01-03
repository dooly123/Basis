using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;
using static SerializableBasis;
namespace Basis.Scripts.Networking.NetworkedPlayer
{
    [System.Serializable]
    public class BasisNetworkedPlayer
    {
        [SerializeField]
        public BasisNetworkSendBase NetworkSend = null;
        public BasisBoneControl MouthBone;
        public BasisPlayer Player;
        [SerializeField]
        public PlayerIdMessage PlayerIDMessage;
        public bool hasID = false;
        public ushort NetId
        {
            get
            {
                if (hasID)
                {
                    return PlayerIDMessage.playerID;
                }
                else
                {
                    BasisDebug.LogError("Missing Network ID!");
                    return 0;
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
        public void InitalizeNetwork()
        {
            NetworkSend.Initialize(this);
            CalibrationComplete();
        }
        public void ProvideNetworkKey(ushort PlayerID)
        {
            PlayerIDMessage.playerID = PlayerID;
            hasID = true;
        }
        public void LocalInitalize(BasisLocalPlayer BasisLocalPlayer)
        {
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
                BasisDebug.LogError("Missing CharacterIKCalibration");
            }
            NetworkSend = new BasisNetworkTransmitter();
            BasisNetworkManagement.Instance.Transmitter = (BasisNetworkTransmitter)NetworkSend;
        }
        public void RemoteInitalization(BasisRemotePlayer RemotePlayer)
        {
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
                BasisDebug.LogError("Missing CharacterIKCalibration");
            }
            if (RemotePlayer.RemoteAvatarDriver != null)
            {
            }
            else
            {
                BasisDebug.LogError("Missing CharacterIKCalibration");
            }
            NetworkSend = new BasisNetworkReceiver();
        }
    }
}
