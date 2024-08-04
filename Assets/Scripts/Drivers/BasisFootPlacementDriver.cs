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
    public float DefaultFootOffset = 0.35f;
    public float LargerThenBeforeRotating = 400;
    public float SquareAngular;
    public float SquareVel;
    public float MaxBeforeDisableIK = 0.01f;
    public float FootDistanceBetweeneachOther;
    public float FootDistanceMulti = 8;
    public float stepHeight;
    public void Initialize()
    {
        Localplayer = BasisLocalPlayer.Instance;
        Localplayer.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);

        Localplayer.LocalBoneDriver.FindBone(out LeftFootSolver.foot, BasisBoneTrackedRole.LeftFoot);
        Localplayer.LocalBoneDriver.FindBone(out RightFootSolver.foot, BasisBoneTrackedRole.RightFoot);

        Localplayer.LocalBoneDriver.FindBone(out LeftFootSolver.lowerLeg, BasisBoneTrackedRole.LeftLowerLeg);
        Localplayer.LocalBoneDriver.FindBone(out RightFootSolver.lowerLeg, BasisBoneTrackedRole.RightLowerLeg);

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
        if(BasisDeviceManagement.Instance.CurrentMode != BasisBootedMode.Desktop)
        {
            return;
        }
        SquareAngular = Localplayer.AvatarDriver.AnimatorDriver.angularVelocity.sqrMagnitude;
        SquareVel = Localplayer.AvatarDriver.AnimatorDriver.currentVelocity.sqrMagnitude;
        Vector3 localTposeHips = Hips.TposeLocal.position;
        Vector3 HipsPosLocal = Hips.OutGoingData.position;

        //  bool LargerthenRotation = SquareAngular <= LargerThenBeforeRotating;
        bool LargerThenMaxBeforeDisableIK = SquareVel <= MaxBeforeDisableIK;

        bool HasLayer = LargerThenMaxBeforeDisableIK;

        RightFootSolver.Simulate(SquareAngular, SquareVel, HasLayer, localTposeHips, HipsPosLocal);
        LeftFootSolver.Simulate(SquareAngular, SquareVel, HasLayer, localTposeHips, HipsPosLocal);
    }
    public void OnCalibration()
    {
        LeftFootSolver.driver = this;
        RightFootSolver.driver = this;

        LeftFootSolver.foot.HasTracked = BasisHasTracked.HasNoTracker;
        RightFootSolver.foot.HasTracked = BasisHasTracked.HasNoTracker;

        LeftFootSolver.ikConstraint = Localplayer.AvatarDriver.LeftFootTwoBoneIK;
        RightFootSolver.ikConstraint = Localplayer.AvatarDriver.RightFootTwoBoneIK;
        Localplayer.AvatarDriver.LeftFootLayer.active = true;
        Localplayer.AvatarDriver.RightFootLayer.active = true;

        LeftFootSolver.Initialize(RightFootSolver);
        RightFootSolver.Initialize(LeftFootSolver);
        stepHeight = DefaultFootOffset * Localplayer.RatioPlayerToAvatarScale;
        FootDistanceBetweeneachOther = Vector3.Distance(LeftFootSolver.foot.TposeLocal.position, RightFootSolver.foot.TposeLocal.position) * (FootDistanceMulti / Localplayer.PlayerEyeHeight);
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
        [SerializeField] public BasisBoneControl lowerLeg;
        [SerializeField] public BasisBoneControl foot;
        [SerializeField] public BasisFootPlacementDriver driver;
        [SerializeField] public TwoBoneIKConstraint ikConstraint;
        [SerializeField] public BasisIKFootSolver otherFoot;

        [SerializeField] public float percentage = 0.25f;
        [SerializeField] public float percentagePastFoot = 4.0f;
        [SerializeField] public float footStepSpeed = 2.5f;
        [SerializeField] public float largerThenThisMoveFeet = 500.0f;
        [SerializeField] public float smallerThenThisAllowVelocity = 0.01f;
        [SerializeField] public float slerpMultiplier = 10.0f;

        public Vector3 topPointLocal;
        public Vector3 bottomPointLocal;
        public Vector3 worldTopPoint;
        public Vector3 worldBottomPoint;
        public float lerp = 0;
        public RaycastHit hitInfo;
        public Quaternion rotation;
        public Vector3 position;

        public Vector3 offset;
        public Vector3 rotatedOffset;
        public Vector3 footExtendedPositionWorld;
        public Vector3 hipHeightFootVector;

        public Vector3 LastFootPosition;
        public Quaternion LastFoorRotation;
        public bool IsFeetFarApart = false;
        public bool HasMotionForMovement = false;
        public bool HasraycastHit = false;
        public float newFootPositionLerpSpeed = 20;
        public void Initialize(BasisIKFootSolver otherFootSolver)
        {
            otherFoot = otherFootSolver;
            lerp = 1;
            OnFootFinishedMoving();
        }

        public bool IsMoving() => lerp < 1;

        public void Simulate(float rotationMagnitude, float velocityMagnitude, bool HasLayerActive, Vector3 localTposeHips, Vector3 hipsPosLocal)
        {
            if (foot.HasTracked == BasisHasTracked.HasTracker)
            {
                return;
            }
            foot.HasRigLayer = HasLayerActive ? BasisHasRigLayer.HasRigLayer : BasisHasRigLayer.HasNoRigLayer;

            CalculateFootPositions(localTposeHips, hipsPosLocal);

            IsFeetFarApart = Vector3.Distance(foot.OutGoingData.position, LastFootPosition) > driver.FootDistanceBetweeneachOther;
            HasMotionForMovement = ShouldStartMoving(rotationMagnitude, velocityMagnitude);
            if (HasMotionForMovement || IsFeetFarApart)
            {
//HasraycastHit = Physics.Linecast(lowerLeg.OutgoingWorldData.position, worldBottomPoint, out hitInfo, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.UseGlobal);
               // if (HasraycastHit)
                {
                    lerp = 0;
                }
            }

            if (lerp <= 1)
            {
                MoveFoot();
            }

            UpdateFootData();
        }
        private void CalculateFootPositions(Vector3 localTposeHips, Vector3 hipsPosLocal)
        {
            offset = foot.TposeLocal.position - localTposeHips;
            rotatedOffset = driver.transform.rotation * driver.Hips.OutgoingWorldData.rotation * offset;
            footExtendedPositionWorld = hipsPosLocal + rotatedOffset;

            hipHeightFootVector = new Vector3(footExtendedPositionWorld.x, driver.Hips.OutgoingWorldData.position.y, footExtendedPositionWorld.z);
            float yDifference = footExtendedPositionWorld.y - hipHeightFootVector.y;

            topPointLocal = Vector3.Lerp(hipHeightFootVector, footExtendedPositionWorld, percentage);
            worldTopPoint = topPointLocal + BasisLocalPlayer.Instance.LocalBoneDriver.transform.position;

            bottomPointLocal = footExtendedPositionWorld + new Vector3(0, yDifference / percentagePastFoot, 0);
            worldBottomPoint = lowerLeg.OutgoingWorldData.position + offset;
        }
        private bool ShouldStartMoving(float rotationMagnitude, float velocityMagnitude)
        {
            return velocityMagnitude < smallerThenThisAllowVelocity && rotationMagnitude > largerThenThisMoveFeet && !otherFoot.IsMoving() && lerp >= 1;
        }
        private void MoveFoot()
        {
            bottomPointLocal.y += Mathf.Sin(lerp * Mathf.PI) * driver.stepHeight;
            lerp += Time.deltaTime * footStepSpeed;
            OnFootFinishedMoving();
        }
        private void UpdateFootData()
        {
            foot.OutGoingData.position = position;
            foot.OutGoingData.rotation = rotation;
            LastFootPosition = position;
            LastFoorRotation = rotation;
        }
        private void OnFootFinishedMoving()
        {
            rotation = Quaternion.Euler(foot.BoneTransform.localEulerAngles.x, driver.Hips.BoneTransform.localEulerAngles.y, foot.BoneTransform.localEulerAngles.z);
            position = Vector3.Lerp(position,bottomPointLocal, newFootPositionLerpSpeed * Time.deltaTime);
        }
        public void Gizmo()
        {
            Gizmos.DrawLine(lowerLeg.OutgoingWorldData.position, worldBottomPoint);
        }
    }
}