using Basis.Scripts.Drivers;
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
        public bool IsAble()
        {
            if (Override)
            {
                return false;
            }
            if (BasisLocalCameraDriver.Instance != null)
            {
                return true;
            }
            return false;
        }
        public void OnDestroy()
        {
            if (GeneralEyeTarget != null)
            {
                GameObject.Destroy(GeneralEyeTarget.gameObject);
            }
        }
        public void CreateEyeLook(BasisAvatarDriver CharacterAvatarDriver)
        {
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
            if (GeneralEyeTarget != null)
            {
                Gizmos.Sphere(GeneralEyeTarget.position, 0.1f, Color.green);
            }
        }
    }
}