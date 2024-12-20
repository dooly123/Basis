using UnityEngine;
using UnityEngine.InputSystem;
using Basis.Scripts.UI;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;

public class PlayerInteract : MonoBehaviour
{
    [Tooltip("How far the player can interact with objects.")]
    public float raycastDistance = 1.0f;

    private BasisInputModuleHandler inputActions;
    private InteractableObject currentlyHighlightedObject = null;
    private InteractableObject currentlyHeldObject = null;

    private void Start()
    {
        inputActions = new BasisInputModuleHandler(); // Instantiate generated input actions
        BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate += Simulate;
    }
    public void OnDestroy()
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate -= Simulate;
    }

    private void Simulate()
    {
        int count = BasisDeviceManagement.Instance.AllInputDevices.Count;
        for (int Index = 0; Index < count; Index++)
        {
            Basis.Scripts.Device_Management.Devices.BasisInput devices = BasisDeviceManagement.Instance.AllInputDevices[Index];
            if (devices.BasisDeviceMatchableNames.HasRayCastSupport)
            {
                if (devices.InputState.Trigger == 1)  // Check if the player is pressing the trigger
                {
                    HandleObjectInteraction();
                }
                else if (currentlyHeldObject != null)
                {
                    // If trigger is released, drop the object
                    currentlyHeldObject.Drop();
                    currentlyHeldObject = null;
                }
            }
        }
    }

    private void HandleObjectInteraction()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        InteractableObject previousHighlighted = currentlyHighlightedObject;
        currentlyHighlightedObject = null;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider == null) return;
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null && !interactable.IsHeld() && interactable.IsWithinRange(transform))
            {
                currentlyHighlightedObject = interactable;
                interactable.HighlightObject(true);  // Highlight the object if within range

                if (currentlyHeldObject != null)
                {
                    currentlyHeldObject.Drop();
                    currentlyHeldObject = null;
                }
                else
                {
                    BasisObjectSyncNetworking syncNetworking = currentlyHighlightedObject.GetComponent<BasisObjectSyncNetworking>();
                    if (syncNetworking != null && !syncNetworking.IsOwner)
                    {
                        currentlyHighlightedObject.PickUp(transform);
                        currentlyHeldObject = currentlyHighlightedObject;
                    }
                }
            }
        }

        // Remove highlight from previously highlighted object
        if (previousHighlighted != null && previousHighlighted != currentlyHighlightedObject) { previousHighlighted.HighlightObject(false); }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * raycastDistance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * raycastDistance, 0.05f);
    }
}
