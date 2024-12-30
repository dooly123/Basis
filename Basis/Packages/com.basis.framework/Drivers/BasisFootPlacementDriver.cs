using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace Basis.Scripts.Drivers
{
    public class BasisFootPlacementDriver : MonoBehaviour
    {
        public BasisLocalPlayer Localplayer;
        public BasisBoneControl Hips;

        [SerializeField] public BasisIKFootSolver leftFootSolver = new BasisIKFootSolver();
        [SerializeField] public BasisIKFootSolver rightFootSolver = new BasisIKFootSolver();

        public bool HasEvents { get; private set; } = false;
        public float DefaultFootOffset = 0.35f;
        public float LargerThenBeforeRotating = 400f;
        public float SquareAngular { get; private set; }
        public float SquareVel { get; private set; }
        public float MaxBeforeDisableIK = 0.01f;
        public float FootDistanceBetweeneachOther { get; private set; }
        public float FootDistanceMulti = 8f;
        public float StepHeight { get; private set; }

        public void Initialize()
        {
            Localplayer = BasisLocalPlayer.Instance;
            Localplayer.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);

            InitializeFootSolvers();

            OnCalibration();

            if (!HasEvents)
            {
                SubscribeEvents();
                HasEvents = true;
            }
        }

        private void InitializeFootSolvers()
        {
            Localplayer.LocalBoneDriver.FindBone(out leftFootSolver.foot, BasisBoneTrackedRole.LeftFoot);
            Localplayer.LocalBoneDriver.FindBone(out rightFootSolver.foot, BasisBoneTrackedRole.RightFoot);

            Localplayer.LocalBoneDriver.FindBone(out leftFootSolver.lowerLeg, BasisBoneTrackedRole.LeftLowerLeg);
            Localplayer.LocalBoneDriver.FindBone(out rightFootSolver.lowerLeg, BasisBoneTrackedRole.RightLowerLeg);
        }

        private void SubscribeEvents()
        {
            Localplayer.AvatarDriver.CalibrationComplete += OnCalibration;
            Localplayer.LocalBoneDriver.OnPostSimulate += Simulate;
            Localplayer.AvatarDriver.TposeStateChange += OnTpose;
        }
        public void OnTpose()
        {

        }
        private void Simulate()
        {
            if (Localplayer.AvatarDriver.CurrentlyTposing == false && Localplayer.AvatarDriver.AnimatorDriver != null)
            {
                UpdateMovementData();

                Vector3 localTposeHips = Hips.TposeLocal.position;
                Vector3 hipsPosLocal = Hips.OutGoingData.position;

                bool hasLayerActive = SquareVel <= MaxBeforeDisableIK;

                float3 HipsEuler = math.Euler(Hips.OutGoingData.rotation);
                HipsEuler = math.degrees(HipsEuler); // Convert to degrees

                leftFootSolver.Simulate(hasLayerActive, localTposeHips, hipsPosLocal, HipsEuler);
                rightFootSolver.Simulate(hasLayerActive, localTposeHips, hipsPosLocal, HipsEuler);
            }
            else
            {

            }
            if (BasisGizmoManager.UseGizmos)
            {
                leftFootSolver.DrawGizmos();
                rightFootSolver.DrawGizmos();
            }
        }

        private void UpdateMovementData()
        {
            SquareAngular = Localplayer.AvatarDriver.AnimatorDriver.angularVelocity.sqrMagnitude;
            SquareVel = Localplayer.AvatarDriver.AnimatorDriver.currentVelocity.sqrMagnitude;
        }

        private void OnCalibration()
        {
            SetupFootSolvers();
            CalculateStepHeight();
            CalculateFootDistance();
        }

        private void SetupFootSolvers()
        {
            leftFootSolver.driver = this;
            rightFootSolver.driver = this;

            leftFootSolver.foot.HasTracked = BasisHasTracked.HasNoTracker;
            rightFootSolver.foot.HasTracked = BasisHasTracked.HasNoTracker;

            leftFootSolver.ikConstraint = Localplayer.AvatarDriver.LeftFootTwoBoneIK;
            rightFootSolver.ikConstraint = Localplayer.AvatarDriver.RightFootTwoBoneIK;

            ActivateFootLayers();

            leftFootSolver.Initialize(rightFootSolver);
            rightFootSolver.Initialize(leftFootSolver);
        }

        private void ActivateFootLayers()
        {
            Localplayer.AvatarDriver.LeftFootLayer.active = true;
            Localplayer.AvatarDriver.RightFootLayer.active = true;
        }

        private void CalculateStepHeight()
        {
            StepHeight = DefaultFootOffset * Localplayer.EyeRatioPlayerToDefaultScale;
        }

        private void CalculateFootDistance()
        {
            FootDistanceBetweeneachOther = Vector3.Distance(leftFootSolver.foot.TposeLocal.position, rightFootSolver.foot.TposeLocal.position) * (FootDistanceMulti / Localplayer.PlayerEyeHeight);
        }

        private void OnDestroy()
        {
            if (HasEvents)
            {
                Localplayer.AvatarDriver.CalibrationComplete -= OnCalibration;
                Localplayer.LocalBoneDriver.OnPostSimulate -= Simulate;
                Localplayer.AvatarDriver.TposeStateChange -= OnTpose;
                HasEvents = false;
            }
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
            public float lerp = 1;
            public RaycastHit hitInfo;
            public Quaternion rotation;
            public Vector3 position;

            public float3 offset;
            public float3 rotatedOffset;
            public Vector3 footExtendedPositionWorld;
            public Vector3 hipHeightFootVector;

            public Vector3 LastFootPosition;
            public Quaternion LastFootRotation;
            public bool IsFeetFarApart = false;
            public bool HasMotionForMovement = false;
            public bool HasRaycastHit = false;
            public float newFootPositionLerpSpeed = 20;
            public float RotationBeforeMove = 15;
            public float RotationDifference;

            public void Initialize(BasisIKFootSolver otherFootSolver)
            {
                otherFoot = otherFootSolver;
                lerp = 1;
                OnFootFinishedMoving();
            }

            public bool IsMoving() => lerp < 1;

            public void Simulate(bool hasLayerActive, Vector3 localTposeHips, Vector3 hipsPosLocal, Vector3 hipsRotation)
            {
                if (foot.HasTracked == BasisHasTracked.HasTracker) return;

                foot.HasRigLayer = hasLayerActive ? BasisHasRigLayer.HasRigLayer : BasisHasRigLayer.HasNoRigLayer;

                CalculateFootPositions(localTposeHips, hipsPosLocal);

                IsFeetFarApart = Vector3.Distance(foot.OutGoingData.position, LastFootPosition) > driver.FootDistanceBetweeneachOther;

                float3 HipsEuler = math.Euler(foot.OutGoingData.rotation);
                HipsEuler = math.degrees(HipsEuler); // Convert to degrees
                RotationDifference = Mathf.Abs(hipsRotation.y - HipsEuler.y);

                if (ShouldMoveFoot())
                {
                    lerp = 0;
                }

                if (lerp <= 1)
                {
                    MoveFoot();
                }

                UpdateFootData();
            }

            private bool ShouldMoveFoot()
            {
                return (IsFeetFarApart || RotationDifference > RotationBeforeMove) && !otherFoot.IsMoving() && lerp >= 1;
            }

            private void CalculateFootPositions(float3 localTposeHips, float3 hipsPosLocal)
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

            private void MoveFoot()
            {
                bottomPointLocal.y += Mathf.Sin(lerp * Mathf.PI) * driver.StepHeight;
                lerp += Time.deltaTime * footStepSpeed;
                OnFootFinishedMoving();
            }

            private void UpdateFootData()
            {
                foot.OutGoingData.position = position;
                foot.OutGoingData.rotation = rotation;
                LastFootPosition = position;
                LastFootRotation = rotation;
            }

            private void OnFootFinishedMoving()
            {
                rotation = Quaternion.Euler(foot.BoneTransform.localEulerAngles.x, driver.Hips.BoneTransform.localEulerAngles.y, foot.BoneTransform.localEulerAngles.z);
                position = Vector3.Lerp(position, bottomPointLocal, newFootPositionLerpSpeed * Time.deltaTime);
            }

            public void DrawGizmos()
            {
              //  BasisGizmoManager.CreateLineGizmo(lowerLeg.OutgoingWorldData.position, worldBottomPoint);
            }
        }
    }
}