using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace JigglePhysics
{

    public static class JiggleRigGizmos
    {
        public static void DebugDraw(JiggleBone JiggleBone, Color simulateColor, Color targetColor, bool interpolated)
        {
            if (JiggleBone.JiggleParent == null) return;
            if (interpolated)
            {
                Debug.DrawLine(JiggleBone.extrapolatedPosition, JiggleBone.JiggleParent.extrapolatedPosition, simulateColor, 0, false);
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
}