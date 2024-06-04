//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using UnityEngine;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    [RequireComponent(typeof(Renderer), typeof(Collider))]
    public class DartBoard : MonoBehaviour
    {
        private Renderer Renderer;
        public uint BendCount = 3, PieCount = 10, FrameWidth = 1;

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            Focus(Vector3.zero);
        }

        public void Focus(Vector3 focusPoint)
        {
            float maxDist = 0.42f * transform.localScale.x;
            float sectionLength = maxDist / BendCount;
            float dist = Vector3.Distance(focusPoint, transform.position);
            uint bendIndex = (uint)(dist / sectionLength);

            Vector3 axis = (focusPoint - transform.position) / Vector3.Distance(focusPoint, transform.position);
            float ang = SignedAngle(transform.right, axis, transform.forward);
            if (ang < 0) ang += 360f;
            float pieCount = 360f / PieCount;
            uint pieIndex = (uint)(ang / pieCount);

            Renderer.material.SetVector("_MeshCenter", new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
            Renderer.material.SetVector("_MeshForward", new Vector4(transform.forward.x, transform.forward.y, transform.forward.z, 0));
            Renderer.material.SetVector("_MeshRight", new Vector4(transform.right.x, transform.right.y, transform.right.z, 0));
            Renderer.material.SetInt("_BendCount", (int)BendCount);
            Renderer.material.SetInt("_PieCount", (int)PieCount);
            Renderer.material.SetInt("_BendIndex", (int)bendIndex);
            Renderer.material.SetInt("_PieIndex", (int)pieIndex);
            Renderer.material.SetFloat("_Scale", transform.localScale.x);
            Renderer.material.SetFloat("_FrameWidth", FrameWidth);
        }

        public float SignedAngle(Vector3 v1, Vector3 v2, Vector3 v_forward)
        {
            float dotP = Vector3.Dot(v1, v2);
            float unsignedAngle = Mathf.Acos(dotP) * (180 / 3.14159f);

            float sign = Vector3.Dot(v_forward, Vector3.Cross(v1, v2));
            float signedAngle = unsignedAngle * (sign > 0f ? 1f : -1f) + 180f;
            return signedAngle;
        }
    }
}
