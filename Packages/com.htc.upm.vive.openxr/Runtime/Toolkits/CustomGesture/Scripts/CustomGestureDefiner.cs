using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.UI;
using VIVE.OpenXR.Hand;


namespace VIVE.OpenXR.Toolkits.CustomGesture
{
    [RequireComponent(typeof(CustomGestureManager))]
    public class CustomGestureDefiner : MonoBehaviour
    {
        public List<CustomGesture> DefinedGestures = new List<CustomGesture>();
        static List<CustomGesture> definedGestures = new List<CustomGesture>();
        CustomGestureManager HGM;
        static CustomGestureManager hGM;

        void Start()
        {
            HGM = GetComponent<CustomGestureManager>();
            definedGestures = DefinedGestures;
            hGM = HGM;
        }

        void Update()
        {

        }


        public static bool IsCurrentGestureTriiggered(string _GestureName, CGEnums.HandFlag _Hand)
        {

            bool SingleDsLeft = true, SingleDsRight = true;
            CustomGesture _Gesture = definedGestures.Find(_x => _x.GestureName == _GestureName);
            if(_Gesture == null)
            {
                Debug.LogWarning("HandGesture is not definded");
                return false;
            }

            if (hGM == null)
            {
                Debug.LogError("hGM is null");
                return false;
            }
            float SingledisXLeft = 0.0f, SingledisYLeft = 0.0f, SingledisXRight = 0.0f, SingledisYRight = 0.0f, /*SingledisZ = 0.0f,*/ SingledistanceLeft = 0.0f, SingledistanceRight = 0.0f;
            HandJoint[] _JointsL, _JointsR;

            switch (_Hand)//(_Gesture.TargetHand)
            {
                case CGEnums.HandFlag.Left:
                    _JointsL = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Left);
                    SingledisXLeft = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.x;
                    SingledisYLeft = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.y;
                    //SingledisZ = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.z;
                    SingledistanceLeft = Mathf.Sqrt(Mathf.Pow(SingledisXLeft, 2) + Mathf.Pow(SingledisYLeft, 2));
                    //Debug.Log("Single CheckHandDistance Thumb L: " + SingledisXLeft + ", " + SingledisYLeft + ", " + SingledisZ + ",  distance:" + SingledistanceLeft*100);
                    if (_Gesture.ThumbIndexDistance != JointDistance.DontCare)
                        SingleDsLeft = (_Gesture.ThumbIndexDistance == JointDistance.Far) ? ((SingledistanceLeft * 100) > _Gesture.SingleFar) : ((SingledistanceLeft * 100) < _Gesture.SingleNear);
                    if ((_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status)         &&
                        (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status)         &&
                        (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status)    &&
                        (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status)              &&
                        (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status))
                    {
                        return (true && SingleDsLeft);
                    }
                    return false;
                case CGEnums.HandFlag.Right:
                    _JointsR = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Right);
                    SingledisXRight = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.x;
                    SingledisYRight = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.y;
                    //SingledisZ = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.z;
                    SingledistanceRight = Mathf.Sqrt(Mathf.Pow(SingledisXRight, 2) + Mathf.Pow(SingledisYRight, 2));
                    //Debug.Log("Single CheckHandDistance Thumb R: " + SingledisXRight + ", " + SingledisYRight + ", " + SingledisZ + ",  distance:" + SingledistanceRight*100);
                    if (_Gesture.ThumbIndexDistance != JointDistance.DontCare)
                        SingleDsRight = (_Gesture.ThumbIndexDistance == JointDistance.Far) ? ((SingledistanceRight * 100) > _Gesture.SingleFar) : ((SingledistanceRight * 100) < _Gesture.SingleNear);
                    if ((_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status)       &&
                        (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status)       &&
                        (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status)  &&
                        (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status)            &&
                        (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status))
                    {
                        return (true && SingleDsRight);
                    }
                    return false;
                case CGEnums.HandFlag.Either:
                    if ((_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status)         &&
                        (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status)         &&
                        (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status)    &&
                        (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status)              &&
                        (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status))
                    {
                        return true;
                    }
                    else if ((_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status)      &&
                            (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status)       &&
                            (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status)  &&
                            (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status)            &&
                            (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status))
                    {
                         return true;
                    }
                    return false;
                case CGEnums.HandFlag.Dual:
                    _JointsL = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Left);
                    _JointsR = CustomGestureManager.GetHandJointLocations(CGEnums.HandFlag.Right);
                    SingledisXLeft = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.x;
                    SingledisYLeft = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.y;
                    //SingledisZ = _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.z;
                    SingledistanceLeft = Mathf.Sqrt(Mathf.Pow(SingledisXLeft, 2) + Mathf.Pow(SingledisYLeft, 2));
                    SingledisXRight = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.x;
                    SingledisYRight = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.y;
                    //SingledisZ = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z - _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT].position.z;
                    SingledistanceRight = Mathf.Sqrt(Mathf.Pow(SingledisXLeft, 2) + Mathf.Pow(SingledisYLeft, 2));
                    //Debug.Log("Single CheckHandDistance Thumb: " + SingledisYLeft + ", " + SingledisYLeft + ", " + SingledisZ + ",  distance:" + SingledistanceLeft);
                    float DualdisX = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.x;
                    float DualdisY = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.y;
                    //float DualdisZ = _JointsR[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z - _JointsL[(int)XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT].position.z;
                    float Dualdistance = Mathf.Sqrt(Mathf.Pow(DualdisX, 2) + Mathf.Pow(DualdisY, 2));
                    //Debug.Log("Dual CheckHandDistance Index: " + DualdisX + ", " + DualdisY + ", " + DualdisZ + ",  distance:" + Dualdistance);

                    bool LGesture = false, RGesture = false, DualDs = true;
                    if(_Gesture.DualHandDistance != JointDistance.DontCare)
                        DualDs = (_Gesture.DualHandDistance == JointDistance.Far) ? ((Dualdistance * 100) > _Gesture.DualFar) : ((Dualdistance * 100) < _Gesture.DualNear);

                    if(_Gesture.ThumbIndexDistance != JointDistance.DontCare)
                        SingleDsLeft = (_Gesture.ThumbIndexDistance == JointDistance.Far) ? ((SingledistanceLeft * 100) > _Gesture.SingleFar) : ((SingledistanceLeft * 100) < _Gesture.SingleNear);

                    if (_Gesture.ThumbIndexDistance != JointDistance.DontCare)
                        SingleDsRight = (_Gesture.ThumbIndexDistance == JointDistance.Far) ? ((SingledistanceRight * 100) > _Gesture.SingleFar) : ((SingledistanceRight * 100) < _Gesture.SingleNear);

                    LGesture = (_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status) &&
                        (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status) &&
                        (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status) &&
                        (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status) &&
                        (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Left, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status);
                    RGesture = (_Gesture.ThumbStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) == _Gesture.ThumbStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Thumb) != _Gesture.ThumbStatusIs.ToExpresstion().Status) &&
                            (_Gesture.IndexStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) == _Gesture.IndexStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Index) != _Gesture.IndexStatusIs.ToExpresstion().Status) &&
                            (_Gesture.MiddleStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) == _Gesture.MiddleStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Middle) != _Gesture.MiddleStatusIs.ToExpresstion().Status) &&
                            (_Gesture.RingStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) == _Gesture.RingStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Ring) != _Gesture.RingStatusIs.ToExpresstion().Status) &&
                            (_Gesture.PinkyStatusIs.ToExpresstion().Is ? hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) == _Gesture.PinkyStatusIs.ToExpresstion().Status : hGM.GetFingerStatus(CGEnums.HandFlag.Right, CGEnums.FingerFlag.Pinky) != _Gesture.PinkyStatusIs.ToExpresstion().Status);
                    //Debug.Log("Dual Gesture rs: "+ _GestureName + LGesture + ", " + RGesture + ", " + SingleDs + ", " + DualDs  );
                    return (LGesture && RGesture && SingleDsLeft && DualDs && SingleDsRight);
                default:
                    Debug.LogError("The HandFlag can only be set to Lef, Right or Both");
                    return false;
            }
        }
    }
}
