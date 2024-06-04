// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.Hand;

namespace VIVE.OpenXR.Samples
{
    /// <summary>
    /// Hold the data of a tracking hand joint
    /// </summary>
    public class HandJoint
    {
        /// <summary>
        /// Tells whether the data of this <see cref = "HandJoint">HandJoints</see> is valid or not; the data shouldn't be used if <c>isValid</c> returns false
        /// </summary>
        public bool isValid;
        /// <summary>
        /// Holds the position of the <see cref = "HandJoint">HandJoints</see>
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Holds the rotation of the <see cref = "HandJoint">HandJoints</see>
        /// </summary>
        public Quaternion rotation;
        public HandJoint()
        {
            isValid = false;
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }
    }

    public class HandTracking
    {
        static HandJoint[] LeftHandJoints = NewHandJointArray();
        static HandJoint[] RightHandJoints = NewHandJointArray();
        static HandJoint[] NewHandJointArray()
        {
            HandJoint[] _Joints = new HandJoint[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            for(int i = 0; i < _Joints.Length; i++)
            {
                _Joints[i] = new HandJoint();
            }
            return _Joints;
        }

        static int LeftLastCalculationTime = -1;
        static int RightLastCalculationTime = -1;

        /// <summary>
        /// Return the array of HandJoints, depending on the HandFlag <c>hand</c>
        /// </summary>
        /// <param name="hand">Which hand should <see cref = "GetHandJointLocations">GetHandJointLocations</see> return?</param>
        /// <returns>The array holding <see cref = "HandJoint">HandJoints</see></returns>
        public static HandJoint[] GetHandJointLocations(HandFlag hand)
        {
            Update_HandJoints(hand);
            return hand == HandFlag.Left ? LeftHandJoints : RightHandJoints;
        }

        static void Update_HandJoints(HandFlag handFlag)
        {
            if ((handFlag == HandFlag.Left ? LeftLastCalculationTime : RightLastCalculationTime) == Time.frameCount)
            {
                return;
            }

            if(handFlag == HandFlag.Left)
            {
                LeftLastCalculationTime = Time.frameCount;
            }
            else
            {
                RightLastCalculationTime = Time.frameCount;
            }

            XrHandJointLocationEXT[] jointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            ViveHandTracking feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();

            if (feature && feature.GetJointLocations(handFlag == HandFlag.Left, out jointLocations))
            {
                for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                {
                    if (handFlag == HandFlag.Left)
                    {
                        LeftHandJoints[i].position.x = jointLocations[i].pose.position.x;
                        LeftHandJoints[i].position.y = jointLocations[i].pose.position.y;
                        LeftHandJoints[i].position.z = -jointLocations[i].pose.position.z;
                        LeftHandJoints[i].rotation.x = jointLocations[i].pose.orientation.x;
                        LeftHandJoints[i].rotation.y = jointLocations[i].pose.orientation.y;
                        LeftHandJoints[i].rotation.z = -jointLocations[i].pose.orientation.z;
                        LeftHandJoints[i].rotation.w = -jointLocations[i].pose.orientation.w;
                    }
                    else
                    {
                        RightHandJoints[i].position.x = jointLocations[i].pose.position.x;
                        RightHandJoints[i].position.y = jointLocations[i].pose.position.y;
                        RightHandJoints[i].position.z = -jointLocations[i].pose.position.z;
                        RightHandJoints[i].rotation.x = jointLocations[i].pose.orientation.x;
                        RightHandJoints[i].rotation.y = jointLocations[i].pose.orientation.y;
                        RightHandJoints[i].rotation.z = -jointLocations[i].pose.orientation.z;
                        RightHandJoints[i].rotation.w = -jointLocations[i].pose.orientation.w;
                    }
                    if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0 && (jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                    {
                        (handFlag == HandFlag.Left ? LeftHandJoints[i] : RightHandJoints[i]).isValid = true;
                    }
                    else
                    {
                        (handFlag == HandFlag.Left ? LeftHandJoints[i] : RightHandJoints[i]).isValid = false;
                    }
                }
            }
            else
            {
                if (handFlag == HandFlag.Left)
                {
                    for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                    {
                        LeftHandJoints[i].isValid = false;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                    {
                        RightHandJoints[i].isValid = false;
                    }
                }
            }
        }
        /*
        public static Vector3?[] Get_HandJointPositions(HandFlag handFlag, bool _Allow_UnpreciseData)
        {
            return handFlag == HandFlag.Left ? Get_LeftHandJointPositions(_Allow_UnpreciseData) : Get_RightHandJointPositions(_Allow_UnpreciseData);
        }

        static Vector3?[] Get_LeftHandJointPositions(bool _Allow_UnpreciseData)
        {
            XrHandJointLocationEXT[] _HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            Vector3?[] _Positions = new Vector3?[_HandjointLocations.Length];
            if (feature && feature.GetJointLocations(true, out _HandjointLocations))
            {
                if (!_Allow_UnpreciseData)
                {
                    for (int i = 0; i < _Positions.Length; i++)
                    {
                        if ((_HandjointLocations[i].locationFlags & OpenXRHelper.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                        {
                            _Positions[i] = new Vector3(_HandjointLocations[i].pose.position.x, _HandjointLocations[i].pose.position.y, -_HandjointLocations[i].pose.position.z);
                        }
                        else
                        {
                            _Positions[i] = null;
                        }
                    }
                    return _Positions;
                }
                else
                {
                    for (int i = 0; i < _Positions.Length; i++)
                    {
                        _Positions[i] = new Vector3(_HandjointLocations[i].pose.position.x, _HandjointLocations[i].pose.position.y, -_HandjointLocations[i].pose.position.z);
                    }
                    return _Positions;
                }
            }
            for (int i = 0; i < _Positions.Length; i++)
            {
                _Positions[i] = null;
            }
            return _Positions;
        }

        static Vector3?[] Get_RightHandJointPositions(bool _Allow_UnpreciseData)
        {
            XrHandJointLocationEXT[] _HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            Vector3?[] _Positions = new Vector3?[_HandjointLocations.Length];
            if (feature && feature.GetJointLocations(false, out _HandjointLocations))
            {
                if (!_Allow_UnpreciseData)
                {
                    for (int i = 0; i < _Positions.Length; i++)
                    {
                        if ((_HandjointLocations[i].locationFlags & OpenXRHelper.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                        {
                            _Positions[i] = new Vector3(_HandjointLocations[i].pose.position.x, _HandjointLocations[i].pose.position.y, -_HandjointLocations[i].pose.position.z);
                        }
                        else
                        {
                            _Positions = null;
                        }
                    }
                    return _Positions;
                }
                else
                {
                    for (int i = 0; i < _Positions.Length; i++)
                    {
                        _Positions[i] = new Vector3(_HandjointLocations[i].pose.position.x, _HandjointLocations[i].pose.position.y, -_HandjointLocations[i].pose.position.z);
                    }
                    return _Positions;
                }
            }
            return null;
        }

        public static Quaternion[] Get_HandJointRotations(HandFlag handFlag, bool _Allow_UnpreciseData)
        {
            return handFlag == HandFlag.Left ? Get_LeftHandJointRotations(_Allow_UnpreciseData) : Get_RightHandJointRotations(_Allow_UnpreciseData);
        }

        static Quaternion[] Get_LeftHandJointRotations(bool _Allow_UnpreciseData)
        {
            XrHandJointLocationEXT[] _HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            if (feature && feature.GetJointLocations(true, out _HandjointLocations))
            {
                Quaternion[] _Rotations = new Quaternion[_HandjointLocations.Length];
                if (!_Allow_UnpreciseData)
                {
                    for (int i = 0; i < _Rotations.Length; i++)
                    {
                        if ((_HandjointLocations[i].locationFlags & OpenXRHelper.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                        {
                            _Rotations[i] = new Quaternion(_HandjointLocations[i].pose.orientation.x, _HandjointLocations[i].pose.orientation.y, -_HandjointLocations[i].pose.orientation.z, -_HandjointLocations[i].pose.orientation.w);
                        }
                        else
                        {
                            _Rotations = null;
                        }
                    }
                    return _Rotations;
                }
                else
                {
                    for (int i = 0; i < _Rotations.Length; i++)
                    {
                        _Rotations[i] = new Quaternion(_HandjointLocations[i].pose.orientation.x, _HandjointLocations[i].pose.orientation.y, -_HandjointLocations[i].pose.orientation.z, -_HandjointLocations[i].pose.orientation.w);
                    }
                    return _Rotations;
                }
            }
            return null;
        }

        static Quaternion[] Get_RightHandJointRotations(bool _Allow_UnpreciseData)
        {
            XrHandJointLocationEXT[] _HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            if (feature && feature.GetJointLocations(false, out _HandjointLocations))
            {
                Quaternion[] _Rotations = new Quaternion[_HandjointLocations.Length];
                if (!_Allow_UnpreciseData)
                {
                    for (int i = 0; i < _Rotations.Length; i++)
                    {
                        if ((_HandjointLocations[i].locationFlags & OpenXRHelper.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                        {
                            _Rotations[i] = new Quaternion(_HandjointLocations[i].pose.orientation.x, _HandjointLocations[i].pose.orientation.y, -_HandjointLocations[i].pose.orientation.z, -_HandjointLocations[i].pose.orientation.w);
                        }
                        else
                        {
                            _Rotations = null;
                        }
                    }
                    return _Rotations;
                }
                else
                {
                    for (int i = 0; i < _Rotations.Length; i++)
                    {
                        _Rotations[i] = new Quaternion(_HandjointLocations[i].pose.orientation.x, _HandjointLocations[i].pose.orientation.y, -_HandjointLocations[i].pose.orientation.z, -_HandjointLocations[i].pose.orientation.w);
                    }
                    return _Rotations;
                }
            }
            return null;
        }
        */
    }
}
