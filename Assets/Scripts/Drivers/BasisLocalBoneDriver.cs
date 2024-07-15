using UnityEngine.InputSystem;

public class BasisLocalBoneDriver : BaseBoneDriver
{
    public void Start()
    {
        InputSystem.onAfterUpdate += SimulateAndApply;
    }
    public void OnDestroy()
    {
        InputSystem.onAfterUpdate -= SimulateAndApply;
    }
}