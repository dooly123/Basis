#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
                    if (renderer.TryGetComponent<JiggleRigVisibleFlag>(out jiggleRigVisibleFlag))
                    {
                        jiggleRigVisibleFlag.VisibleFlagIndex = Index;
                        Visible[Index] = renderer.isVisible;
                    }
                    else
                    {
                        jiggleRigVisibleFlag = renderer.gameObject.AddComponent<JiggleRigVisibleFlag>();
                        jiggleRigVisibleFlag.VisibleFlagIndex = Index;
                        Visible[Index] = renderer.isVisible;
                        jiggleRigVisibleFlag.VisibilityChange += VisiblityChange;
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
            if(VisibleCount < Index)
            {
                Debug.LogError("Visible Array " + VisibleCount +"| is smaller then " + Index);
                return;
            }
            Visible[Index] = Visibility;
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
            return Vector3.Distance(camera.transform.position, position) < distance;
        }
        public override JiggleSettingsData AdjustJiggleSettingsData(Vector3 position, JiggleSettingsData data)
        {
            if (!TryGetCamera(out Camera camera))
            {
                return data;
            }
            var currentBlend = (Vector3.Distance(camera.transform.position, position) - distance + blend) / blend;
            currentBlend = Mathf.Clamp01(1f - currentBlend);
            data.blend = currentBlend;
            return data;
        }
    }
}