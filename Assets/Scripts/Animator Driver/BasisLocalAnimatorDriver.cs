using UnityEngine;
public class BasisLocalAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private BasisAnimatorVariableApply basisAnimatorVariableApply = new BasisAnimatorVariableApply();
    [SerializeField]
    private Animator animator;
    public float LargerThenVelocityCheck = 0.01f;
    private BasisLocalPlayer localPlayer;
    public float ScaleMovementBy = 1;
    public float dampeningFactor = 0.1f; // Adjust this value to control the dampening effect
    private Vector3 previousRawVelocity = Vector3.zero;
    public BasisCharacterController Controller;
    public BasisBoneControl Hips;
    public Vector3 currentVelocity;
    public Vector3 dampenedVelocity;
    void Simulate()
    {
        if (localPlayer.AvatarDriver.InTPose == false)
        {
            //calculate the velocity of the character controller taking into the account the direction occuring to the hips
            //this works well in fullbody
            currentVelocity = Quaternion.Inverse(Hips.FinalisedWorldData.rotation) * (Controller.bottomPoint - Controller.LastbottomPoint) / Time.deltaTime;

            dampenedVelocity = Vector3.Lerp(previousRawVelocity, currentVelocity, dampeningFactor);

            basisAnimatorVariableApply.BasisAnimatorVariables.Velocity = dampenedVelocity;
            basisAnimatorVariableApply.BasisAnimatorVariables.isMoving = basisAnimatorVariableApply.BasisAnimatorVariables.Velocity.sqrMagnitude > LargerThenVelocityCheck;

            basisAnimatorVariableApply.BasisAnimatorVariables.AnimationsCurrentSpeed = 1;
            if (HasHipsInput && basisAnimatorVariableApply.BasisAnimatorVariables.isMoving == false)
            {
                if (HipsInput.TryGetRole(out BasisBoneTrackedRole role))
                {
                    if (role == BasisBoneTrackedRole.Hips)
                    {
                        basisAnimatorVariableApply.BasisAnimatorVariables.AnimationsCurrentSpeed = 0;
                    }
                }
            }
            basisAnimatorVariableApply.BasisAnimatorVariables.IsFalling = localPlayer.Move.IsFalling;
            basisAnimatorVariableApply.BasisAnimatorVariables.IsCrouching = BasisLocalInputActions.Crouching;

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
        BasisDeviceManagement.Instance.AllInputDevices.OnListChanged += AssignHipsFBTracker;
        AssignHipsFBTracker();
    }
    public BasisInput HipsInput;
    public bool HasHipsInput = false;
    public void AssignHipsFBTracker()
    {
        HasHipsInput = BasisDeviceManagement.Instance.FindDevice(out HipsInput, BasisBoneTrackedRole.Hips);
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