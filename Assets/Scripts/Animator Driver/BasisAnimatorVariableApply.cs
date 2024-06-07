using UnityEngine;
[System.Serializable]
public class BasisAnimatorVariableApply
{
    public Animator Animator;
    [SerializeField]
    public BasisAvatarAnimatorHash BasisAvatarAnimatorHash = new BasisAvatarAnimatorHash();
    [SerializeField]
    public BasisAnimatorVariables BasisAnimatorVariables = new BasisAnimatorVariables();
    public void UpdateAnimator(float Scale)
    {
        // Check if values have changed before applying updates
        if (BasisAnimatorVariables.cachedAnimSpeed != BasisAnimatorVariables.animSpeed)
        {
            Animator.SetFloat(BasisAvatarAnimatorHash.HashCurrentSpeed, BasisAnimatorVariables.animSpeed);
            BasisAnimatorVariables.cachedAnimSpeed = BasisAnimatorVariables.animSpeed;
        }

        if (BasisAnimatorVariables.cachedIsMoving != BasisAnimatorVariables.isMoving)
        {
            Animator.SetBool(BasisAvatarAnimatorHash.HashMovingState, BasisAnimatorVariables.isMoving);
            BasisAnimatorVariables.cachedIsMoving = BasisAnimatorVariables.isMoving;
        }
        if (BasisAnimatorVariables.cachedIsCrouching != BasisAnimatorVariables.IsCrouching)
        {
            Animator.SetBool(BasisAvatarAnimatorHash.HashCrouchedState, BasisAnimatorVariables.IsCrouching);
            BasisAnimatorVariables.cachedIsCrouching = BasisAnimatorVariables.IsCrouching;
        }
        if (BasisAnimatorVariables.cachedIsFalling != BasisAnimatorVariables.IsFalling)
        {
            Animator.SetBool(BasisAvatarAnimatorHash.HashIsFalling, BasisAnimatorVariables.IsFalling);
            BasisAnimatorVariables.cachedIsFalling = BasisAnimatorVariables.IsFalling;
        }

        float horizontalMovement = BasisAnimatorVariables.velocityLocal.x / Scale;
        if (BasisAnimatorVariables.cachedHorizontalMovement != horizontalMovement)
        {
            Animator.SetFloat(BasisAvatarAnimatorHash.HashCurrentHorizontalMovement, horizontalMovement);
            BasisAnimatorVariables.cachedHorizontalMovement = horizontalMovement;
        }

        float verticalMovement = BasisAnimatorVariables.velocityLocal.z / Scale;
        if (BasisAnimatorVariables.cachedVerticalMovement != verticalMovement)
        {
            Animator.SetFloat(BasisAvatarAnimatorHash.HashCurrentVerticalMovement, verticalMovement);
            BasisAnimatorVariables.cachedVerticalMovement = verticalMovement;
        }
        UpdateJumpState();
    }
    public void LoadCachedAnimatorHashes(Animator Anim)
    {
        Animator = Anim;
        BasisAvatarAnimatorHash.HashCurrentHorizontalMovement = Animator.StringToHash("CurrentHorizontalMovement");
        BasisAvatarAnimatorHash.HashCurrentVerticalMovement = Animator.StringToHash("CurrentVerticalMovement");
        BasisAvatarAnimatorHash.HashCurrentSpeed = Animator.StringToHash("CurrentSpeed");
        BasisAvatarAnimatorHash.HashCrouchedState = Animator.StringToHash("CrouchedState");
        BasisAvatarAnimatorHash.HashMovingState = Animator.StringToHash("MovingState");

        BasisAvatarAnimatorHash.HashIsFalling = Animator.StringToHash("IsFalling");
        BasisAvatarAnimatorHash.HashIsLanding = Animator.StringToHash("IsLanding");
        BasisAvatarAnimatorHash.HashIsJumping = Animator.StringToHash("IsJumping");
    }
    public void UpdateJumpState()
    {
        if (BasisAnimatorVariables.cachedIsJumping != BasisAnimatorVariables.IsJumping)
        {
            Animator.SetBool(BasisAvatarAnimatorHash.HashIsJumping, BasisAnimatorVariables.IsJumping);
            BasisAnimatorVariables.cachedIsJumping = BasisAnimatorVariables.IsJumping;
        }
    }
    public void UpdateIsLandingState()
    {
        Animator.SetTrigger(BasisAvatarAnimatorHash.HashIsLanding);
    }
}