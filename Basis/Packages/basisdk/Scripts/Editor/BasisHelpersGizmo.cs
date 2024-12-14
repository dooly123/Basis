using System;
using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Basis.Scripts.BasisSdk.Helpers.Editor
{
    public static class BasisHelpersGizmo
    {
        public static void PositionHandler(ref Vector3 Position, Quaternion Rotation)
        {
            Position = Handles.PositionHandle(Position, Rotation);
        }
        public static Button Button(VisualElement VisualElement, string ButttonNameIdentifier)
        {
            // Find the button by name
            Button button = VisualElement.Q<Button>(ButttonNameIdentifier);

            // Check if the button is found
            if (button != null)
            {
                // Subscribe to the button click event
                return button;
            }
            else
            {
                Debug.LogError("Button not found! " + ButttonNameIdentifier);
                return null;
            }
        }
        public static EventCallback<ChangeEvent<Vector2>> CallBackVector2Field(VisualElement visualElement, string fieldNameIdentifier, Vector2 InitalValue)
        {
            Vector2Field Field = visualElement.Q<Vector2Field>(fieldNameIdentifier);
            if (Field != null)
            {
                Field.value = InitalValue;
                var changeEvent = new EventCallback<ChangeEvent<Vector2>>(evt => OnVector2FieldValueChanged(evt));
                Field.RegisterCallback(changeEvent);
                return changeEvent;
            }
            else
            {
                return null;
            }
        }

        private static void OnVector2FieldValueChanged(ChangeEvent<Vector2> evt)
        {
        }

        public static void SetValueVector2Field(VisualElement visualElement, string fieldNameIdentifier, Vector2 Value)
        {
            Vector2Field Field = visualElement.Q<Vector2Field>(fieldNameIdentifier);
            if (Field != null)
            {
                Field.value = Value;
            }
        }
        public static EventCallback<ChangeEvent<Vector3>> CallBackVector3Field(VisualElement visualElement, string fieldNameIdentifier, Vector3 InitalValue)
        {
            Vector3Field Field = visualElement.Q<Vector3Field>(fieldNameIdentifier);
            if (Field != null)
            {
                Field.value = InitalValue;
                var changeEvent = new EventCallback<ChangeEvent<Vector3>>(evt => OnVector3FieldValueChanged(evt));
                Field.RegisterCallback(changeEvent);
                return changeEvent;
            }
            else
            {
                return null;
            }
        }

        private static void OnVector3FieldValueChanged(ChangeEvent<Vector3> evt)
        {
        }

        public static void SetValueVector3Field(VisualElement visualElement, string fieldNameIdentifier, Vector3 Value)
        {
            Vector3Field Field = visualElement.Q<Vector3Field>(fieldNameIdentifier);
            if (Field != null)
            {
                Field.value = Value;
            }
        }
    }
}