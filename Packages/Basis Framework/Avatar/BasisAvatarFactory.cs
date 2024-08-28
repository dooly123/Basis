using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Addressable_Driver.Loading;
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
        public const string AssetSubDirectory = "Avatars";
        public const string HashExtension = ".hash";

        public static async Task LoadAvatar(BasisLocalPlayer Player, string AvatarAddress, string hash = "")
        {
            if (string.IsNullOrEmpty(AvatarAddress))
            {
                Debug.LogError("Avatar Address was empty or null! Falling back to loading avatar.");
                await LoadAvatarAfterError(Player, AvatarAddress);
                return;
            }

            hash = await GetHashOrFallback(hash, AvatarAddress);

            DeleteLastAvatar(Player);
            LoadLoadingAvatar(Player, LoadingAvatar);

            try
            {
                GameObject Output = await DownloadAndLoadAvatar(AvatarAddress, hash, Player);
                if (Output != null)
                {
                    InitializePlayerAvatar(Player, Output);
                    Player.SetPlayersEyeHeight(Player.PlayerEyeHeight, Player.AvatarDriver.ActiveEyeHeight);
                    Player.AvatarSwitched();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading avatar failed: {e}");
                await LoadAvatarAfterError(Player, AvatarAddress);
            }
        }

        public static async Task LoadAvatar(BasisRemotePlayer Player, string AvatarAddress, string hash = "")
        {
            if (string.IsNullOrEmpty(AvatarAddress))
            {
                Debug.LogError("Avatar Address was empty or null! Falling back to loading avatar.");
                await LoadAvatarAfterError(Player, AvatarAddress);
                return;
            }

            hash = await GetHashOrFallback(hash, AvatarAddress);

            DeleteLastAvatar(Player);
            LoadLoadingAvatar(Player, LoadingAvatar);

            try
            {
                GameObject Output = await DownloadAndLoadAvatar(AvatarAddress, hash, Player);
                if (Output != null)
                {
                    InitializePlayerAvatar(Player, Output);
                    Player.AvatarSwitched();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading avatar failed: {e}");
                await LoadAvatarAfterError(Player, AvatarAddress);
            }
        }

        private static async Task<string> GetHashOrFallback(string hash, string AvatarAddress)
        {
            if (string.IsNullOrEmpty(hash))
            {
                hash = AddressableManagement.ChangeExtension(AvatarAddress, HashExtension);
                return await AddressableManagement.LoadTextFromURLAsync(hash);
            }
            else if (hash.Contains("https://") || hash.Contains("http://"))
            {
                return await AddressableManagement.LoadTextFromURLAsync(hash);
            }

            return hash;
        }

        private static async Task<GameObject> DownloadAndLoadAvatar(string AvatarAddress, string hash, BasisPlayer Player)
        {
            return await GameObjectAssetBundleManager.DownloadAndLoadGameObjectAsync(
                AvatarAddress,
                hash,
                AddressableManagement.GetFileNameFromUrlWithoutExtension(AvatarAddress),
                AssetSubDirectory,
                Player.ProgressReportAvatarLoad
            );
        }

        private static void InitializePlayerAvatar(BasisPlayer Player, GameObject Output)
        {
            if (Output.TryGetComponent(out BasisAvatar Avatar))
            {
                DeleteLastAvatar(Player);
                Player.Avatar = Avatar;

                if (Player is BasisLocalPlayer localPlayer)
                {
                    CreateLocal(localPlayer);
                    localPlayer.InitalizeIKCalibration(localPlayer.AvatarDriver);
                }
                else if (Player is BasisRemotePlayer remotePlayer)
                {
                    CreateRemote(remotePlayer);
                    remotePlayer.InitalizeIKCalibration(remotePlayer.RemoteAvatarDriver);
                }
            }
        }

        public static async Task LoadAvatarAfterError(BasisPlayer Player, string AvatarAddress)
        {
            try
            {
                var Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Quaternion.identity, null);
                (List<GameObject> GameObjects, AddressableGenericResource resource) = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadingAvatar, Para);

                if (GameObjects.Count > 0)
                {
                    InitializePlayerAvatar(Player, GameObjects[0]);
                }

                Player.AvatarSwitched();
            }
            catch (Exception e)
            {
                Debug.LogError($"Fallback avatar loading failed: {e}");
            }
        }

        public static void LoadLoadingAvatar(BasisPlayer Player, string LoadingAvatarToUse)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(LoadingAvatarToUse);
            var LoadingAvatar = op.WaitForCompletion();
            var InSceneLoadingAvatar = GameObject.Instantiate(LoadingAvatar, Player.transform.position, Quaternion.identity);

            if (InSceneLoadingAvatar.TryGetComponent(out BasisAvatar Avatar))
            {
                Player.Avatar = Avatar;

                if (Player.IsLocal)
                {
                    BasisLocalPlayer BasisLocalPlayer =(BasisLocalPlayer)Player;
                    CreateLocal(BasisLocalPlayer);
                    Player.InitalizeIKCalibration(BasisLocalPlayer.AvatarDriver);
                }
                else
                {
                    BasisRemotePlayer BasisRemotePlayer =(BasisRemotePlayer)Player;
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