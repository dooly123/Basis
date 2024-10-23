using UnityEngine;

namespace JigglePhysics {
    public class JiggleRigRendererLOD : JiggleRigLOD
    {

        [Tooltip("Distance to disable the jiggle rig.")]
        [SerializeField] float distance = 20f;
        [Tooltip("Distance past distance from which it blends out rather than instantly disabling.")]
        [SerializeField] float blend = 5f;

        private float DistancePlus;
        public static Camera Camera;

        public bool[] Visible;
        public bool LastVisiblity;
        public int VisibleCount;
        protected override void Awake()
        {
            DistancePlus = distance + blend;
            base.Awake();
            MonoBehaviorHider.JiggleRigLODRenderComponent jiggleRigVisibleFlag = null;
            var renderers = GetComponentsInChildren<Renderer>();
            VisibleCount = renderers.Length;
            Visible = new bool[VisibleCount];
            for (int i = 0; i < VisibleCount; i++)
            {
                if (!renderers[i].TryGetComponent(out jiggleRigVisibleFlag))
                {
                    jiggleRigVisibleFlag = renderers[i].gameObject.AddComponent<MonoBehaviorHider.JiggleRigLODRenderComponent>();
                }
                Visible[i] = renderers[i].isVisible;
                var index = i;
                jiggleRigVisibleFlag.VisibilityChange += (visible) =>
                {
                    // Check if the index is out of bounds
                    if (index < 0 || index >= Visible.Length)
                    {
                        Debug.LogError("Index out of bounds: " + index + ". Valid range is 0 to " + (Visible.Length - 1));
                        return;
                    }
                    // Update the visibility at the specified index
                    Visible[index] = visible;
                    // Re-evaluate visibility
                    RevalulateVisiblity();
                };
            }
            RevalulateVisiblity();
        }
        private void RevalulateVisiblity()
        {
            for (int visibleIndex = 0; visibleIndex < VisibleCount; visibleIndex++)
            {
                if (Visible[visibleIndex])
                {
                    LastVisiblity = true;
                    return;
                }
            }
            LastVisiblity = false;
        }

        private bool TryGetCamera(out Camera camera)
        {
            camera = Camera;
            return true;
        }
        protected override bool CheckActive()
        {
            if (LastVisiblity == false)
            {
                return false;
            }
            if (TryGetCamera(out Camera camera) == false)
            {
                return false;
            }
            float DistanceToCamera = Vector3.Distance(camera.transform.position, transform.position);
            var currentBlend = (DistanceToCamera - DistancePlus) / blend;
            currentBlend = Mathf.Clamp01(1f - currentBlend);
            for (int Index = 0; Index < jigglesCount; Index++)
            {
                jiggles[Index].blend = currentBlend;
            }
            return DistanceToCamera < distance;
        }

    }

}