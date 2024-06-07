using UnityEngine;

public class BasisLocalAnimatorDriver : MonoBehaviour
{
    [SerializeField]
    public BasisAnimatorVariableApply BasisAnimatorVariableApply = new BasisAnimatorVariableApply();
    [SerializeField]
    public Animator Animator;
    [SerializeField]
    public BasisLocalBoneDriver Driver;
    [SerializeField]
    public BasisPreviousAndCurrentAnimatorData dataStore = new BasisPreviousAndCurrentAnimatorData();
    [SerializeField]
    public BasisAnimatorBoneControl cachedBones = new BasisAnimatorBoneControl();
    public float moveThreshold = 0.3f;
    public float minAnimationSpeed = 0.2f;
    public float maxAnimationSpeed = 3f;
    public float animationSmoothTime = 0.1f;
    public float maxRootOffset = 0.5f;
    public float maxRootAngleMoving = 10f;
    public float maxRootAngleStanding = 90f;
    private Vector3 velocityLocalV;
    private Vector3 lastCorrection;
    private Vector3 lastHeadTargetPos;
    private Vector3 lastSpeedRootPos;
    private Vector3 lastEndRootPos;
    private float animSpeedV;
    private float stopMoveTimer;
    private float turn;
    private float currentAnimationSmoothTime = 0.05f;
    public float scale = 1;
    public float IsTurningLargerThenCheck = 0.2f;
    public float AngleCorrection = 90f;
    public float TurnSpeedup = 3;
    public float velLocalMag;
    public float deltaTime;
    public float animationSmoothTimeTarget;
    public bool isTurning;
    public float ScaledMoveThreshold;
    public Vector3 rootUp;
    public Vector3 offset;
    public Vector3 velocityLocalTarget;
    public Vector3 currentRootPos;
    public Vector3 externalDelta;
    public Vector3 headTargetPos;
    public Vector3 headTargetVelocity;
    public Vector3 rootVelocity;
    public bool isMovingRaw;
    public float AnimationSpeedTarget;
    public float rootVelocityMag;
    public Vector3 RootPosition;
    public float offsetMag;
    public void Initalize(BasisLocalBoneDriver driver, Animator anim)
    {
        Driver = driver;
        Animator = anim;
        Animator.updateMode = AnimatorUpdateMode.Normal;
        Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;//just to make sure localy its always good
        BasisAnimatorVariableApply.LoadCachedAnimatorHashes(Animator);
        if (Driver.FindBone(out cachedBones.Hips, BasisBoneTrackedRole.Hips))
        {

        }
        if (Driver.FindBone(out cachedBones.Head, BasisBoneTrackedRole.Head))
        {

        }
        lastHeadTargetPos = cachedBones.Head.BoneTransform.position;
        BasisLocalPlayer.Instance.Move.ReadyToRead += Simulate;
        BasisLocalPlayer.Instance.Move.JustJumped += JustJumped;
        BasisLocalPlayer.Instance.Move.JustLanded += JustLanded;
    }
    public void Simulate()
    {
        deltaTime = Time.deltaTime;
        CalculateRootUpVector(cachedBones.Hips);
        CalculateExternalDelta(cachedBones.Hips, cachedBones.Head);
        CalculateHeadTargetVelocity();
        CalculateTurning(cachedBones.Head, cachedBones.Hips);
        CalculateOffset(cachedBones.Hips);
        CalculateVelocityAndSmoothness(cachedBones.Hips);
        CheckIfMoving();
        CalculateAnimationSpeed(cachedBones.Hips);
        ApplyRootLerpAndLimits(cachedBones.Hips);
        BasisAnimatorVariableApply.BasisAnimatorVariables.IsFalling = BasisLocalPlayer.Instance.Move.IsFalling;
        if (BasisLocalInputActions.Instance != null)
        {
            BasisAnimatorVariableApply.BasisAnimatorVariables.IsCrouching = BasisLocalInputActions.Instance.Crouching;
        }
        BasisAnimatorVariableApply.UpdateAnimator(scale);
        if (BasisAnimatorVariableApply.BasisAnimatorVariables.IsFalling == false)
        {
        }
        else
        {
            BasisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = false;
        }
    }
    public void JustJumped()
    {
        BasisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = true;
        BasisAnimatorVariableApply.UpdateJumpState();
    }
    public void JustLanded()
    {
        BasisAnimatorVariableApply.UpdateIsLandingState();
    }
    private void CalculateRootUpVector(BasisBoneControl root)
    {
        rootUp = root.BoneTransform.rotation * Vector3.up;
    }

    private void CalculateExternalDelta(BasisBoneControl root, BasisBoneControl head)
    {
        externalDelta = root.BoneTransform.position - lastEndRootPos;
        externalDelta -= Animator.deltaPosition;
        headTargetPos = head.BoneTransform.position;
    }

    private void CalculateHeadTargetVelocity()
    {
        headTargetVelocity = (headTargetPos - lastHeadTargetPos) / deltaTime;
        lastHeadTargetPos = headTargetPos;
        headTargetVelocity = Flatten(headTargetVelocity, rootUp);
    }

    private void CalculateTurning(BasisBoneControl head, BasisBoneControl root)
    {
        CalculateTurning(head, root, out isTurning, deltaTime);
    }

    private void CalculateOffset(BasisBoneControl root)
    {
        offset = headTargetPos - root.BoneTransform.position;
        offset -= externalDelta;
        offset -= lastCorrection;
        offset = Flatten(offset, rootUp);
    }

    private void CalculateVelocityAndSmoothness(BasisBoneControl root)
    {
        velocityLocalTarget = Quaternion.Inverse(root.BoneTransform.rotation) * (headTargetVelocity + offset);
        animationSmoothTimeTarget = isTurning && !BasisAnimatorVariableApply.BasisAnimatorVariables.isMoving ? 0.2f : animationSmoothTime;
        currentAnimationSmoothTime = Mathf.Lerp(currentAnimationSmoothTime, animationSmoothTimeTarget, deltaTime * 20f);
        BasisAnimatorVariableApply.BasisAnimatorVariables.velocityLocal = Vector3.SmoothDamp(BasisAnimatorVariableApply.BasisAnimatorVariables.velocityLocal, velocityLocalTarget, ref velocityLocalV, currentAnimationSmoothTime, Mathf.Infinity, deltaTime);
        velLocalMag = BasisAnimatorVariableApply.BasisAnimatorVariables.velocityLocal.magnitude;
    }

    private void CheckIfMoving()
    {
        ScaledMoveThreshold = moveThreshold * scale;
        if (BasisAnimatorVariableApply.BasisAnimatorVariables.isMoving)
        {
            ScaledMoveThreshold *= 0.9f;
        }
        isMovingRaw = BasisAnimatorVariableApply.BasisAnimatorVariables.velocityLocal.sqrMagnitude > ScaledMoveThreshold * ScaledMoveThreshold;
        if (isMovingRaw) stopMoveTimer = 0f;
        else stopMoveTimer += deltaTime;
        BasisAnimatorVariableApply.BasisAnimatorVariables.isMoving = stopMoveTimer < 0.05f;
    }

    private void CalculateAnimationSpeed(BasisBoneControl root)
    {
        currentRootPos = root.BoneTransform.position;
        currentRootPos -= externalDelta;
        currentRootPos -= lastCorrection;
        rootVelocity = (currentRootPos - lastSpeedRootPos) / deltaTime;
        lastSpeedRootPos = root.BoneTransform.position;
        rootVelocityMag = rootVelocity.magnitude;
        AnimationSpeedTarget = minAnimationSpeed;
        if (rootVelocityMag > 0f && isMovingRaw)
        {
            AnimationSpeedTarget = BasisAnimatorVariableApply.BasisAnimatorVariables.animSpeed * (velLocalMag / rootVelocityMag);
        }
        AnimationSpeedTarget = Mathf.Clamp(AnimationSpeedTarget, minAnimationSpeed, maxAnimationSpeed);
        BasisAnimatorVariableApply.BasisAnimatorVariables.animSpeed = Mathf.SmoothDamp(BasisAnimatorVariableApply.BasisAnimatorVariables.animSpeed, AnimationSpeedTarget, ref animSpeedV, 0.05f, Mathf.Infinity, deltaTime);
    }

    private void ApplyRootLerpAndLimits(BasisBoneControl root)
    {
        headTargetPos += ExtractVertical(root.BoneTransform.position - headTargetPos, rootUp);
        if (maxRootOffset > 0f)
        {
            lastCorrection = Vector3.zero;
            offset = headTargetPos - root.BoneTransform.position;
            offset = Flatten(offset, rootUp);
            offsetMag = offset.magnitude;
            if (offsetMag > maxRootOffset)
            {
                lastCorrection += (offset - (offset / offsetMag) * maxRootOffset);
            }
        }
        else
        {
            lastCorrection = (headTargetPos - root.BoneTransform.position);
        }
        lastEndRootPos = root.BoneTransform.position;
    }
    public void CalculateTurning(BasisBoneControl head, BasisBoneControl root, out bool isTurning, float DeltaTime)
    {
        Vector3 Forward = head.BoneTransform.forward;
        Forward.y = 0f;
        Vector3 headForwardLocal = Quaternion.Inverse(root.BoneTransform.rotation) * Forward;
        float angle = Mathf.Atan2(headForwardLocal.x, headForwardLocal.z) * Mathf.Rad2Deg;
        float turnTarget = angle / AngleCorrection;
        isTurning = Mathf.Abs(turnTarget) < IsTurningLargerThenCheck;

        turn = Mathf.Lerp(turn, turnTarget, DeltaTime * TurnSpeedup);
    }
    public static Vector3 ExtractVertical(Vector3 v, Vector3 verticalAxis)
    {
        if (verticalAxis == Vector3.up) return Vector3.up * v.y;
        return Vector3.Project(v, verticalAxis);
    }
    public static Vector3 Flatten(Vector3 v, Vector3 normal)
    {
        if (normal == Vector3.up) return new Vector3(v.x, 0f, v.z);
        return v - Vector3.Project(v, normal);
    }
}