using UnityEngine;
using UnityEngine.EventSystems;
public class BasisPointRaycaster : MonoBehaviour
{
    public Vector3 Direction = Vector3.forward;
    public float MaxDistance = 30;
    public LayerMask Mask;
    public LayerMask UIMask;
    public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.UseGlobal;
    public Ray ray;
    public RaycastHit hit;

    public Vector3 startPoint;
    public Vector3 endPoint;
    public Material lineMaterial;
 //   public Color lineColor = Color.white;
    public float lineWidth = 0.01f;

    public LineRenderer LineRenderer;
    public bool CachedLinerRenderState = false;
    public bool HasLineRenderer = false;
    public static string LoadMaterialAddress = "Assets/UI/Material/RayCastMaterial.mat";
    public async void Start()
    {
        // Get the layer number for "Ignore Raycast" layer
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        UIMask = LayerMask.NameToLayer("UI");
        // Create a LayerMask that includes all layers
        LayerMask allLayers = ~0;

        // Exclude the "Ignore Raycast" layer using bitwise AND and NOT operations
        Mask = allLayers & ~(1 << ignoreRaycastLayer);
        HasLineRenderer = false;
        // Add a Line Renderer component to the GameObject
        LineRenderer = BasisHelpers.GetOrAddComponent<LineRenderer>(this.gameObject);
        LineRenderer.enabled = false;
        AddressableGenericResource Resource = new AddressableGenericResource(LoadMaterialAddress, AddressableExpectedResult.SingleItem);
        if (await AddressableLoadFactory.LoadAddressableResourceAsync<Material>(Resource))
        {
            lineMaterial = (Material)Resource.Handles[0].Result;
            // Set the Line Renderer properties
            LineRenderer.material = lineMaterial;
        }
        LineRenderer.startWidth = lineWidth;
        LineRenderer.endWidth = lineWidth;

        // Set the number of points in the Line Renderer
        LineRenderer.positionCount = 2;
        HasLineRenderer = true;
        LineRenderer.enabled = HasLineRenderer;
        CachedLinerRenderState = HasLineRenderer;
    }
    public float LastTrigger = 0;
    public void RayCastUI(float Trigger)
    {
        if (CheckRayCast())
        {
            // Ray hit the collider, get the hit point
            Vector3 hitPoint = hit.point;
            // Debug.Log("Hit point: " + hitPoint);

            // Optionally, you can visualize the ray and hit point
            // Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            if (hit.transform.gameObject.layer == UIMask)
            {
                if (CachedLinerRenderState == false)
                {
                    LineRenderer.enabled = true;
                    CachedLinerRenderState = true;
                }
                LineRenderer.SetPosition(0, ray.origin);
                LineRenderer.SetPosition(1, hitPoint);
                if (Trigger != LastTrigger)
                {
                    if (Trigger == 1)
                    {
                        TriggerClick(hit.transform.gameObject);
                    }
                    LastTrigger = Trigger;
                }
            }
            else
            {
                if (CachedLinerRenderState)
                {
                    LineRenderer.enabled = false;
                    CachedLinerRenderState = false;
                }
            }
        }
        else
        {
            if (CachedLinerRenderState)
            {
                LineRenderer.enabled = false;
                CachedLinerRenderState = false;
            }
           // LineRenderer.SetPosition(0, ray.origin);
           // LineRenderer.SetPosition(1, transform.forward * MaxDistance);
        }
    }
    void TriggerClick(GameObject target)
    {
        Debug.Log("triggering for " + target);
        // Create a pointer event data instance
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

        // Execute the IPointerClickHandler interface on the target object
        ExecuteEvents.Execute<IPointerClickHandler>(target, pointerEventData, ExecuteEvents.pointerClickHandler);
    }
    public bool CheckRayCast()
    {
        ray = new Ray(transform.position, transform.forward);
        return Physics.Raycast(ray, out hit, MaxDistance, Mask, TriggerInteraction);
    }
}