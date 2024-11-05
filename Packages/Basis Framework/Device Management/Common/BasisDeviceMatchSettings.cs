using Basis.Scripts.TransformBinders.BoneControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Basis.Scripts.Device_Management
{
    [Serializable]
    public class BasisDeviceMatchSettings
    {
        [Header("Identification")]
        public string DeviceID = string.Empty;
        public int VersionNumber = 1;

        [Header("Match with Ids")]
        [SerializeField]
        public string[] matchableDeviceIds = Array.Empty<string>();
        public ReadOnlySpan<string> MatchableDeviceIds => matchableDeviceIds;

        public IEnumerable<string> MatchableDeviceIdsLowered()
        {
            foreach (var id in matchableDeviceIds)
            {
                yield return id.ToLower();
            }
        }

        [Header("Raycast Support")]
        public bool HasRayCastSupport = false;

        [Header("Physical Device")]
        public bool CanDisplayPhysicalTracker = false;

        [Header("Raycast Visuals")]
        public bool HasRayCastVisual = false;
        public bool HasRayCastRedical = false;

        [Header("Raycast Offsets")]
        public Vector3 PositionRayCastOffset = Vector3.zero;
        public Vector3 RotationRaycastOffset = Vector3.zero;

        [Header("Avatar Offsets")]
        public Vector3 AvatarPositionOffset = Vector3.zero;
        public Vector3 AvatarRotationOffset = Vector3.zero;

        [Header("Tracked Role Override")]
        public bool HasTrackedRole = false;
        public BasisBoneTrackedRole TrackedRole;

        // Clone method for deep copy
        public BasisDeviceMatchSettings Clone()
        {
            return new BasisDeviceMatchSettings
            {
                DeviceID = this.DeviceID,
                VersionNumber = this.VersionNumber,
                matchableDeviceIds = (string[])this.matchableDeviceIds.Clone(),
                HasRayCastSupport = this.HasRayCastSupport,
                CanDisplayPhysicalTracker = this.CanDisplayPhysicalTracker,
                HasRayCastVisual = this.HasRayCastVisual,
                HasRayCastRedical = this.HasRayCastRedical,
                PositionRayCastOffset = this.PositionRayCastOffset,
                RotationRaycastOffset = this.RotationRaycastOffset,
                AvatarPositionOffset = this.AvatarPositionOffset,
                AvatarRotationOffset = this.AvatarRotationOffset,
                HasTrackedRole = this.HasTrackedRole,
                TrackedRole = this.TrackedRole
            };
        }
    }
}