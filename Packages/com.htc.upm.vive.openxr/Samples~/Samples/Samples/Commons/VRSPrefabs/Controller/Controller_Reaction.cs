using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Controller_Reaction : MonoBehaviour
{
    public string LorR = "L";
    public InputActionAsset Asset;
    public InputActionReference TriggerR;
    public InputActionReference GripR;
    public InputActionReference JoyStickR;
    public InputActionReference Button1R;
    public InputActionReference Button2R;

    GameObject Trigger;
    GameObject Grip;
    GameObject JoyStick;
    GameObject Button1;
    GameObject Button2;

    private void OnEnable()
    {
        if (!Asset.enabled)
        {
            Asset.Enable();
        }
    }

    void Start()
    {
        Trigger = transform.GetChild(0).gameObject;
        Grip = transform.GetChild(1).gameObject;
        JoyStick = transform.GetChild(2).gameObject;
        Button1 = transform.GetChild(3).gameObject;
        Button2 = transform.GetChild(4).gameObject;
        Grip_OriginPos = Grip.transform.localPosition;
        Button1_OriginPos = Button1.transform.localPosition;
        Button2_OriginPos = Button2.transform.localPosition;
    }

    void Update()
    {
        Controller_React();
    }

    void Controller_React()
    {
        Trigger_Reaction(TriggerR.action.ReadValue<float>());
        Grip_Reaction(GripR.action.ReadValue<float>());
        JoyStick_Reaction(JoyStickR.action.ReadValue<Vector2>());
        Button1_Reaction(Button1R.action.ReadValue<float>());
        Button2_Reaction(Button2R.action.ReadValue<float>());
    }

    void Trigger_Reaction(float _Value)
    {
        Trigger.transform.localRotation = Quaternion.Euler(_Value * -15f, 0, 0);
    }

    Vector3 Grip_OriginPos;
    void Grip_Reaction(float _Value)
    {
        switch (LorR)
        {
            case "L":
                Grip.transform.localPosition = Grip_OriginPos + Vector3.right * _Value * 0.002f;
                break;
            case "R":
                Grip.transform.localPosition = Grip_OriginPos + Vector3.left * _Value * 0.002f;
                break;
        }
    }

    void JoyStick_Reaction(Vector2 _Value)
    {
        JoyStick.transform.localRotation = Quaternion.Euler(_Value.y * -25f, 0, _Value.x * 25f);
    }

    Vector3 Button1_OriginPos;
    void Button1_Reaction(float _Value)
    {
        Button1.transform.localPosition = Button1_OriginPos + Vector3.down * (_Value > 0.5f ? 0.00125f : 0);
    }

    Vector3 Button2_OriginPos;
    void Button2_Reaction(float _Value)
    {
        Button2.transform.localPosition = Button2_OriginPos + Vector3.down * (_Value > 0.5f ? 0.00125f : 0);
    }

    void Button1_Reaction(bool _Value)
    {
        Button1.transform.localPosition = Button1_OriginPos + Vector3.down * (_Value ? 0.00125f : 0);
    }

    void Button2_Reaction(bool _Value)
    {
        Button2.transform.localPosition = Button2_OriginPos + Vector3.down * (_Value ? 0.00125f : 0);
    }
}
