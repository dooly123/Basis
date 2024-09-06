#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;
namespace JigglePhysics
{
    [System.Serializable]
    public class JiggleRigSimpleLOD : JiggleRigLOD
    {

        [Tooltip("Distance to disable the jiggle rig")]
        [SerializeField]
        float distance = 30;
        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        [SerializeField]
        float blend = 0.5f;
        [SerializeField]
        public Camera currentCamera;
        public bool LastVisiblity;
        public bool[] Visible;
        public int VisibleCount;
        public Vector3 CameraPositon;
        public class JiggleRigVisibleFlag : MonoBehaviour
        {
            public int VisibleFlagIndex;
            public Action<bool, int> VisibilityChange;
            public void OnBecameInvisible()
            {
                VisibilityChange?.Invoke(false, VisibleFlagIndex);
            }
            public void OnBecameVisible()
            {
                VisibilityChange?.Invoke(true, VisibleFlagIndex);
            }
        }
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
            UpdateCameraPosition();
            return Vector3.Distance(camera.transform.position, position) < distance;
        }
        /// <summary>
        /// needs to stop computing every frame
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override JiggleSettingsData AdjustJiggleSettingsData(Vector3 position, JiggleSettingsData data)
        {
            var currentBlend = (Vector3.Distance(CameraPositon, position) - distance + blend) / blend;
            currentBlend = Mathf.Clamp01(1f - currentBlend);
            data.blend = currentBlend;
            return data;
        }
        public override void UpdateCameraPosition()
        {
            CameraPositon = currentCamera.transform.position;
        }
    }
}