using UnityEngine;
using UnityEngine.EventSystems;

public class BasisGraphicUIRayCaster : MonoBehaviour
{
    public Canvas Canvas;
    public void Start()
    {
        AddCanvasCollider();
        UpdateColliderSize();
    }
    private void UpdateColliderSize()
    {
        Canvas = BasisHelpers.GetOrAddComponent<Canvas>(this.gameObject);
        IPointerClickHandler[] ClickHandles = GetComponentsInChildren<IPointerClickHandler>(true);

        foreach (IPointerClickHandler clickHandler in ClickHandles)
        {
            GameObject handlerGameObject = (clickHandler as MonoBehaviour).gameObject;
            if (handlerGameObject != null)
            {
                BoxCollider boxCollider = BasisHelpers.GetOrAddComponent<BoxCollider>(handlerGameObject);
                RectTransform rectTransform = handlerGameObject.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    Vector2 size = rectTransform.sizeDelta;
                    Vector3 newSize = new Vector3(size.x, size.y, 1); // Assuming a depth of 1 for the collider
                    boxCollider.size = newSize;

                    // Adjust the position of the collider to match the RectTransform position
                    Vector3 adjustedPosition = rectTransform.localPosition;
                    // Correct the position considering the pivot
                    Vector3 pivotCorrection = new Vector3(
                        size.x * (0.5f - rectTransform.pivot.x),
                        size.y * (0.5f - rectTransform.pivot.y),
                        0);
                    //  adjustedPosition + pivotCorrection;
                    boxCollider.center = Vector3.zero;
                }
                else
                {
                    Debug.LogWarning("No RectTransform found on " + handlerGameObject.name);
                }
            }
        }
    }
    private void AddCanvasCollider()
    {
        RectTransform canvasRectTransform = Canvas.GetComponent<RectTransform>();
        BoxCollider canvasCollider = BasisHelpers.GetOrAddComponent<BoxCollider>(Canvas.gameObject);

        if (canvasRectTransform != null)
        {
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