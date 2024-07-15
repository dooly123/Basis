using UnityEngine;
using static BaseBoneDriver;
public class BasisCharacterController : MonoBehaviour
{
    public CharacterController characterController;
    public Vector3 bottomPoint;
    public Vector3 LastbottomPoint;
    public bool groundedPlayer;
    [SerializeField] public float RunSpeed = 2f;
    [SerializeField] public float playerSpeed = 1.5f;
    [SerializeField] public float gravityValue = -9.81f;
    [SerializeField] public float RaycastDistance = 0.2f;
    [SerializeField] public float MinimumColliderSize = 0.01f;
    [SerializeField] public Vector2 MovementVector;
    [SerializeField] public bool Running;
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
    public Vector3 OutPosition;
    public Vector3 OutLastPosition;
    public Vector2 Rotation;
    public float RotationSpeed = 200;
    public void OnEnable()
    {
        BasisLocalPlayer.Instance.OnLocalAvatarChanged += Initialize;
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead += Simulate;
        Initialize();
    }
    public void OnDisable()
    {
        BasisLocalPlayer.Instance.OnLocalAvatarChanged -= Initialize;
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead -= Simulate;
    }
    public void Initialize()
    {
        driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        BasisLocalPlayer.Instance.Move = this;
        HasEye = driver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye);
        HasHead = driver.FindBone(out Head, BasisBoneTrackedRole.Head);
        characterController.minMoveDistance = 0;
        characterController.skinWidth = 0.1f;
        currentRotation = Quaternion.identity;
    }
    public void Simulate()
    {
        CalculateCharacterSize();
        HandleMovement();
        GroundCheck();
        OutLastPosition = OutPosition;
        OutPosition = transform.position;

        // Calculate the rotation amount for this frame
        float rotationAmount = Rotation.x * RotationSpeed * Time.deltaTime;

        // Get the current rotation of the player
        Vector3 currentRotation = BasisLocalPlayer.Instance.transform.eulerAngles;

        // Add the rotation amount to the current y-axis rotation and use modulo to keep it within 0-360 degrees
        float newRotationY = (currentRotation.y + rotationAmount) % 360f;

        // Apply the new rotation to the player
        BasisLocalPlayer.Instance.transform.eulerAngles = new Vector3(currentRotation.x, newRotationY, currentRotation.z);

        BasisLocalPlayer.Instance.transform.position = OutPosition;
        ReadyToRead?.Invoke();
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = groundedPlayer ? Color.green : Color.red;
        Gizmos.DrawWireSphere(bottomPoint, characterController.radius);
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
        LastbottomPoint = bottomPoint;
        Vector3 Position = characterController.transform.position;

        float HeightOffset = (characterController.height / 2) - characterController.radius; //+ characterController.skinWidth;
        bottomPoint = Position + (characterController.center - new Vector3(0, HeightOffset, 0));
        groundedPlayer = characterController.isGrounded;
        IsFalling = !groundedPlayer;

        if (groundedPlayer && !LastWasGrounded)
        {
            JustLanded?.Invoke();
            currentVerticalSpeed = 0f; // Reset vertical speed on landing
        }

        LastWasGrounded = groundedPlayer;
    }
    public void HandleMovement()
    {
        if (BlockMovement)
        {
            HasJumpAction = false;
            return;
        }

        // Cache current rotation and zero out x and z components
        currentRotation = Head.CurrentWorldData.rotation;
        Vector3 rotationEulerAngles = currentRotation.eulerAngles;
        rotationEulerAngles.x = 0;
        rotationEulerAngles.z = 0;

        Quaternion flattenedRotation = Quaternion.Euler(rotationEulerAngles);

        // Calculate horizontal movement direction
        Vector3 horizontalMoveDirection = new Vector3(MovementVector.x, 0, MovementVector.y).normalized;
        float speed = Running ? RunSpeed : playerSpeed;
        Vector3 totalMoveDirection = flattenedRotation * horizontalMoveDirection * speed * Time.deltaTime;

        // Handle jumping and falling
        if (groundedPlayer && HasJumpAction)
        {
            currentVerticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            JustJumped?.Invoke();
        }
        else
        {
            currentVerticalSpeed += gravityValue * Time.deltaTime;
        }

        // Ensure we don't exceed maximum gravity value speed
        currentVerticalSpeed = Mathf.Max(currentVerticalSpeed, -Mathf.Abs(gravityValue));


        HasJumpAction = false;
        totalMoveDirection.y = currentVerticalSpeed * Time.deltaTime;

        // Move character
        characterController.Move(totalMoveDirection);
    }

    public void RunningToggle()
    {
        Running = !Running;
    }

    public void CalculateCharacterSize()
    {
        eyeHeight = HasEye ? Eye.FinalApplied.position.y : 1.73f;
        float adjustedHeight = eyeHeight;
        adjustedHeight = Mathf.Max(adjustedHeight, MinimumColliderSize);
        SetCharacterHeight(adjustedHeight);
    }

    public void SetCharacterHeight(float height)
    {
        characterController.height = height;
        float SkinModifiedHeight = height / 2;

        characterController.center = HasEye ? new Vector3(Eye.FinalApplied.position.x, SkinModifiedHeight, Eye.FinalApplied.position.z) : new Vector3(0, SkinModifiedHeight, 0);
    }
}