using Basis.Scripts.Avatar;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.NamePlate;
using System.Threading.Tasks;
using UnityEngine;
using static SerializableBasis;

namespace Basis.Scripts.BasisSdk.Players
{
    public class BasisRemotePlayer : BasisPlayer
    {
        public BasisRemoteBoneDriver RemoteBoneDriver;
        public BasisRemoteAvatarDriver RemoteAvatarDriver;
        public Transform AudioSourceTransform;
        public BasisBoneControl MouthControl;
        public bool HasEvents = false;
        public bool LockAvatarFromChanging;
        public bool IsNotFallBack = false;
        public bool OutOfRangeFromLocal = false;
        public ClientAvatarChangeMessage CACM;
        public async Task RemoteInitialize(ClientAvatarChangeMessage cACM, PlayerMetaDataMessage PlayerMetaDataMessage)
        {
            CACM = cACM;
            DisplayName = PlayerMetaDataMessage.playerDisplayName;
            UUID = PlayerMetaDataMessage.playerUUID;
            IsLocal = false;
            RemoteBoneDriver.CreateInitialArrays(RemoteBoneDriver.transform, false);
            RemoteBoneDriver.Initialize();
            if (HasEvents == false)
            {
                RemoteAvatarDriver.CalibrationComplete += RemoteCalibration;
                HasEvents = true;
            }
            RemoteBoneDriver.FindBone(out MouthControl, BasisBoneTrackedRole.Mouth);
            AudioSourceTransform.parent = MouthControl.BoneTransform;
            await BasisRemoteNamePlate.LoadRemoteNamePlate(this);
        }
        public async Task LoadAvatarFromInital(ClientAvatarChangeMessage CACM)
        {
            if (BasisAvatar == null)
            {
                this.CACM = CACM;
              //  if (IsNotFallBack)
               // {
                    BasisLoadableBundle BasisLoadedBundle = BasisBundleConversionNetwork.ConvertNetworkBytesToBasisLoadableBundle(CACM.byteArray);
                    await BasisAvatarFactory.LoadAvatarRemote(this, CACM.loadMode, BasisLoadedBundle);
               // }
              //  else
              //  {
               //     BasisAvatarFactory.LoadLoadingAvatar(this, BasisAvatarFactory.LoadingAvatar.BasisLocalEncryptedBundle.LocalBundleFile);
               // }
            }
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
        public async void CreateAvatar(byte Mode, BasisLoadableBundle BasisLoadableBundle)
        {
          //  if (IsNotFallBack)
          //  {
                if (BasisLoadableBundle.BasisLocalEncryptedBundle.LocalBundleFile == BasisAvatarFactory.LoadingAvatar.BasisLocalEncryptedBundle.LocalBundleFile)
                {
                    BasisDebug.Log("Avatar Load string was null or empty using fallback!");
                    await BasisAvatarFactory.LoadAvatarRemote(this, BasisPlayer.LoadModeError, BasisLoadableBundle);
                }
                else
                {
                    BasisDebug.Log("loading avatar from " + BasisLoadableBundle.BasisLocalEncryptedBundle.LocalBundleFile + " with net mode " + Mode);
                    if (LockAvatarFromChanging == false)
                    {
                        await BasisAvatarFactory.LoadAvatarRemote(this, Mode, BasisLoadableBundle);
                    }
                }
          //  }
          //  else
           // {
           //     BasisAvatarFactory.LoadLoadingAvatar(this, BasisAvatarFactory.LoadingAvatar.BasisLocalEncryptedBundle.LocalBundleFile);
           // }
        }
        public void RemoteCalibration()
        {
            RemoteBoneDriver.OnCalibration(this);
        }
    }
}
