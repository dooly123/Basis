using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Basis.Scripts.Avatar
{
    public static class BasisAvatarFactory
    {
        public const string LoadingAvatar = "LoadingAvatar";
        public static async Task LoadAvatarLocal(BasisLocalPlayer Player, string AvatarAddress, byte Mode,string AvatarMetaUrl,string UnlockPassword, BasisBundleInformation BasisBundleInformation)
        {
            if (string.IsNullOrEmpty(AvatarAddress))
            {
                Debug.LogError("Avatar Address was empty or null! Falling back to loading avatar.");
                await LoadAvatarAfterError(Player);
                return;
            }

            DeleteLastAvatar(Player);
            LoadLoadingAvatar(Player, LoadingAvatar);

            try
            {
                GameObject Output = null;
                switch (Mode)
                {
                    case 0://download
                        Debug.Log("Requested Avatar was a AssetBundle Avatar " + AvatarAddress);
                        Output = await DownloadAndLoadAvatar(AvatarAddress, BasisBundleInformation, Player);
                        break;
                    case 1://localload
                           //Player.transform.position, Quaternion.identity
                        break;
                    default:
                        Debug.Log("Using Default, this means index was out of acceptable range! " + AvatarAddress);
                        Output = await DownloadAndLoadAvatar(AvatarAddress, BasisBundleInformation, Player);
                        break;
                }
                Player.AvatarNetworkLoadInformation = new BasisPlayer.AvatarNetworkLoadInformation() {  AvatarBundleUrl = AvatarAddress, AvatarMetaUrl = AvatarMetaUrl, UnlockPassword = UnlockPassword };
                Player.AvatarLoadMode = Mode;

                InitializePlayerAvatar(Player, Output);
                BasisHeightDriver.SetPlayersEyeHeight(Player);
                Player.AvatarSwitched();
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading avatar failed: {e}");
                await LoadAvatarAfterError(Player);
            }
        }

        public static async Task LoadAvatarRemote(BasisRemotePlayer Player,byte Mode, BasisLoadableBundle BasisLoadableBundle)
        {
            if (string.IsNullOrEmpty(BasisLoadableBundle.BasisRemoteBundleEncypted.BundleURL))
            {
                Debug.LogError("Avatar Address was empty or null! Falling back to loading avatar.");
                await LoadAvatarAfterError(Player);
                return;
            }

            DeleteLastAvatar(Player);
            LoadLoadingAvatar(Player, LoadingAvatar);

            try
            {
                GameObject Output = null;
                switch (Mode)
                {
                    case 0://download
                        Output = await DownloadAndLoadAvatar(AvatarAddress, BasisBundleInformation, Player);
                        break;
                    case 1://localload
                           //Player.transform.position, Quaternion.identity
                           break;
                    default:
                        Output = await DownloadAndLoadAvatar(AvatarAddress, BasisBundleInformation, Player);
                        break;
                }
                Player.AvatarNetworkLoadInformation = new BasisPlayer.AvatarNetworkLoadInformation() { AvatarBundleUrl = AvatarAddress, AvatarMetaUrl = AvatarMetaUrl, UnlockPassword = UnlockPassword };
                Player.AvatarLoadMode = Mode;

               InitializePlayerAvatar(Player, Output);
                Player.AvatarSwitched();
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading avatar failed: {e}");
                await LoadAvatarAfterError(Player);
            }
        }

        private static async Task<GameObject> DownloadAndLoadAvatar(string AvatarAddress, BasisBundleInformation hash, BasisPlayer Player)
        {
            return await BasisGameObjectAssetBundleManager.DownloadAndLoadGameObjectAsync(AvatarAddress, hash, FileName, BasisStorageManagement.AssetSubDirectory, Player.transform.position,Quaternion.identity, Player.ProgressReportAvatarLoad);
        }

        private static void InitializePlayerAvatar(BasisPlayer Player, GameObject Output)
        {
            if (Output.TryGetComponent(out BasisAvatar Avatar))
            {
                DeleteLastAvatar(Player);
                Player.Avatar = Avatar;
                Player.Avatar.Renders = Player.Avatar.GetComponentsInChildren<Renderer>(true);
                if (Player is BasisLocalPlayer localPlayer)
                {
                    Player.Avatar.IsOwnedLocally = true;
                    CreateLocal(localPlayer);
                    localPlayer.InitalizeIKCalibration(localPlayer.AvatarDriver);
                    Avatar.OnAvatarReady?.Invoke(true);
                    for (int Index = 0; Index < Avatar.Renders.Length; Index++)
                    {
                        Avatar.Renders[Index].gameObject.layer = 6;
                    }
                }
                else if (Player is BasisRemotePlayer remotePlayer)
                {
                    Player.Avatar.IsOwnedLocally = false;
                    CreateRemote(remotePlayer);
                    remotePlayer.InitalizeIKCalibration(remotePlayer.RemoteAvatarDriver);
                    Avatar.OnAvatarReady?.Invoke(false);
                    for (int Index = 0; Index < Avatar.Renders.Length; Index++)
                    {
                        Avatar.Renders[Index].gameObject.layer = 7;
                    }
                }
            }
        }

        public static async Task LoadAvatarAfterError(BasisPlayer Player)
        {
            try
            {
                var Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Quaternion.identity, null);
                (List<GameObject> GameObjects, AddressableGenericResource resource) = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadingAvatar, Para);

                if (GameObjects.Count > 0)
                {
                 InitializePlayerAvatar(Player, GameObjects[0]);
                }
                Player.AvatarNetworkLoadInformation = new BasisPlayer.AvatarNetworkLoadInformation() { AvatarBundleUrl = LoadingAvatar, AvatarMetaUrl = LoadingAvatar, UnlockPassword = "N/A" };
                Player.AvatarLoadMode = 1;
                Player.AvatarSwitched();

                //we want to use Avatar Switched instead of the fallback version to let the server know this is what we actually want to use.
            }
            catch (Exception e)
            {
                Debug.LogError($"Fallback avatar loading failed: {e}");
            }
        }
        /// <summary>
        /// no content searching is done here since its local content.
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="LoadingAvatarToUse"></param>
        public static void LoadLoadingAvatar(BasisPlayer Player, string LoadingAvatarToUse)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(LoadingAvatarToUse);
            var LoadingAvatar = op.WaitForCompletion();

            var InSceneLoadingAvatar = GameObject.Instantiate(LoadingAvatar, Player.transform.position, Quaternion.identity);


            if (InSceneLoadingAvatar.TryGetComponent(out BasisAvatar Avatar))
            {
                Player.Avatar = Avatar;
                Player.Avatar.Renders = Player.Avatar.GetComponentsInChildren<Renderer>(true);
                if (Player.IsLocal)
                {
                    BasisLocalPlayer BasisLocalPlayer =(BasisLocalPlayer)Player;
                    Player.Avatar.IsOwnedLocally = true;
                    CreateLocal(BasisLocalPlayer);
                    Player.InitalizeIKCalibration(BasisLocalPlayer.AvatarDriver);
                    for (int Index = 0; Index < Player.Avatar.Renders.Length; Index++)
                    {
                        Avatar.Renders[Index].gameObject.layer = 6;
                    }
                }
                else
                {
                    BasisRemotePlayer BasisRemotePlayer =(BasisRemotePlayer)Player;
                    Player.Avatar.IsOwnedLocally = false;
                    CreateRemote(BasisRemotePlayer);
                    Player.InitalizeIKCalibration(BasisRemotePlayer.RemoteAvatarDriver);
                    for (int Index = 0; Index < Player.Avatar.Renders.Length; Index++)
                    {
                        Avatar.Renders[Index].gameObject.layer = 7;
                    }
                }
            }
        }

        public static void DeleteLastAvatar(BasisPlayer Player)
        {
            if (Player.Avatar != null)
            {
                GameObject.Destroy(Player.Avatar.gameObject);
            }

            if (Player.AvatarAddressableGenericResource != null)
            {
                AddressableLoadFactory.ReleaseResource(Player.AvatarAddressableGenericResource);
            }
        }

        public static void CreateRemote(BasisRemotePlayer Player)
        {
            if (Player == null || Player.Avatar == null)
            {
                Debug.LogError("Missing RemotePlayer or Avatar");
                return;
            }

            Player.RemoteAvatarDriver.RemoteCalibration(Player);
        }

        public static void CreateLocal(BasisLocalPlayer Player)
        {
            if (Player == null || Player.Avatar == null)
            {
                Debug.LogError("Missing LocalPlayer or Avatar");
                return;
            }

            Player.AvatarDriver.InitialLocalCalibration(Player);
        }
    }
}