using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Drivers;
using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.Eye_Follow
{
    [DefaultExecutionOrder(15002)]
    public class BasisEyeFollowBase : MonoBehaviour
    {
        public quaternion leftEyeInitialRotation;
        public quaternion rightEyeInitialRotation;
        public bool HasEvents = false;
        public bool Override = false;
        public float lookSpeed; // Speed of looking
                                // Adjustable parameters
        public float MinlookAroundInterval = 1; // Interval between each look around in seconds
        public float MaxlookAroundInterval = 6;
        public float MaximumLookDistance = 0.25f; // Maximum offset from the target position
        public float minLookSpeed = 0.03f; // Minimum speed of looking
        public float maxLookSpeed = 0.1f; // Maximum speed of looking
        public bool HasRendererCheckWiredUp = false;
        public BasisPlayer LinkedPlayer;
        public BasisAvatarDriver BasisAvatarDriver;
        public Transform leftEyeTransform;
        public Transform rightEyeTransform;
        public Transform HeadTransform;
        public BasisCalibratedCoords LeftEyeInitallocalSpace;
        public BasisCalibratedCoords RightEyeInitallocalSpace;
        public float3 RandomizedPosition; // Target position to look at

        public bool HasLeftEye = false;
        public bool HasRightEye = false;
        public bool HasHead = false;
        public bool LeftEyeHasGizmo;
        public bool RightEyeHasGizmo;
        public int LeftEyeGizmoIndex;
        public int RightEyeGizmoIndex;
        public float3 LeftEyeTargetWorld;
        public float3 RightEyeTargetWorld;
        public float3 CenterTargetWorld;
        public float3 AppliedOffset;
        public float3 EyeFowards = new float3(0, 0, 1);

        public float CurrentlookAroundInterval;
        public float timer; // Timer to track look-around interval
        public float DistanceBeforeTeleport = 30;
        public void OnDestroy()
        {
            if (HasEvents)
            {
                if (LinkedPlayer.IsLocal)
                {
                    BasisLocalPlayer.Instance.OnSpawnedEvent -= AfterTeleport;
                }
                HasEvents = false;
            }
            BasisGizmoManager.OnUseGizmosChanged -= UpdatGizmoUsage;
            if (HasRendererCheckWiredUp && LinkedPlayer != null && LinkedPlayer.FaceRenderer != null)
            {
                LinkedPlayer.FaceRenderer.Check -= UpdateFaceVisibility;
            }
            //its regenerated this script will be nuked and rebuilt BasisLocalPlayer.OnLocalAvatarChanged -= AfterTeleport;
        }
        public void Initalize(BasisAvatarDriver CAD, BasisPlayer Player)
        {
            BasisAvatarDriver = CAD;
            LinkedPlayer = Player;
            // Initialize look speed
            lookSpeed = UnityEngine.Random.Range(minLookSpeed, maxLookSpeed);
            if (HasEvents == false)
            {
                if (Player.IsLocal)
                {
                    BasisLocalPlayer.Instance.OnSpawnedEvent += AfterTeleport;
                }
                HasEvents = true;
            }
            rightEyeTransform = BasisAvatarDriver.References.RightEye;
            leftEyeTransform = BasisAvatarDriver.References.LeftEye;
            HeadTransform = BasisAvatarDriver.References.head;

            HasLeftEye = BasisAvatarDriver.References.HasLeftEye;
            HasRightEye = BasisAvatarDriver.References.HasRightEye;
            HasHead = BasisAvatarDriver.References.Hashead;
            Vector3 HeadPosition = BasisAvatarDriver.References.head.position;
            if (HasLeftEye)
            {
                LeftEyeInitallocalSpace.rotation = leftEyeTransform.rotation;
                LeftEyeInitallocalSpace.position = leftEyeTransform.position - HeadPosition;

                leftEyeInitialRotation = leftEyeTransform.localRotation;
            }

            if (HasRightEye)
            {
                RightEyeInitallocalSpace.rotation = rightEyeTransform.rotation;
                RightEyeInitallocalSpace.position = rightEyeTransform.position - HeadPosition;

                rightEyeInitialRotation = rightEyeTransform.localRotation;
            }
            BasisGizmoManager.OnUseGizmosChanged += UpdatGizmoUsage;
            if (HasRendererCheckWiredUp == false)
            {
                if (LinkedPlayer != null && LinkedPlayer.FaceRenderer != null)
                {
                    BasisDebug.Log("Wired up Renderer Check For Blinking");
                    LinkedPlayer.FaceRenderer.Check += UpdateFaceVisibility;
                    UpdateFaceVisibility(LinkedPlayer.FaceisVisible);
                    HasRendererCheckWiredUp = true;
                }
            }
        }
        private void UpdateFaceVisibility(bool State)
        {
            enabled = State;
        }
        public void UpdatGizmoUsage(bool State)
        {
            BasisDebug.Log("Running Bone EyeFollow Gizmos");
            if (State)
            {
                if (LeftEyeHasGizmo == false)
                {
                    BasisGizmoManager.CreateSphereGizmo(out LeftEyeGizmoIndex, LeftEyeTargetWorld, 0.1f, Color.cyan);
                    LeftEyeHasGizmo = true;
                }
                if (RightEyeHasGizmo == false)
                {
                    BasisGizmoManager.CreateSphereGizmo(out RightEyeGizmoIndex, RightEyeTargetWorld, 0.1f, Color.magenta);
                    RightEyeHasGizmo = true;
                }
            }
            else
            {
                LeftEyeHasGizmo = false;
                RightEyeHasGizmo = false;
            }
        }
        public void AfterTeleport()
        {
            Simulate();
            CenterTargetWorld = RandomizedPosition;//will be caught up

        }
        public void LateUpdate()
        {
            Simulate();
        }
        public void OnDisable()
        {
            if (HasLeftEye && leftEyeTransform != null)
            {
                leftEyeTransform.rotation = LeftEyeInitallocalSpace.rotation;
            }

            if (HasRightEye && rightEyeTransform != null)
            {
                rightEyeTransform.rotation = RightEyeInitallocalSpace.rotation;
            }
            CenterTargetWorld = RandomizedPosition;
            wasDisabled = true;
        }
        bool wasDisabled = false;
        public void Simulate()
        {
            if (Override == false && HasHead)
            {
                // Update timer using DeltaTime
                timer += Time.deltaTime;

                // Check if it's time to look around
                if (timer > CurrentlookAroundInterval)
                {
                    CurrentlookAroundInterval = UnityEngine.Random.Range(MinlookAroundInterval, MaxlookAroundInterval);
                    AppliedOffset = UnityEngine.Random.insideUnitSphere * MaximumLookDistance;

                    // Reset timer and randomize look speed
                    timer = 0f;
                    lookSpeed = UnityEngine.Random.Range(minLookSpeed, maxLookSpeed);
                }

                HeadTransform.GetPositionAndRotation(out Vector3 headPosition, out Quaternion headRotation);
                float3 float3headPosition = headPosition;
                quaternion QheadRotation = headRotation;
                quaternion InversedHeadRotation = math.inverse(headRotation);
                // Calculate the randomized target position using float3 for optimized math operations
                float3 targetPosition = float3headPosition + math.mul(QheadRotation, EyeFowards) + AppliedOffset;

                // Check distance for teleporting, otherwise smooth move
                if (math.distance(targetPosition, CenterTargetWorld) > DistanceBeforeTeleport || wasDisabled)
                {
                    CenterTargetWorld = targetPosition;
                    wasDisabled = false;
                }
                else
                {
                    CenterTargetWorld = Vector3.MoveTowards(CenterTargetWorld, targetPosition, lookSpeed);
                }
                // Set eye rotations using optimized float3 and quaternion operations
                if (HasLeftEye)
                {
                    LeftEyeTargetWorld = CenterTargetWorld + LeftEyeInitallocalSpace.position;
                    leftEyeTransform.rotation = LookAtTarget(leftEyeTransform.position, LeftEyeTargetWorld, math.mul(LeftEyeInitallocalSpace.rotation, InversedHeadRotation), HeadTransform.up);
                }
                if (HasRightEye)
                {
                    RightEyeTargetWorld = CenterTargetWorld + RightEyeInitallocalSpace.position;
                    rightEyeTransform.rotation = LookAtTarget(rightEyeTransform.position, RightEyeTargetWorld, math.mul(RightEyeInitallocalSpace.rotation, InversedHeadRotation), HeadTransform.up);
                }
                if (BasisGizmoManager.UseGizmos)
                {
                    if (RightEyeHasGizmo)
                    {
                        if (BasisGizmoManager.UpdateSphereGizmo(RightEyeGizmoIndex, RightEyeTargetWorld) == false)
                        {
                            RightEyeHasGizmo = false;
                        }
                    }
                    if (LeftEyeHasGizmo)
                    {
                        if (BasisGizmoManager.UpdateSphereGizmo(LeftEyeGizmoIndex, LeftEyeTargetWorld) == false)
                        {
                            LeftEyeHasGizmo = false;
                        }
                    }
                }
            }
        }
        private quaternion LookAtTarget(Vector3 observerPosition, Vector3 targetPosition, Quaternion initialRotation, Vector3 UP)
        {
            // Calculate direction to target
            float3 direction = (targetPosition - observerPosition).normalized;

            // Calculate look rotation
            quaternion lookRotation = Quaternion.LookRotation(direction, UP);

            // Combine with initial rotation for maintained orientation
            return initialRotation * math.inverse(initialRotation) * lookRotation;
        }
    }
}