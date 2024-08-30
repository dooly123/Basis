using UnityEngine;

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
    }
}