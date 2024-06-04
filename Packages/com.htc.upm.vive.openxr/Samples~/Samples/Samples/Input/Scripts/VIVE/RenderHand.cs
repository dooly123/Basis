// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR;
using VIVE.OpenXR.Hand;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    public class RenderHand : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Samples.Hand.Tracking.RenderHand";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + (isLeft ? "Left" : "Right") + ", " + msg); }
        void INTERVAL(string msg) { if (printIntervalLog) { DEBUG(msg); } }

        // Links between keypoints, 2*i & 2*i+1 forms a link.
        // keypoint index: 1: palm, 2-5: thumb, 6-10: index, 11-15: middle, 16-20: ring, 21-25: pinky
        // fingers are counted from bottom to top
        private static int[] Connections = new int[] {
            1,  2,  1,  6,  1,  11, 1,  16, 1, 21,  // palm and finger starts
            3,  6,  6,  11, 11, 16, 16, 21,         // finger starts
            2,  3,  3,  4,  4,  5,                  // thumb
            6,  7,  7,  8,  8,  9,  9,  10,                  // index
            11, 12, 12, 13, 13, 14, 14, 15,                 // middle
            16, 17, 17, 18, 18, 19, 19, 20,                // ring
            21, 22, 22, 23, 23, 24, 24, 25                 // pinky
        };
        [Tooltip("Draw left hand if true, right hand otherwise")]
        public bool isLeft = false;
        [Tooltip("Use inferred or last-known posed when hand loses tracking if true.")]
        public bool allowUntrackedPose = false;
        [Tooltip("Default color of hand points")]
        public Color pointColor = Color.green;
        [Tooltip("Default color of links between keypoints in skeleton mode")]
        public Color linkColor = Color.white;
        [Tooltip("Material for hand points and links")]
        [SerializeField]
        private Material material = null;


        private List<GameObject> points = new List<GameObject>();
        // list of links created (only for skeleton)
        private List<GameObject> links = new List<GameObject>();
        // Start is called before the first frame update
        private XrHandJointLocationEXT[] HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        // shared material for all point objects
        private Material pointMat = null;
        // shared material for all link objects
        private Material linkMat = null;
        private void Start()
        {
            pointMat = new Material(material);
            if (isLeft)
            {
                pointColor = Color.blue;
            }
            else
            {
                pointColor = Color.red;
            }
            pointMat.color = pointColor;
            linkMat = new Material(material);
            linkMat.color = linkColor;

            for (int i = 0; i < (int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = ((XrHandJointEXT)i).ToString();
                go.transform.parent = transform;
                go.transform.localScale = Vector3.one * 0.012f;
                go.SetActive(false);
                points.Add(go);
                go.transform.position = new Vector3((float)i * 0.1f, 0, 0);
                // handle layer
                go.layer = gameObject.layer;
                // handle material
                go.GetComponent<Renderer>().sharedMaterial = pointMat;
            }

            // create game objects for links between keypoints, only used in skeleton mode
            for (int i = 0; i < Connections.Length; i += 2)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "link" + i;
                go.transform.parent = transform;
                go.transform.localScale = Vector3.one * 0.005f;
                go.SetActive(false);
                links.Add(go);
                // handle layer
                go.layer = gameObject.layer;
                // handle material
                go.GetComponent<Renderer>().sharedMaterial = linkMat;
            }
        }

        int printFrame = 0;
        private bool printIntervalLog = false;
        private void Update()
        {
            printFrame++;
            printFrame %= 300;
            printIntervalLog = (printFrame == 0);

            var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
            if (feature && feature.GetJointLocations(isLeft, out HandjointLocations))
            {
                UpdateJointLocation();
            }
            else
            {
                for (int i = 0; i < points.Count; i++)
                {
                    var go = points[i];
                    go.SetActive(false);
                }

                for (int i = 0; i < links.Count; i++)
                {
                    var link = links[i];
                    link.SetActive(false);
                }
            }
        }

        public void UpdateJointLocation()
        {
            for (int i = 0; i < points.Count; i++)
            {
                var go = points[i];
                XrQuaternionf orientation;
                XrVector3f position;
                go.GetComponent<SphereCollider>().radius = HandjointLocations[i].radius;
                INTERVAL(go.name + " radius: " + go.GetComponent<SphereCollider>().radius);
                if (allowUntrackedPose) //Use inferred or last-known pose when lost tracking 
                {
                    orientation = HandjointLocations[i].pose.orientation;
                    position = HandjointLocations[i].pose.position;
                    go.transform.localPosition = position.ToUnityVector();//new Vector3(position.x, position.y, -position.z);
                    go.SetActive(true);
                }
                else
                {
                    if ((HandjointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
                    {
                        orientation = HandjointLocations[i].pose.orientation;
                    }
                    if ((HandjointLocations[i].locationFlags & XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0)
                    {
                        position = HandjointLocations[i].pose.position;
                        go.transform.localPosition = new Vector3(position.x, position.y, -position.z);
                        go.SetActive(true);
                    }
                    else
                    {
                        INTERVAL("Lost tracking");
                        go.SetActive(false);
                    }
                    /*if (i == 1 && isLeft)
                    {
                        DEBUG("points[1]: " + go.name + " active? " + go.activeSelf
                            + ", locationFlags: " + HandjointLocations[i].locationFlags
                            + ", position (" + go.transform.localPosition.x.ToString() + ", " + go.transform.localPosition.y.ToString() + ", " + go.transform.localPosition.z.ToString() + ")"
                            + ", Camera (" + Camera.main.gameObject.transform.localPosition.x.ToString() + ", " + Camera.main.gameObject.transform.localPosition.y.ToString() + ", " + Camera.main.gameObject.transform.localPosition.z.ToString() + ")");
                    }*/
                }
            }

            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];
                if (!points[Connections[i * 2]].activeSelf || !points[Connections[i * 2 + 1]].activeSelf)
                {
                    link.SetActive(false);
                    continue;
                }

                var pose1 = points[Connections[i * 2]].transform.position;
                var pose2 = points[Connections[i * 2 + 1]].transform.position;

                // calculate link position and rotation based on points on both end
                link.SetActive(true);
                link.transform.position = (pose1 + pose2) / 2;
                var direction = pose2 - pose1;
                link.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                link.transform.localScale = new Vector3(0.006f, direction.magnitude / 2f - 0.0051f, 0.006f);
            }

        }
        public void OnDestroy()
        {
            DEBUG("OnDestroy");
        }
    }
}
