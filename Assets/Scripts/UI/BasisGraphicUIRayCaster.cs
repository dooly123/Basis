using UnityEngine;
using UnityEngine.EventSystems;

public class BasisGraphicUIRayCaster : MonoBehaviour
{
    public Canvas Canvas;
    public void Start()
    {
        AddCanvasCollider();
        UpdateColliderSize(this.gameObject);
    }
    public void UpdateColliderSize(GameObject Parent)
    {
        Canvas = BasisHelpers.GetOrAddComponent<Canvas>(this.gameObject);
        IEventSystemHandler[] ClickHandles = Parent.GetComponentsInChildren<IEventSystemHandler>(true);

        foreach (IEventSystemHandler clickHandler in ClickHandles)
        {
            GameObject handlerGameObject = (clickHandler as MonoBehaviour).gameObject;
            if (handlerGameObject != null)
            {
                BoxCollider boxCollider = BasisHelpers.GetOrAddComponent<BoxCollider>(handlerGameObject);
                if (handlerGameObject.TryGetComponent<RectTransform>(out RectTransform RectTransform))
                {
                    Vector2 size = RectTransform.sizeDelta;
                    Vector3 newSize = new Vector3(size.x, size.y, 1); // Assuming a depth of 1 for the collider
                    boxCollider.size = newSize;
                    boxCollider.center = Vector3.zero;
                }
                else
                {
                    Debug.LogWarning("No RectTransform found on " + handlerGameObject.name);
                }
            }
        }
    }
    public void AddCanvasCollider()
    {
        if (Canvas.TryGetComponent<RectTransform>(out RectTransform canvasRectTransform))
        {
            BoxCollider canvasCollider = BasisHelpers.GetOrAddComponent<BoxCollider>(Canvas.gameObject);
            Vector2 canvasSize = canvasRectTransform.sizeDelta;
            Vector3 canvasNewSize = new Vector3(canvasSize.x, canvasSize.y, 0.1f); // Assuming a depth of 1 for the collider
            canvasCollider.size = canvasNewSize;

            // Set the collider's position to the center of the canvas
            canvasCollider.center = new Vector3(
                canvasRectTransform.rect.width * (0.5f - canvasRectTransform.pivot.x),
                canvasRectTransform.rect.height * (0.5f - canvasRectTransform.pivot.y),
                0);
        }
        else
        {
            Debug.LogWarning("No RectTransform found on the Canvas.");
        }
    }
}