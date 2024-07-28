using UnityEngine;
[System.Serializable]
public class BasisAnimatorVariableApply
{
    public Animator Animator;
    [SerializeField]
    public BasisAvatarAnimatorHash BasisAvatarAnimatorHash = new BasisAvatarAnimatorHash();
    [SerializeField]
    public BasisAnimatorVariables BasisAnimatorVariables = new BasisAnimatorVariables();
    [SerializeField]
    public BasisAnimatorhandHash BasisHandHash;
    public void UpdateAnimator(float Scale)
    {
        // Check if values have changed before applying updates
        if (BasisAnimatorVariables.cachedAnimSpeed != BasisAnimatorVariables.AnimationsCurrentSpeed)
        {
            Animator.SetFloat(BasisAvatarAnimatorHash.HashCurrentSpeed, BasisAnimatorVariables.AnimationsCurrentSpeed);
            BasisAnimatorVariables.cachedAnimSpeed = BasisAnimatorVariables.AnimationsCurrentSpeed;
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

        float horizontalMovement = BasisAnimatorVariables.Velocity.x / Scale;
        if (BasisAnimatorVariables.cachedHorizontalMovement != horizontalMovement)
        {
            Animator.SetFloat(BasisAvatarAnimatorHash.HashCurrentHorizontalMovement, horizontalMovement);
            BasisAnimatorVariables.cachedHorizontalMovement = horizontalMovement;
        }

        float verticalMovement = BasisAnimatorVariables.Velocity.z / Scale;
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
        BasisHandHash = new BasisAnimatorhandHash
        {
            rightFingersParameterHash = new int[5],
            leftFingersParameterHash = new int[5]
        };

        for (int Index = 0; Index < 5; Index++)
        {
            BasisHandHash.leftFingersParameterHash[Index] = Animator.StringToHash("LeftHandFinger" + (Index + 1));
        }

        for (int Index = 0; Index < 5; Index++)
        {
            BasisHandHash.rightFingersParameterHash[Index] = Animator.StringToHash("RightHandFinger" + (Index + 1));
        }

        BasisHandHash.LeftHandPoseHash = Animator.StringToHash("LeftHandPose");
        BasisHandHash.RightHandPoseHash = Animator.StringToHash("RightHandPose");
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
    [System.Serializable]
    public struct BasisAnimatorhandHash
    {
        public int RightHandPoseHash;
        public int LeftHandPoseHash;
        public int[] leftFingersParameterHash;
        public int[] rightFingersParameterHash;
    }
    enum Fingers : int // Used for naming transforms
    {
        Thumb = 0,
        Index = 1,
        Middle = 2,
        Ring = 3,
        Little = 4,
    }
    public void LeftHandCurls(float[] leftCurls)
    {
        for (int i = 0; i < 5; i++)
        {
            Animator.SetFloat(BasisHandHash.leftFingersParameterHash[i], leftCurls[i]);
        }
     //   CheckHandPoses(leftCurls, BasisHandHash.LeftHandPoseHash);
    }
    public void RightHandCurls(float[] rightCurls)
    {
        for (int i = 0; i < 5; i++)
        {
            Animator.SetFloat(BasisHandHash.rightFingersParameterHash[i], rightCurls[i]);
        }
      //  CheckHandPoses(rightCurls, BasisHandHash.RightHandPoseHash);
    }

    private void CheckHandPoses(float[] curls, int poseID)
    {
        // Fist
        if (curls[(int)Fingers.Thumb] > 0.8f &&
            curls[(int)Fingers.Index] > 0.8f &&
            curls[(int)Fingers.Middle] > 0.8f &&
            curls[(int)Fingers.Ring] > 0.8f &&
            curls[(int)Fingers.Little] > 0.8f)
        {
            Animator.SetInteger(poseID, 1);
        }

        // Open hand
        else if (curls[(int)Fingers.Thumb] < 0.4f &&
            curls[(int)Fingers.Index] < 0.4f &&
            curls[(int)Fingers.Middle] < 0.4f &&
            curls[(int)Fingers.Ring] < 0.4f &&
            curls[(int)Fingers.Little] < 0.4f)
        {
            Animator.SetInteger(poseID, 2);
        }

        // Finger point
        else if (curls[(int)Fingers.Thumb] > 0.5f &&
            curls[(int)Fingers.Index] < 0.5f &&
            curls[(int)Fingers.Middle] > 0.5f &&
            curls[(int)Fingers.Ring] > 0.5f &&
            curls[(int)Fingers.Little] > 0.5f)
        {
            Animator.SetInteger(poseID, 3);
        }

        // Peace
        else if (curls[(int)Fingers.Thumb] > 0.5f &&
                 curls[(int)Fingers.Index] < 0.5f &&
                 curls[(int)Fingers.Middle] < 0.5f &&
                 curls[(int)Fingers.Ring] > 0.5f &&
                 curls[(int)Fingers.Little] > 0.5f)
        {
            Animator.SetInteger(poseID, 4);
        }

        // Rock and roll
        else if (curls[(int)Fingers.Thumb] < 0.5f &&
                 curls[(int)Fingers.Index] < 0.5f &&
                 curls[(int)Fingers.Middle] > 0.5f &&
                 curls[(int)Fingers.Ring] > 0.5f &&
                 curls[(int)Fingers.Little] < 0.5f)
        {
            Animator.SetInteger(poseID, 5);
        }

        // Finger gun
        else if (curls[(int)Fingers.Thumb] < 0.5f &&
                 curls[(int)Fingers.Index] < 0.5f &&
                 curls[(int)Fingers.Middle] > 0.5f &&
                 curls[(int)Fingers.Ring] > 0.5f &&
                 curls[(int)Fingers.Little] > 0.5f)
        {
            Animator.SetInteger(poseID, 6);
        }

        // Thumbs up
        else if (curls[(int)Fingers.Thumb] < 0.5f &&
                 curls[(int)Fingers.Index] > 0.5f &&
                 curls[(int)Fingers.Middle] > 0.5f &&
                 curls[(int)Fingers.Ring] > 0.5f &&
                 curls[(int)Fingers.Little] > 0.5f)
        {
            Animator.SetInteger(poseID, 7);
        }

        // Neutral
        else
        {
            Animator.SetInteger(poseID, 0);
        }
    }
}