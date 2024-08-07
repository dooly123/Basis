using Assets.Scripts.BasisSdk.Players;
using Assets.Scripts.Device_Management;
using Assets.Scripts.Device_Management.Devices;
using Assets.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Assets.Scripts.TransformBinders
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
        if(AttachedInput == null || AttachedInput == input)
        {
            Debug.Log("ReParenting Camera");
            this.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        }
    }

    public void FindRole()
    {
        this.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        for (int Index = 0; Index < BasisDeviceManagement.Instance.AllInputDevices.Count; Index++)
        {
            BasisInput Input = BasisDeviceManagement.Instance.AllInputDevices[Index];
            if (Input != null)
            {
                if (Input.TryGetRole(out BasisBoneTrackedRole role))
                {
                    if (role == TrackedRole)
                    {
                        AttachedInput = Input;
                        this.transform.parent = AttachedInput.transform;
                        this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        this.transform.localScale = Vector3.one;
                        return;
                    }
                }
            }
        }
    }
}
}