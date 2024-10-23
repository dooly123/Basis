using Basis.Scripts.Avatar;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.NamePlate;
using BasisSerializer.OdinSerializer;
using System.Threading;
using System.Threading.Tasks;
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
        public bool LockAvatarFromChanging;
        public async Task RemoteInitialize(ClientAvatarChangeMessage CACM, PlayerMetaDataMessage PlayerMetaDataMessage)
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
                BasisLoadableBundle BasisLoadedBundle = BasisBundleConversionNetwork.ConvertNetworkBytesToBasisLoadableBundle(CACM.byteArray);
                await BasisAvatarFactory.LoadAvatarRemote(this, CACM.loadMode, BasisLoadedBundle);
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
        public async void CreateAvatar(byte Mode, BasisLoadableBundle BasisLoadableBundle)
        {
            if (BasisLoadableBundle.BasisStoredEncryptedBundle.LocalBundleFile == BasisAvatarFactory.LoadingAvatar.BasisStoredEncryptedBundle.LocalBundleFile)
            {
                Debug.Log("Avatar Load string was null or empty using fallback!");
                await BasisAvatarFactory.LoadAvatarRemote(this, BasisPlayer.LoadModeError, BasisLoadableBundle);
            }
            else
            {
                Debug.Log("loading avatar from " + BasisLoadableBundle.BasisStoredEncryptedBundle.LocalBundleFile + " with net mode " + Mode);
                if (LockAvatarFromChanging == false)
                {
                    await BasisAvatarFactory.LoadAvatarRemote(this, Mode, BasisLoadableBundle);
                }
            }
        }
        public void RemoteCalibration()
        {
            RemoteBoneDriver.OnCalibration(this);
        }
    }
}