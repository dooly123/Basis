using UnityEngine;

public class BasisLockToInput : MonoBehaviour
{
    public BasisDeviceManagement BasisDeviceManagement;
    public BasisBoneTrackedRole TrackedRole;
    public BasisInput AttachedInput = null;
    public bool HasInput;
    public void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        BasisDeviceManagement = BasisDeviceManagement.Instance;
        if (BasisDeviceManagement.BasisLockToInputs.Contains(this) == false)
        {
            BasisDeviceManagement.BasisLockToInputs.Add(this);
        }
        FindRole();
    }
    public void OnEnable()
    {
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
                        Debug.Log("Assigning " + AttachedInput.name);
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
    public void OnDisable()
    {
        if (AttachedInput != null)
        {
            AttachedInput.AfterControlApply -= Simulation;
        }
    }
    void Simulation()
    {
        if (HasInput)
        {
            transform.SetPositionAndRotation(AttachedInput.LocalRawPosition + BasisLocalPlayer.Instance.transform.position, AttachedInput.LocalRawRotation);
        }
    }
}