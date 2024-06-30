using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

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