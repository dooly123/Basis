using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Drivers;
using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.Eye_Follow
{
    using Gizmos = Popcron.Gizmos;
    public abstract class BasisEyeFollowBase : MonoBehaviour
    {
        public Quaternion leftEyeInitialRotation;
        public Quaternion rightEyeInitialRotation;
        public bool HasEvents = false;
        public bool Override = false;
        public float lookSpeed; // Speed of looking
                                // Adjustable parameters
        public float MinlookAroundInterval = 1; // Interval between each look around in seconds
        public float MaxlookAroundInterval = 6;
        public float MaximumLookDistance = 0.25f; // Maximum offset from the target position
        public float minLookSpeed = 0.03f; // Minimum speed of looking
        public float maxLookSpeed = 0.1f; // Maximum speed of looking
        public Transform leftEyeTransform;
        public Transform rightEyeTransform;
        public Transform HeadTransform;
        public BasisCalibratedCoords LeftEyeInitallocalSpace;
        public BasisCalibratedCoords RightEyeInitallocalSpace;
        public Vector3 RandomizedPosition; // Target position to look at
        public bool IsAble()
        {
            if (Override)
            {
                return false;
            }
            return true;
        }
        public void OnDestroy()
        {
            if (HasEvents)
            {
                BasisLocalPlayer.Instance.OnSpawnedEvent -= AfterTeleport;
                HasEvents = false;
            }
            //its regenerated this script will be nuked and rebuilt BasisLocalPlayer.OnLocalAvatarChanged -= AfterTeleport;
        }
        public void Initalize(BasisAvatarDriver CharacterAvatarDriver)
        {
            // Initialize look speed
            lookSpeed = UnityEngine.Random.Range(minLookSpeed, maxLookSpeed);
            if (HasEvents == false)
            {
                BasisLocalPlayer.Instance.OnSpawnedEvent += AfterTeleport;
                HasEvents = true;
            }
            rightEyeTransform = CharacterAvatarDriver.References.RightEye;
            leftEyeTransform = CharacterAvatarDriver.References.LeftEye;
            HeadTransform = CharacterAvatarDriver.References.head;

            HasLeftEye = CharacterAvatarDriver.References.HasLeftEye;
            HasRightEye = CharacterAvatarDriver.References.HasRightEye;
            HasHead = CharacterAvatarDriver.References.Hashead;

            if (HasLeftEye)
            {
                LeftEyeInitallocalSpace.rotation = leftEyeTransform.rotation;
                LeftEyeInitallocalSpace.position = leftEyeTransform.position - BasisLocalPlayer.Instance.AvatarDriver.References.head.position;

                leftEyeInitialRotation = leftEyeTransform.localRotation;
            }

            if (HasRightEye)
            {
                RightEyeInitallocalSpace.rotation = rightEyeTransform.rotation;
                RightEyeInitallocalSpace.position = rightEyeTransform.position - BasisLocalPlayer.Instance.AvatarDriver.References.head.position;

                rightEyeInitialRotation = rightEyeTransform.localRotation;
            }
        }
        public bool HasLeftEye = false;
        public bool HasRightEye = false;
        public bool HasHead = false;
        public void OnRenderObject()
        {
            if (Gizmos.Enabled)
            {
                Gizmos.Sphere(LeftEyeTargetWorld, 0.1f, Color.cyan);
                Gizmos.Sphere(RightEyeTargetWorld, 0.1f, Color.magenta);
            }
        }
        public Vector3 LeftEyeTargetWorld;
        public Vector3 RightEyeTargetWorld;
        public float3 CenterTargetWorld;
        public abstract void Simulate();
        public void AfterTeleport()
        {
            Simulate();
            CenterTargetWorld = RandomizedPosition;//will be caught up

        }
    }
}