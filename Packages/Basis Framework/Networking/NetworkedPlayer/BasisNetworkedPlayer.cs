using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.TransformBinders.BoneControl;
using System;
using UnityEngine;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.NetworkedPlayer
{
    public partial class BasisNetworkedPlayer : MonoBehaviour
    {
        public BasisNetworkSendBase NetworkSend;
        public BasisBoneControl MouthBone;
        public BasisPlayer Player;
        public ushort NetId => NetworkSend.NetworkNetID.playerID;
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
        public void ReInitialize(BasisPlayer player, ushort PlayerID)
        {
            LocalAvatarSyncMessage Stub = new LocalAvatarSyncMessage
            {
                array = new byte[212]
            };
            ReInitialize(player, PlayerID, Stub);
        }
        public void ReInitialize(BasisPlayer player, ushort PlayerID, LocalAvatarSyncMessage sspm)
        {
            if (Player != null && Player != player)
            {
                if (player.IsLocal)
                {
                    BasisLocalPlayer LocalPlayer = player as BasisLocalPlayer;
                    if (LocalPlayer.AvatarDriver != null)
                    {
                        if (LocalPlayer.AvatarDriver.HasEvents == false)
                        {
                            LocalPlayer.AvatarDriver.CalibrationComplete += CalibrationComplete;
                            LocalPlayer.AvatarDriver.HasEvents = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Missing CharacterIKCalibration");
                    }
                }
                else
                {
                    BasisRemotePlayer RemotePlayer = player as BasisRemotePlayer;
                    if (RemotePlayer.RemoteAvatarDriver != null)
                    {
                        if (RemotePlayer.RemoteAvatarDriver.HasEvents == false)
                        {
                            RemotePlayer.RemoteAvatarDriver.CalibrationComplete += CalibrationComplete;
                            RemotePlayer.RemoteAvatarDriver.HasEvents = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Missing CharacterIKCalibration");
                    }

                }
            }
            if (Player != player && player != null)
            {
                Player = player;
                if (player.IsLocal)
                {
                    BasisLocalPlayer LocalPlayer = player as BasisLocalPlayer;
                    if (LocalPlayer.AvatarDriver != null)
                    {
                        if (LocalPlayer.AvatarDriver.HasEvents == false)
                        {
                            LocalPlayer.AvatarDriver.CalibrationComplete += CalibrationComplete;
                            LocalPlayer.AvatarDriver.HasEvents = true;
                        }
                        LocalPlayer.LocalBoneDriver.FindBone(out MouthBone, BasisBoneTrackedRole.Mouth);
                    }
                    else
                    {
                        Debug.LogError("Missing CharacterIKCalibration");
                    }
                }
                else
                {
                    BasisRemotePlayer RemotePlayer = player as BasisRemotePlayer;
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
                }
                this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            if (player.IsLocal)
            {
                NetworkSend = GetOrCreateNetworkComponent<BasisNetworkTransmitter>();
            }
            else
            {
                BasisNetworkReceiver BasisNetworkReceiver = GetOrCreateNetworkComponent<BasisNetworkReceiver>();
                NetworkSend = BasisNetworkReceiver;
                // if (sspm.array != null && sspm.array.Length != 0)
                //  {
                //  BasisNetworkReceiver.ReceiveNetworkAvatarData = sspm;
                //  }
            }
            NetworkSend.NetworkNetID.playerID = PlayerID;
            NetworkSend.Initialize(this);
            CalibrationComplete();
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