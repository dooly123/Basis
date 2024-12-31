using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Enums;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.Drivers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Basis.Scripts.UI
{
    public partial class BasisPointRaycaster : BaseRaycaster
    {
        public Vector3 Direction = Vector3.forward;
        public float MaxDistance = 30;
        public LayerMask Mask;
        public LayerMask UIMask;
        public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.UseGlobal;
        public Ray ray;
        public RaycastHit PhysicHit;
        public Material lineMaterial;
        public float lineWidth = 0.01f;
        public LineRenderer LineRenderer;
        public bool CachedLinerRenderState = false;
        public bool HasLineRenderer = false;
        public bool HasRedicalRenderer = false;
        public static string LoadMaterialAddress = "Assets/UI/Material/RayCastMaterial.mat";
        public static string LoadUIRedicalAddress = "Assets/UI/Prefabs/highlightQuad.prefab";
        public BasisDeviceMatchSettings BasisDeviceMatchableNames;
        public GameObject highlightQuadInstance;
        public Canvas FoundCanvas;
        public RaycastResult RaycastResult = new RaycastResult();
        public BasisInput BasisInput;
        public BasisPointerEventData CurrentEventData;
        public bool HadRaycastUITarget = false;
        public bool WasCorrectLayer = false;
        static readonly Vector3[] s_Corners = new Vector3[4];
        static readonly RaycastHitComparer s_RaycastHitComparer = new RaycastHitComparer();
        [SerializeField]
        public List<RaycastHitData> SortedGraphics = new List<RaycastHitData>();
        [SerializeField]
        public List<RaycastResult> SortedRays = new List<RaycastResult>();
        public bool IgnoreReversedGraphics;
        public override Camera eventCamera => BasisLocalCameraDriver.Instance.Camera;
        public static string UILayer = "UI";
        public static string Player = "Player";
        public static string IgnoreRayCastLayer = "Ignore Raycast";
        public async Task Initialize(BasisInput basisInput)
        {
            CurrentEventData = new BasisPointerEventData(EventSystem.current);
            BasisInput = basisInput;
            BasisDeviceMatchableNames = BasisInput.BasisDeviceMatchableNames;
            ApplyStaticDataToRaycastResult();
            // Get the layer number for "Ignore Raycast" layer
            int ignoreRaycastLayer = LayerMask.NameToLayer(IgnoreRayCastLayer);

            // Get the layer number for "Player" layer
            int playerLayer = LayerMask.NameToLayer(Player);

            // Get the layer number for UI Mask
             UIMask = LayerMask.NameToLayer(UILayer);

            // Create a LayerMask that includes all layers
            LayerMask allLayers = ~0;

            // Exclude the "Ignore Raycast" and "Player" layers using bitwise AND and NOT operations
             Mask = allLayers & ~(1 << ignoreRaycastLayer) & ~(1 << playerLayer);
            HasLineRenderer = false;
            HasRedicalRenderer = false;
            // Create the ray with the adjusted starting position and direction
            ray = new Ray(Vector3.zero, transform.forward);
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
                LineRenderer.numCapVertices = 12;
                LineRenderer.numCornerVertices = 12;
                LineRenderer.gameObject.layer = LayerMask.NameToLayer(UILayer);
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
            ChecksRequired Required = new ChecksRequired
            {
                UseContentRemoval = false,
                DisableAnimatorEvents = false
            };
            (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadUIRedicalAddress, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(), Required);
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
                    gameObject.transform.SetParent(this.transform);
                    highlightQuadInstance = gameObject;
                    if (highlightQuadInstance.TryGetComponent(out Canvas Canvas))
                    {
                        Canvas.worldCamera = BasisLocalCameraDriver.Instance.Camera;
                    }
                }
            }
        }
        public void ApplyStaticDataToRaycastResult()
        {
            RaycastResult.displayIndex = 0;
            RaycastResult.index = 0;
            RaycastResult.depth = 0;
            RaycastResult.module = this;
        }
        public void RayCastUI()
        {
            HadRaycastUITarget = CheckRayCast();
            if (HadRaycastUITarget)
            {
                HandleValidHit();
            }
            else
            {
                HandleNoHit();
            }
        }
        private void HandleValidHit()
        {
            WasCorrectLayer = PhysicHit.transform.gameObject.layer == UIMask;
            if (WasCorrectLayer)
            {
                UpdateRayCastResult();//sets all RaycastResult data
                UpdateLineRenderer();//updates the line denderer
                UpdateRadicalRenderer();// moves the redical renderer
            }
            else
            {
                ResetRenderers();
            }
        }
        public Vector2 ScreenPoint;
        public bool UseWorldPosition = true;
        private void UpdateRayCastResult()
        {
            RaycastResult.gameObject = PhysicHit.transform.gameObject;
            RaycastResult.distance = PhysicHit.distance;
            if (UseWorldPosition)
            {
                ScreenPoint = BasisLocalCameraDriver.Instance.Camera.WorldToScreenPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
            }
            else
            {
                // we assign screenpoint manually example in BasisLocalCameraDriver
            }
            RaycastResult.screenPosition = ScreenPoint;
            FoundCanvas = PhysicHit.transform.gameObject.GetComponentInParent<Canvas>();
            if (FoundCanvas != null)
            {
                RaycastResult.sortingLayer = FoundCanvas.sortingLayerID;
                RaycastResult.sortingOrder = FoundCanvas.sortingOrder;
            }
            RaycastResult.worldPosition = ray.origin + ray.direction * PhysicHit.distance;
            RaycastResult.worldNormal = PhysicHit.normal;
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
                LineRenderer.SetPosition(1, PhysicHit.point);
            }
        }

        private void UpdateRadicalRenderer()
        {
            if (HasRedicalRenderer)
            {
                if (PhysicHit.transform != null)
                {
                    if (BasisDeviceManagement.IsUserInDesktop() && BasisCursorManagement.ActiveLockState() != CursorLockMode.Locked)
                    {
                        highlightQuadInstance.SetActive(false);
                    }
                    else
                    {
                        highlightQuadInstance.SetActive(true);
                        highlightQuadInstance.transform.SetPositionAndRotation(PhysicHit.point, Quaternion.LookRotation(PhysicHit.normal));
                    }
                }
                else
                {
                    highlightQuadInstance.SetActive(false);
                }
            }
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
        private void HandleNoHit()
        {
            ResetRenderers();
            RaycastResult = new RaycastResult();
            PhysicHit = new RaycastHit();
        }
        public Vector3 LastRotation;
        public List<Canvas> Results = new List<Canvas>();
        public bool CheckRayCast()
        {
            if (LastRotation != BasisDeviceMatchableNames.RotationRaycastOffset)
            {
                this.transform.localRotation = Quaternion.Euler(BasisDeviceMatchableNames.RotationRaycastOffset);
                LastRotation = BasisDeviceMatchableNames.RotationRaycastOffset;
            }
            if (UseWorldPosition)
            {
                transform.GetPositionAndRotation(out Vector3 Position, out Quaternion Rotation);
                // Create the ray with the adjusted starting position and direction
                ray.origin = Position + (Rotation * BasisDeviceMatchableNames.PositionRayCastOffset);
                ray.direction = transform.forward;
            }
            else
            {
                ray = BasisLocalCameraDriver.Instance.Camera.ScreenPointToRay(ScreenPoint, Camera.MonoOrStereoscopicEye.Mono);
            }
            if (Physics.Raycast(ray, out PhysicHit, MaxDistance, Mask, TriggerInteraction))
            {
                SortedGraphics.Clear();
                SortedRays.Clear();
                PhysicHit.transform.GetComponentsInChildren<Canvas>(false, Results);
                if (Results.Count != 0)
                {
                    return RaycastToUI();
                }
            }
            return false;
        }
        public bool RaycastToUI()
        {
            Results.Sort((c1, c2) => c2.sortingOrder.CompareTo(c1.sortingOrder));
            int Count = Results.Count;
            for (int Index = 0; Index < Count; Index++)
            {
                Canvas CurrentTopLevel = Results[Index];
                if (CurrentTopLevel != null)
                {
                    if(CurrentTopLevel.worldCamera == null)
                    {
                        CurrentTopLevel.worldCamera = BasisLocalCameraDriver.Instance.Camera;
                    }
                    SortedRaycastGraphics(CurrentTopLevel, ray, MaxDistance, Mask, CurrentTopLevel.worldCamera, ref SortedGraphics);
                    ProcessSortedHitsResults(CurrentTopLevel, ray, MaxDistance, true, SortedGraphics, SortedRays);
                    if (SortedGraphics.Count != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void Sort<T>(IList<T> hits, IComparer<T> comparer) where T : struct => Sort(hits, comparer, hits.Count);
        public bool ProcessSortedHitsResults(Canvas canvas, Ray ray, float hitDistance, bool hitSomething, List<RaycastHitData> raycastHitDatums, List<RaycastResult> resultAppendList)
        {
            // Now that we have a list of sorted hits, process any extra settings and filters.
            foreach (var hitData in raycastHitDatums)
            {
                var validHit = true;

                if (hitData.graphic == null)
                {
                    continue;
                }
                var go = hitData.graphic.gameObject;
                if (IgnoreReversedGraphics)
                {
                    var forward = ray.direction;
                    var goDirection = go.transform.rotation * Vector3.forward;
                    validHit = Vector3.Dot(forward, goDirection) > 0;
                }

                validHit &= hitData.distance < hitDistance;

                if (validHit)
                {
                    var trans = go.transform;
                    var transForward = trans.forward;
                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = hitData.distance,
                        index = resultAppendList.Count,
                        depth = hitData.graphic.depth,
                        sortingLayer = canvas.sortingLayerID,
                        sortingOrder = canvas.sortingOrder,
                        worldPosition = hitData.worldHitPosition,
                        worldNormal = -transForward,
                        screenPosition = hitData.screenPosition,
                        displayIndex = hitData.displayIndex,
                    };
                    resultAppendList.Add(castResult);

                    hitSomething = true;
                }
            }

            return hitSomething;
        }
        public static void Sort<T>(IList<T> hits, IComparer<T> comparer, int count) where T : struct
        {
            if (count <= 1)
                return;

            bool fullPass;
            do
            {
                fullPass = true;
                for (var i = 1; i < count; ++i)
                {
                    var result = comparer.Compare(hits[i - 1], hits[i]);
                    if (result > 0)
                    {
                        (hits[i - 1], hits[i]) = (hits[i], hits[i - 1]);
                        fullPass = false;
                    }
                }
            } while (fullPass == false);
        }
        public void SortedRaycastGraphics(Canvas canvas, Ray ray, float maxDistance, LayerMask layerMask, Camera eventCamera, ref List<RaycastHitData> results)
        {
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            results.Clear();
            for (int i = 0; i < graphics.Count; ++i)
            {
                var graphic = graphics[i];

                if (!ShouldTestGraphic(graphic, layerMask))
                    continue;

                var raycastPadding = graphic.raycastPadding;

                if (RayIntersectsRectTransform(graphic.rectTransform, raycastPadding, ray, out var worldPos, out var distance))
                {
                    if (distance <= maxDistance)
                    {
                        Vector2 screenPos = eventCamera.WorldToScreenPoint(worldPos);
                        // mask/image intersection - See Unity docs on eventAlphaThreshold for when this does anything
                        if (graphic.Raycast(screenPos, eventCamera))
                        {
                            results.Add(new RaycastHitData(graphic, worldPos, screenPos, distance, eventCamera.targetDisplay));
                        }
                    }
                }
            }

            Sort(results, s_RaycastHitComparer);
        }

        public bool ShouldTestGraphic(Graphic graphic, LayerMask layerMask)
        {
            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
            if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                return false;

            if (((1 << graphic.gameObject.layer) & layerMask) == 0)
                return false;

            return true;
        }

        public bool SphereIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Vector3 from, out Vector3 worldPosition, out float distance)
        {
            var plane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
            var closestPoint = plane.ClosestPointOnPlane(from);
            var ray = new Ray(from, closestPoint - from);
            return RayIntersectsRectTransform(ray, plane, out worldPosition, out distance);
        }

        public bool RayIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Ray ray, out Vector3 worldPosition, out float distance)
        {
            var plane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
            return RayIntersectsRectTransform(ray, plane, out worldPosition, out distance);
        }

        public bool RayIntersectsRectTransform(Ray ray, Plane plane, out Vector3 worldPosition, out float distance)
        {
            if (plane.Raycast(ray, out var enter))
            {
                var intersection = ray.GetPoint(enter);

                var bottomEdge = s_Corners[3] - s_Corners[0];
                var leftEdge = s_Corners[1] - s_Corners[0];
                var bottomDot = Vector3.Dot(intersection - s_Corners[0], bottomEdge);
                var leftDot = Vector3.Dot(intersection - s_Corners[0], leftEdge);

                // If the intersection is right of the left edge and above the bottom edge.
                if (leftDot >= 0f && bottomDot >= 0f)
                {
                    var topEdge = s_Corners[1] - s_Corners[2];
                    var rightEdge = s_Corners[3] - s_Corners[2];
                    var topDot = Vector3.Dot(intersection - s_Corners[2], topEdge);
                    var rightDot = Vector3.Dot(intersection - s_Corners[2], rightEdge);

                    // If the intersection is left of the right edge, and below the top edge
                    if (topDot >= 0f && rightDot >= 0f)
                    {
                        worldPosition = intersection;
                        distance = enter;
                        return true;
                    }
                }
            }

            worldPosition = Vector3.zero;
            distance = 0f;
            return false;
        }

        public Plane GetRectTransformPlane(RectTransform transform, Vector4 raycastPadding, Vector3[] fourCornersArray)
        {
            GetRectTransformWorldCorners(transform, raycastPadding, fourCornersArray);
            return new Plane(fourCornersArray[0], fourCornersArray[1], fourCornersArray[2]);
        }

        // This method is similar to RecTransform.GetWorldCorners, but with support for the raycastPadding offset.
        public void GetRectTransformWorldCorners(RectTransform transform, Vector4 offset, Vector3[] fourCornersArray)
        {
            if (fourCornersArray == null || fourCornersArray.Length < 4)
            {
                BasisDebug.LogError("Calling GetRectTransformWorldCorners with an array that is null or has less than 4 elements.");
                return;
            }

            // GraphicRaycaster.Raycast uses RectTransformUtility.RectangleContainsScreenPoint instead,
            // which redirects to PointInRectangle defined in RectTransformUtil.cpp. However, that method
            // uses the Camera to convert from the given screen point to a ray, but this class uses
            // the ray from the Ray Interactor that feeds the event data.
            // Offset calculation for raycastPadding from PointInRectangle method, which replaces RectTransform.GetLocalCorners.
            var rect = transform.rect;
            var x0 = rect.x + offset.x;
            var y0 = rect.y + offset.y;
            var x1 = rect.xMax - offset.z;
            var y1 = rect.yMax - offset.w;
            fourCornersArray[0] = new Vector3(x0, y0, 0f);
            fourCornersArray[1] = new Vector3(x0, y1, 0f);
            fourCornersArray[2] = new Vector3(x1, y1, 0f);
            fourCornersArray[3] = new Vector3(x1, y0, 0f);

            // Transform the local corners to world space, which is from RectTransform.GetWorldCorners.
            var localToWorldMatrix = transform.localToWorldMatrix;
            for (var index = 0; index < 4; ++index)
                fourCornersArray[index] = localToWorldMatrix.MultiplyPoint(fourCornersArray[index]);
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
        }
    }
}