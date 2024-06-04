// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace VIVE.OpenXR.Toolkits.Common
{
    public class ControllerReaction : MonoBehaviour
    {
        public bool IsLeft = true;
        public InputActionAsset Asset;
        public InputActionReference TriggerAxis;
        public InputActionReference GripAxis;
        public InputActionReference ThumbstickAxis;
        public InputActionReference PrimaryButtonClick;
        public InputActionReference SecondButtonClick;

        GameObject Trigger;
        GameObject Grip;
        GameObject Thumbstick;
        GameObject Button1;
        GameObject Button2;

        private void OnEnable()
        {
            if (Asset != null && !Asset.enabled)
            {
                Asset.Enable();
            }
        }

        void Start()
        {
            Trigger = transform.GetChild(0).gameObject;
            Grip = transform.GetChild(1).gameObject;
            Thumbstick = transform.GetChild(2).gameObject;
            Button1 = transform.GetChild(3).gameObject;
            Button2 = transform.GetChild(4).gameObject;
            Grip_OriginPos = Grip.transform.localPosition;
            Button1_OriginPos = Button1.transform.localPosition;
            Button2_OriginPos = Button2.transform.localPosition;
        }

        void Update()
        {
            FireReactions();
        }

        void FireReactions()
        {
            TriggerReaction(TriggerAxis.action.ReadValue<float>());
            GripReaction(GripAxis.action.ReadValue<float>());
            ThumbstickReaction(ThumbstickAxis.action.ReadValue<Vector2>());
            Button1Reaction(PrimaryButtonClick.action.ReadValue<float>());
            Button2Reaction(SecondButtonClick.action.ReadValue<float>());
        }

        void TriggerReaction(float axis)
        {
            Trigger.transform.localRotation = Quaternion.Euler(axis * -15f, 0, 0);
        }

        Vector3 Grip_OriginPos;
        void GripReaction(float axis)
        {
            if (IsLeft)
                Grip.transform.localPosition = Grip_OriginPos + Vector3.right * axis * 0.002f;
            else
                Grip.transform.localPosition = Grip_OriginPos + Vector3.left * axis * 0.002f;
        }

        void ThumbstickReaction(Vector2 axis)
        {
            Thumbstick.transform.localRotation = Quaternion.Euler(axis.y * -25f, 0, axis.x * 25f);
        }

        Vector3 Button1_OriginPos;
        void Button1Reaction(float axis)
        {
            Button1.transform.localPosition = Button1_OriginPos + Vector3.down * (axis > 0.5f ? 0.00125f : 0);
        }

        Vector3 Button2_OriginPos;
        void Button2Reaction(float axis)
        {
            Button2.transform.localPosition = Button2_OriginPos + Vector3.down * (axis > 0.5f ? 0.00125f : 0);
        }

        void Button1Reaction(bool click)
        {
            Button1.transform.localPosition = Button1_OriginPos + Vector3.down * (click ? 0.00125f : 0);
        }

        void Button2Reaction(bool click)
        {
            Button2.transform.localPosition = Button2_OriginPos + Vector3.down * (click ? 0.00125f : 0);
        }
    }
}