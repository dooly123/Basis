using UnityEngine;
public class BasisLocalAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private BasisAnimatorVariableApply basisAnimatorVariableApply = new BasisAnimatorVariableApply();
    [SerializeField]
    private Animator animator;
    public float IsMoving = 0.1f;
    private BasisLocalPlayer localPlayer;
    public float ScaleMovementBy = 1;
    public float dampeningFactor = 0.1f; // Adjust this value to control the dampening effect
    private Vector3 previousRawVelocity = Vector3.zero;
    public BasisCharacterController Controller;
    public BasisBoneControl Hips;
    void Simulate()
    {
        if (localPlayer.AvatarDriver.InTPose == false)
        {
            //calculate the velocity of the character controller taking into the account the direction occuring to the hips
            //this works well in fullbody
            Vector3 currentVelocity = Quaternion.Inverse(Hips.FinalisedWorldData.rotation) * (Controller.bottomPoint - Controller.LastbottomPoint) / Time.deltaTime;

            Vector3 dampenedVelocity = Vector3.Lerp(previousRawVelocity, currentVelocity, dampeningFactor);

            basisAnimatorVariableApply.BasisAnimatorVariables.Velocity = dampenedVelocity;
            basisAnimatorVariableApply.BasisAnimatorVariables.isMoving = basisAnimatorVariableApply.BasisAnimatorVariables.Velocity.sqrMagnitude > IsMoving;

            basisAnimatorVariableApply.BasisAnimatorVariables.AnimationsCurrentSpeed = 1;

            basisAnimatorVariableApply.BasisAnimatorVariables.IsFalling = localPlayer.Move.IsFalling;

            if (BasisLocalInputActions.Instance != null)
            {
                basisAnimatorVariableApply.BasisAnimatorVariables.IsCrouching = BasisLocalInputActions.Instance.Crouching;
            }
            basisAnimatorVariableApply.UpdateAnimator(ScaleMovementBy);

            if (basisAnimatorVariableApply.BasisAnimatorVariables.IsFalling)
            {
                basisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = false;
            }
            // Update the previous velocity with the current dampened velocity for the next frame
            previousRawVelocity = dampenedVelocity;
        }
    }
    private void JustJumped()
    {
        basisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = true;
        basisAnimatorVariableApply.UpdateJumpState();
    }
    private void JustLanded()
    {
        basisAnimatorVariableApply.UpdateIsLandingState();
    }
    public void Initialize(Animator anim)
    {
        FindReferences();
        this.animator = anim;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        basisAnimatorVariableApply.LoadCachedAnimatorHashes(animator);
        Controller = BasisLocalPlayer.Instance.Move;
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);
    }
    private void FindReferences()
    {
        if (localPlayer == null)
        {
            localPlayer = BasisLocalPlayer.Instance;
            localPlayer.Move.ReadyToRead += Simulate;
            localPlayer.Move.JustJumped += JustJumped;
            localPlayer.Move.JustLanded += JustLanded;
        }
    }
    private void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.Move.ReadyToRead -= Simulate;
            localPlayer.Move.JustJumped -= JustJumped;
            localPlayer.Move.JustLanded -= JustLanded;
        }
    }
}