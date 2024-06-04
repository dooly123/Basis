// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
    public class SetTrackerRole : MonoBehaviour
    {
        public int TrackerIndex = 0;
        public BodyManager bodyManager = null;

        const int roleNum = 5;
        private readonly TrackedDeviceRole[] s_Roles = new TrackedDeviceRole[roleNum]
        {
            TrackedDeviceRole.ROLE_HIP,
            TrackedDeviceRole.ROLE_LEFTWRIST,
            TrackedDeviceRole.ROLE_RIGHTWRIST,
            TrackedDeviceRole.ROLE_LEFTANKLE,
            TrackedDeviceRole.ROLE_RIGHTANKLE,
        };

		private void Start()
		{
            OnTrackerRoleChanged(TrackerIndex);
        }

		public void OnTrackerRoleChanged(int value)
		{
            if (bodyManager == null || TrackerIndex  < 0 || TrackerIndex >= roleNum || value < 0 || value >= roleNum) { return; }

            Debug.Log("VIVE.OpenXR.Toolkits.BodyTracking.Demo OnTrackerRoleChanged() Set Tracker " + value + " to role " + s_Roles[value].Name());
            bodyManager.SetTrackerRole((uint)TrackerIndex, s_Roles[value]);
		}
    }
}