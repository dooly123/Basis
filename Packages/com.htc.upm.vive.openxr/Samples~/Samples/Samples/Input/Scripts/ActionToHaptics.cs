using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToHaptics : MonoBehaviour
    {
        public InputActionReference action;
        public float _amplitude = 1.0f;
        public float _duration = 0.1f;

        private void Start()
        {
            if (action == null)
                return;

            action.action.Enable();
            action.action.performed += (ctx) =>
            {
                var control = action.action.activeControl;
                if (null == control)
                    return;

                if (control.device is XRControllerWithRumble rumble)
                    rumble.SendImpulse(_amplitude, _duration);
            };
        }
    }
}
