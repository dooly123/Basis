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
    public void Initialize()
    {
        Localplayer = BasisLocalPlayer.Instance;
        Localplayer.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);

        Localplayer.LocalBoneDriver.FindBone(out LeftFootSolver.Foot, BasisBoneTrackedRole.LeftFoot);
        Localplayer.LocalBoneDriver.FindBone(out RightFootSolver.Foot, BasisBoneTrackedRole.RightFoot);
        OnCalibration();
        Localplayer.AvatarDriver.CalibrationComplete.AddListener(OnCalibration);
    }
    public void OnCalibration()
    {
        LeftFootSolver.Driver = this;
        RightFootSolver.Driver = this;

        LeftFootSolver.Foot.HasTrackerPositionDriver =  BasisBoneControl.BasisHasTracked.HasNoTracker;
        LeftFootSolver.Foot.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;

        RightFootSolver.Foot.HasTrackerPositionDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        RightFootSolver.Foot.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;

        LeftFootSolver.MyIkConstraint = Localplayer.AvatarDriver.LeftFootTwoBoneIK;
        RightFootSolver.MyIkConstraint = Localplayer.AvatarDriver.RightFootTwoBoneIK;
        Localplayer.AvatarDriver.LeftFootLayer.active = true;
        Localplayer.AvatarDriver.RightFootLayer.active = true;

        LeftFootSolver.Initialize(RightFootSolver);
        RightFootSolver.Initialize(LeftFootSolver);
    }
    void Update()
    {
        LeftFootSolver.Simulate();
        RightFootSolver.Simulate();
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
        public Vector3 Offset;
        private RaycastHit _hitInfo;
        private Vector3 _topPoint;
        private Vector3 _bottomPoint;

        public void Initialize(BasisIKFootSolver otherFoot)
        {
            OtherFoot = otherFoot;
        }

        public void Simulate()
        {
            SimulateTopPoint();
            SimulateBottomPoint();
            if (Physics.Linecast(_topPoint, _bottomPoint, out _hitInfo, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.UseGlobal))
            {
                Vector3 footPosition = _hitInfo.point;
                Foot.BoneTransform.position = footPosition;
                // Calculate the rotation to align the foot with the normal and up direction
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, _hitInfo.normal);
                Foot.BoneTransform.rotation = targetRotation;
            }
            Foot.BoneTransform.eulerAngles = new Vector3(Foot.BoneTransform.eulerAngles.x, Driver.Hips.BoneTransform.eulerAngles.y, Foot.BoneTransform.eulerAngles.z);
        }

        private void SimulateTopPoint()
        {
            _topPoint = Driver.Hips.BoneTransform.rotation * (Driver.Hips.BoneTransform.position + Foot.RestingLocalSpace.BeginningPosition + Offset);
        }

        private void SimulateBottomPoint()
        {
            _bottomPoint = Driver.Hips.BoneTransform.rotation * (Driver.Hips.BoneTransform.position + new Vector3(Foot.RestingLocalSpace.BeginningPosition.x, -Driver.Hips.RestingLocalSpace.BeginningPosition.y, Foot.RestingLocalSpace.BeginningPosition.z) + Offset);
        }

        public void Gizmo()
        {
            Gizmos.DrawLine(_topPoint, _bottomPoint);
        }
    }
}