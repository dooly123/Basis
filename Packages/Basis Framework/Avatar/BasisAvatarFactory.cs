using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Addressable_Driver.Loading;
using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Basis.Scripts.Avatar
{
    public static class BasisAvatarFactory
    {
        public const string LoadingAvatar = "LoadingAvatar";
        public const string AssetSubDirectory = "Avatars";
        public static async Task LoadAvatar(BasisLocalPlayer Player, string AvatarAddress)
        {
            DeleteLastAvatar(Player);
            UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Quaternion.identity, null);
            if (string.IsNullOrEmpty(AvatarAddress))
            {
                Debug.LogError("Avatar Address was empty or null! falling back");

                await LoadAvatarAfterError(Player, AvatarAddress, Para);
                return;
            }
            LoadLoadingAvatar(Player, LoadingAvatar);
            Player.AvatarSwitchedFallBack();
            try
            {
                AddressableManagement.Instance.UnloadAssetBundle(AvatarAddress);
                GameObject Output = await GameObjectAssetBundleManager.DownloadAndLoadGameObjectAsync(AvatarAddress, GetFileNameFromUrl(AvatarAddress), AssetSubDirectory);
                Player.AvatarUrl = AvatarAddress;
                if (Output.TryGetComponent(out BasisAvatar Avatar))
                {
                    DeleteLastAvatar(Player);
                    Player.Avatar = Avatar;
                    CreateLocal(Player);
                    Player.InitalizeIKCalibration(Player.AvatarDriver);
                }
                Player.SetPlayersEyeHeight(Player.PlayerEyeHeight, Player.AvatarDriver.ActiveEyeHeight);
                Player.AvatarSwitched();
            }
            catch (Exception E)
            {
                Debug.LogError("loading avatar failed " + E);
                await LoadAvatarAfterError(Player, AvatarAddress, Para);
            }
        }
        public static async Task LoadAvatarAfterError(BasisLocalPlayer Player, string AvatarAddress, UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters Para)
        {

            (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadingAvatar, Para);
            List<GameObject> Gameobjects = data.Item1;
            Player.AvatarUrl = AvatarAddress;
            if (Gameobjects.Count != 0)
            {
                foreach (GameObject gameObject in Gameobjects)
                {
                    if (gameObject.TryGetComponent(out BasisAvatar Avatar))
                    {
                        DeleteLastAvatar(Player);
                        Player.Avatar = Avatar;
                        CreateLocal(Player);
                        Player.InitalizeIKCalibration(Player.AvatarDriver);
                    }
                }
            }
            Player.SetPlayersEyeHeight(Player.PlayerEyeHeight, Player.AvatarDriver.ActiveEyeHeight);
            Player.AvatarSwitched();
        }
        public static async Task LoadAvatar(BasisRemotePlayer Player, string AvatarAddress)
        {
            DeleteLastAvatar(Player);
            LoadLoadingAvatar(Player, LoadingAvatar);
            Player.AvatarSwitchedFallBack();
            Player.AvatarUrl = AvatarAddress;
            AddressableManagement.Instance.UnloadAssetBundle(AvatarAddress);
            GameObject Output = await GameObjectAssetBundleManager.DownloadAndLoadGameObjectAsync(AvatarAddress, GetFileNameFromUrl(AvatarAddress), AssetSubDirectory);
            if (Output.TryGetComponent(out BasisAvatar Avatar))
            {
                DeleteLastAvatar(Player);
                Player.Avatar = Avatar;
                CreateRemote(Player);
                Player.InitalizeIKCalibration(Player.RemoteAvatarDriver);
            }
            Player.AvatarSwitched();
        }
        private static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath); // Get the file name with extension
            return Path.GetFileNameWithoutExtension(fileName); // Remove the extension and return
        }
        public static void LoadLoadingAvatar(BasisPlayer Player, string LoadingAvatarToUse)
        {
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(LoadingAvatarToUse);
            GameObject LoadingAvatar = op.WaitForCompletion();
            GameObject InSceneLoadingAvatar = GameObject.Instantiate(LoadingAvatar, Player.transform.position, Quaternion.identity);
            if (InSceneLoadingAvatar.TryGetComponent(out BasisAvatar Avatar))
            {
                Player.Avatar = Avatar;
                if (Player.IsLocal)
                {
                    BasisLocalPlayer BasisLocalPlayer = (BasisLocalPlayer)Player;
                    CreateLocal(BasisLocalPlayer);
                    Player.InitalizeIKCalibration(BasisLocalPlayer.AvatarDriver);
                }
                else
                {
                    BasisRemotePlayer BasisRemotePlayer = (BasisRemotePlayer)Player;
                    CreateRemote(BasisRemotePlayer);
                    Player.InitalizeIKCalibration(BasisRemotePlayer.RemoteAvatarDriver);
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
            if (Player == null)
            {
                Debug.LogError("Missing RemotePlayer");
                return;
            }
            if (Player.Avatar == null)
            {
                Debug.LogError("Missing Avatar");
                return;
            }
            Player.RemoteAvatarDriver.RemoteCalibration(Player);
        }
        public static void CreateLocal(BasisLocalPlayer Player)
        {
            if (Player == null)
            {
                Debug.LogError("Missing LocalPlayer");
                return;
            }
            if (Player.Avatar == null)
            {
                Debug.LogError("Missing Avatar");
                return;
            }
            Player.AvatarDriver.InitialLocalCalibration(Player);
        }
    }
}