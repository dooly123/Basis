using UnityEngine;
public class BasisLocalAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private BasisAnimatorVariableApply basisAnimatorVariableApply = new BasisAnimatorVariableApply();
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private BasisAnimatorBoneControl cachedBones = new BasisAnimatorBoneControl();
    public float IsMoving = 0.1f;
    private BasisLocalPlayer localPlayer;
    public float ScaleMovementBy = 1;
    public float dampeningFactor = 0.1f; // Adjust this value to control the dampening effect
    private Vector3 previousRawVelocity = Vector3.zero;
    void Simulate()
    {
        //ofset vertical of body position
        Vector3 currentVelocity = Quaternion.Inverse(cachedBones.Head.FinalisedWorldData.rotation) * (cachedBones.Hips.FinalisedWorldData.position - cachedBones.Hips.FinalisedLastWorldData.position) / Time.deltaTime;

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
    private void JustJumped()
    {
        basisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = true;
        basisAnimatorVariableApply.UpdateJumpState();
    }
    private void JustLanded()
    {
        basisAnimatorVariableApply.UpdateIsLandingState();
    }
    public void Initialize(BasisLocalBoneDriver driver, Animator anim)
    {
        FindReferences();
        this.animator = anim;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        basisAnimatorVariableApply.LoadCachedAnimatorHashes(animator);

        if (driver.FindBone(out cachedBones.Hips, BasisBoneTrackedRole.Hips) && driver.FindBone(out cachedBones.Head, BasisBoneTrackedRole.Head))
        {

        }
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