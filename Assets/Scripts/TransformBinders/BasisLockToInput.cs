using UnityEngine;
using UnityEngine.InputSystem;

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
        BasisDeviceManagement.AllInputDevices.OnListChanged += FindRole;
        InputSystem.onAfterUpdate += Simulation;
        if (BasisDeviceManagement.basisLockToInputs.Contains(this) == false)
        {
            BasisDeviceManagement.basisLockToInputs.Add(this);
        }
        FindRole();
    }
    public void OnEnable()
    {
        FindRole();
    }
    public void FindRole()
    {
        if (AttachedInput == null)
        {
            Debug.Log("Has no AttachedInput ");
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (Input.hasRoleAssigned)
                {
                    Debug.Log("Has Role hasRoleAssigned");
                    if (Input.TrackedRole == TrackedRole)
                    {
                        Debug.Log("Has Role " + TrackedRole);
                        AttachedInput = Input;
                        AttachedInput.AfterControlApply += Simulation;
                        HasInput = true;
                        return;
                    }
                }
            }
            HasInput = false;
        }
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
            transform.SetPositionAndRotation(AttachedInput.LocalRawPosition + BasisLocalPlayer.Instance.transform.position, AttachedInput.LocalRawRotation);
        }
    }
}