using Basis.Scripts.Avatar;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.NamePlate;
using BasisSerializer.OdinSerializer;
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
                AvatarNetworkLoadInformation ALI = SerializationUtility.DeserializeValue<AvatarNetworkLoadInformation>(CACM.byteArray, DataFormat.Binary);
                BasisLoadableBundle BasisLoadedBundle = new BasisLoadableBundle
                {
                    BasisRemoteBundleEncypted = new BasisRemoteEncyptedBundle() { BundleURL = ALI.AvatarBundleUrl, MetaURL = ALI.AvatarMetaUrl },
                    UnlockPassword = ALI.UnlockPassword,
                    BasisBundleInformation = new BasisBundleInformation(),//self assigned internally
                     BasisStoredEncyptedBundle = new BasisStoredEncyptedBundle(),//self assigned internally
                    LoadedAssetBundle = null,
                };


                BasisLoadedBundle =   await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadedBundle, AvatarProgress, new System.Threading.CancellationToken());
             //here  CreateAvatar(ALI.AvatarBundleUrl, CACM.loadMode, BasisLoadedBundle);
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
        public async void CreateAvatar(byte Mode, BasisLoadableBundle BasisLoadableBundle, string Loader = BasisAvatarFactory.LoadingAvatar)
        {
            if (string.IsNullOrEmpty(Loader))
            {
                Debug.Log("Avatar Load string was null or empty using fallback!");
              //here  await BasisAvatarFactory.LoadAvatarRemote(this, BasisAvatarFactory.LoadingAvatar, BasisPlayer.LoadModeError, BasisAvatarFactory.LoadingAvatar, "N/A", BasisLoadableBundle);
            }
            else
            {
                Debug.Log("loading avatar from " + Loader + " with net mode " + Mode);
                if (LockAvatarFromChanging == false)
                {
                   //here await BasisAvatarFactory.LoadAvatarRemote(this, Loader, Mode, BasisLoadableBundle);
                }
            }
        }
        public void RemoteCalibration()
        {
            RemoteBoneDriver.OnCalibration(this);
        }
    }
}