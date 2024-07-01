using System;
using UnityEngine;
[System.Serializable]
public class BasisInputState
{
    public event Action OnGripButtonChanged;
    public event Action OnMenuButtonChanged;
    public event Action OnPrimaryButtonGetStateChanged;
    public event Action OnSecondaryButtonGetStateChanged;
    public event Action OnSecondary2DAxisClickChanged;
    public event Action OnPrimary2DAxisClickChanged;
    public event Action OnTriggerChanged;
    public event Action OnPrimary2DAxisChanged;
    public event Action OnSecondary2DAxisChanged;

    private bool gripButton;
    private bool menuButton;
    private bool primaryButtonGetState;
    private bool secondaryButtonGetState;
    private bool secondary2DAxisClick;
    private bool primary2DAxisClick;
    private float trigger;
    private Vector2 primary2DAxis;
    private Vector2 secondary2DAxis;

    public bool GripButton
    {
        get => gripButton;
        set
        {
            if (gripButton != value)
            {
                gripButton = value;
                OnGripButtonChanged?.Invoke();
            }
        }
    }

    public bool MenuButton
    {
        get => menuButton;
        set
        {
            if (menuButton != value)
            {
                menuButton = value;
                OnMenuButtonChanged?.Invoke();
            }
        }
    }

    public bool PrimaryButtonGetState
    {
        get => primaryButtonGetState;
        set
        {
            if (primaryButtonGetState != value)
            {
                primaryButtonGetState = value;
                OnPrimaryButtonGetStateChanged?.Invoke();
            }
        }
    }

    public bool SecondaryButtonGetState
    {
        get => secondaryButtonGetState;
        set
        {
            if (secondaryButtonGetState != value)
            {
                secondaryButtonGetState = value;
                OnSecondaryButtonGetStateChanged?.Invoke();
            }
        }
    }

    public bool Secondary2DAxisClick
    {
        get => secondary2DAxisClick;
        set
        {
            if (secondary2DAxisClick != value)
            {
                secondary2DAxisClick = value;
                OnSecondary2DAxisClickChanged?.Invoke();
            }
        }
    }

    public bool Primary2DAxisClick
    {
        get => primary2DAxisClick;
        set
        {
            if (primary2DAxisClick != value)
            {
                primary2DAxisClick = value;
                OnPrimary2DAxisClickChanged?.Invoke();
            }
        }
    }

    public float Trigger
    {
        get => trigger;
        set
        {
            if (Math.Abs(trigger - value) > 0.0001f)
            {
                trigger = value;
                OnTriggerChanged?.Invoke();
            }
        }
    }

    public Vector2 Primary2DAxis
    {
        get => primary2DAxis;
        set
        {
            if (primary2DAxis != value)
            {
                primary2DAxis = value;
                OnPrimary2DAxisChanged?.Invoke();
            }
        }
    }

    public Vector2 Secondary2DAxis
    {
        get => secondary2DAxis;
        set
        {
            if (secondary2DAxis != value)
            {
                secondary2DAxis = value;
                OnSecondary2DAxisChanged?.Invoke();
            }
        }
    }
}