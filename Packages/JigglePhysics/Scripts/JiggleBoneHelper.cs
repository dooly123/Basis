using UnityEngine;
namespace JigglePhysics
{
    public class JiggleBoneHelper
    {
        public static JiggleBone JiggleBone(Transform transform, JiggleBone parent, float projectionAmount = 1f)
        {
            JiggleBone JiggleBone = new JiggleBone
            {
                transform = transform,
                JiggleParent = parent,
                projectionAmount = projectionAmount
            };
            Vector3 position;
            if (transform != null)
            {
                transform.GetLocalPositionAndRotation(out JiggleBone.lastValidPoseBoneLocalPosition, out JiggleBone.lastValidPoseBoneRotation);
                position = transform.position;
            }
            else
            {
                position = JiggleRig.GetProjectedPosition(JiggleBone);
            }
            double timeAsDouble = Time.timeAsDouble;
            JiggleBone.targetAnimatedBoneSignal = new PositionSignal(position, timeAsDouble);
            JiggleBone.particleSignal = new PositionSignal(position, timeAsDouble);

            JiggleBone.hasTransform = transform != null;
            if (parent == null)
            {
                return JiggleBone;
            }
            JiggleBone.JiggleParent.child = JiggleBone;
            return JiggleBone;
        }
    }
}