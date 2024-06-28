using UnityEngine;

public class BasisLockToInput : MonoBehaviour
{
    public BasisBoneTrackedRole TrackedRole;
    public BasisInput AttachedInput = null;
    public bool HasInput;
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
        BasisDeviceManagement.Instance.AllInputDevices.OnListChanged += FindRole;
        FindRole();
    }
    public void FindRole()
    {
        for (int Index = 0; Index < BasisDeviceManagement.Instance.AllInputDevices.Count; Index++)
        {
            BasisInput Input = BasisDeviceManagement.Instance.AllInputDevices[Index];
            if (Input != null)
            {
                if (Input.hasRoleAssigned)
                {
                    if (Input.TrackedRole == TrackedRole)
                    {
                        AttachedInput = Input;
                        //Debug.Log("Assigning " + AttachedInput.name);
                        AttachedInput.AfterControlApply += Simulation;
                        HasInput = true;
                        return;
                    }
                }
            }
        }
        HasInput = false;
    }
    public void OnDestroy()
    {
        HasInput = false;
        if (AttachedInput != null) 
        {
            AttachedInput.AfterControlApply -= Simulation;
        }
    }
    void Simulation()
    {
        if (HasInput)
        {
            transform.SetPositionAndRotation(AttachedInput.transform.position, AttachedInput.transform.rotation);
        }
    }
}