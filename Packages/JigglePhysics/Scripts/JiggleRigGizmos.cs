using System;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace JigglePhysics
{
    /*
    public static class JiggleRigGizmos
    {
        public  void DebugDraw(JiggleRig JiggleRig, JiggleBone JiggleBone,int JiggleIndex, Color simulateColor, Color targetColor, bool interpolated)
        {
            if (JiggleBone.JiggleParent == null)
            {
                return;
            }
            int Index = Array.IndexOf(JiggleRig.SPoints, SPoints[SimulatedIndex].child);
            int Index = Array.IndexOf(JiggleRig.SPoints, SPoints[SimulatedIndex].child);
            if (interpolated)
            {
                Debug.DrawLine(JiggleRig.extrapolatedPosition[JiggleIndex], JiggleBone.JiggleParent.extrapolatedPosition, simulateColor, 0, false);
            }
            else
            {
                Debug.DrawLine(JiggleBone.workingPosition, JiggleBone.JiggleParent.workingPosition, simulateColor, 0, false);
            }
            Debug.DrawLine(JiggleBone.currentFixedAnimatedBonePosition, JiggleBone.JiggleParent.currentFixedAnimatedBonePosition, targetColor, 0, false);
        }
        public static void OnDrawGizmos(JiggleBone JiggleBone, JiggleSettingsBase jiggleSettings, double TimeAsDouble)
        {
            Vector3 pos = PositionSignalHelper.SamplePosition(JiggleBone.particleSignal, TimeAsDouble);
            if (JiggleBone.child != null)
            {
                Gizmos.Line(pos, PositionSignalHelper.SamplePosition(JiggleBone.child.particleSignal, TimeAsDouble));
            }
            if (jiggleSettings != null)
            {
                Gizmos.Sphere(pos, jiggleSettings.GetRadius(JiggleBone.normalizedIndex));
            }
        }
    }
    */
}