using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToVisibilityISX : MonoBehaviour
    {

        [SerializeField]
        InputActionProperty m_ActionReference;
        public InputActionProperty actionReference { get => m_ActionReference; set => m_ActionReference = value; }


        [SerializeField]
        GameObject m_TargetGameobject = null;
        public GameObject targetGameObject { get => m_TargetGameobject; set => m_TargetGameobject = value; }

        private void Start()
        {
            if (m_ActionReference != null && m_ActionReference.action != null)
                m_ActionReference.action.Enable();
        }

        void Update()
        {
            if (m_TargetGameobject == null)
                return;

            if (m_ActionReference != null
                && m_ActionReference.action != null
                && m_ActionReference.action.controls.Count > 0
                && m_ActionReference.action.enabled == true)
            {
                m_TargetGameobject.SetActive(true);
                return;
            }
            else
            {
                // No Matching devices:
                m_TargetGameobject.SetActive(false);
            }
        }
    }
}
