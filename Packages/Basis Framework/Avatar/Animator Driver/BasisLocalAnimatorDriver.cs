using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.Device_Management.Devices.Desktop;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;
namespace Basis.Scripts.Animator_Driver
{
    public class BasisLocalAnimatorDriver : MonoBehaviour
    {
        [SerializeField]
        private BasisAnimatorVariableApply basisAnimatorVariableApply = new BasisAnimatorVariableApply();
        [SerializeField]
        private Animator animator;
        public float LargerThenVelocityCheck = 0.01f;
        public float LargerThenVelocityCheckRotation = 0.03f;
        private BasisLocalPlayer localPlayer;
        public float ScaleMovementBy = 1;
        public float dampeningFactor = 30; // Adjust this value to control the dampening effect
        private Vector3 previousRawVelocity = Vector3.zero;
        private Vector3 previousAngularVelocity = Vector3.zero; // New field for previous angular velocity
        private Quaternion previousHipsRotation;
        public BasisCharacterController.BasisCharacterController Controller;
        public BasisBoneControl Hips;
        public Vector3 currentVelocity;
        public Vector3 dampenedVelocity;
        public Vector3 angularVelocity;
        public Vector3 dampenedAngularVelocity; // New field for dampened angular velocity
        public Quaternion deltaRotation;
        public bool HasEvents = false;
        public BasisInput HipsInput;
        public bool HasHipsInput = false;
        void Simulate()
        {
            if (localPlayer.AvatarDriver.InTPose)
            {
                return;
            }
            float DeltaTime = Time.deltaTime;
            // Calculate the velocity of the character controller taking into account the direction occurring to the hips
            currentVelocity = Quaternion.Inverse(Hips.OutgoingWorldData.rotation) * (Controller.bottomPoint - Controller.LastbottomPoint) / DeltaTime;

            // Apply dampening to the velocity
            dampenedVelocity = Vector3.Lerp(previousRawVelocity, currentVelocity, DeltaTime * dampeningFactor);
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

            // Calculate the angular velocity of the hips
            deltaRotation = Hips.OutgoingWorldData.rotation * Quaternion.Inverse(previousHipsRotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            angularVelocity = axis * angle / DeltaTime;

            // Apply dampening to the angular velocity
            dampenedAngularVelocity = Vector3.Lerp(previousAngularVelocity, angularVelocity, dampeningFactor);


            basisAnimatorVariableApply.BasisAnimatorVariables.AngularVelocity = dampenedAngularVelocity;
            /*
            if (basisAnimatorVariableApply.BasisAnimatorVariables.isMoving == false)
            {
                basisAnimatorVariableApply.BasisAnimatorVariables.isMoving = angularVelocity.sqrMagnitude > LargerThenVelocityCheckRotation;
                basisAnimatorVariableApply.BasisAnimatorVariables.Velocity = dampenedAngularVelocity; // Update to use dampened angular velocity
            }
            */

            basisAnimatorVariableApply.UpdateAnimator(ScaleMovementBy);

            if (basisAnimatorVariableApply.BasisAnimatorVariables.IsFalling)
            {
                basisAnimatorVariableApply.BasisAnimatorVariables.IsJumping = false;
            }
            // Update the previous velocities and rotations for the next frame
            previousRawVelocity = dampenedVelocity;
            previousAngularVelocity = dampenedAngularVelocity;
            previousHipsRotation = Hips.OutgoingWorldData.rotation;
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
            if (HasEvents == false)
            {
                BasisDeviceManagement.Instance.AllInputDevices.OnListChanged += AssignHipsFBTracker;
                HasEvents = true;
            }
            AssignHipsFBTracker();
        }
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

        public void HandleTeleport()
        {
            currentVelocity = Vector3.zero;
            dampenedVelocity = Vector3.zero;
            previousAngularVelocity = Vector3.zero; // Reset angular velocity dampening on teleport
        }

        private void OnDestroy()
        {
            if (localPlayer != null)
            {
                localPlayer.Move.ReadyToRead -= Simulate;
                localPlayer.Move.JustJumped -= JustJumped;
                localPlayer.Move.JustLanded -= JustLanded;
            }
            if (HasEvents)
            {
                BasisDeviceManagement.Instance.AllInputDevices.OnListChanged -= AssignHipsFBTracker;
            }
        }
    }
}