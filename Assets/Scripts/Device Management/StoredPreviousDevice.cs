using Basis.Scripts.Common;
using Basis.Scripts.TransformBinders.BoneControl;

namespace Basis.Scripts.Device_Management
{
    public partial class BasisDeviceManagement
    {
        [System.Serializable]
    public class StoredPreviousDevice
    {
        public BasisCalibratedOffsetData InverseOffsetFromBone;
        public BasisBoneTrackedRole trackedRole;
        public bool hasRoleAssigned = false;
        public string SubSystem;
        public string UniqueID;
    }
}
}