using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.TransformBinders
{
    public class BasisLockToInput : MonoBehaviour
    {
        public BasisBoneTrackedRole TrackedRole;
        public BasisInput AttachedInput = null;
        public bool HasEvent = false;
        public void Awake()
        {
            Initialize();
        }
        public void Initialize()
        {
            if (BasisDeviceManagement.Instance.BasisLockToInputs.Contains(this) == false)
            {
                BasisDeviceManagement.Instance.BasisLockToInputs.Add(this);
            }
            if (HasEvent == false)
            {
                BasisDeviceManagement.Instance.AllInputDevices.OnListChanged += FindRole;
                BasisDeviceManagement.Instance.AllInputDevices.OnListItemRemoved += ResetIfNeeded;
                HasEvent = true;
            }
            FindRole();
        }
        public void OnDestroy()
        {
            if (HasEvent)
            {
                BasisDeviceManagement.Instance.AllInputDevices.OnListChanged -= FindRole;
                BasisDeviceManagement.Instance.AllInputDevices.OnListItemRemoved -= ResetIfNeeded;
                HasEvent = false;
            }
        }
        private void ResetIfNeeded(BasisInput input)
        {
            if (AttachedInput == null || AttachedInput == input)
            {
                Debug.Log("ReParenting Camera");
                transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
            }
        }

        public void FindRole()
        {
            transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
            int count = BasisDeviceManagement.Instance.AllInputDevices.Count;
            Debug.Log("finding Lock " + TrackedRole);
            for (int Index = 0; Index < count; Index++)
            {
                BasisInput Input = BasisDeviceManagement.Instance.AllInputDevices[Index];
                if (Input != null)
                {
                    if (Input.TryGetRole(out BasisBoneTrackedRole role))
                    {
                        if (role == TrackedRole)
                        {
                            AttachedInput = Input;
                            transform.parent = AttachedInput.transform;
                            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                            transform.localScale = Vector3.one;
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogError("Missing Role " + role);
                    }
                }
                else
                {
                    Debug.LogError("There was a missing BasisInput at " + Index);
                }
            }
        }
    }
}