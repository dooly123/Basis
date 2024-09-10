using Basis.Scripts.Avatar;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.NamePlate;
using UnityEngine;
using static SerializableDarkRift;
namespace Basis.Scripts.BasisSdk.Players
{
    public class BasisRemotePlayer : BasisPlayer
    {
        public BasisRemoteBoneDriver RemoteBoneDriver;
        public BasisRemoteAvatarDriver RemoteAvatarDriver;
        public GameObject AudioSourceGameobject;
        public BasisBoneControl MouthControl;
        public bool HasEvents = false;
        public async void RemoteInitialize(ClientAvatarChangeMessage CACM, PlayerMetaDataMessage PlayerMetaDataMessage)
        {
            DisplayName = PlayerMetaDataMessage.playerDisplayName;
            UUID = PlayerMetaDataMessage.playerUUID;
            IsLocal = false;
            RemoteBoneDriver.CreateInitialArrays(RemoteBoneDriver.transform);
            RemoteBoneDriver.Initialize();
            if (HasEvents == false)
            {
                RemoteAvatarDriver.CalibrationComplete += RemoteCalibration;
                HasEvents = true;
            }
            if (Avatar == null)
            {
                CreateAvatar(CACM.avatarID, CACM.loadMode);
            }
            RemoteBoneDriver.FindBone(out MouthControl, BasisBoneTrackedRole.Mouth);
            await BasisRemoteNamePlate.LoadRemoteNamePlate(this);
        }
        public void OnDestroy()
        {
            if (HasEvents)
            {
                if (RemoteAvatarDriver != null)
                {
                    RemoteAvatarDriver.CalibrationComplete -= RemoteCalibration;
                    HasEvents = false;
                }
            }
        }
        public void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            AudioSourceGameobject.transform.SetPositionAndRotation(position, rotation);
        }
        public bool LockAvatarFromChanging;
        public async void CreateAvatar(string Loader = BasisAvatarFactory.LoadingAvatar,byte NetworkMode = 0)
        {
            if (string.IsNullOrEmpty(Loader))
            {
                Debug.Log("Avatar Load string was null or empty using fallback!");
                await BasisAvatarFactory.LoadAvatar(this, BasisAvatarFactory.LoadingAvatar, BasisPlayer.LoadModeLocal, string.Empty);
            }
            else
            {
                Debug.Log("loading avatar from " + Loader + " with net mode " + NetworkMode);
                if (LockAvatarFromChanging == false)
                {
                    await BasisAvatarFactory.LoadAvatar(this, Loader, NetworkMode, string.Empty);
                }
            }
        }
        public void RemoteCalibration()
        {
            RemoteBoneDriver.OnCalibration(this);
        }
    }
}