using System;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Eye_Follow;
using Basis.Scripts.Networking;
using Unity.Mathematics;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [DefaultExecutionOrder(15010)] // Run after BasisEyeFollowBase
    [AddComponentMenu("HVR.Basis/Comms/Eye Tracking Bone Actuation")]
    public class EyeTrackingBoneActuation : MonoBehaviour, ICommsNetworkable
    {
        private const string EyeLeftX = "FT/v2/EyeLeftX";
        private const string EyeRightX = "FT/v2/EyeRightX";
        private const string EyeY = "FT/v2/EyeY";
        private static readonly string[] OurAddresses = { EyeLeftX, EyeRightX, EyeY };
        
        [HideInInspector] [SerializeField] private BasisAvatar avatar;
        [HideInInspector] [SerializeField] private FeatureNetworking featureNetworking;
        [HideInInspector] [SerializeField] private AcquisitionService acquisition;
        [SerializeField] private float multiplyX = 1f;
        [SerializeField] private float multiplyY = 1f;
        
        private float _fEyeLeftX;
        private float _fEyeRightX;
        private float _fEyeY;
        private bool _needsUpdateThisFrame;
        
        private bool _anyAddressUpdated;
        private bool _isWearer;
        private BasisEyeFollowBase _eyeFollowDriverLateInit;
        
#region NetworkingFields
        private int _guidIndex;
        // Can be null due to:
        // - Application with no network, or
        // - Network late initialization.
        // Nullability is needed for local tests without initialization scene.
        // - Becomes non-null after HVRAvatarComms.OnAvatarNetworkReady is successfully invoked
        private FeatureInterpolator _featureInterpolator;

        #endregion

        private void Awake()
        {
            if (avatar == null) avatar = CommsUtil.GetAvatar(this);
            if (featureNetworking == null) featureNetworking = CommsUtil.FeatureNetworkingFromAvatar(avatar);
            if (acquisition == null) acquisition = AcquisitionService.SceneInstance;

            avatar.OnAvatarReady -= OnAvatarReady;
            avatar.OnAvatarReady += OnAvatarReady;

            avatar.OnAvatarNetworkReady -= OnAvatarNetworkReady;
            avatar.OnAvatarNetworkReady += OnAvatarNetworkReady;
        }

        private void OnAvatarNetworkReady()
        {
            if (BasisNetworkManagement.AvatarToPlayer(avatar, out var player) && player is BasisRemotePlayer remote)
            {
                _eyeFollowDriverLateInit = remote.RemoteAvatarDriver.BasisEyeFollowDriver;
            }
        }

        private void OnAvatarReady(bool isWearer)
        {
            _isWearer = isWearer;
            
            if (isWearer)
            {
                acquisition.RegisterAddresses(OurAddresses, OnAddressUpdated);
                _eyeFollowDriverLateInit = BasisLocalPlayer.Instance.AvatarDriver.BasisLocalEyeFollowDriver;
            }
        }

        private void OnEnable()
        {
            SetBuiltInEyeFollowDriverOverriden(true);
        }

        private void OnDisable()
        {
            SetBuiltInEyeFollowDriverOverriden(false);
        }

        private void OnDestroy()
        {
            if (_isWearer)
            {
                acquisition.UnregisterAddresses(OurAddresses, OnAddressUpdated);
            }
        }

        private void OnAddressUpdated(string address, float value)
        {
            // FIXME: Temp fix, we'll need to hook to NetworkReady instead.
            // This is a quick fix so that we don't need to reupload the avatar.
            _anyAddressUpdated = _anyAddressUpdated || value != 0f;
            
            switch (address)
            {
                case EyeLeftX:
                {
                    _fEyeLeftX = value;
                    if (_featureInterpolator != null) _featureInterpolator.Store(0, (value + 1) / 2f);
                    break;
                }
                case EyeRightX:
                {
                    _fEyeRightX = value;
                    if (_featureInterpolator != null) _featureInterpolator.Store(1, (value + 1) / 2f); 
                    break;
                }
                case EyeY:
                {
                    _fEyeY = value;
                    if (_featureInterpolator != null) _featureInterpolator.Store(2, (value + 1) / 2f);
                    break;
                }
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
            if (!_eyeFollowDriverLateInit) return;
            if (_isWearer && !_anyAddressUpdated) return;
            
            // FIXME: Temp fix, we'll need to hook to NetworkReady instead.
            // This is a quick fix so that we don't need to reupload the avatar.
            SetBuiltInEyeFollowDriverOverriden(false);
            SetBuiltInEyeFollowDriverOverriden(true);

            SetEyeRotation(_fEyeLeftX, _fEyeY, EyeSide.Left);
            SetEyeRotation(_fEyeRightX, _fEyeY, EyeSide.Right);
        }

        private void SetEyeRotation(float x, float y, EyeSide side)
        {
            if (!_eyeFollowDriverLateInit) return;
            
            var xDeg = Mathf.Asin(x) * Mathf.Rad2Deg * multiplyX;
            var yDeg = Mathf.Asin(-y) * Mathf.Rad2Deg * multiplyY;
            var euler = Quaternion.Euler(yDeg, xDeg, 0);
            switch (side)
            {
                // FIXME: This wrongly assumes that eye bone transforms are oriented the same.
                // This needs to be fixed later by using the work-in-progress normalized muscle system instead.
                case EyeSide.Left: _eyeFollowDriverLateInit.leftEyeTransform.localRotation = math.mul(_eyeFollowDriverLateInit.leftEyeInitialRotation , euler);
                    break;
                case EyeSide.Right: _eyeFollowDriverLateInit.rightEyeTransform.localRotation = math.mul(_eyeFollowDriverLateInit.rightEyeInitialRotation , euler);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        private void SetBuiltInEyeFollowDriverOverriden(bool value)
        {
            if (!_eyeFollowDriverLateInit) return;
            _eyeFollowDriverLateInit.Override = value;
        }

        private enum EyeSide
        {
            Left, Right
        }

#region NetworkingMethods
        public void OnGuidAssigned(int guidIndex, Guid guid)
        {
            _guidIndex = guidIndex;
            _featureInterpolator = featureNetworking.NewInterpolator(_guidIndex, 3, OnInterpolatedDataChanged);
        }

        private void OnInterpolatedDataChanged(float[] current)
        {
            _fEyeLeftX = current[0] * 2 - 1;
            _fEyeRightX = current[1] * 2 - 1;
            _fEyeY = current[2] * 2 - 1;
        }
#endregion
    }
}