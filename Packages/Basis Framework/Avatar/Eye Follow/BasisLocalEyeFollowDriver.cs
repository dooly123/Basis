using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace Basis.Scripts.Eye_Follow
{
    public class BasisLocalEyeFollowDriver : BasisEyeFollowBase
    {
        public void LateUpdate()
        {
            if (IsAble())
            {
                Simulate();
            }
        }
        public override void Simulate()
        {
            if (Eye.HasBone)
            {
                // Update timer
                timer += Time.deltaTime;
                FowardsLookPoint = Eye.OutgoingWorldData.position + Eye.OutgoingWorldData.rotation * EyeFowards;
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

                if (Vector3.Distance(RandomizedPosition, GeneralEyeTarget.position) > DistanceBeforeTeleport)
                {
                    GeneralEyeTarget.position = RandomizedPosition;
                }
                else
                {
                    GeneralEyeTarget.position = Vector3.MoveTowards(GeneralEyeTarget.position, RandomizedPosition, lookSpeed);
                }
                if (leftEyeTransform != null)
                {
                    LookAtTarget(leftEyeTransform, leftEyeInitialRotation);
                }
                if (rightEyeTransform != null)
                {
                    LookAtTarget(rightEyeTransform, rightEyeInitialRotation);
                }
            }
        }
        private void LookAtTarget(Transform eyeTransform, Quaternion initialRotation)
        {
            Vector3 directionToTarget = GeneralEyeTarget.position - eyeTransform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

            // Adjust the rotation based on the initial rotation of the eye
            Quaternion finalRotation = targetRotation * Quaternion.Inverse(eyeTransform.parent.rotation) * initialRotation;

            // Ensure we are only rotating the eye around the Y and Z axes.
            Vector3 finalEulerAngles = finalRotation.eulerAngles;
            finalRotation = Quaternion.Euler(finalEulerAngles);
            eyeTransform.localRotation = finalRotation;
        }
        public new void OnRenderObject()
        {
            if (Gizmos.Enabled)
            {
                Gizmos.Sphere(RandomizedPosition, 0.1f, Color.yellow);
                base.OnRenderObject();
            }
        }
    }
}