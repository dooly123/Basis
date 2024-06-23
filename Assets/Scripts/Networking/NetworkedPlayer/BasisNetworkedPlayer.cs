using UnityEngine;
using static SerializableDarkRift;
[DefaultExecutionOrder(15002)]
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
        ServerSideSyncPlayerMessage Stub = new ServerSideSyncPlayerMessage();
        Stub.playerIdMessage.playerID = PlayerID;
        ReInitialize(player, Stub);
    }
    public void ReInitialize(BasisPlayer player, ServerSideSyncPlayerMessage sspm)
    {
        if (Player != null && Player != player)
        {
            if (player.IsLocal)
            {
                BasisLocalPlayer LocalPlayer = player as BasisLocalPlayer;
                if (LocalPlayer.AvatarDriver != null)
                {
                    LocalPlayer.AvatarDriver.CalibrationComplete.RemoveListener(CalibrationComplete);
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
                    RemotePlayer.RemoteAvatarDriver.CalibrationComplete.RemoveListener(CalibrationComplete);
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
                    LocalPlayer.AvatarDriver.CalibrationComplete.AddListener(CalibrationComplete);
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
                    RemotePlayer.RemoteAvatarDriver.CalibrationComplete.AddListener(CalibrationComplete);
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
            NetworkSend = GetOrCreateNetworkComponent<BasisNetworkReceiver>();
            if (sspm.avatarSerialization.array != null && sspm.avatarSerialization.array.Length != 0)
            {
                NetworkSend.LASM = sspm.avatarSerialization;
            }
        }
        NetworkSend.NetworkNetID.playerID = sspm.playerIdMessage.playerID;
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