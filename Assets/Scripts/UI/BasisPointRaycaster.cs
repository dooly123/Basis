using System.Collections.Generic;
using System.Threading.Tasks;
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
    public bool HasRedicalRenderer = false;
    public static string LoadMaterialAddress = "Assets/UI/Material/RayCastMaterial.mat";
    public static string LoadUIRedicalAddress = "Assets/UI/Prefabs/highlightQuad.prefab";
    public BasisDeviceMatchableNames BasisDeviceMatchableNames;
    public GameObject highlightQuadInstance;
    public float LastTrigger = 0;
    public async Task Initalize(BasisDeviceMatchableNames basisDeviceMatchableNames)
    {
        BasisDeviceMatchableNames = basisDeviceMatchableNames;
        // Get the layer number for "Ignore Raycast" layer
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        UIMask = LayerMask.NameToLayer("UI");
        // Create a LayerMask that includes all layers
        LayerMask allLayers = ~0;

        // Exclude the "Ignore Raycast" layer using bitwise AND and NOT operations
        Mask = allLayers & ~(1 << ignoreRaycastLayer);
        HasLineRenderer = false;
        HasRedicalRenderer = false;
        if (BasisDeviceMatchableNames.HasRayCastVisual)
        {
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
        }
        if (BasisDeviceMatchableNames.HasRayCastRedical)
        {
            await CreateRedical();
            HasRedicalRenderer = true;
        }
        CachedLinerRenderState = HasLineRenderer;
    }
    public async Task CreateRedical()
    {
        (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadUIRedicalAddress, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
        List<GameObject> gameObjects = data.Item1;
        if (gameObjects == null)
        {
            return;
        }
        if (gameObjects.Count != 0)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.name = BasisDeviceMatchableNames.DeviceID + "_Redical";
                gameObject.transform.parent = this.transform;
                highlightQuadInstance = gameObject;
            }
        }
    }
    public void RayCastUI(float Trigger)
    {
        if (CheckRayCast())
        {
            if (hit.transform.gameObject.layer == UIMask)
            {
                if (HasLineRenderer)
                {
                    if (CachedLinerRenderState == false)
                    {
                        LineRenderer.enabled = true;
                        CachedLinerRenderState = true;
                    }
                    LineRenderer.SetPosition(0, ray.origin);
                    LineRenderer.SetPosition(1, hit.point);
                }
                if(HasRedicalRenderer)
                {
                    highlightQuadInstance.SetActive(true);
                    highlightQuadInstance.transform.position = hit.point;
                    highlightQuadInstance.transform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0);
                }
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
                if (CachedLinerRenderState && HasLineRenderer)
                {
                    LineRenderer.enabled = false;
                    CachedLinerRenderState = false;
                }
                if (HasRedicalRenderer)
                {
                    highlightQuadInstance.SetActive(false);
                }
            }
        }
        else
        {
            if (CachedLinerRenderState && HasLineRenderer)
            {
                LineRenderer.enabled = false;
                CachedLinerRenderState = false;
            }
            if (HasRedicalRenderer)
            {
                highlightQuadInstance.SetActive(false);
            }
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