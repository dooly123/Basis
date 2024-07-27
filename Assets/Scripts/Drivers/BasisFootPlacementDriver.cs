using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BasisFootPlacementDriver : MonoBehaviour
{
    public BasisLocalPlayer Localplayer;
    public BasisBoneControl Hips;
    [SerializeField]
    public BasisIKFootSolver LeftFootSolver = new BasisIKFootSolver();
    [SerializeField]
    public BasisIKFootSolver RightFootSolver = new BasisIKFootSolver();
    public bool HasEvents = false;
    public float DefaultFootOffset = 0.5f;
    public float LargerThenBeforeRotating = 400;
    public void Initialize()
    {
        Localplayer = BasisLocalPlayer.Instance;
        Localplayer.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);

        Localplayer.LocalBoneDriver.FindBone(out LeftFootSolver.Foot, BasisBoneTrackedRole.LeftFoot);
        Localplayer.LocalBoneDriver.FindBone(out RightFootSolver.Foot, BasisBoneTrackedRole.RightFoot);
        OnCalibration();
        if (HasEvents == false)
        {
            Localplayer.AvatarDriver.CalibrationComplete += OnCalibration;
            Localplayer.LocalBoneDriver.OnPostSimulate += Simulate;
            HasEvents = true;
        }
    }
    public void Simulate()
    {
        Vector3 Angular = Localplayer.AvatarDriver.AnimatorDriver.angularVelocity;
        float Square = Angular.sqrMagnitude;
        Vector3 localTposeHips = Hips.TposeLocal.position;
        Vector3 HipsPosLocal = Hips.OutGoingData.position;
        /*
        if(Square <= LargerThenBeforeRotating)
        {
            Localplayer.AvatarDriver.LeftFootLayer.active = false;
            Localplayer.AvatarDriver.RightFootLayer.active = false;
        }
        else
        {
            Localplayer.AvatarDriver.LeftFootLayer.active = true;
            Localplayer.AvatarDriver.RightFootLayer.active = true;
        }
        */
        RightFootSolver.Simulate(Square, localTposeHips, HipsPosLocal);
        LeftFootSolver.Simulate(Square, localTposeHips, HipsPosLocal);
    }
    public void OnCalibration()
    {
        LeftFootSolver.Driver = this;
        RightFootSolver.Driver = this;

        LeftFootSolver.Foot.HasTracked = BasisHasTracked.HasNoTracker;
        RightFootSolver.Foot.HasTracked = BasisHasTracked.HasNoTracker;

        LeftFootSolver.MyIkConstraint = Localplayer.AvatarDriver.LeftFootTwoBoneIK;
        RightFootSolver.MyIkConstraint = Localplayer.AvatarDriver.RightFootTwoBoneIK;
        Localplayer.AvatarDriver.LeftFootLayer.active = true;
        Localplayer.AvatarDriver.RightFootLayer.active = true;

        LeftFootSolver.Initialize(RightFootSolver);
        RightFootSolver.Initialize(LeftFootSolver);
        LeftFootSolver.stepHeight = DefaultFootOffset * Localplayer.RatioAvatarToAvatarEyeDefaultScale;
        RightFootSolver.stepHeight = DefaultFootOffset * Localplayer.RatioAvatarToAvatarEyeDefaultScale;
    }
    public void OnDestroy()
    {
        if (HasEvents)
        {
            Localplayer.AvatarDriver.CalibrationComplete -= OnCalibration;
            HasEvents = false;
        }
    }
    public void OnDrawGizmos()
    {
        LeftFootSolver.Gizmo();
        RightFootSolver.Gizmo();
    }
    [System.Serializable]
    public class BasisIKFootSolver
    {
        public BasisBoneControl Foot;
        public BasisFootPlacementDriver Driver;
        public TwoBoneIKConstraint MyIkConstraint;
        public BasisIKFootSolver OtherFoot;
        public float percentage = 0.5f;
        public float PercentagePastFoot = 4;
        private Vector3 topPointLocal;
        private Vector3 bottomPointLocal;
        public Vector3 WorldTopPoint;
        public Vector3 WorldBottomPoint;
        [SerializeField] public float FootStepSpeed = 3;
        [SerializeField] public float stepHeight;
        public float LargerThenThisMoveFeet = 500;

        public float lerp = 0;
        private RaycastHit _hitInfo;

        public void Initialize(BasisIKFootSolver otherFoot)
        {
            OtherFoot = otherFoot;
            lerp = 1;
        }
        public bool IsMoving()
        {
            return lerp < 1;
        }
        public void Simulate(float RotationMagnitude, Vector3 localTposeHips, Vector3 HipsPosLocal)
        {
            // Get the local position of the foot in T-pose
            Vector3 localTposeFoot = Foot.TposeLocal.position;

            Vector3 Offset = localTposeFoot - localTposeHips;
            Vector3 RotatedOffset = Driver.transform.rotation * Driver.Hips.OutgoingWorldData.rotation * Offset;
            Vector3 FootExtendedPositionWorld = HipsPosLocal + RotatedOffset;

            // Create a vector for the hip height at the foot's X and Z coordinates
            Vector3 hipHeightFootVector = new Vector3(FootExtendedPositionWorld.x, Driver.Hips.OutgoingWorldData.position.y, FootExtendedPositionWorld.z);

            // Calculate the vertical difference
            float yDifference = FootExtendedPositionWorld.y - hipHeightFootVector.y;

            // Interpolate between the hip height vector and the T-pose foot position
            topPointLocal = Vector3.Lerp(hipHeightFootVector, FootExtendedPositionWorld, percentage);
            WorldTopPoint = topPointLocal + BasisLocalPlayer.Instance.LocalBoneDriver.transform.position;
            // Determine the bottom point with the vertical difference adjusted
            bottomPointLocal = FootExtendedPositionWorld + new Vector3(0, (yDifference / PercentagePastFoot), 0);
            WorldBottomPoint = bottomPointLocal + BasisLocalPlayer.Instance.LocalBoneDriver.transform.position;

            Vector3 FinalApplied = bottomPointLocal;
            if (RotationMagnitude > LargerThenThisMoveFeet && OtherFoot.IsMoving() == false && lerp >= 1)//only can move one up at at a time
            {
              //disabled as world top and world bottom is wrong  if (Physics.Linecast(WorldTopPoint, WorldBottomPoint, out _hitInfo, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.UseGlobal))
                {
                    lerp = 0;
                }
            }
            if (lerp <= 1)
            {
                FinalApplied = new Vector3(bottomPointLocal.x, bottomPointLocal.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight, bottomPointLocal.z);
                lerp += Time.deltaTime * FootStepSpeed;
            }
            Foot.OutGoingData.position = FinalApplied;
            Foot.OutGoingData.rotation = Quaternion.Euler(Foot.BoneTransform.localEulerAngles.x, Driver.Hips.BoneTransform.localEulerAngles.y, Foot.BoneTransform.localEulerAngles.z);
        }
        public void Gizmo()
        {
            Gizmos.DrawLine(WorldTopPoint, WorldBottomPoint);
        }
    }
}
[System.Serializable]
public class SpringLerp
{
    public float damping = 0.5f;  // Damping factor, controls how quickly the spring settles
    public float stiffness = 0.5f; // Stiffness factor, controls the springiness
    public float velocity;  // Current velocity of the spring

    // Method to update the spring interpolation
    public float Update(float currentValue, float targetValue, float deltaTime)
    {
        float force = stiffness * (targetValue - currentValue);
        float dampingForce = -damping * velocity;
        float acceleration = force + dampingForce;

        velocity += acceleration * deltaTime;
        currentValue += velocity * deltaTime;

        return currentValue;
    }

    // Method to reset the velocity (useful if the spring is to be reused)
    public void Reset()
    {
        velocity = 0f;
    }

}
//            springLerp = new SpringLerp();
// float timedelta = Time.deltaTime;
//float TimeDeltaMulti = timedelta * TimeMultiplier;
//   Foot.FinalApplied.rotation = Driver.Hips.
/*
if (Physics.Linecast(_topPoint, _bottomPoint, out _hitInfo, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.UseGlobal))
{
    Vector3 footPosition = _hitInfo.point;
    Foot.FinalApplied.position = footPosition;
    // Calculate the rotation to align the foot with the normal and up direction
    Foot.FinalApplied.rotation = Quaternion.LookRotation(Driver.Hips.BoneTransform.forward, _hitInfo.normal);
}            //   Vector3 BodyPose = BasisLocalPlayer.Instance.Avatar.Animator.transform.position + LocalTposeFoot;
        private RaycastHit _hitInfo;
            // Interpolate position using SpringLerp
    //    Vector3 SpringAppliedPosition = new Vector3(springLerp.Update(LastPosition.x, FinalPosition.x, TimeDeltaMulti), springLerp.Update(LastPosition.y, FinalPosition.y, TimeDeltaMulti), springLerp.Update(LastPosition.z, FinalPosition.z, TimeDeltaMulti) );

        // Apply the final local position and rotation to the foot
*/
//  