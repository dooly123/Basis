using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public static class BasisAvatarFactory
{
    public static async Task LoadAvatar(BasisLocalPlayer Player, string AvatarAddress)
    {
        DeleteLastAvatar(Player);
        LoadLoadingAvatar(Player, "LoadingAvatar");
        Player.OnAvatarSwitchedFallBack?.Invoke();
        UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Player.transform.rotation, null);
        (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(AvatarAddress, Para);
        List<GameObject> Gameobjects = data.Item1;
        Player.AvatarUrl = AvatarAddress;
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                if (gameObject.TryGetComponent(out BasisAvatar Avatar))
                {
                 //   Debug.Log("avatar position 1" + Avatar.transform.name + " at " + Avatar.transform.position + " with rotation " + Avatar.transform.rotation);
                    DeleteLastAvatar(Player);
                    Player.Avatar = Avatar;
                  //  Debug.Log("avatar position 2" + Avatar.transform.name + " at " + Avatar.transform.position + " with rotation " + Avatar.transform.rotation);
                   CreateLocal(Player);
                 //   Debug.Log("avatar position 3" + Avatar.transform.name + " at " + Avatar.transform.position + " with rotation " + Avatar.transform.rotation);
                    Player.InitalizeIKCalibration(Player.AvatarDriver);
                  //  Debug.Log("avatar position 4" + Avatar.transform.name + " at " + Avatar.transform.position + " with rotation " + Avatar.transform.rotation);
                }
            }
        }
        Player.SetPlayersEyeHeight(Player.PlayerEyeHeight, Player.AvatarDriver.ActiveEyeHeight);
        Player.OnAvatarSwitched?.Invoke();
    }
    public static async Task LoadAvatar(BasisRemotePlayer Player, string AvatarAddress)
    {
        DeleteLastAvatar(Player);
        LoadLoadingAvatar(Player, "LoadingAvatar");
        Player.OnAvatarSwitchedFallBack?.Invoke();
        UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters Para = new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(Player.transform.position, Player.transform.rotation, null);
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
        Player.OnAvatarSwitched?.Invoke();
    }
    public static void LoadLoadingAvatar(BasisPlayer Player, string LoadingAvatarToUse = "Assets/Prefabs/Loadins/LankyLoad.prefab")
    {
        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(LoadingAvatarToUse);
        GameObject LoadingAvatar = op.WaitForCompletion();
        GameObject InSceneLoadingAvatar = GameObject.Instantiate(LoadingAvatar, Player.transform.position, Player.transform.rotation);
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
    public static async Task LoadAvatar(BasisPlayer Player, string AvatarAddress)
    {
        if (Player.IsLocal)
        {
            await LoadAvatar((BasisLocalPlayer)Player, AvatarAddress);
        }
        else
        {
            await LoadAvatar((BasisRemotePlayer)Player, AvatarAddress);
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