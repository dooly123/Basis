#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace JigglePhysics
{
    public class JiggleRigSimpleLOD : JiggleRigLOD
    {

        [Tooltip("Distance to disable the jiggle rig.")]
        [SerializeField] float distance = 20f;
        [Tooltip("Distance past distance from which it blends out rather than instantly disabling.")]
        [SerializeField] float blend = 5f;

        public static Camera currentCamera;
        public Transform TargetPoint;

        protected virtual bool TryGetCamera(out Camera camera)
        {
            camera = currentCamera;
            return currentCamera;
        }
        protected override bool CheckActive()
        {
            if (!TryGetCamera(out Camera camera))
            {
                return false;
            }
            var cameraDistance = Vector3.Distance(camera.transform.position, TargetPoint.position);
            var currentBlend = (cameraDistance - distance + blend) / blend;
            currentBlend = Mathf.Clamp01(1f - currentBlend);
            for (int Index = 0; Index < JiggleCount; Index++)
            {
                jiggles[Index].blend = currentBlend;
            }
            return cameraDistance < distance;
        }

    }
}