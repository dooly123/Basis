using UnityEngine;

public class BasisLocalEyeFollowDriver : BasisEyeFollowBase
{
    // Adjustable parameters
    public float MinlookAroundInterval = 1; // Interval between each look around in seconds
    public float MaxlookAroundInterval = 6;
    public float MaximumLookDistance = 0.25f; // Maximum offset from the target position
    public float minLookSpeed = 0.03f; // Minimum speed of looking
    public float maxLookSpeed = 0.1f; // Maximum speed of looking

    private float CurrentlookAroundInterval;
    private float timer; // Timer to track look around interval
    private Vector3 RandomizedPosition; // Target position to look at
    private float lookSpeed; // Speed of looking
    public Vector3 FowardsLookPoint;
    public Vector3 AppliedOffset;

    public void Start()
    {
        // Initialize look speed
        lookSpeed = Random.Range(minLookSpeed, maxLookSpeed);
        BasisLocalPlayer.OnSpawnedEvent += AfterTeleport;
      //  BasisLocalPlayer.OnLocalAvatarChanged += AfterTeleport;
    }
    public new void OnDestroy()
    {
        base.OnDestroy();
        BasisLocalPlayer.OnSpawnedEvent -= AfterTeleport;
       // BasisLocalPlayer.OnLocalAvatarChanged -= AfterTeleport;
    }
    public void LateUpdate()
    {
        if (IsAble())
        {
            Simulate();
        }
    }
    public void Simulate()
    {
        // Update timer
        timer += Time.deltaTime;
        Camera Camera = BasisLocalCameraDriver.Instance.Camera;
        FowardsLookPoint = Camera.transform.position + Camera.transform.rotation * EyeFowards;
        // Check if it's time to look around
        if (timer >= CurrentlookAroundInterval)
        {
            CurrentlookAroundInterval = Random.Range(MinlookAroundInterval, MaxlookAroundInterval);
            AppliedOffset = Random.insideUnitSphere * MaximumLookDistance;

            // Reset timer
            timer = 0f;

            // Randomize look speed
            lookSpeed = Random.Range(minLookSpeed, maxLookSpeed);
        }
        // Randomize target position within maxOffset
        RandomizedPosition = FowardsLookPoint + AppliedOffset;
        // Smoothly interpolate towards the target position with randomized speed
        GeneralEyeTarget.position = Vector3.MoveTowards(GeneralEyeTarget.position, RandomizedPosition, lookSpeed);

        if (leftEyeTransform != null)
        {
            LookAtTarget(leftEyeTransform, leftEyeInitialRotation);
        }
        if (rightEyeTransform != null)
        {
            LookAtTarget(rightEyeTransform, rightEyeInitialRotation);
        }
    }
    public void AfterTeleport()
    {
        Simulate();
        GeneralEyeTarget.position = RandomizedPosition;//will be caught up

    }
    private void LookAtTarget(Transform eyeTransform, Quaternion initialRotation)
    {
        Vector3 directionToTarget = GeneralEyeTarget.position - eyeTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        // Adjust the rotation based on the initial rotation of the eye
        Quaternion finalRotation = targetRotation * Quaternion.Inverse(eyeTransform.parent.rotation) * initialRotation;

        // Ensure we are only rotating the eye around the Y and Z axes.
        Vector3 finalEulerAngles = finalRotation.eulerAngles;
        // finalEulerAngles.x = eyeTransform.localEulerAngles.x;
        // finalEulerAngles.x = ClampAngle(finalEulerAngles.x, -maxHorizontalAngle, maxHorizontalAngle);
        // Clamp the horizontal and vertical angles to the specified ranges
        // finalEulerAngles.y = ClampAngle(finalEulerAngles.y, -maxHorizontalAngle, maxHorizontalAngle);
        // finalEulerAngles.z = ClampAngle(finalEulerAngles.z, -maxVerticalAngle, maxVerticalAngle);

        finalRotation = Quaternion.Euler(finalEulerAngles);
        eyeTransform.localRotation = finalRotation;
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(RandomizedPosition, 0.1f);
        DrawGizmo();
    }
}