using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasisUIBase : MonoBehaviour
{
    public AddressableGenericResource LoadedMenu;
    public static bool IsLoading;
    public UnityEvent OnCloseMenu = new UnityEvent();
    public UnityEvent OnCreatedMenu = new UnityEvent();
    public void CloseThisMenu()
    {
        OnCloseMenu.Invoke();
        AddressableLoadFactory.ReleaseResource(LoadedMenu);
        Destroy(this.gameObject);
        if (BasisAvatarEyeInput.Instance != null)
        {
            BasisAvatarEyeInput.Instance.HandleEscape();

        }
    }
    public static async Task OpenThisMenu(string AddressableID)
    {
        IsLoading = true;
        AddressableGenericResource resource = new AddressableGenericResource(AddressableID, AddressableExpectedResult.SingleItem);
        await AddressableLoadFactory.LoadAddressableResourceAsync<GameObject>(resource);
        GameObject Result = (GameObject)resource.Handles[0].Result;
        Result = GameObject.Instantiate(Result);
        BasisUIBase BasisHamburgerMenu = BasisHelpers.GetOrAddComponent<BasisUIBase>(Result);
        IsLoading = false;
        BasisHamburgerMenu.OnCreatedMenu.Invoke();
    }
}