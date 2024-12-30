using System.Collections.Generic;
using UnityEngine;

namespace JigglePhysics
{
    internal class JiggleRigFixedUpdateHandler : JiggleRigHandler<JiggleRigFixedUpdateHandler>
    {
        public Vector3 GlobalGravity;
        public void Start()
        {
            GlobalGravity = Physics.gravity;
        }
        private void FixedUpdate()
        {
            var deltaTime = Time.deltaTime;
            var timeAsDouble = Time.timeAsDouble;
            var timeAsDoubleOneStepBack = timeAsDouble - JiggleRigBuilder.VERLET_TIME_STEP;
            for (int Index = 0; Index < JiggleRigCount; Index++)
            {
                jiggleRigsArray[Index].Advance(deltaTime, GlobalGravity, timeAsDouble, timeAsDoubleOneStepBack);
            }
        }
    }
}