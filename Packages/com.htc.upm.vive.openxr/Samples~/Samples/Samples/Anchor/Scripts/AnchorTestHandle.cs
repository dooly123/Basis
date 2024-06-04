// Copyright HTC Corporation All Rights Reserved.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using VIVE.OpenXR.Toolkits.Anchor;

namespace VIVE.OpenXR.Samples.Anchor
{

    public class AnchorTestHandle : MonoBehaviour
    {
        public Transform rig;
        public Transform anchorPoseD;
        public Transform anchorPose1;
        public Transform anchorPose2;
        public Transform obj;
        public AnchorManager.Anchor anchor1;
        public AnchorManager.Anchor anchor2;
        public TextMeshProUGUI status;
        public TextMeshProUGUI statusOrigin;

        Pose originFloor;


        public XRInputSubsystem xrInputSubsystem;

        void GetXRInputSubsystem()
        {
            List<XRInputSubsystem> xrSubsystemList = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(xrSubsystemList);
            foreach (var xrSubsystem in xrSubsystemList)
            {
                if (xrSubsystem.running)
                {
                    xrInputSubsystem = xrSubsystem;
                    break;
                }
            }
        }

        IEnumerator Start()
        {
            originFloor = new Pose(rig.position, rig.rotation);

            float t = Time.unscaledTime;
            while (xrInputSubsystem == null)
            {
                yield return null;
                GetXRInputSubsystem();
                if (Time.unscaledTime - t > 5)
                {
                    Debug.LogError("Get XRInputSubsystem timeout");
                    status.text = "Get XRInputSubsystem timeout";
                    yield break;
                }
            }
        }

        public Pose GetRelatedPoseToRig(Transform t)
        {
            return new Pose(rig.InverseTransformPoint(t.position), Quaternion.Inverse(rig.rotation) * t.rotation);
        }

        /// <summary>
        /// Help create anchor by anchor manager
        /// </summary>
        /// <param name="relatedPose">pose related to camera rig</param>
        /// <param name="name">the anchor's name</param>
        /// <returns></returns>
        AnchorManager.Anchor CreateAnchor(Pose relatedPose, string name)
        {
            if (!AnchorManager.IsSupported())
            {
                Debug.LogError("AnchorManager is not supported.");
                status.text = "AnchorManager is not supported.";
                return null;
            }
            var anchor = AnchorManager.CreateAnchor(relatedPose, name + " (" + Time.frameCount + ")");
            if (anchor == null)
            {
                status.text = "Create " + name + " failed";
                Debug.LogError("Create " + name + " failed");
                // Even error, still got.  Use fake data.
                return anchor;
            }
            else
            {
                string msg = "Create Anchor n=" + anchor.Name + " space=" + anchor.GetXrSpace() + " at p=" + relatedPose.position + " & r=" + relatedPose.rotation.eulerAngles;
                status.text = msg;
                Debug.Log(msg);
                return  anchor;
            }
        }

        public void OnCreateAnchor1()
        {
            if (anchor1 != null)
            {
                anchor1.Dispose();
                anchor1 = null;
            }
            anchor1 = CreateAnchor(GetRelatedPoseToRig(anchorPose1), "anchor1");
        }

        public void OnCreateAnchor2()
        {
            if (anchor2 != null)
            {
                anchor2.Dispose();
                anchor2 = null;
            }
            anchor2 = CreateAnchor(GetRelatedPoseToRig(anchorPose2), "anchor2");
        }

        public void MoveObjToAnchor(AnchorManager.Anchor anchor)
        {
            if (!AnchorManager.IsSupported())
                return;

            if (anchor == null)
            {
                status.text = "anchor is null";
                return;
            }

            if (AnchorManager.GetTrackingSpacePose(anchor, out Pose pose))
            {
                // Convert tracking space pose to rig space pose
                obj.position = rig.TransformPoint(pose.position);
                obj.rotation = rig.rotation * pose.rotation;

                status.text = "Obj move to " + anchor.GetSpatialAnchorName();
            }
            else
            {
                status.text = "Fail to get anchor's pose";
            }
        }

        public void OnFollowAnchor1()
        {
            MoveObjToAnchor(anchor1);
        }

        public void OnFollowAnchor2()
        {
            MoveObjToAnchor(anchor2);
        }

        public void OnResetObj()
        {
            obj.position = anchorPoseD.position;
            obj.rotation = anchorPoseD.rotation;

            status.text = "Obj move to default pose";
        }


        public void OnFloor()
        {
            if (xrInputSubsystem == null)
            {
                Debug.LogError("xrInputSubsystem is null");
                statusOrigin.text = "xrInputSubsystem is null";
                return;
            }


            if (xrInputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
            {
                statusOrigin.text = "Set tracking origin to floor. Reset Obj and anchors";
                rig.position = originFloor.position;
                OnResetObj();
                anchor1 = null;
                anchor2 = null;
            }
            else
            {
                statusOrigin.text = "Fail to set tracking origin to floor";
            }
        }

        public void OnDevice()
        {
            if (xrInputSubsystem == null)
            {
                Debug.LogError("xrInputSubsystem is null");
                statusOrigin.text = "xrInputSubsystem is null";
                return;
            }


            if (xrInputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device))
            {
                statusOrigin.text = "Set tracking origin to device. Reset Obj and anchors";
                rig.position = originFloor.position + Vector3.up;
                OnResetObj();
                if (anchor1 != null)
                {
                    anchor1.Dispose();
                    anchor1 = null;
                }
                if (anchor2 != null)
                {
                    anchor2.Dispose();
                    anchor2 = null;
                }
            }
            else
            {
                statusOrigin.text = "Fail to set tracking origin to device";
            }
        }
    }
}