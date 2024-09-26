using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
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
                Vector3 randomOffset = new Vector3(Random.Range(-MinMaxOffset, MinMaxOffset), Random.Range(-MinMaxOffset, MinMaxOffset), Random.Range(-MinMaxOffset, MinMaxOffset));

                Quaternion randomRotation = Random.rotation;
                Quaternion lerpedRotation = Quaternion.Lerp(FollowMovement.localRotation, randomRotation, LerpAmount * Time.deltaTime);

                Vector3 originalPosition = FollowMovement.localPosition;
                Vector3 newPosition = Vector3.Lerp(originalPosition, originalPosition + randomOffset, LerpAmount * Time.deltaTime);

                FollowMovement.SetLocalPositionAndRotation(newPosition, lerpedRotation);
            }
            FollowMovement.GetLocalPositionAndRotation(out LocalRawPosition, out LocalRawRotation);
            LocalRawPosition /= BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale;

            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale;
            FinalRotation = LocalRawRotation;
            if (hasRoleAssigned)
            {
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;
                    Control.IncomingData.position = FinalPosition - FinalRotation * AvatarPositionOffset;
                }
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    AvatarRotationOffset = Quaternion.Euler(BasisDeviceMatchableNames.AvatarRotationOffset);
                    Control.IncomingData.rotation = FinalRotation * AvatarRotationOffset;
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