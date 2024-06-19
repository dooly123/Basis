using UnityEngine.InputSystem;

public class BasisLocalBoneDriver : BaseBoneDriver
{
    public bool IsFlip;
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
    public void ValidatorSimulate()
    {
        if (IsFlip)
        {
            Simulate();
            IsFlip = false;
        }
        else
        {
            Simulate();
            IsFlip = true;
        }
    }
}