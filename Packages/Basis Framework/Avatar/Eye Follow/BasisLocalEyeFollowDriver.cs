using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
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
                        leftEyeTransform.localRotation = LookAtTarget(leftEyeTransform.position, LeftEyeTargetWorld, LeftEyeInitallocalSpace.rotation);
                    }
                    if (HasRightEye)
                    {
                        RightEyeTargetWorld = CenterTargetWorld + RightEyeInitallocalSpace.position;
                        rightEyeTransform.localRotation = LookAtTarget(rightEyeTransform.position, RightEyeTargetWorld, RightEyeInitallocalSpace.rotation);
                    }
                }
            }
        }

        private quaternion LookAtTarget(float3 observerPosition, float3 targetPosition, quaternion initialRotation)
        {
            // Calculate direction to target
            float3 direction = math.normalize(targetPosition - observerPosition);

            // Calculate look rotation
            Quaternion lookRotation = quaternion.LookRotationSafe(direction, math.up());

            // Combine with initial rotation for maintained orientation
            return math.mul(initialRotation, math.inverse(initialRotation) * lookRotation);
        }
    }
}