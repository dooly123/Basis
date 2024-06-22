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
    public Material lineMaterial;
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
    public bool WasLastDown = false;
    public GameObject LastHit;
    public PointerEventData pointerEventData;
    public Canvas FoundCanvas;
    public RaycastResult RaycastResult = new RaycastResult();
    public float Trigger;
    public async Task Initialize(BasisDeviceMatchableNames basisDeviceMatchableNames)
    {
        BasisDeviceMatchableNames = basisDeviceMatchableNames;
        ApplyStaticDataToRaycastResult();
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
            await CreateRadical();
            HasRedicalRenderer = true;
        }
        CachedLinerRenderState = HasLineRenderer;
    }
    public async Task CreateRadical()
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
    public void ApplyStaticDataToRaycastResult()
    {
        RaycastResult.displayIndex = 0;
        RaycastResult.index = 0;
        RaycastResult.depth = 0;
        RaycastResult.module = null;
    }
    public void RayCastUI(float trigger)
    {
        if (CheckRayCast())
        {
            HandleValidHit(trigger);
        }
        else
        {
            HandleNoHit();
        }
    }

    private void HandleValidHit(float trigger)
    {
        if (hit.transform.gameObject.layer == UIMask)
        {
            UpdateRayCastResult();
            HandleUIEvents(trigger);
            UpdateLineRenderer();
            UpdateRadicalRenderer();
            UpdatePointerState();
            UpdateDebugDraw(Color.green, hit.point);
        }
        else
        {
            ResetRenderers();
            UpdateDebugDraw(Color.blue, hit.point);
        }
    }

    private void UpdateRayCastResult()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        RaycastResult.gameObject = hit.transform.gameObject;
        RaycastResult.distance = hit.distance;
        RaycastResult.screenPosition = BasisLocalCameraDriver.Instance.Camera.WorldToScreenPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
        FoundCanvas = hit.transform.gameObject.GetComponentInParent<Canvas>();
        if (FoundCanvas != null)
        {
            RaycastResult.sortingLayer = FoundCanvas.sortingLayerID;
            RaycastResult.sortingOrder = FoundCanvas.sortingOrder;
        }
        RaycastResult.worldPosition = ray.origin + ray.direction * hit.distance;
        RaycastResult.worldNormal = hit.normal;
        pointerEventData.pointerCurrentRaycast = RaycastResult;
        pointerEventData.position = RaycastResult.screenPosition;
        pointerEventData.pressPosition = RaycastResult.screenPosition;
    }

    private void HandleUIEvents(float trigger)
    {
        Trigger = trigger;
        if (Trigger != LastTrigger)
        {
            if (Trigger == 1)
            {
                ExecuteClickHandler();
                if (!WasLastDown)
                {
                    WasLastDown = true;
                    ExecuteDownHandler();
                }
            }
            else
            {
                if (WasLastDown)
                {
                    ExecuteUpHandler();
                    WasLastDown = true;
                }
            }
            LastTrigger = Trigger;
        }

        ExecuteMoveHandler();
        HandlePointerEnterAndExit();
    }

    private void ExecuteClickHandler()
    {
        ExecuteEvents.Execute<IPointerClickHandler>(hit.transform.gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
    }

    private void ExecuteDownHandler()
    {
        ExecuteEvents.Execute<IPointerDownHandler>(hit.transform.gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);
    }

    private void ExecuteUpHandler()
    {
        ExecuteEvents.Execute<IPointerUpHandler>(hit.transform.gameObject, pointerEventData, ExecuteEvents.pointerUpHandler);
    }

    private void ExecuteMoveHandler()
    {
        ExecuteEvents.Execute<IPointerMoveHandler>(hit.transform.gameObject, pointerEventData, ExecuteEvents.pointerMoveHandler);
    }

    private void HandlePointerEnterAndExit()
    {
        if (LastHit != hit.transform.gameObject)
        {
            ExecuteEvents.Execute<IPointerEnterHandler>(hit.transform.gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
            if (LastHit != null)
            {
                ExecuteEvents.Execute<IPointerExitHandler>(LastHit, pointerEventData, ExecuteEvents.pointerExitHandler);
            }
        }
        LastHit = hit.transform.gameObject;
    }

    private void UpdateLineRenderer()
    {
        if (HasLineRenderer && !CachedLinerRenderState)
        {
            LineRenderer.enabled = true;
            CachedLinerRenderState = true;
        }
        else if (!HasLineRenderer && CachedLinerRenderState)
        {
            LineRenderer.enabled = false;
            CachedLinerRenderState = false;
        }

        if (HasLineRenderer)
        {
            LineRenderer.SetPosition(0, ray.origin);
            LineRenderer.SetPosition(1, hit.point);
        }
    }

    private void UpdateRadicalRenderer()
    {
        if (HasRedicalRenderer)
        {
            if (hit.transform != null)
            {
                highlightQuadInstance.SetActive(true);
                highlightQuadInstance.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0));
            }
            else
            {
                highlightQuadInstance.SetActive(false);
            }
        }
    }

    private void UpdatePointerState()
    {
        WasLastDown = Trigger == 1;
    }

    private void ResetRenderers()
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
    private void UpdateDebugDraw(Color color,Vector3 Destination)
    {
        Debug.DrawLine(ray.origin, Destination, color);
    }
    private void HandleNoHit()
    {
        ResetRenderers();
        UpdateDebugDraw(Color.red,transform.forward * MaxDistance);
    }
    public bool CheckRayCast()
    {
        ray = new Ray(transform.position, transform.forward);
        return Physics.Raycast(ray, out hit, MaxDistance, Mask, TriggerInteraction);
    }
}