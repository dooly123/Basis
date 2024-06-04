// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//using VIVE.OpenXR.Utils;
using VIVE.OpenXR.Hand;

namespace VIVE.OpenXR
{
    public class XR_EXT_hand_tracking_defs
    {
        public virtual XrResult xrCreateHandTrackerEXT(ref XrHandTrackerCreateInfoEXT createInfo, out ulong handTracker)
        {
            handTracker = 0;
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }
        public virtual XrResult xrDestroyHandTrackerEXT(ulong handTracker)
        {
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }
        protected XrHandJointLocationsEXT m_JointLocations = new XrHandJointLocationsEXT(XrStructureType.XR_TYPE_HAND_JOINT_LOCATIONS_EXT, IntPtr.Zero, 0, 0, IntPtr.Zero);
        protected void InitializeHandJointLocations()
        {
            if (m_JointLocations.jointCount != 0) { return; }

            m_JointLocations.type = XrStructureType.XR_TYPE_HAND_JOINT_LOCATIONS_EXT;
            m_JointLocations.next = IntPtr.Zero;
            m_JointLocations.isActive = 0;
            m_JointLocations.jointCount = (uint)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT;

            XrHandJointLocationEXT joint_location_ext_type = default(XrHandJointLocationEXT);
            m_JointLocations.jointLocations = Marshal.AllocHGlobal(Marshal.SizeOf(joint_location_ext_type) * (int)m_JointLocations.jointCount);
        }
        public virtual XrResult xrLocateHandJointsEXT(ulong handTracker, XrHandJointsLocateInfoEXT locateInfo, out XrHandJointLocationsEXT locations)
        {
            locations = m_JointLocations;
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }

        protected Dictionary<bool, XrHandJointLocationEXT[]> s_JointLocation = new Dictionary<bool, XrHandJointLocationEXT[]>()
        {
            { true, new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT] },
            { false, new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT] }
        };
        protected List<XrHandJointLocationEXT> l_HandJointLocation = new List<XrHandJointLocationEXT>();
        /// <summary>
        /// A convenient function to retrieve the left/right hand joint <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrHandJointLocationEXT">location data</see>.
        /// </summary>
        /// <param name="isLeft">True for left hand.</param>
        /// <param name="handJointLocation">Joint location data in <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrHandJointLocationEXT">XrHandJointLocationEXT</see>.</param>
        /// <returns>True for valid data.</returns>
        public virtual bool GetJointLocations(bool isLeft, out XrHandJointLocationEXT[] handJointLocation)
        {
            handJointLocation = s_JointLocation[isLeft];

            if (m_JointLocations.isActive == 1)
            {
                long offset = 0;
                XrHandJointLocationEXT joint_location_ext_type = default(XrHandJointLocationEXT);

                if (IntPtr.Size == 4)
                    offset = m_JointLocations.jointLocations.ToInt32();
                else
                    offset = m_JointLocations.jointLocations.ToInt64();

                for (int i = 0; i < m_JointLocations.jointCount; i++)
                {
                    IntPtr joint_location_ext_ptr = new IntPtr(offset);
                    s_JointLocation[isLeft][i] = (XrHandJointLocationEXT)Marshal.PtrToStructure(joint_location_ext_ptr, typeof(XrHandJointLocationEXT));
                    offset += Marshal.SizeOf(joint_location_ext_type);
                }

                handJointLocation = s_JointLocation[isLeft];
                return true;
            }

            return false;
        }
    }

    public static class XR_EXT_hand_tracking
    {
        static XR_EXT_hand_tracking_defs m_Instance = null;
        public static XR_EXT_hand_tracking_defs Interop
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new XR_EXT_hand_tracking_impls();
                }
                return m_Instance;
            }
        }

        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateHandTrackerEXT">xrCreateHandTrackerEXT</see>.
        /// </summary>
        /// <param name="createInfo"></param>
        /// <param name="handTracker"></param>
        /// <returns></returns>
        public static XrResult xrCreateHandTrackerEXT(ref XrHandTrackerCreateInfoEXT createInfo, out ulong handTracker)
        {
            return Interop.xrCreateHandTrackerEXT(ref createInfo, out handTracker);
        }
        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyHandTrackerEXT">xrDestroyHandTrackerEXT</see>.
        /// </summary>
        /// <param name="handTracker"></param>
        /// <returns></returns>
        public static XrResult xrDestroyHandTrackerEXT(ulong handTracker)
        {
            return Interop.xrDestroyHandTrackerEXT(handTracker);
        }
        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrLocateHandJointsEXT">xrLocateHandJointsEXT</see>.
        /// </summary>
        /// <param name="handTracker"></param>
        /// <param name="locateInfo"></param>
        /// <param name="locations"></param>
        /// <returns></returns>
        public static XrResult xrLocateHandJointsEXT(ulong handTracker, XrHandJointsLocateInfoEXT locateInfo, out XrHandJointLocationsEXT locations)
        {
            return Interop.xrLocateHandJointsEXT(handTracker, locateInfo, out locations);
        }
    }
}
