using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.Hand;


namespace VIVE.OpenXR.Toolkits.CustomGesture
{
    //Straight = 1,
    //Bending = 2,
    //Bended = 3,
    public class CustomGestureManager : MonoBehaviour
    {
        //public CGEnums.HandFlag Hand;
        public FingerStatusDefiner ThumbDefiner;
        public FingerStatusDefiner IndexDefiner;
        public FingerStatusDefiner MiddleDefiner;
        public FingerStatusDefiner RingDefiner;
        public FingerStatusDefiner PinkyDefiner;
        static HandJoint[] LeftHandJoints = NewHandJointArray();
        static HandJoint[] RightHandJoints = NewHandJointArray();
        static int LeftLastCalculationTime = -1;
        static int RightLastCalculationTime = -1;
        static HandJoint[] NewHandJointArray()
        {
            HandJoint[] _Joints = new HandJoint[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            for (int i = 0; i < _Joints.Length; i++)
            {
                _Joints[i] = new HandJoint();
            }
            return _Joints;
        }

        public static HandJoint[] GetHandJointLocations(CGEnums.HandFlag hand)
        {
            Update_HandJoints(hand);
            return hand == CGEnums.HandFlag.Left ? LeftHandJoints : RightHandJoints;
        }

        static void Update_HandJoints(CGEnums.HandFlag hand)
        {
            if ((hand == CGEnums.HandFlag.Left ? LeftLastCalculationTime : RightLastCalculationTime) == Time.frameCount)
            {
                return;
            }

            if (hand == CGEnums.HandFlag.Left)
            {
                LeftLastCalculationTime = Time.frameCount;
            }
            else
            {
                RightLastCalculationTime = Time.frameCount;
            }

            XrHandJointLocationEXT[] jointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
            ViveHandTracking feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            //Debug.Log("CustomGestureManager GetHandJointLocations() feat: " + feature
               //+", fGetJLocLeft: " + feature.GetJointLocations(hand == CGEnums.HandFlag.Left, out jointLocations)
               //+", fGetJLocRight: " + feature.GetJointLocations(hand == CGEnums.HandFlag.Right, out jointLocations));
            if (feature && feature.GetJointLocations(hand == CGEnums.HandFlag.Left, out jointLocations))
            {
                //Debug.Log("CustomGestureManager GetHandJointLocations()!");
                for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                {
                    if (hand == CGEnums.HandFlag.Left)
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
                    //if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
                        //Debug.Log("CustomGestureManager GetHandJointLocations() ORIENTATION_VALID_BIT not 0");
                    //if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0)
                        //Debug.Log("CustomGestureManager GetHandJointLocations() LOCATION_POSITION_VALID_BIT not 0");
                    //if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0)
                        //Debug.Log("CustomGestureManager GetHandJointLocations() POSITION_TRACKED_BIT not 0");
                    //if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                        //Debug.Log("CustomGestureManager GetHandJointLocations() ORIENTATION_TRACKED_BIT not 0");

                    if ((jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0 && (jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0
                        && (jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0 && (jointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0)
                    {
                        (hand == CGEnums.HandFlag.Left ? LeftHandJoints[i] : RightHandJoints[i]).isValid = true;
                        //if (hand == CGEnums.HandFlag.Left) Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to true(If_BIT)! Left");
                        //else Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to true(If_BIT)! Right");
                    }
                    else
                    {
                        (hand == CGEnums.HandFlag.Left ? LeftHandJoints[i] : RightHandJoints[i]).isValid = false;
                        //if (hand == CGEnums.HandFlag.Left) Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to false(If)! Left");
                        //else Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to true(If)! Right");
                    }
                }
            }
            else
            {
                if (hand == CGEnums.HandFlag.Left)
                {
                    for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                    {
                        LeftHandJoints[i].isValid = false;
                    }
                    //Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to false(else) Left!");
                }
                else
                {
                    for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
                    {
                        RightHandJoints[i].isValid = false;
                    }
                    //Debug.Log("CustomGestureManager GetHandJointLocations() set isValid to false(else) Right!");
                }
            }

        }

        public CGEnums.FingerStatus GetFingerStatus(CGEnums.HandFlag _Hand, CGEnums.FingerFlag _Finger)
        {
            switch (_Finger)
            {
                case CGEnums.FingerFlag.Thumb:
                    if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT) == 0.0f &&
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT) == 0.0f)
                    {
                        return CGEnums.FingerStatus.None;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT) > ThumbDefiner.StraightDistalLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT) > ThumbDefiner.StraightProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Straight;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT) < ThumbDefiner.BendingDistalLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT) < ThumbDefiner.BendingProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Bended;
                    }
                    return CGEnums.FingerStatus.Bending;
                case CGEnums.FingerFlag.Index:
                    if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT) == 0.0f &&
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT) == 0.0f &&
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT) == 0.0f)
                    {
                        return CGEnums.FingerStatus.None;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT) > IndexDefiner.StraightDistalLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT) > IndexDefiner.StraightIntermediateLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT) > IndexDefiner.StraightProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Straight;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT) < IndexDefiner.BendingDistalLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT) < IndexDefiner.BendingIntermediateLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT) < IndexDefiner.BendingProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Bended;
                    }
                    return CGEnums.FingerStatus.Bending;
                case CGEnums.FingerFlag.Middle:
                    if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT) > MiddleDefiner.StraightDistalLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT) > MiddleDefiner.StraightIntermediateLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT) > MiddleDefiner.StraightProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Straight;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT) < MiddleDefiner.BendingDistalLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT) < MiddleDefiner.BendingIntermediateLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT) < MiddleDefiner.BendingProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Bended;
                    }
                    return CGEnums.FingerStatus.Bending;
                case CGEnums.FingerFlag.Ring:
                    if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT) > RingDefiner.StraightDistalLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT) > RingDefiner.StraightIntermediateLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT) > RingDefiner.StraightProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Straight;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT) < RingDefiner.BendingDistalLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT) < RingDefiner.BendingIntermediateLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT) < RingDefiner.BendingProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Bended;
                    }
                    return CGEnums.FingerStatus.Bending;
                case CGEnums.FingerFlag.Pinky:
                    if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT) > PinkyDefiner.StraightDistalLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT) > PinkyDefiner.StraightIntermediateLowBound &&
                        GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT) > PinkyDefiner.StraightProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Straight;
                    }
                    else if (GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT) < PinkyDefiner.BendingDistalLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT) < PinkyDefiner.BendingIntermediateLowBound ||
                             GetAngleofHandNode(_Hand, XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT) < PinkyDefiner.BendingProximalLowBound)
                    {
                        return CGEnums.FingerStatus.Bended;
                    }
                    return CGEnums.FingerStatus.Bending;
            }
            return CGEnums.FingerStatus.None;
        }

        static float GetAngleofHandNode(CGEnums.HandFlag _Hand, XrHandJointEXT _HandJoint)
        {
            Vector3 _Bone1Dir = Vector3.zero, _Bone2Dir = Vector3.zero;
            if (_HandJoint != XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT &&
               _HandJoint != XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT)
            {
                Debug.LogError("_HandJoint has to be a joint between two bones on the fingers!");
                return (float)double.NaN;
            }
            HandJoint[] _Joints = GetHandJointLocations(_Hand);
            _Bone1Dir = _Joints[(int)(_HandJoint + 1)].position - _Joints[(int)_HandJoint].position;
            _Bone2Dir = _Joints[(int)(_HandJoint - 1)].position - _Joints[(int)_HandJoint].position;

            return Vector3.Angle(_Bone1Dir, _Bone2Dir);
        }
    }
}
