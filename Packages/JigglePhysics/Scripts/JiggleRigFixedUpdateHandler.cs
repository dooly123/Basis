using System.Collections.Generic;
using UnityEngine;

namespace JigglePhysics
{

    internal class JiggleRigFixedUpdateHandler : JiggleRigHandler<JiggleRigFixedUpdateHandler>
    {
        private void FixedUpdate()
        {
            var gravity = Physics.gravity;
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