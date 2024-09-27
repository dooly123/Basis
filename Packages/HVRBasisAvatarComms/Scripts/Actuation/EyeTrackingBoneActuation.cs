using System;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Eye_Follow;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Eye Tracking Bone Actuation")]
    public class EyeTrackingBoneActuation : MonoBehaviour
    {
        private const string EyeLeftX = "FT/v2/EyeLeftX";
        private const string EyeRightX = "FT/v2/EyeRightX";
        private const string EyeY = "FT/v2/EyeY";
        private static readonly string[] OurAddresses = { EyeLeftX, EyeRightX, EyeY };
        
        [SerializeField] private AcquisitionService acquisitionService;
        [SerializeField] private float multiplyX = 1f;
        [SerializeField] private float multiplyY = 1f;
        
        private float _fEyeLeftX;
        private float _fEyeRightX;
        private float _fEyeY;
        private bool _needsUpdateThisFrame;

        private void OnEnable()
        {
            // SetBuiltInEyeFollowDriverOverriden(true);
            acquisitionService.RegisterAddresses(OurAddresses, OnAddressUpdated);
        }

        private void OnDisable()
        {
            SetBuiltInEyeFollowDriverOverriden(false);
            acquisitionService.UnregisterAddresses(OurAddresses, OnAddressUpdated);
        }

        private void OnAddressUpdated(string address, float value)
        {
            switch (address)
            {
                case EyeLeftX: _fEyeLeftX = value; break;
                case EyeRightX: _fEyeRightX = value; break;
                case EyeY: _fEyeY = value; break;
            }
        }

        private void Update()
        {
            ForceUpdate();
        }

        private void LateUpdate()
        {
            ForceUpdate();
        }

        private void ForceUpdate()
        {
            SetBuiltInEyeFollowDriverOverriden(true);
            
            // FIXME: Debug a problem with our address registration
            acquisitionService.UnregisterAddresses(OurAddresses, OnAddressUpdated);
            acquisitionService.RegisterAddresses(OurAddresses, OnAddressUpdated);

            SetEyeRotation(_fEyeLeftX, _fEyeY, EyeSide.Left);
            SetEyeRotation(_fEyeRightX, _fEyeY, EyeSide.Right);
        }

        private void SetEyeRotation(float x, float y, EyeSide side)
        {
            var xDeg = Mathf.Asin(x) * Mathf.Rad2Deg * multiplyX;
            var yDeg = Mathf.Asin(-y) * Mathf.Rad2Deg * multiplyY;
            var euler = Quaternion.Euler(yDeg, xDeg, 0);
            switch (side)
            {
                // FIXME: This wrongly assumes that eye bone transforms are oriented the same.
                // This needs to be fixed later by using the work-in-progress normalized muscle system instead.
                case EyeSide.Left: ;EyeFollowDriver.leftEyeTransform.localRotation = EyeFollowDriver.leftEyeInitialRotation * euler;
                    break;
                case EyeSide.Right: ;EyeFollowDriver.rightEyeTransform.localRotation = EyeFollowDriver.rightEyeInitialRotation * euler;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        private static void SetBuiltInEyeFollowDriverOverriden(bool value)
        {
            EyeFollowDriver.Override = value;
        }

        private static BasisLocalEyeFollowDriver EyeFollowDriver => BasisLocalPlayer.Instance.AvatarDriver.BasisLocalEyeFollowDriver;

        private enum EyeSide
        {
            Left, Right
        }
    }
}