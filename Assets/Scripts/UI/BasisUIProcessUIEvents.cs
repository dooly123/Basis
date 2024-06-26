using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
public class BasisUIProcessUIEvents : BaseInputModule
{
    [Header("Configuration")]
    [Tooltip("The maximum time (in seconds) between two mouse presses for it to be consecutive click.")]
    public float m_ClickSpeed = 0.3f;
    [Tooltip("The absolute value required by a move action on either axis required to trigger a move event.")]
    public float m_MoveDeadzone = 0.6f;
    [Tooltip("The Initial delay (in seconds) between an initial move action and a repeated move action.")]
    public float m_RepeatDelay = 0.5f;
    [SerializeField, Tooltip("The speed (in seconds) that the move action repeats itself once repeating.")]
    public float m_RepeatRate = 0.1f;
    public float m_TrackedDeviceDragThresholdMultiplier = 1.4f;
    [SerializeField, Tooltip("Scales the scrollDelta in event data, for tracked devices, to scroll at an expected speed.")]
    public float m_TrackedScrollDeltaMultiplier = 5f;
    public Camera m_UICamera;
    AxisEventData m_CachedAxisEvent;
    /// <summary>
    /// See <a href="https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInputModule.html#UnityEngine_EventSystems_BaseInputModule_Process">BaseInputModule.Process()</a>.
    /// </summary>
    public override void Process()
    {
        // Postpone processing until later in the frame
    }
    /// <summary>
    /// Sends an update event to the currently selected object.
    /// </summary>
    /// <returns>Returns whether the update event was used by the selected object.</returns>
    public static bool SendUpdateEventToSelectedObject(BaseEventData Data)
    {
        var selectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (selectedGameObject == null)
        {
            return false;
        }

        ExecuteEvents.Execute(selectedGameObject, Data, ExecuteEvents.updateSelectedHandler);
        return Data.used;
    }
    /// <summary>
    /// Takes an existing <see cref="MouseModel"/> and dispatches all relevant changes through the event system.
    /// It also updates the internal data of the <see cref="MouseModel"/>.
    /// </summary>
    /// <param name="mouseState">The mouse state you want to forward into the UI Event System.</param>
    internal void ProcessMouseState(ButtonDeltaState ButtonDeltaState, PointerEventData PointerEventData,GameObject CurrentlySelected)
    {
        PointerEventData.Reset();
        // Left Mouse Button
        // The left mouse button is 'dominant' and we want to also process hover and scroll events as if the occurred during the left click.
        PointerEventData.button = PointerEventData.InputButton.Left;
        ProcessPointerButton(ButtonDeltaState, PointerEventData);

        ProcessPointerMovement(PointerEventData, CurrentlySelected);
        ProcessScrollWheel(PointerEventData);
        ProcessPointerButtonDrag(PointerEventData, UIPointerType.MouseOrPen);
        PointerEventData.button = PointerEventData.InputButton.Right;
        ProcessPointerButton(ButtonDeltaState, PointerEventData);
        ProcessPointerButtonDrag(PointerEventData, UIPointerType.MouseOrPen);
        // Middle Mouse Button
        PointerEventData.button = PointerEventData.InputButton.Middle;
        ProcessPointerButton(ButtonDeltaState, PointerEventData);
        ProcessPointerButtonDrag(PointerEventData, UIPointerType.MouseOrPen);
    }
    void ProcessPointerMovement(PointerEventData eventData,GameObject CurrentGameobject)
    {
        // If the pointer moved, send move events to all UI elements the pointer is
        // currently over.
        var wasMoved = eventData.IsPointerMoving();
        if (wasMoved)
        {
            for (var i = 0; i < eventData.hovered.Count; ++i)
            {
                ExecuteEvents.Execute(eventData.hovered[i], eventData, ExecuteEvents.pointerMoveHandler);
            }
        }
        // If we have no target or pointerEnter has been deleted,
        // we just send exit events to anything we are tracking
        // and then exit.
        if (CurrentGameobject == null || eventData.pointerEnter == null)
        {
            foreach (var hovered in eventData.hovered)
            {
                ExecuteEvents.Execute(hovered, eventData, ExecuteEvents.pointerExitHandler);
            }

            eventData.hovered.Clear();

            if (CurrentGameobject == null)
            {
                eventData.pointerEnter = null;
                return;
            }
        }

        if (eventData.pointerEnter == CurrentGameobject)
            return;

        var commonRoot = FindCommonRoot(eventData.pointerEnter, CurrentGameobject);

        // We walk up the tree until a common root and the last entered and current entered object is found.
        // Then send exit and enter events up to, but not including, the common root.
        if (eventData.pointerEnter != null)
        {
            var target = eventData.pointerEnter.transform;

            while (target != null)
            {
                if (commonRoot != null && commonRoot.transform == target)
                    break;

                var targetGameObject = target.gameObject;
                ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerExitHandler);

                eventData.hovered.Remove(targetGameObject);

                target = target.parent;
            }
        }

        eventData.pointerEnter = CurrentGameobject;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- Could be null if it was destroyed immediately after executing above
        if (CurrentGameobject != null)
        {
            var target = CurrentGameobject.transform;

            while (target != null && target.gameObject != commonRoot)
            {
                var targetGameObject = target.gameObject;
                ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerEnterHandler);
                if (wasMoved)
                {
                    ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerMoveHandler);
                }
                eventData.hovered.Add(targetGameObject);

                target = target.parent;
            }
        }
    }

    void ProcessPointerButton(ButtonDeltaState mouseButtonChanges, PointerEventData eventData)
    {
        var hoverTarget = eventData.pointerCurrentRaycast.gameObject;

        if ((mouseButtonChanges & ButtonDeltaState.Pressed) != 0)
        {
            eventData.eligibleForClick = true;
            eventData.delta = Vector2.zero;
            eventData.dragging = false;
            eventData.pressPosition = eventData.position;
            eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
            eventData.useDragThreshold = true;

            var selectHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(hoverTarget);

            // If we have clicked something new, deselect the old thing
            // and leave 'selection handling' up to the press event.
            if (selectHandler != eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null, eventData);

            // search for the control that will receive the press.
            // if we can't find a press handler set the press
            // handler to be what would receive a click.

            var newPressed = ExecuteEvents.ExecuteHierarchy(hoverTarget, eventData, ExecuteEvents.pointerDownHandler);

            // We didn't find a press handler, so we search for a click handler.
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);

            var time = Time.unscaledTime;

            if (newPressed == eventData.lastPress && ((time - eventData.clickTime) < m_ClickSpeed))
                ++eventData.clickCount;
            else
                eventData.clickCount = 1;

            eventData.clickTime = time;

            eventData.pointerPress = newPressed;
            eventData.rawPointerPress = hoverTarget;

            // Save the drag handler for drag events during this mouse down.
            var dragObject = ExecuteEvents.GetEventHandler<IDragHandler>(hoverTarget);
            eventData.pointerDrag = dragObject;

            if (dragObject != null)
            {
                ExecuteEvents.Execute(dragObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        if ((mouseButtonChanges & ButtonDeltaState.Released) != 0)
        {
            var target = eventData.pointerPress;
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);

            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);
            var pointerDrag = eventData.pointerDrag;
            if (target == pointerUpHandler && eventData.eligibleForClick)
            {
                ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
            }
            else if (eventData.dragging && pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(hoverTarget, eventData, ExecuteEvents.dropHandler);
            }

            eventData.eligibleForClick = false;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;

            if (eventData.dragging && pointerDrag != null)
            {
                ExecuteEvents.Execute(pointerDrag, eventData, ExecuteEvents.endDragHandler);
            }

            eventData.dragging = false;
            eventData.pointerDrag = null;
        }
    }

    void ProcessPointerButtonDrag(PointerEventData eventData, UIPointerType pointerType, float pixelDragThresholdMultiplier = 1.0f)
    {
        if (!eventData.IsPointerMoving() ||
            (pointerType == UIPointerType.MouseOrPen && Cursor.lockState == CursorLockMode.Locked) ||
            eventData.pointerDrag == null)
        {
            return;
        }

        if (!eventData.dragging)
        {
            var threshold = eventSystem.pixelDragThreshold * pixelDragThresholdMultiplier;
            if (!eventData.useDragThreshold || (eventData.pressPosition - eventData.position).sqrMagnitude >= (threshold * threshold))
            {
                var target = eventData.pointerDrag;
                ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);
                eventData.dragging = true;
            }
        }

        if (eventData.dragging)
        {
            // If we moved from our initial press object, process an up for that object.
            var target = eventData.pointerPress;
            if (target != eventData.pointerDrag)
            {
                ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);

                eventData.eligibleForClick = false;
                eventData.pointerPress = null;
                eventData.rawPointerPress = null;
            }
            ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
        }
    }

    void ProcessScrollWheel(PointerEventData eventData)
    {
        var scrollDelta = eventData.scrollDelta;
        if (!Mathf.Approximately(scrollDelta.sqrMagnitude, 0f))
        {
            var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerEnter);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
        }
    }
    /// <summary>
    /// Takes an existing NavigationModel and dispatches all relevant changes through the event system.
    /// It also updates the internal data of the NavigationModel.
    /// </summary>
    /// <param name="navigationState">The navigation state you want to forward into the UI Event System</param>
    public void ProcessNavigationState(ref NavigationModel navigationState, GameObject Hit)
    {
        // Don't send move events if disabled in the EventSystem.
        if (!eventSystem.sendNavigationEvents)
            return;

        var implementationData = navigationState.implementationData;
        var selectedGameObject = eventSystem.currentSelectedGameObject;

        var movement = navigationState.move;
        if (Hit != null && (!Mathf.Approximately(movement.x, 0f) || !Mathf.Approximately(movement.y, 0f)))
        {
            var time = Time.unscaledTime;

            var moveDirection = MoveDirection.None;
            if (movement.sqrMagnitude > m_MoveDeadzone * m_MoveDeadzone)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                    moveDirection = (movement.x > 0f) ? MoveDirection.Right : MoveDirection.Left;
                else
                    moveDirection = (movement.y > 0f) ? MoveDirection.Up : MoveDirection.Down;
            }

            if (moveDirection != implementationData.lastMoveDirection)
            {
                implementationData.consecutiveMoveCount = 0;
            }

            if (moveDirection != MoveDirection.None)
            {
                var allow = true;
                if (implementationData.consecutiveMoveCount != 0)
                {
                    if (implementationData.consecutiveMoveCount > 1)
                        allow = (time > (implementationData.lastMoveTime + m_RepeatRate));
                    else
                        allow = (time > (implementationData.lastMoveTime + m_RepeatDelay));
                }

                if (allow)
                {
                    var eventData = GetOrCreateCachedAxisEvent();
                    eventData.Reset();

                    eventData.moveVector = movement;
                    eventData.moveDir = moveDirection;
                    ExecuteEvents.Execute(selectedGameObject, eventData, ExecuteEvents.moveHandler);
                    implementationData.consecutiveMoveCount++;
                    implementationData.lastMoveTime = time;
                    implementationData.lastMoveDirection = moveDirection;
                }
            }
            else
            {
                implementationData.consecutiveMoveCount = 0;
            }
        }
        else
        {
            implementationData.consecutiveMoveCount = 0;
        }

        if (selectedGameObject != null)
        {
            var data = GetBaseEventData();
            if ((navigationState.submitButtonDelta & ButtonDeltaState.Pressed) != 0)
            {
                ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.submitHandler);
            }

            if (!data.used && (navigationState.cancelButtonDelta & ButtonDeltaState.Pressed) != 0)
            {
                ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.cancelHandler);
            }
        }

        navigationState.implementationData = implementationData;
        navigationState.OnFrameFinished();
    }
    AxisEventData GetOrCreateCachedAxisEvent()
    {
        var result = m_CachedAxisEvent;
        if (result == null)
        {
            result = new AxisEventData(eventSystem);
            m_CachedAxisEvent = result;
        }

        return result;
    }
    [Flags]
    public enum ButtonDeltaState { NoChange, Pressed , Released }
    /// <summary>
    /// Represents the state of a navigation in the Unity UI (UGUI) system. Keeps track of various book-keeping regarding UI selection, and move and button states.
    /// </summary>
    public struct NavigationModel
    {
        public struct ImplementationData
        {
            /// <summary>
            /// Bookkeeping value for Unity UI (UGUI) that tracks the number of sequential move commands in the same direction that have been sent.  Used to handle proper repeat timing.
            /// </summary>
            public int consecutiveMoveCount { get; set; }

            /// <summary>
            /// Bookkeeping value for Unity UI (UGUI) that tracks the direction of the last move command.  Used to handle proper repeat timing.
            /// </summary>
            public MoveDirection lastMoveDirection { get; set; }

            /// <summary>
            /// Bookkeeping value for Unity UI (UGUI) that tracks the last time a move command was sent.  Used to handle proper repeat timing.
            /// </summary>
            public float lastMoveTime { get; set; }

            /// <summary>
            /// Resets this object to its default, unused state.
            /// </summary>
            public void Reset()
            {
                consecutiveMoveCount = 0;
                lastMoveTime = 0.0f;
                lastMoveDirection = MoveDirection.None;
            }
        }

        /// <summary>
        /// A 2D Vector that represents the up/down/left/right movement to apply to UI selection, such as moving up and down in options menus or highlighting options.
        /// </summary>
        public Vector2 move { get; set; }

        /// <summary>
        /// Tracks the current state of the submit or 'move forward' button.  Setting this also updates the <see cref="submitButtonDelta"/> to track if a press or release occurred in the frame.
        /// </summary>
        public bool submitButtonDown;

        /// <summary>
        /// Tracks the changes in <see cref="submitButtonDown"/> between calls to <see cref="OnFrameFinished"/>.
        /// </summary>
        internal ButtonDeltaState submitButtonDelta { get; private set; }

        /// <summary>
        /// Tracks the current state of the submit or 'move backward' button.  Setting this also updates the <see cref="cancelButtonDelta"/> to track if a press or release occurred in the frame.
        /// </summary>
        public bool cancelButtonDown;

        /// <summary>
        /// Tracks the changes in <see cref="cancelButtonDown"/> between calls to <see cref="OnFrameFinished"/>.
        /// </summary>
        internal ButtonDeltaState cancelButtonDelta { get; private set; }

        /// <summary>
        /// Internal bookkeeping data used by the Unity UI (UGUI) system.
        /// </summary>
        internal ImplementationData implementationData { get; set; }

        /// <summary>
        /// Resets this object to it's default, unused state.
        /// </summary>
        public void Reset()
        {
            move = Vector2.zero;
            m_SubmitButtonDown = m_CancelButtonDown = false;
            submitButtonDelta = cancelButtonDelta = ButtonDeltaState.NoChange;

            implementationData.Reset();
        }

        /// <summary>
        /// Call this at the end of polling for per-frame changes.  This resets delta values, such as <see cref="submitButtonDelta"/> and <see cref="cancelButtonDelta"/>.
        /// </summary>
        public void OnFrameFinished()
        {
            submitButtonDelta = ButtonDeltaState.NoChange;
            cancelButtonDelta = ButtonDeltaState.NoChange;
        }

        bool m_SubmitButtonDown;
        bool m_CancelButtonDown;
    }
}