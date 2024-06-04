using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToButtonISX : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference m_ActionReference;
        public InputActionReference actionReference { get => m_ActionReference ; set => m_ActionReference = value; }

        [SerializeField]
        Color enabledColor = Color.green;

        [SerializeField]
        Color disabledColor = Color.red;

        [SerializeField]
        Image image = null;

        Graphic graphic = null;
        Graphic[] graphics = new Graphic[] { };

        private void OnEnable()
        {
            if (image == null)
                Debug.LogWarning("ActionToButton Monobehaviour started without any associated image. This input will not be reported.", this);

            graphic = gameObject.GetComponent<Graphic>();
            graphics = gameObject.GetComponentsInChildren<Graphic>();
        }

        Type lastActiveType = null;

        void Update()
        {
            if (actionReference != null && actionReference.action != null && image != null && actionReference.action.enabled && actionReference.action.controls.Count > 0)
            {
                SetVisible(true);

                Type typeToUse = null;

                if (actionReference.action.activeControl != null)
                {
                    typeToUse = actionReference.action.activeControl.valueType;
                }
                else
                {
                    typeToUse = lastActiveType;
                }

                if(typeToUse == typeof(bool))
                {
                    lastActiveType = typeof(bool);
                    bool value = actionReference.action.ReadValue<bool>();
                    image.color = value ? enabledColor : disabledColor;
                }
                else if(typeToUse == typeof(float))
                {
                    lastActiveType = typeof(float);
                    float value = actionReference.action.ReadValue<float>();
                    image.color = value > 0.5 ? enabledColor : disabledColor;
                }
                else
                {
                    image.color = disabledColor;
                }
            }
            else
            {
                SetVisible(false);
            }
        }

        void SetVisible(bool visible)
        {
            if (graphic != null)
                graphic.enabled = visible;

            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].enabled = visible;
            }
        }
    }

}
