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
    private bool isHighlighting = false;
    private Ray ray;

    private void Start()
    {
        inputActions = new BasisInputModuleHandler();
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
                ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;

                if (devices.InputState.Trigger == 1)
                {
                    HandleObjectInteraction();
                }
                else if (currentlyHeldObject != null)
                {
                    currentlyHeldObject.Drop();
                    currentlyHeldObject = null;
                }

                InteractableObject previousHighlighted = currentlyHighlightedObject;
                currentlyHighlightedObject = null;

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    if (hit.collider == null) return;
                    InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

                    if (interactable != null && !interactable.IsHeld() && interactable.IsWithinRange(transform))
                    {
                        currentlyHighlightedObject = interactable;
                        if (!isHighlighting)
                        {
                            interactable.HighlightObject(true);
                            isHighlighting = true;
                        }
                    }
                }

                if (previousHighlighted != null && previousHighlighted != currentlyHighlightedObject)
                {
                    previousHighlighted.HighlightObject(false);
                    isHighlighting = false;
                }
            }
        }
    }

    private void HandleObjectInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider == null) return;
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null && !interactable.IsHeld() && interactable.IsWithinRange(transform))
            {

                if (currentlyHeldObject != null)
                {
                    currentlyHeldObject.Drop();
                    currentlyHeldObject = null;
                }
                else
                {
                    BasisObjectSyncNetworking syncNetworking = interactable.GetComponent<BasisObjectSyncNetworking>();
                    if (syncNetworking != null && !syncNetworking.IsOwner)
                    {
                        currentlyHighlightedObject.PickUp(transform);
                        currentlyHeldObject = currentlyHighlightedObject;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * raycastDistance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * raycastDistance, 0.05f);
    }
}
