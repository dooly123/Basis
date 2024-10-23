using System.Collections.Generic;
using UnityEngine;

namespace JigglePhysics
{

    internal class JiggleRigLateUpdateHandler : JiggleRigHandler<JiggleRigLateUpdateHandler>
    {
        public Vector3 gravity;
        public void Start()
        {
            gravity = Physics.gravity;
        }
        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var timeAsDouble = Time.timeAsDouble;
            var timeAsDoubleOneStepBack = timeAsDouble - JiggleRigBuilder.VERLET_TIME_STEP;
            for (int Index = 0; Index < JiggleRigsLength; Index++)
            {
                JiggleRigsArray[Index].Advance(deltaTime, gravity, timeAsDouble, timeAsDoubleOneStepBack);
            }
        }
    }
}