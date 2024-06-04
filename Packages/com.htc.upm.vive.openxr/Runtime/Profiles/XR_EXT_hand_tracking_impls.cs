// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using System.Collections.Generic;
using UnityEngine;


using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.Hand;

namespace VIVE.OpenXR
{
    public class XR_EXT_hand_tracking_impls : XR_EXT_hand_tracking_defs
    {
        const string LOG_TAG = "VIVE.OpenXR.Android.XR_EXT_hand_tracking_impls";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

        public XR_EXT_hand_tracking_impls() { DEBUG("XR_EXT_hand_tracking_impls()"); }

        private ViveHandTracking feature = null;
        private void ASSERT_FEATURE() {
            if (feature == null) { feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>(); }
        }

        public override XrResult xrCreateHandTrackerEXT(ref XrHandTrackerCreateInfoEXT createInfo, out ulong handTracker)
        {
            DEBUG("xrCreateHandTrackerEXT");
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;
            handTracker = 0;

            ASSERT_FEATURE();
            if (feature)
            {
                XrHandTrackerCreateInfoEXT info = createInfo;
                result = (XrResult)feature.CreateHandTrackerEXT(ref info, out XrHandTrackerEXT tracker);
                if (result == XrResult.XR_SUCCESS) { handTracker = tracker; }
            }

            return result;
        }
        public override XrResult xrDestroyHandTrackerEXT(ulong handTracker)
        {
            DEBUG("xrDestroyHandTrackerEXT");

            ASSERT_FEATURE();
            if (feature) { return (XrResult)feature.DestroyHandTrackerEXT(handTracker); }

            return XrResult.XR_ERROR_VALIDATION_FAILURE;
        }
        public override XrResult xrLocateHandJointsEXT(ulong handTracker, XrHandJointsLocateInfoEXT locateInfo, out XrHandJointLocationsEXT locations)
        {
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;

            InitializeHandJointLocations();
            locations = m_JointLocations;

            ASSERT_FEATURE();
            if (feature)
            {
                XrHandJointLocationsEXT joints = m_JointLocations;
                result = (XrResult)feature.LocateHandJointsEXT(handTracker, locateInfo, ref joints);
                if (result == XrResult.XR_SUCCESS) { locations = joints; }
            }

            return result;
        }

        public override bool GetJointLocations(bool isLeft, out XrHandJointLocationEXT[] handJointLocation)
        {
            ASSERT_FEATURE();
            if (feature)
            {
                if (feature.GetJointLocations(isLeft, out XrHandJointLocationEXT[] array))
                {
                    if (l_HandJointLocation == null) { l_HandJointLocation = new List<XrHandJointLocationEXT>(); }
                    l_HandJointLocation.Clear();
                    for (int i = 0; i < array.Length; i++) { l_HandJointLocation.Add(array[i]); }

                    handJointLocation = l_HandJointLocation.ToArray();
                    return true;
                }
            }

            handJointLocation = s_JointLocation[isLeft];
            return false;
        }
    }
}
