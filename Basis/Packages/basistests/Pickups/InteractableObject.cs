using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pickupRange = 1.0f;

    [Header("References")]
    public Renderer objectRenderer;
    public Material highlightMaterial;
    public Material originalMaterial;
    public bool isHeld = false;
    private Rigidbody rb;

    public BasisObjectSyncNetworking syncNetworking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.sharedMaterial;
        }

        syncNetworking = GetComponent<BasisObjectSyncNetworking>();
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public bool IsWithinRange(Transform playerCamera)
    {
        return Vector3.Distance(transform.position, playerCamera.position) <= pickupRange;
    }

    public void HighlightObject(bool highlight)
    {
        // Debug log to check if HighlightObject is being called correctly
        Debug.Log("Highlighting object: " + (highlight ? "ON" : "OFF"));
        if (objectRenderer && highlightMaterial && originalMaterial)
        {
            // Log the material being applied
            Debug.Log("Applying material: " + (highlight ? highlightMaterial.name : originalMaterial.name));
            objectRenderer.sharedMaterial = highlight ? highlightMaterial : originalMaterial;
        }
    }

    public void PickUp(Transform parent)
    {
        if (!isHeld)
        {
            isHeld = true;

            // Disable physics (set Rigidbody to kinematic) while holding
            rb.isKinematic = true;

            // Parent the object to the player's hand (or camera, etc.)
            transform.SetParent(parent);

            // Store the object's local position relative to the parent
            transform.localPosition = transform.localPosition;
            transform.localRotation = transform.localRotation;

            // Update the networked data (Storeddata) to reflect the position, rotation, and scale
            syncNetworking.Storeddata.Position = transform.position;
            syncNetworking.Storeddata.Rotation = transform.rotation;
            syncNetworking.Storeddata.Scale = transform.localScale;

            // Set ownership to the local player when they pick up the object
            syncNetworking.IsOwner = true;

            // Disable object highlight once picked up
            HighlightObject(false);
        }
    }

    public void Drop()
    {
        if (isHeld)
        {
            isHeld = false;

            // Unparent the object and re-enable physics
            transform.SetParent(null);
            rb.isKinematic = false;  // Re-enable physics

            // When dropped, update the networked data to reflect the new position and rotation
            syncNetworking.Storeddata.Position = transform.position;
            syncNetworking.Storeddata.Rotation = transform.rotation;
            syncNetworking.Storeddata.Scale = transform.localScale;

            // Transfer ownership back when dropped | Local player no longer owns the object
            syncNetworking.IsOwner = false;
        }
    }
}