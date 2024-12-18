using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;
using static Basis.Scripts.Drivers.BaseBoneDriver;
namespace Basis.Scripts.BasisCharacterController
{
    public class BasisCharacterController : MonoBehaviour
    {
        public CharacterController characterController;
        public Vector3 bottomPointLocalspace;
        public Vector3 LastbottomPoint;
        public bool groundedPlayer;
        [SerializeField] public float FastestRunSpeed = 4;
        [SerializeField] public float SlowestPlayerSpeed = 0.5f;
        [SerializeField] public float gravityValue = -9.81f;
        [SerializeField] public float RaycastDistance = 0.2f;
        [SerializeField] public float MinimumColliderSize = 0.01f;
        [SerializeField] public Vector2 MovementVector;
        [SerializeField] public BasisLocalBoneDriver driver;
        [SerializeField] public BasisBoneControl Eye;
        [SerializeField] public BasisBoneControl Head;
        public bool HasEye;
        public bool HasHead;
        private Quaternion currentRotation;
        private float eyeHeight;
        public SimulationHandler JustJumped;
        public SimulationHandler JustLanded;
        public bool LastWasGrounded = true;
        public event SimulationHandler ReadyToRead;
        public bool BlockMovement = false;
        public bool IsFalling;
        public bool HasJumpAction = false;
        public float jumpHeight = 1.0f; // Jump height set to 1 meter
        public float currentVerticalSpeed = 0f; // Vertical speed of the character
        public Vector2 Rotation;
        public float RotationSpeed = 200;
        public bool HasEvents = false;
        public float pushPower = 1f;
        private const float SnapTurnAbsoluteThreshold = 0.8f;
        private bool UseSnapTurn => SMModuleControllerSettings.SnapTurnAngle != -1;
        private float SnapTurnAngle => SMModuleControllerSettings.SnapTurnAngle;
        private bool isSnapTurning;

        public void OnDestroy()
        {
            if (HasEvents)
            {
               driver.ReadyToRead.RemoveAction(98,Simulate);
                HasEvents = false;
            }
        }
        public void Initialize()
        {
            driver = BasisLocalPlayer.Instance.LocalBoneDriver;
            BasisLocalPlayer.Instance.Move = this;
            HasEye = driver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye);
            HasHead = driver.FindBone(out Head, BasisBoneTrackedRole.Head);
            characterController.minMoveDistance = 0;
            characterController.skinWidth = 0.01f;
            if (HasEvents == false)
            {
                driver.ReadyToRead.AddAction(98, Simulate);
                HasEvents = true;
            }
        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Check if the hit object has a Rigidbody and if it is not kinematic
            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
            {
                return;
            }

            // Ensure we're only pushing objects in the horizontal plane
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            // Apply the force to the object
            body.AddForce(pushDir * pushPower, ForceMode.Impulse);
        }
        public void Simulate()
        {
            LastbottomPoint = bottomPointLocalspace;
            CalculateCharacterSize();
            HandleMovement();
            GroundCheck();

            // Calculate the rotation amount for this frame
            float rotationAmount;
            if (UseSnapTurn)
            {
                var isAboveThreshold = math.abs(Rotation.x) > SnapTurnAbsoluteThreshold;
                if (isAboveThreshold != isSnapTurning)
                {
                    isSnapTurning = isAboveThreshold;
                    if (isSnapTurning)
                    {
                        rotationAmount = math.sign(Rotation.x) * SnapTurnAngle;
                    }
                    else
                    {
                        rotationAmount = 0f;
                    }
                }
                else
                {
                    rotationAmount = 0f;
                }
            }
            else
            {
                rotationAmount = Rotation.x * RotationSpeed * driver.DeltaTime;
            }


            transform.GetPositionAndRotation(out Vector3 CurrentPosition, out Quaternion CurrentRotation);
            // Get the current rotation and position of the player
            Vector3 pivot = Eye.OutgoingWorldData.position;
            Vector3 upAxis = Vector3.up;

            // Calculate direction from the pivot to the current position
            Vector3 directionToPivot = CurrentPosition - pivot;

            // Calculate rotation quaternion based on the rotation amount and axis
            Quaternion rotation = Quaternion.AngleAxis(rotationAmount, upAxis);

            // Apply rotation to the direction vector
            Vector3 rotatedDirection = rotation * directionToPivot;

            Vector3 FinalRotation = pivot + rotatedDirection;

            transform.SetPositionAndRotation(FinalRotation, rotation * CurrentRotation);

            float HeightOffset = (characterController.height / 2) - characterController.radius;
            bottomPointLocalspace = FinalRotation + (characterController.center - new Vector3(0, HeightOffset, 0));

            ReadyToRead?.Invoke();
        }

        public void HandleJump()
        {
            if (groundedPlayer && !HasJumpAction)
            {
                HasJumpAction = true;
            }
        }
        public void GroundCheck()
        {
            groundedPlayer = characterController.isGrounded;
            IsFalling = !groundedPlayer;

            if (groundedPlayer && !LastWasGrounded)
            {
                JustLanded?.Invoke();
                currentVerticalSpeed = 0f; // Reset vertical speed on landing
            }

            LastWasGrounded = groundedPlayer;
        }
        public float CurrentSpeed;
        public void HandleMovement()
        {
            if (BlockMovement)
            {
                HasJumpAction = false;
                return;
            }

            // Cache current rotation and zero out x and z components
            currentRotation = Head.OutgoingWorldData.rotation;
            Vector3 rotationEulerAngles = currentRotation.eulerAngles;
            rotationEulerAngles.x = 0;
            rotationEulerAngles.z = 0;

            Quaternion flattenedRotation = Quaternion.Euler(rotationEulerAngles);

            // Calculate horizontal movement direction
            Vector3 horizontalMoveDirection = new Vector3(MovementVector.x, 0, MovementVector.y).normalized;

            SpeedMultiplyer = math.abs(SpeedMultiplyer);
            CurrentSpeed = math.lerp(SlowestPlayerSpeed,FastestRunSpeed,SpeedMultiplyer);
            CurrentSpeed = math.clamp(CurrentSpeed, 0, FastestRunSpeed);

            Vector3 totalMoveDirection = flattenedRotation * horizontalMoveDirection * CurrentSpeed * driver.DeltaTime;

            // Handle jumping and falling
            if (groundedPlayer && HasJumpAction)
            {
                currentVerticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
                JustJumped?.Invoke();
            }
            else
            {
                currentVerticalSpeed += gravityValue * driver.DeltaTime;
            }

            // Ensure we don't exceed maximum gravity value speed
            currentVerticalSpeed = Mathf.Max(currentVerticalSpeed, -Mathf.Abs(gravityValue));


            HasJumpAction = false;
            totalMoveDirection.y = currentVerticalSpeed * driver.DeltaTime;

            // Move character
            characterController.Move(totalMoveDirection);
        }
        public float SpeedMultiplyer = 0.5f;

        public void CalculateCharacterSize()
        {
            eyeHeight = HasEye ? Eye.OutGoingData.position.y : 1.73f;
            float adjustedHeight = eyeHeight;
            adjustedHeight = Mathf.Max(adjustedHeight, MinimumColliderSize);
            SetCharacterHeight(adjustedHeight);
        }
        public void SetCharacterHeight(float height)
        {
            characterController.height = height;
            float SkinModifiedHeight = height / 2;

            characterController.center = HasEye ? new Vector3(Eye.OutGoingData.position.x, SkinModifiedHeight, Eye.OutGoingData.position.z) : new Vector3(0, SkinModifiedHeight, 0);
        }
    }
}