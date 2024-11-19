using Basis.Scripts.BasisSdk.Players;
using Unity.Mathematics;
using UnityEngine;
namespace Basis.Scripts.Eye_Follow
{
    public class BasisLocalEyeFollowDriver : BasisEyeFollowBase
    {
        public void LateUpdate()
        {
            Simulate();
        }

        public float3 AppliedOffset;
        public float3 EyeFowards = new float3(0, 0, 1);

        public float CurrentlookAroundInterval;
        public float timer; // Timer to track look-around interval
        public float DistanceBeforeTeleport = 30;
        public override void Simulate()
        {
            if (IsAble())
            {
                if (HasHead)
                {
                    // Update timer using DeltaTime
                    timer += BasisLocalPlayer.Instance.LocalBoneDriver.DeltaTime;

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

                    // Calculate the randomized target position using float3 for optimized math operations
                    float3 targetPosition = float3headPosition + math.mul(QheadRotation, EyeFowards) + AppliedOffset;

                    // Check distance for teleporting, otherwise smooth move
                    if (math.distance(targetPosition, CenterTargetWorld) > DistanceBeforeTeleport)
                    {
                        CenterTargetWorld = targetPosition;
                    }
                    else
                    {
                        CenterTargetWorld = Vector3.MoveTowards(CenterTargetWorld, targetPosition, lookSpeed);
                    }

                    // Set eye rotations using optimized float3 and quaternion operations
                    if (HasLeftEye)
                    {
                        LeftEyeTargetWorld = CenterTargetWorld + LeftEyeInitallocalSpace.position;
                        leftEyeTransform.rotation = LookAtTarget(leftEyeTransform.position, LeftEyeTargetWorld, LeftEyeInitallocalSpace.rotation * Quaternion.Inverse(BasisLocalPlayer.Instance.AvatarDriver.References.head.rotation));
                    }
                    if (HasRightEye)
                    {
                        RightEyeTargetWorld = CenterTargetWorld + RightEyeInitallocalSpace.position;
                        rightEyeTransform.rotation = LookAtTarget(rightEyeTransform.position, RightEyeTargetWorld, RightEyeInitallocalSpace.rotation * Quaternion.Inverse(BasisLocalPlayer.Instance.AvatarDriver.References.head.rotation));
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
        }

        private quaternion LookAtTarget(Vector3 observerPosition, Vector3 targetPosition, Quaternion initialRotation)
        {
            // Calculate direction to target
            Vector3 direction = (targetPosition - observerPosition).normalized;

            // Calculate look rotation
            Quaternion lookRotation = Quaternion.LookRotation(direction, BasisLocalPlayer.Instance.AvatarDriver.References.head.up);

            // Combine with initial rotation for maintained orientation
            return initialRotation * Quaternion.Inverse(initialRotation) * lookRotation;
        }
    }
}