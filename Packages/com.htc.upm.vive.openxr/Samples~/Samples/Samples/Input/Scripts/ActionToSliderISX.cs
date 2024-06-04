using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToSliderISX : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference m_ActionReference;
        public InputActionReference actionReference { get => m_ActionReference; set => m_ActionReference = value; }

        [SerializeField]
        Slider slider = null;

        Graphic graphic = null;
        Graphic[] graphics = new Graphic[]{ };

        private void OnEnable()
        {
            if (slider == null)
                Debug.LogWarning("ActionToSlider Monobehaviour started without any associated slider. This input will not be reported.", this);

            graphic = gameObject.GetComponent<Graphic>();
            graphics = gameObject.GetComponentsInChildren<Graphic>();
        }

        void Update()
        {
            if (actionReference != null && actionReference.action != null && slider != null)
            {
                if (actionReference.action.enabled)
                {
                    SetVisible(true);
                }

                float value = actionReference.action.ReadValue<float>();
                slider.value = value;
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