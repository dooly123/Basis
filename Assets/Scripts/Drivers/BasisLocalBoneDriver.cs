using UnityEngine.InputSystem;

public class BasisLocalBoneDriver : BaseBoneDriver
{
    public void Start()
    {
        InputSystem.onAfterUpdate += Simulate;
    }
    public void OnDestroy()
    {
        InputSystem.onAfterUpdate -= Simulate;
    }
    public void Update()
    {
        ApplyMovement();
    }
}