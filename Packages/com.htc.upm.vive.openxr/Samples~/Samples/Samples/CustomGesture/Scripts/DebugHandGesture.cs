using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.Hand;
using VIVE.OpenXR.Toolkits.CustomGesture;

public class DebugHandGesture : MonoBehaviour
{
    public Text[] LFingers;
    public Text[] RFingers;
    public Text CurrentGestureL;
    public Text CurrentGestureR;
    public Text CurrentGestureDual;
    CustomGestureManager HGM;
    CustomGestureDefiner GD;
    void Start()
    {
        GD = GetComponent<CustomGestureDefiner>();
        HGM = GetComponent<CustomGestureManager>();
    }

    void Update()
    {
        UpdateFingerStatus(CGEnums.HandFlag.Left); //get real left hand finger status
        UpdateFingerStatus(CGEnums.HandFlag.Right); //get real right hand finger status
        ShowCurrentGesture();
    }

    void ShowCurrentGesture()
    {
        CurrentGestureL.text = "LGesture: " + "No Gesture";
        CurrentGestureR.text = "RGesture: " + "No Gesture";
        CurrentGestureDual.text = "DualGesture: " + "No Gesture";

        if (!IsGestureReady())
            return;

        foreach (CustomGesture _Gestures in GD.DefinedGestures)
        {
            switch (_Gestures.TargetHand)
            {
                case CGEnums.HandFlag.Either:
                    //check left hand gesture
                    if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Left) && CheckHandValid(CGEnums.HandFlag.Left))
                        CurrentGestureL.text = "LGesture: " + _Gestures.GestureName;
                    //check right hand gesture
                    if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Right) && CheckHandValid(CGEnums.HandFlag.Right))
                        CurrentGestureR.text = "RGesture: " + _Gestures.GestureName;
                    //Debug.Log("DebugHandGesture ShowCurrentGesture()  " + _Gestures.GestureName);
                    break;
                case CGEnums.HandFlag.Dual:
                    if (CustomGestureDefiner.IsCurrentGestureTriiggered(_Gestures.GestureName, CGEnums.HandFlag.Dual) && CheckHandValid(CGEnums.HandFlag.Left) && CheckHandValid(CGEnums.HandFlag.Right))
                    {
                        CurrentGestureDual.text = "DualGesture: " + _Gestures.GestureName;
                    }
                    break;
                default:
                    CurrentGestureL.text = "LGesture: " + "No Gesture";
                    CurrentGestureR.text = "RGesture: " + "No Gesture";
                    CurrentGestureDual.text = "DualGesture: " + "No Gesture";
                    break;
            }
        }

    }

    void UpdateFingerStatus(CGEnums.HandFlag _Hand)
    {
        Text[] _Fingers = (_Hand == CGEnums.HandFlag.Left) ? LFingers : RFingers;
        _Fingers[0].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Thumb).ToString();
        _Fingers[1].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Index).ToString();
        _Fingers[2].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Middle).ToString();
        _Fingers[3].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Ring).ToString();
        _Fingers[4].text = HGM.GetFingerStatus(_Hand, CGEnums.FingerFlag.Pinky).ToString();
    }


    bool CheckHandValid(CGEnums.HandFlag _Hand) {
        HandJoint[] _Joints = CustomGestureManager.GetHandJointLocations(_Hand);
        //Debug.Log("CheckHandValid() 0:" + _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].isValid + ", 1:" +
        //_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT].isValid + ", 2:" +
        //_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].isValid + ", 3:" +
        //_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].isValid + ", 4:" +
        //_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_TIP_EXT].isValid);


        if (!(_Joints[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].isValid &&
            _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_METACARPAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_METACARPAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_MIDDLE_TIP_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_METACARPAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_RING_TIP_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_METACARPAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT].isValid &&
             _Joints[(int)XrHandJointEXT.XR_HAND_JOINT_LITTLE_TIP_EXT].isValid))
        {
            //Debug.Log("DebugHandGesture CheckHandValid() not valid hand: "+ _Hand);
            return false;
        }
        //Debug.Log("DebugHandGesture CheckHandValid() valid hand: " + _Hand);
        return true;
    }

    bool IsGestureReady() {
        HandJoint[] _JointsL = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Left);
        HandJoint[] _JointsR = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Right);
        //Debug.Log("IsGestureReady left before: " + HandTracking.GetHandJointLocations(HandFlag.Left)[0].isValid + ", " +
        //_JointsL[1].isValid + ", " + _JointsL[2].isValid + ", " + _JointsL[3].isValid + ", " + _JointsL[4].isValid + ", " +
        //_JointsL[5].isValid + ", " + _JointsL[6].isValid + ", " + _JointsL[7].isValid + ", " + _JointsL[8].isValid + ", " +
        //_JointsL[9].isValid + ", " + _JointsL[10].isValid + ", " + _JointsL[11].isValid + ", " + _JointsL[12].isValid + ", " +
        //_JointsL[13].isValid + ", " + _JointsL[14].isValid + ", " + _JointsL[15].isValid + ", " + _JointsL[16].isValid + ", " +
        //_JointsL[17].isValid + ", " + _JointsL[18].isValid + ", " + _JointsL[19].isValid + ", " + _JointsL[20].isValid);*/


        if (_JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.x == 0 &&
            _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.y == 0 &&
            _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.z == 0 &&
            _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.x == 0 &&
            _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.y == 0 &&
            _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].position.z == 0)
        {
            //Debug.Log("IsGestureReady left palm before: " + _JointsL[0].position.x + ", " + _JointsL[0].position.y + ", " + _JointsL[0].position.z);
            //Debug.Log("IsGestureReady right palm before: " + _JointsR[0].position.x + ", " + _JointsR[0].position.y + ", " + _JointsR[0].position.z);
            //Debug.Log("IsGestureReady left wrist before: " + _JointsL[1].position.x + ", " + _JointsL[1].position.y + ", " + _JointsL[1].position.z);
            //Debug.Log("IsGestureReady right wrist before: " + _JointsR[1].position.x + ", " + _JointsR[1].position.y + ", " + _JointsR[1].position.z);
            //Debug.Log("DebugHandGesture IsGestureReady() not ready");
            return false;
        }

        //Vector3 _LBone1Dir = Vector3.zero, _LBone2Dir = Vector3.zero, _RBone1Dir = Vector3.zero, _RBone2Dir = Vector3.zero;
        //HandJoint[] _LJoints = /*HandTracking.*/CustomGestureManager.GetHandJointLocations(HandFlag.Left);
        //_LBone1Dir = _LJoints[(int)(XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT + 1)].position - _LJoints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].position;
        //_LBone2Dir = _LJoints[(int)(XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT - 1)].position - _LJoints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].position;
        //Debug.Log("IsGestureReady left : " + Vector3.Angle(_LBone1Dir, _LBone2Dir));
        //HandJoint[] _RJoints = /*HandTracking.*/CustomGestureManager.GetHandJointLocations(HandFlag.Right);
        //_RBone1Dir = _RJoints[(int)(XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT + 1)].position - _RJoints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].position;
        //_RBone2Dir = _RJoints[(int)(XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT - 1)].position - _RJoints[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT].position;
        //Debug.Log("IsGestureReady right : " + Vector3.Angle(_RBone1Dir, _RBone2Dir));

        return true;
    }


}

