using UnityEngine;
using UnityEngine.Events;
using static BaseBoneDriver;

public class BasisCharacterController : MonoBehaviour
{
    public CharacterController characterController;
    public Vector3 bottomPoint;
    public bool groundedPlayer;

    [SerializeField] public float RunSpeed = 2f;
    [SerializeField] public float playerSpeed = 1.5f;
    [SerializeField] public float gravityValue = -9.81f;

    [SerializeField] public float RaycastDistance = 0.2f;

    [SerializeField] public float MinimumColliderSize = 0.01f;

    [SerializeField] public Vector2 MovementVector;
    [SerializeField] public Vector2 Rotation;
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
    public float RotationSpeed = 50;
    public event SimulationHandler ReadyToRead;
    public bool BlockMovement = false;
    public bool IsFalling;
    public bool HasJumpAction = false;
    public float jumpHeight = 1.0f; // Jump height set to 1 meter
    public float currentVerticalSpeed = 0f; // Vertical speed of the character

    public void OnEnable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged += Initialize;
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead += Simulate;
        Initialize();
    }

    public void OnDisable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged -= Initialize;
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead -= Simulate;
    }

    public void Initialize()
    {
        driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        BasisLocalPlayer.Instance.Binder.SetCharacterControllerMove(this);
        BasisLocalPlayer.Instance.Move = this;
        HasEye = driver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye);
        HasHead = driver.FindBone(out Head, BasisBoneTrackedRole.Head);
        characterController.minMoveDistance = 0;
        currentRotation = Quaternion.identity;
    }

    public void Simulate()
    {
        CalculateCharacterSize();
        HandleRotation();
        HandleMovement();
        GroundCheck();
        ReadyToRead?.Invoke();
    }

    public void OnDrawGizmos()
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
        Vector3 rotatedCenter = characterController.transform.rotation * characterController.center;
        bottomPoint = characterController.transform.position + (rotatedCenter - new Vector3(0, characterController.height / 2 - characterController.radius + characterController.skinWidth, 0));
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
            return;
        }

        // Cache current rotation and zero out x and z components
        currentRotation = Head.FinalisedWorldData.rotation;
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

        HasJumpAction = false;
        totalMoveDirection.y = currentVerticalSpeed * Time.deltaTime;

        // Move character
        characterController.Move(totalMoveDirection);
    }
    public void HandleRotation()
    {
        float rotationAmount = Rotation.x * RotationSpeed * Time.deltaTime;
        Vector3 center = transform.position;
        transform.RotateAround(center, Vector3.up, rotationAmount);
    }

    public void RunningToggle()
    {
        Running = !Running;
    }

    public void CalculateCharacterSize()
    {
        eyeHeight = HasEye ? Eye.RawLocalData.position.y : 1.73f;
        float skinWidth = characterController.skinWidth;
        float adjustedHeight = eyeHeight + skinWidth * 2f;
        adjustedHeight = Mathf.Max(adjustedHeight, MinimumColliderSize);
        SetCharacterHeight(adjustedHeight);
    }

    public void SetCharacterHeight(float height)
    {
        characterController.height = height;
        float SkinModifiedHeight = height / 2 - characterController.skinWidth;

        characterController.center = HasHead ? new Vector3(Head.RawLocalData.position.x, SkinModifiedHeight, Head.RawLocalData.position.z): new Vector3(0, SkinModifiedHeight, 0);
    }
}