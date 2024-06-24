using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BasisInputModuleHandler : BaseInputModule
{
    public EventSystem EventSystem;
    private InputAction tabAction;
    private InputAction enterAction;
    private InputAction keypadEnterAction;
    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialize the input actions for Tab and Enter keys
        tabAction = new InputAction(binding: "<Keyboard>/tab");
        tabAction.performed += OnTabPerformed;
        tabAction.Enable();

        enterAction = new InputAction(binding: "<Keyboard>/enter");
        enterAction.performed += OnEnterPerformed;
        enterAction.Enable();

        // For the Keypad Enter
        keypadEnterAction = new InputAction(binding: "<Keyboard>/numpadEnter");
        keypadEnterAction.performed += OnEnterPerformed;
        keypadEnterAction.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Disable the input actions when the module is disabled
        tabAction.Disable();
        enterAction.Disable();
        keypadEnterAction.Disable();
    }

    public override void Process()
    {
        // Process your input events here
        if (EventSystem.currentSelectedGameObject != null)
        {
            var data = GetBaseEventData();
            ExecuteEvents.Execute(EventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
        }
    }

    private void OnTabPerformed(InputAction.CallbackContext context)
    {
        // Handle Tab key press
        if (context.performed)
        {
            GameObject next = FindNextSelectable(EventSystem.currentSelectedGameObject);
            if (next != null)
            {
                EventSystem.SetSelectedGameObject(next);
            }
        }
    }

    private void OnEnterPerformed(InputAction.CallbackContext context)
    {
        // Handle Enter key press
        if (context.performed)
        {
            GameObject current = EventSystem.currentSelectedGameObject;
            if (current != null)
            {
                ExecuteEvents.Execute(current, new BaseEventData(EventSystem), ExecuteEvents.submitHandler);
            }
        }
    }

    private GameObject FindNextSelectable(GameObject current)
    {
        // Logic to find the next selectable UI element
        if (current.TryGetComponent<Selectable>(out Selectable Selectable))
        {
            Selectable nextSelectable = Selectable.FindSelectableOnDown();
            return nextSelectable != null ? nextSelectable.gameObject : null;
        }
        return null;
    }
}