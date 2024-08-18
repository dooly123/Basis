using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Eye_Follow
{
    using Gizmos = Popcron.Gizmos;
    public abstract class BasisEyeFollowBase : MonoBehaviour
    {
        public Vector3 EyeFowards = new Vector3(0, 0, 1);
        public Transform GeneralEyeTarget;
        public bool Override = false;
        public Transform leftEyeTransform;
        public Transform rightEyeTransform;
        public Quaternion leftEyeInitialRotation;
        public Quaternion rightEyeInitialRotation;
        public BasisBoneControl Eye;
        public float lookSpeed; // Speed of looking
                                // Adjustable parameters
        public float MinlookAroundInterval = 1; // Interval between each look around in seconds
        public float MaxlookAroundInterval = 6;
        public float MaximumLookDistance = 0.25f; // Maximum offset from the target position
        public float minLookSpeed = 0.03f; // Minimum speed of looking
        public float maxLookSpeed = 0.1f; // Maximum speed of looking

        public float CurrentlookAroundInterval;
        public float timer; // Timer to track look around interval
        public Vector3 RandomizedPosition; // Target position to look at
        public Vector3 FowardsLookPoint;
        public Vector3 AppliedOffset;
        public float DistanceBeforeTeleport = 30;
        public bool HasEvents = false;
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
            if (GeneralEyeTarget != null)
            {
                GameObject.Destroy(GeneralEyeTarget.gameObject);
            }
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
            lookSpeed = Random.Range(minLookSpeed, maxLookSpeed);
            if (HasEvents == false)
            {
                BasisLocalPlayer.Instance.OnSpawnedEvent += AfterTeleport;
                HasEvents = true;
            }
            BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye);
            //its regenerated this script will be nuked and rebuilt BasisLocalPlayer.OnLocalAvatarChanged += AfterTeleport;
            if (GeneralEyeTarget == null)
            {
                // GameObject EyeIK = CharacterAvatarDriver.CreateRig("Eye", true, out EyeRig, out EyeLayer);
                GeneralEyeTarget = new GameObject("Eye Target " + CharacterAvatarDriver.Player.DisplayName).transform;
                CharacterAvatarDriver.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                GeneralEyeTarget.SetPositionAndRotation(position, rotation);
                //  CharacterAvatarDriver.MultiRotation(EyeIK, CharacterAvatarDriver.References.LeftEye, GeneralEyeTarget);
                //CharacterAvatarDriver.MultiRotation(EyeIK, CharacterAvatarDriver.References.RightEye, GeneralEyeTarget);
            }

            rightEyeTransform = CharacterAvatarDriver.References.RightEye;
            leftEyeTransform = CharacterAvatarDriver.References.LeftEye;

            if (leftEyeTransform != null)
            {
                leftEyeInitialRotation = leftEyeTransform.localRotation;
            }

            if (rightEyeTransform != null)
            {
                rightEyeInitialRotation = rightEyeTransform.localRotation;
            }
        }

        public void OnRenderObject()
        {
            if (Gizmos.Enabled)
            {
                if (GeneralEyeTarget != null)
                {
                    Gizmos.Sphere(GeneralEyeTarget.position, 0.1f, Color.green);
                }
            }
        }
        public abstract void Simulate();
        public void AfterTeleport()
        {
            Simulate();
            GeneralEyeTarget.position = RandomizedPosition;//will be caught up

        }
    }
}