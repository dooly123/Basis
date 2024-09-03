using System;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace JigglePhysics
{
    public partial class JiggleSkin
    {
        [Serializable]
        public class JiggleZone : JiggleRigBuilder.JiggleRig {
            [Tooltip("How large of a radius the zone should effect, in target-space meters. (Scaling the target will effect the radius.)")]
            public float radius;
            public JiggleZone(Transform rootTransform, JiggleSettingsBase jiggleSettings, ICollection<Transform> ignoredTransforms, ICollection<Collider> colliders) : base(rootTransform, jiggleSettings, ignoredTransforms, colliders) { }
            protected override void CreateSimulatedPoints(ICollection<JiggleBone> outputPoints, ICollection<Transform> ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone) {
                //base.CreateSimulatedPoints(outputPoints, ignoredTransforms, currentTransform, parentJiggleBone);
                var parent = new JiggleBone(currentTransform, null);
                outputPoints.Add(parent);
                outputPoints.Add(new JiggleBone(null, parent, 0f));
            }
            public void DebugDraw() {
                Debug.DrawLine(GetPointSolve(), GetRootTransform().position, Color.cyan, 0, false);
            }
            public Vector3 GetPointSolve() => simulatedPoints[1].GetCachedSolvePosition();
            public new void OnRenderObject()
            {
                if (GetRootTransform() == null) {
                    return;
                }
                Gizmos.Sphere(GetRootTransform().position, radius * GetRootTransform().lossyScale.x, new Color(0.1f, 0.1f, 0.8f, 0.5f));
            }
        }
}

}