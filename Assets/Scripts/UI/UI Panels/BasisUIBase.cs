using Assets.Scripts.Addressable_Driver;
using Assets.Scripts.Addressable_Driver.Factory;
using Assets.Scripts.BasisSdk.Helpers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace Assets.Scripts.UI.UI_Panels
{
public abstract class BasisUIBase : MonoBehaviour
{
    public AddressableGenericResource LoadedMenu;
    public UnityEvent OnCloseMenu = new UnityEvent();
    public UnityEvent OnCreatedMenu = new UnityEvent();
    public void CloseThisMenu()
    {
        OnCloseMenu.Invoke();
        AddressableLoadFactory.ReleaseResource(LoadedMenu);
        Destroy(this.gameObject);
    }
    public static async Task OpenThisMenu(AddressableGenericResource resource)
    {
        await AddressableLoadFactory.LoadAddressableResourceAsync<GameObject>(resource);
        GameObject Result = (GameObject)resource.Handles[0].Result;
        Result = GameObject.Instantiate(Result);
        BasisUIBase BasisUIBase = BasisHelpers.GetOrAddComponent<BasisUIBase>(Result);
        BasisUIBase.OnCreatedMenu?.Invoke();
    }
    public static void OpenMenuNow(AddressableGenericResource AddressableGenericResource)
    {
        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(AddressableGenericResource.Key);
        GameObject RAC = op.WaitForCompletion();
        GameObject Result = GameObject.Instantiate(RAC);
        BasisUIBase BasisUIBase = BasisHelpers.GetOrAddComponent<BasisUIBase>(Result);
        BasisUIBase.OnCreatedMenu?.Invoke();
    }
}
}