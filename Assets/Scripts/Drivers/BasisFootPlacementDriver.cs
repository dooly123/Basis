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
            HasEvents = true;
        }
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
        public float PercentagePastFoot = 5;
        private RaycastHit _hitInfo;
        private Vector3 _topPoint;
        private Vector3 _bottomPoint;
        public bool HasEvent = false;
        public void Initialize(BasisIKFootSolver otherFoot)
        {
            OtherFoot = otherFoot;
            if (HasEvent == false)
            {
                BasisLocalPlayer.Instance.LocalBoneDriver.OnPostSimulate += Simulate;
                HasEvent = true;
            }
        }

        public void Simulate()
        {
            Vector3 LocalTposeFoot = Foot.TposeLocal.position;
         //   Vector3 BodyPose = BasisLocalPlayer.Instance.Avatar.Animator.transform.position + LocalTposeFoot;

            Vector3 HipHeightFootVector = new Vector3(LocalTposeFoot.x, Driver.Hips.CurrentWorldData.position.y, LocalTposeFoot.z);
            _topPoint = Vector3.Lerp(HipHeightFootVector, LocalTposeFoot, percentage);

            float y = LocalTposeFoot.y - HipHeightFootVector.y;

            _bottomPoint = LocalTposeFoot + new Vector3(0, (y / PercentagePastFoot), 0);

            if (Physics.Linecast(_topPoint, _bottomPoint, out _hitInfo, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.UseGlobal))
            {
                Vector3 footPosition = _hitInfo.point;
                Foot.FinalApplied.position = footPosition;
                // Calculate the rotation to align the foot with the normal and up direction
                Foot.FinalApplied.rotation = Quaternion.LookRotation(Driver.Hips.BoneTransform.forward, _hitInfo.normal);
            }
            Foot.FinalApplied.rotation.eulerAngles = new Vector3(Foot.BoneTransform.localEulerAngles.x, Driver.Hips.BoneTransform.localEulerAngles.y, Foot.BoneTransform.localEulerAngles.z);
        }
        public void Gizmo()
        {
            Gizmos.DrawLine(_topPoint, _bottomPoint);
        }
    }
}