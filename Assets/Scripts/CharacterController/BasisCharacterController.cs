using UnityEngine;
using UnityEngine.Events;
using static BaseBoneDriver;

public class BasisCharacterController : MonoBehaviour
{
    public CharacterController characterController;
    public Vector3 playerVelocity;
    public Vector3 bottomPoint;
    public bool groundedPlayer;

    [SerializeField] public float RunSpeed = 2f;
    [SerializeField] public float playerSpeed = 1.5f;
    [SerializeField] public float jumpHeight = 1;
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

    public bool IsFalling;
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
    }
    public void Simulate()
    {
        CalculateCharacterSize();
        GroundCheck();
        // Handle jumping
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        HandleRotation();
        HandleMovement();
        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;

        // Apply final movement
        characterController.Move(playerVelocity * Time.deltaTime);
        ReadyToRead?.Invoke();
    }
    public void OnDrawGizmos()
    {
        if (groundedPlayer)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(bottomPoint, characterController.radius);
    }
    public void HandleJump()
    {
        if (groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            JustJumped?.Invoke();
        }
    }

    public void GroundCheck()
    {
        // Get the current rotation of the character controller
        Quaternion rotation = characterController.transform.rotation;

        // Rotate the center point based on the character controller's rotation
        Vector3 rotatedCenter = rotation * characterController.center;

        // Calculate the bottom point using the rotated center point
        bottomPoint = characterController.transform.position + (rotatedCenter - new Vector3(0, characterController.height / 2 - characterController.radius + characterController.skinWidth, 0));

        groundedPlayer = Physics.CheckSphere(bottomPoint, characterController.radius, BasisLocalPlayer.Instance.GroundMask, QueryTriggerInteraction.Ignore);

        if(groundedPlayer == false)
        {
            IsFalling = true;
        }
        else
        {
            IsFalling = false;
        }
        if (groundedPlayer && !LastWasGrounded)
        {
            JustLanded?.Invoke();
        }
        LastWasGrounded = groundedPlayer;
    }
    public void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(MovementVector.x, 0, MovementVector.y).normalized;

        // Get the current rotation of the character controller
        if (HasHead)
        {
            currentRotation = Head.BoneTransform.rotation;
        }
        else
        {
            currentRotation = Quaternion.identity;
        }
        Vector3 Rotation = currentRotation.eulerAngles;
        // Ignore rotation around the x and z axes
        Rotation.x = 0;
        Rotation.z = 0;

        // Rotate the input direction based on the character controller's y-axis rotation
        Vector3 moveDirection = Quaternion.Euler(Rotation) * inputDirection;

        // Move the character controller
        if (Running)
        {
            characterController.Move(RunSpeed * Time.deltaTime * moveDirection);
        }
        else
        {
            characterController.Move(playerSpeed * Time.deltaTime * moveDirection);
        }
    }
    public void HandleRotation()
    {
        float rotationAmount = Rotation.x * RotationSpeed * Time.deltaTime;
        // Get the center position of the GameObject
        Vector3 center = transform.position;
        // Rotate the player GameObject around its center on the Y axis
        transform.RotateAround(center, Vector3.up, rotationAmount);
    }
    public void RunningToggle()
    {
        Running = !Running;
    }
    public void CalculateCharacterSize()
    {
        if (HasEye)
        {
            eyeHeight = Eye.RawLocalData.Position.y;
        }
        else
        {
            eyeHeight = 1.73f;
        }
        float skinWidth = characterController.skinWidth;

        // Adjust the height to account for skinWidth
        float adjustedHeight = eyeHeight + skinWidth * 2f;
        adjustedHeight = Mathf.Max(adjustedHeight, MinimumColliderSize);
        SetCharacterHeight(adjustedHeight);
    }

    public void SetCharacterHeight(float height)
    {
        characterController.height = height;
        float SkinModifiedHeight = height / 2 - characterController.skinWidth;
        // Adjust the center to keep the character on the ground correctly
        if (HasHead)
        {
            characterController.center = new Vector3(Head.RawLocalData.Position.x, SkinModifiedHeight, Head.RawLocalData.Position.z);
        }
        else
        {
            characterController.center = new Vector3(0, SkinModifiedHeight, 0);
        }
    }
}