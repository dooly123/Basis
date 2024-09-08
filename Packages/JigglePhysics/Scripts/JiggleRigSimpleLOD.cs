#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace JigglePhysics
{
    [System.Serializable]
    public class JiggleRigSimpleLOD : JiggleRigLOD
    {
        [Tooltip("Distance to disable the jiggle rig")]
        public float DisableAtDistance = 30;
        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        public float blend = 0.5f;
        public Camera currentCamera;
        public bool LastVisiblity;
        public bool[] Visible;
        public int VisibleCount;
        public Vector3 CameraPositon;
        public float CalculatedDistance;
        public float currentBlend;
        public void Initalize(Renderer[] Renderer)
        {
            JiggleRigVisibleFlag jiggleRigVisibleFlag = null;
            VisibleCount = Renderer.Length;
            Visible = new bool[VisibleCount];
            for (int Index = 0; Index < VisibleCount; Index++)
            {
                Renderer renderer = Renderer[Index];
                if (renderer != null)
                {
                    if (renderer.TryGetComponent(out jiggleRigVisibleFlag))
                    {
                        jiggleRigVisibleFlag.VisibleFlagIndex = Index;
                        Visible[Index] = renderer.isVisible;
                    }
                    else
                    {
                        jiggleRigVisibleFlag = renderer.gameObject.AddComponent<JiggleRigVisibleFlag>();
                        jiggleRigVisibleFlag.VisibleFlagIndex = Index;
                        Visible[Index] = renderer.isVisible;
                        jiggleRigVisibleFlag.VisibilityChange += VisiblityChange;//there is no -= assuming that if this goes the avatar goes
                    }
                }
                else
                {
                    Visible[Index] = false;
                }
            }
            RevalulateVisiblity();
        }
        public void VisiblityChange(bool Visibility, int Index)
        {
            // Check if the index is out of bounds
            if (Index < 0 || Index >= Visible.Length)
            {
                Debug.LogError("Index out of bounds: " + Index + ". Valid range is 0 to " + (Visible.Length - 1));
                return;
            }

            // Update the visibility at the specified index
            Visible[Index] = Visibility;

            // Re-evaluate visibility
            RevalulateVisiblity();
        }
        public void RevalulateVisiblity()
        {
            for (int VisibleIndex = 0; VisibleIndex < VisibleCount; VisibleIndex++)
            {
                if (Visible[VisibleIndex])
                {
                    LastVisiblity = true;
                    return;
                }
            }
            LastVisiblity = false;
        }
        private bool TryGetCamera(out Camera camera)
        {
#if UNITY_EDITOR
            if (EditorWindow.focusedWindow is SceneView view)
            {
                camera = view.camera;
                return camera != null;
            }
#endif
            camera = currentCamera;
            return currentCamera != null;
        }
        /// <summary>
        /// enables or disables camera by distance
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool CheckActive(Vector3 position)
        {
            if (LastVisiblity == false)
            {
                return false;
            }
            if (!TryGetCamera(out Camera camera))
            {
                return false;
            }
            UpdateCameraPosition(camera);
            return Vector3.Distance(camera.transform.position, position) < DisableAtDistance;
        }
        public override void UpdateDistance(Vector3 position)
        {
            CalculatedDistance = Vector3.Distance(CameraPositon, position);
            currentBlend = (CalculatedDistance - DisableAtDistance + blend) / blend;
            currentBlend = Mathf.Clamp01(1f - currentBlend);
        }
        public override void UpdateCameraPosition(Camera Camera)
        {
            CameraPositon = Camera.transform.position;
        }
    }
}