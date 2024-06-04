using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VIVE.OpenXR.Samples
{
    public class VRSInputManager : MonoBehaviour
    {
        [SerializeField] InputActionAsset ActionAsset;
        void OnEnable()
        {
            if (ActionAsset != null)
            {
                ActionAsset.Enable();
            }
        }

        private void Awake()
        {
            if (instance)
            {
                Destroy(instance);
            }

            instance = this;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            //buttonReferences = ButtonReferences;
            //positionReferences = PositionReferences;
            //rotationReferences = RotationReferences;
            //joystickReferences = JoysitckReferences;
        }

        void Update()
        {
            Check_Input_State();
        }

        public static VRSInputManager instance = null;

        private List<InputActionReference> Pressing_ButtonRs = new List<InputActionReference>();
        private List<InputActionReference> Releasing_ButtonRs = new List<InputActionReference>();
        private List<InputActionReference> Current_ButtonRs = new List<InputActionReference>();

        public List<InputActionReference> ButtonReferences;
        //static List<InputActionReference> buttonReferences;
        public void Check_Input_State()
        {
            Pressing_ButtonRs.Clear();
            foreach (InputActionReference _ButtonR in ButtonReferences)
            {
                if (!Current_ButtonRs.Contains(_ButtonR) && (_ButtonR.action.ReadValue<float>() > 0.5f ? true : false))
                {
                    Pressing_ButtonRs.Add(_ButtonR);
                }
            }

            Releasing_ButtonRs.Clear();
            foreach (InputActionReference _ButtonR in ButtonReferences)
            {
                if (Current_ButtonRs.Contains(_ButtonR) && (_ButtonR.action.ReadValue<float>() > 0.5f ? false : true))
                {
                    Releasing_ButtonRs.Add(_ButtonR);
                }
                else
                {

                }
            }
            Current_ButtonRs.Clear();
            foreach (InputActionReference _ButtonR in ButtonReferences)
            {
                if (_ButtonR.action.ReadValue<float>() > 0.5f ? true : false)
                {
                    Current_ButtonRs.Add(_ButtonR);
                }
            }
        }
        public bool GetButtonDown(VRSButtonReference _Button)
        {
            return Pressing_ButtonRs.Contains(ButtonReferences[(int)_Button]);
        }
        public bool GetButton(VRSButtonReference _Button)
        {
            return Current_ButtonRs.Contains(ButtonReferences[(int)_Button]);
        }
        public bool GetButtonUp(VRSButtonReference _Button)
        {
            return Releasing_ButtonRs.Contains(ButtonReferences[(int)_Button]);
        }

        public List<InputActionReference> PositionReferences;
        //static List<InputActionReference> positionReferences;
        public Vector3 GetPosition(VRSPositionReference _Component)
        {
            return PositionReferences[(int)_Component].action.ReadValue<Vector3>();
        }

        public List<InputActionReference> RotationReferences;
        //static List<InputActionReference> rotationReferences;
        public Quaternion GetRotation(VRSRotationReference _Component)
        {
            return RotationReferences[(int)_Component].action.ReadValue<Quaternion>();
        }

        public List<InputActionReference> JoysitckReferences;
        //static List<InputActionReference> joystickReferences;
        public Vector2 GetAxis(VRSHandFlag _Hand)
        {
            return JoysitckReferences[(int)_Hand].action.ReadValue<Vector2>();
        }

        public void Attach_to_Device(GameObject _Gobj, VRSDevice _AttachingDevices)
        {
            switch (_AttachingDevices)
            {
                case VRSDevice.HMD:
                    _Gobj.transform.position = GetPosition(VRSPositionReference.Head);
                    _Gobj.transform.rotation = GetRotation(VRSRotationReference.Head);
                    break;
                case VRSDevice.LeftController:
                    _Gobj.transform.position = GetPosition(VRSPositionReference.GripL);
                    _Gobj.transform.rotation = GetRotation(VRSRotationReference.GripL);
                    break;
                case VRSDevice.RightController:
                    _Gobj.transform.position = GetPosition(VRSPositionReference.GripR);
                    _Gobj.transform.rotation = GetRotation(VRSRotationReference.GripR);
                    break;
            }
        }
    }
}
