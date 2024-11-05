using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Enums;
namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUISettings : BasisUIBase
    {
        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUISettings));
        }

        public override void InitalizeEvent()
        {
            BasisCursorManagement.UnlockCursor(nameof(BasisUISettings));
        }
        public void OpenConsole()
        {
            BasisUIManagement.Instance.CloseAllMenus();
            AddressableGenericResource resource = new AddressableGenericResource("LoggerUI", AddressableExpectedResult.SingleItem);
            OpenMenuNow(resource);
        }
        public void OpenControllerConfig()
        {
            BasisUIManagement.Instance.CloseAllMenus();
            AddressableGenericResource resource = new AddressableGenericResource("Assets/Prefabs/UI/BasisUIOffsetsManager.prefab", AddressableExpectedResult.SingleItem);
            OpenMenuNow(resource);
        }
    }
}