using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.Device_Management.Devices.Simulation
{
    public class BasisInputXRSimulate : BasisInput
    {
        public Transform FollowMovement;
        public bool AddSomeRandomizedInput = false;
        public float MinMaxOffset = 0.0001f;
        public float LerpAmount = 0.1f;
        public override void DoPollData()
        {
            if (AddSomeRandomizedInput)
            {
                Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-MinMaxOffset, MinMaxOffset), UnityEngine.Random.Range(-MinMaxOffset, MinMaxOffset), UnityEngine.Random.Range(-MinMaxOffset, MinMaxOffset));

                Quaternion randomRotation = UnityEngine.Random.rotation;
                Quaternion lerpedRotation = Quaternion.Lerp(FollowMovement.localRotation, randomRotation, LerpAmount * Time.deltaTime);

                Vector3 originalPosition = FollowMovement.localPosition;
                Vector3 newPosition = Vector3.Lerp(originalPosition, originalPosition + randomOffset, LerpAmount * Time.deltaTime);

                FollowMovement.SetLocalPositionAndRotation(newPosition, lerpedRotation);
            }
            Quaternion QOut;
            FollowMovement.GetLocalPositionAndRotation(out Vector3 VOut, out QOut);
            LocalRawPosition = VOut;
            LocalRawRotation = QOut;

            LocalRawPosition /= BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale;

            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale;
            FinalRotation = LocalRawRotation;
            if (hasRoleAssigned)
            {
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    // Apply the position offset using math.mul for quaternion-vector multiplication
                    Control.IncomingData.position = FinalPosition - math.mul(FinalRotation, AvatarPositionOffset * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale);
                    Control.IncomingData.rotation = math.mul(FinalRotation, Quaternion.Euler(AvatarRotationOffset));
                }
            }
            UpdatePlayerControl();
        }
        public new void OnDestroy()
        {
            if (FollowMovement != null)
            {
                GameObject.Destroy(FollowMovement.gameObject);
            }
            base.OnDestroy();
        }
    }
}