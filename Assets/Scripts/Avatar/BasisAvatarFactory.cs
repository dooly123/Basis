using Assets.Scripts.Addressable_Driver;
using Assets.Scripts.Addressable_Driver.Factory;
using Assets.Scripts.Addressable_Driver.Resource;
using Assets.Scripts.BasisSdk;
using Assets.Scripts.BasisSdk.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.Avatar
{
public static class BasisAvatarFactory
{
    public const string LoadingAvatar = "LoadingAvatar";
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
            (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(AvatarAddress, Para);
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
        UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Quaternion.identity, null);
        (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(AvatarAddress, Para);
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
                    CreateRemote(Player);
                    Player.InitalizeIKCalibration(Player.RemoteAvatarDriver);
                }
            }
        }
        Player.AvatarSwitched();
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