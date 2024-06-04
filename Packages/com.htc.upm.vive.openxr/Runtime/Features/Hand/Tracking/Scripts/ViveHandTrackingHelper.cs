// Copyright HTC Corporation All Rights Reserved.

using System;

namespace VIVE.OpenXR.Hand
{
    /// <summary>
    /// The XrHandTrackerEXT handle represents the resources for hand tracking of the specific hand.
    /// </summary>
    public struct XrHandTrackerEXT : IEquatable<ulong>
    {
        private readonly ulong value;

        public XrHandTrackerEXT(ulong u)
        {
            value = u;
        }

        public static implicit operator ulong(XrHandTrackerEXT xrInst)
        {
            return xrInst.value;
        }
        public static implicit operator XrHandTrackerEXT(ulong u)
        {
            return new XrHandTrackerEXT(u);
        }

        public bool Equals(XrHandTrackerEXT other)
        {
            return value == other.value;
        }
        public bool Equals(ulong other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrHandTrackerEXT && Equals((XrHandTrackerEXT)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.Equals(b); }
        public static bool operator !=(XrHandTrackerEXT a, XrHandTrackerEXT b) { return !a.Equals(b); }
        public static bool operator >=(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value >= b.value; }
        public static bool operator <=(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value <= b.value; }
        public static bool operator >(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value > b.value; }
        public static bool operator <(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value < b.value; }
        public static XrHandTrackerEXT operator +(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value + b.value; }
        public static XrHandTrackerEXT operator -(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value - b.value; }
        public static XrHandTrackerEXT operator *(XrHandTrackerEXT a, XrHandTrackerEXT b) { return a.value * b.value; }
        public static XrHandTrackerEXT operator /(XrHandTrackerEXT a, XrHandTrackerEXT b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }

    /// <summary>
    /// The XrHandEXT describes which hand the <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> is tracking.
    /// </summary>
    public enum XrHandEXT
    {
        /// <summary>
        /// Specifies the hand tracker will be tracking the user’s left hand.
        /// </summary>
        XR_HAND_LEFT_EXT = 1,
        /// <summary>
        /// Specifies the hand tracker will be tracking the user’s right hand.
        /// </summary>
        XR_HAND_RIGHT_EXT = 2,
        XR_HAND_MAX_ENUM_EXT = 3
    }
    /// <summary>
    /// Defines 26 joints for hand tracking: 4 joints for the thumb finger, 5 joints for the other four fingers, and the wrist and palm of the hands.
    /// </summary>
    public enum XrHandJointEXT
    {
        XR_HAND_JOINT_PALM_EXT = 0,
        XR_HAND_JOINT_WRIST_EXT = 1,
        XR_HAND_JOINT_THUMB_METACARPAL_EXT = 2,
        XR_HAND_JOINT_THUMB_PROXIMAL_EXT = 3,
        XR_HAND_JOINT_THUMB_DISTAL_EXT = 4,
        XR_HAND_JOINT_THUMB_TIP_EXT = 5,
        XR_HAND_JOINT_INDEX_METACARPAL_EXT = 6,
        XR_HAND_JOINT_INDEX_PROXIMAL_EXT = 7,
        XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT = 8,
        XR_HAND_JOINT_INDEX_DISTAL_EXT = 9,
        XR_HAND_JOINT_INDEX_TIP_EXT = 10,
        XR_HAND_JOINT_MIDDLE_METACARPAL_EXT = 11,
        XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT = 12,
        XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT = 13,
        XR_HAND_JOINT_MIDDLE_DISTAL_EXT = 14,
        XR_HAND_JOINT_MIDDLE_TIP_EXT = 15,
        XR_HAND_JOINT_RING_METACARPAL_EXT = 16,
        XR_HAND_JOINT_RING_PROXIMAL_EXT = 17,
        XR_HAND_JOINT_RING_INTERMEDIATE_EXT = 18,
        XR_HAND_JOINT_RING_DISTAL_EXT = 19,
        XR_HAND_JOINT_RING_TIP_EXT = 20,
        XR_HAND_JOINT_LITTLE_METACARPAL_EXT = 21,
        XR_HAND_JOINT_LITTLE_PROXIMAL_EXT = 22,
        XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT = 23,
        XR_HAND_JOINT_LITTLE_DISTAL_EXT = 24,
        XR_HAND_JOINT_LITTLE_TIP_EXT = 25,
        XR_HAND_JOINT_MAX_ENUM_EXT = 26
    }
    /// <summary>
    /// The XrHandJointSetEXT enum describes the set of hand joints to track when creating an <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see>.
    /// </summary>
    public enum XrHandJointSetEXT
    {
        /// <summary>
        /// Indicates that the created <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> tracks the set of hand joints described by <see cref="XrHandJointEXT">XrHandJointEXT</see> enum, i.e. the <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrLocateHandJointsEXT">xrLocateHandJointsEXT</see> function returns an array of joint locations with the count of <see cref="ViveHandTrackingHelper.XR_HAND_JOINT_COUNT_EXT">XR_HAND_JOINT_COUNT_EXT</see> and can be indexed using <see cref="XrHandJointEXT">XrHandJointEXT</see>.
        /// </summary>
        XR_HAND_JOINT_SET_DEFAULT_EXT = 0,
        XR_HAND_JOINT_SET_MAX_ENUM_EXT = 1
    }

    /// <summary>
    /// An application can inspect whether the system is capable of hand tracking input by extending the <see cref="XrSystemProperties">XrSystemProperties</see> with XrSystemHandTrackingPropertiesEXT structure when calling <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>.
    /// </summary>
    public struct XrSystemHandTrackingPropertiesEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// An <see cref="XrBool32">XrBool32</see>, indicating if current system is capable of hand tracking input.
        /// </summary>
        public XrBool32 supportsHandTracking;
    };
    /// <summary>
    /// The XrHandTrackerCreateInfoEXT structure describes the information to create an <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> handle.
    /// </summary>
    public struct XrHandTrackerCreateInfoEXT
    {
        /// <summary>
        /// The XrStructureType of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// An <see cref="XrHandEXT">XrHandEXT</see> which describes which hand the tracker is tracking.
        /// </summary>
        public XrHandEXT hand;
        /// <summary>
        /// An <see cref="XrHandJointSetEXT">XrHandJointSetEXT</see> describe the set of hand joints to retrieve.
        /// </summary>
        public XrHandJointSetEXT handJointSet;
        /// <param name="in_type">The XrStructureType of this structure.</param>
        /// <param name="in_next">NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.</param>
        /// <param name="in_hand">An <see cref="XrHandEXT">XrHandEXT</see> which describes which hand the tracker is tracking.</param>
        /// <param name="in_handJointSet">An <see cref="XrHandJointSetEXT">XrHandJointSetEXT</see> describe the set of hand joints to retrieve.</param>
        public XrHandTrackerCreateInfoEXT(XrStructureType in_type, IntPtr in_next, XrHandEXT in_hand, XrHandJointSetEXT in_handJointSet)
        {
            type = in_type;
            next = in_next;
            hand = in_hand;
            handJointSet = in_handJointSet;
        }
    }
    /// <summary>
    /// The XrHandJointsLocateInfoEXT structure describes the information to locate hand joints.
    /// </summary>
    public struct XrHandJointsLocateInfoEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// An <see cref="XrSpace">XrSpace</see> within which the returned hand joint locations will be represented.
        /// </summary>
        public XrSpace baseSpace;
        /// <summary>
        /// An <see cref="XrTime">XrTime</see> at which to locate the hand joints.
        /// </summary>
        public XrTime time;
        /// <param name="in_type">The <see cref="XrStructureType">XrStructureType</see> of this structure.</param>
        /// <param name="in_next">NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.</param>
        /// <param name="in_baseSpace">An <see cref="XrSpace">XrSpace</see> within which the returned hand joint locations will be represented.</param>
        /// <param name="in_time">An <see cref="XrTime">XrTime</see> at which to locate the hand joints.</param>
        public XrHandJointsLocateInfoEXT(XrStructureType in_type, IntPtr in_next, XrSpace in_baseSpace, XrTime in_time)
        {
            type = in_type;
            next = in_next;
            baseSpace = in_baseSpace;
            time = in_time;
        }
    };
    /// <summary>
    /// XrHandJointLocationEXT structure describes the position, orientation, and radius of a hand joint.
    /// </summary>
    public struct XrHandJointLocationEXT
    {
        /// <summary>
        /// A bitfield, with bit masks defined in <see cref="XrSpaceLocationFlags">XrSpaceLocationFlags</see>, to indicate which members contain valid data. If none of the bits are set, no other fields in this structure should be considered to be valid or meaningful.
        /// </summary>
        public XrSpaceLocationFlags locationFlags;
        /// <summary>
        /// An <see cref="XrPosef">XrPosef</see> defining the position and orientation of the origin of a hand joint within the reference frame of the corresponding <see cref="XrHandJointsLocateInfoEXT.baseSpace">XrHandJointsLocateInfoEXT::baseSpace</see>.
        /// </summary>
        public XrPosef pose;
        /// <summary>
        /// A float value radius of the corresponding joint in units of meters.
        /// </summary>
        public float radius;
    }
    /// <summary>
    /// XrHandJointVelocityEXT structure describes the linear and angular velocity of a hand joint.
    /// </summary>
    public struct XrHandJointVelocityEXT
    {
        /// <summary>
        /// A bitfield, with bit masks defined in <see cref="XrSpaceVelocityFlags">XrSpaceVelocityFlags</see>, to indicate which members contain valid data. If none of the bits are set, no other fields in this structure should be considered to be valid or meaningful.
        /// </summary>
        public XrSpaceVelocityFlags velocityFlags;
        /// <summary>
        /// The relative linear velocity of the hand joint with respect to and expressed in the reference frame of the corresponding <see cref="XrHandJointsLocateInfoEXT.baseSpace">XrHandJointsLocateInfoEXT::baseSpace</see>, in units of meters per second.
        /// </summary>
        public XrVector3f linearVelocity;
        /// <summary>
        /// The relative angular velocity of the hand joint with respect to the corresponding <see cref="XrHandJointsLocateInfoEXT.baseSpace">XrHandJointsLocateInfoEXT::baseSpace</see>. The vector’s direction is expressed in the reference frame of the corresponding <see cref="XrHandJointsLocateInfoEXT.baseSpace">XrHandJointsLocateInfoEXT::baseSpace</see> and is parallel to the rotational axis of the hand joint. The vector’s magnitude is the relative angular speed of the hand joint in radians per second. The vector follows the right-hand rule for torque/rotation.
        /// </summary>
        public XrVector3f angularVelocity;
    }
    /// <summary>
    /// The application can chain an XrHandJointVelocitiesEXT structure to the next pointer of <see cref="XrHandJointLocationsEXT">XrHandJointLocationsEXT</see> when calling <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrLocateHandJointsEXT">xrLocateHandJointsEXT</see> to retrieve the hand joint velocities.
    /// </summary>
    public struct XrHandJointVelocitiesEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// A uint32_t describing the number of elements in jointVelocities array.
        /// </summary>
        UInt32 jointCount;
        /// <summary>
        /// An array of <see cref="XrHandJointVelocityEXT">XrHandJointVelocityEXT</see> receiving the returned hand joint velocities.
        /// </summary>
        IntPtr jointVelocities; //XrHandJointVelocityEXT*
    }
    /// <summary>
    /// XrHandJointLocationsEXT structure returns the state of the hand joint locations.
    /// </summary>
    public struct XrHandJointLocationsEXT
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain, such as <see cref="XrHandJointVelocitiesEXT">XrHandJointVelocitiesEXT</see>.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// An <see cref="XrBool32">XrBool32</see> indicating if the hand tracker is actively tracking.
        /// </summary>
        public XrBool32 isActive;
        /// <summary>
        /// A uint32_t describing the count of elements in jointLocations array.
        /// </summary>
        public UInt32 jointCount;
        /// <summary>
        /// An array of <see cref="XrHandJointLocationEXT">XrHandJointLocationEXT</see> receiving the returned hand joint locations.
        /// </summary>
        public IntPtr jointLocations;  //XrHandJointLocationEXT*
        /// <param name="in_type">The <see cref="XrStructureType">XrStructureType</see> of this structure.</param>
        /// <param name="in_next">NULL or a pointer to the next structure in a structure chain, such as <see cref="XrHandJointVelocitiesEXT">XrHandJointVelocitiesEXT</see>.</param>
        /// <param name="in_isActive">An <see cref="XrBool32">XrBool32</see> indicating if the hand tracker is actively tracking.</param>
        /// <param name="in_jointCount">A uint32_t describing the count of elements in jointLocations array.</param>
        /// <param name="in_jointLocations">An array of <see cref="XrHandJointLocationEXT">XrHandJointLocationEXT</see> receiving the returned hand joint locations.</param>
        public XrHandJointLocationsEXT(XrStructureType in_type, IntPtr in_next, XrBool32 in_isActive, UInt32 in_jointCount, IntPtr in_jointLocations)
        {
            type = in_type;
            next = in_next;
            isActive = in_isActive;
            jointCount = in_jointCount;
            jointLocations = in_jointLocations;
        }
    }

    public static class ViveHandTrackingHelper
    {
        /// <summary>
        /// Defines the number of hand joint enumerants defined in <see cref="XrHandJointEXT">XrHandJointEXT</see>.
        /// </summary>
        public const int XR_HAND_JOINT_COUNT_EXT = 26;

        /// <summary>
        /// The function delegate of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateHandTrackerEXT">xrCreateHandTrackerEXT</see>.
        /// </summary>
        /// <param name="session">An <see cref="XrSession">XrSession</see> in which the hand tracker will be active.</param>
        /// <param name="createInfo">The <see cref="XrHandTrackerCreateInfoEXT">XrHandTrackerCreateInfoEXT</see> used to specify the hand tracker.</param>
        /// <param name="handTracker">The returned <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrCreateHandTrackerEXTDelegate(
            XrSession session,
            ref XrHandTrackerCreateInfoEXT createInfo,
            out XrHandTrackerEXT handTracker);
        /// <summary>
        /// The function delegate of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyHandTrackerEXT">xrDestroyHandTrackerEXT</see>.
        /// </summary>
        /// <param name="handTracker">An <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> previously created by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateHandTrackerEXT">xrCreateHandTrackerEXT</see>.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrDestroyHandTrackerEXTDelegate(
            XrHandTrackerEXT handTracker);
        /// <summary>
        /// The function delegate of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrLocateHandJointsEXT">xrLocateHandJointsEXT</see>.
        /// </summary>
        /// <param name="handTracker">An <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> previously created by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateHandTrackerEXT">xrCreateHandTrackerEXT</see>.</param>
        /// <param name="locateInfo">A pointer to <see cref="XrHandJointsLocateInfoEXT">XrHandJointsLocateInfoEXT</see> describing information to locate hand joints.</param>
        /// <param name="locations">A pointer to <see cref="XrHandJointLocationsEXT">XrHandJointLocationsEXT</see> receiving the returned hand joint locations.</param>
        /// <returns></returns>
        public delegate XrResult xrLocateHandJointsEXTDelegate(
            XrHandTrackerEXT handTracker,
            XrHandJointsLocateInfoEXT locateInfo,
            ref XrHandJointLocationsEXT locations);
    }
}
