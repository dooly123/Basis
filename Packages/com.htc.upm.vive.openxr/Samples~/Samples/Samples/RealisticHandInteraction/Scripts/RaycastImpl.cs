// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wave.Generic.Sample
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class RaycastImpl : BaseRaycaster
	{
		const string LOG_TAG = "Wave.Generic.Sample.RaycastImpl";
		void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		private int logFrame = 0;
		protected bool printIntervalLog = false;
		void INTERVAL(string msg) { if (printIntervalLog) { DEBUG(msg); } }

		#region Inspector
		[SerializeField]
		private bool m_IgnoreReversedGraphics = false;
		public bool IgnoreReversedGraphics { get { return m_IgnoreReversedGraphics; } set { m_IgnoreReversedGraphics = value; } }
		[SerializeField]
		private float m_PhysicsCastDistance = 100;
		public float PhysicsCastDistance { get { return m_PhysicsCastDistance; } set { m_PhysicsCastDistance = value; } }
		[SerializeField]
		private LayerMask m_PhysicsEventMask = ~0;
		public LayerMask PhysicsEventMask { get { return m_PhysicsEventMask; } set { m_PhysicsEventMask = value; } }

		[SerializeField]
		private List<string> s_GraphicTags = new List<string>();
		public List<string> GraphicTags { get { return s_GraphicTags; } set { s_GraphicTags = value; } }
		#endregion

		private Camera m_Camera = null;
		public override Camera eventCamera { get { return m_Camera; } }

		#region MonoBehaviour overrides
		protected override void OnEnable()
		{
			DEBUG("OnEnable()");
			base.OnEnable();

			/// 1. Set up the event camera.
			m_Camera = GetComponent<Camera>();
			m_Camera.stereoTargetEye = StereoTargetEyeMask.None;
			m_Camera.enabled = false;

			/// 2. Set up the EventSystem.
			if (EventSystem.current == null)
			{
				var eventSystemObject = new GameObject("EventSystem");
				eventSystemObject.AddComponent<EventSystem>();
			}
		}
		protected override void OnDisable()
		{
			DEBUG("OnDisable()");
			base.OnDisable();
		}

		protected bool m_Interactable = true;
		protected virtual void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);

			if (!m_Interactable) return;

			/// Use the event camera and EventSystem to reset PointerEventData.
			ResetEventData();

			/// Update the raycast results
			resultAppendList.Clear();
			Raycast(pointerData, resultAppendList);

			pointerData.pointerCurrentRaycast = currentRaycastResult;

			/// Send events
			HandleRaycastEvent();
		}
		#endregion

		#region Raycast Result Handling
		static readonly Comparison<RaycastResult> rrComparator = RaycastResultComparator;
		private RaycastResult GetFirstRaycastResult(List<RaycastResult> results)
		{
			RaycastResult rr = default;

			results.Sort(rrComparator);
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].isValid)
				{
					rr = results[i];
					break;
				}
			}

			return rr;
		}
		private static int RaycastResultComparator(RaycastResult lhs, RaycastResult rhs)
		{
			if (lhs.module != rhs.module)
			{
				if (lhs.module.eventCamera != null && rhs.module.eventCamera != null && lhs.module.eventCamera.depth != rhs.module.eventCamera.depth)
				{
					// need to reverse the standard compareTo
					if (lhs.module.eventCamera.depth < rhs.module.eventCamera.depth) { return 1; }
					if (lhs.module.eventCamera.depth == rhs.module.eventCamera.depth) { return 0; }
					return -1;
				}

				if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
				{
					return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);
				}

				if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
				{
					return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
				}
			}

			if (lhs.sortingLayer != rhs.sortingLayer)
			{
				// Uses the layer value to properly compare the relative order of the layers.
				var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
				var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
				return rid.CompareTo(lid);
			}

			if (lhs.sortingOrder != rhs.sortingOrder)
			{
				return rhs.sortingOrder.CompareTo(lhs.sortingOrder);
			}

			if (!Mathf.Approximately(lhs.distance, rhs.distance))
			{
				return lhs.distance.CompareTo(rhs.distance);
			}

			if (lhs.depth != rhs.depth)
			{
				return rhs.depth.CompareTo(lhs.depth);
			}

			return lhs.index.CompareTo(rhs.index);
		}
		#endregion

#if UNITY_EDITOR
		bool drawDebugLine = false;
#endif

		#region Raycast
		protected PointerEventData pointerData = null;
		protected Vector3 pointerLocalOffset = Vector3.forward;
		private Vector3 physicsWorldPosition = Vector3.zero;
		private Vector2 graphicScreenPosition = Vector2.zero;
		private void UpdatePointerDataPosition()
		{
			/// 1. Calculate the pointer offset in "local" space.
			pointerLocalOffset = Vector3.forward;

			/// 2. Calculate the pointer position in "world" space.
			Vector3 rotated_offset = transform.rotation * pointerLocalOffset;
			physicsWorldPosition = transform.position + rotated_offset;
			graphicScreenPosition = m_Camera.WorldToScreenPoint(physicsWorldPosition);
			// The graphicScreenPosition.x should be equivalent to (0.5f * Screen.width);
			// The graphicScreenPosition.y should be equivalent to (0.5f * Screen.height);
		}
		private void ResetEventData()
		{
			if (pointerData == null) { pointerData = new RaycastEventData(EventSystem.current, gameObject); }

			UpdatePointerDataPosition();
			pointerData.position = graphicScreenPosition;
		}
		List<RaycastResult> resultAppendList = new List<RaycastResult>();
		private RaycastResult currentRaycastResult = default;
		protected GameObject raycastObject = null;
		protected List<GameObject> s_raycastObjects = new List<GameObject>();
		protected GameObject raycastObjectEx = null;
		protected List<GameObject> s_raycastObjectsEx = new List<GameObject>();
		/**
		 * Call to
		 * GraphicRaycast(Canvas canvas, Camera eventCamera, Vector2 screenPosition, List<RaycastResult> resultAppendList)
		 * PhysicsRaycast(Ray ray, Camera eventCamera, List<RaycastResult> resultAppendList)
		 **/
		public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
		{
			// --------------- Previous Results ---------------
			raycastObjectEx = raycastObject;
			s_raycastObjects.Clear();

			// --------------- Graphic Raycast ---------------
			Canvas[] canvases = FindObjectsOfType<Canvas>();	// note: GC.Alloc
			for (int i = 0; i < canvases.Length; i++)
			{
				GraphicRaycast(canvases[i], m_Camera, eventData.position, resultAppendList);
			}

			// --------------- Physics Raycast ---------------
			Ray ray = new Ray(transform.position, (physicsWorldPosition - transform.position));
			PhysicsRaycast(ray, m_Camera, resultAppendList);

			currentRaycastResult = GetFirstRaycastResult(resultAppendList);

			// --------------- Current Results ---------------
			raycastObject = currentRaycastResult.gameObject;

			GameObject raycastTarget = currentRaycastResult.gameObject;
			while (raycastTarget != null)
			{
				s_raycastObjects.Add(raycastTarget);
				raycastTarget = (raycastTarget.transform.parent != null ? raycastTarget.transform.parent.gameObject : null);
			}

#if UNITY_EDITOR
			if (drawDebugLine)
			{
				Vector3 end = transform.position + (transform.forward * 100);
				Debug.DrawLine(transform.position, end, Color.red, 1);
			}
#endif
		}

		Ray ray = new Ray();
		protected virtual void GraphicRaycast(Canvas canvas, Camera eventCamera, Vector2 screenPosition, List<RaycastResult> resultAppendList)
		{
			if (canvas == null)
				return;

			IList<Graphic> foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
			if (foundGraphics == null || foundGraphics.Count == 0)
				return;

			int displayIndex = 0;
			var currentEventCamera = eventCamera; // Property can call Camera.main, so cache the reference

			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
				displayIndex = canvas.targetDisplay;
			else
				displayIndex = currentEventCamera.targetDisplay;

			if (currentEventCamera != null)
				ray = currentEventCamera.ScreenPointToRay(screenPosition);

			// Necessary for the event system
			for (int i = 0; i < foundGraphics.Count; ++i)
			{
				Graphic graphic = foundGraphics[i];

				if (s_GraphicTags != null && s_GraphicTags.Count != 0 && !s_GraphicTags.Contains(graphic.gameObject.tag)) { continue; }

				// -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
				if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1) { continue; }

				if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPosition, currentEventCamera)) { continue; }

				if (currentEventCamera != null && currentEventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > currentEventCamera.farClipPlane) { continue; }

				if (graphic.Raycast(screenPosition, currentEventCamera))
				{
					var go = graphic.gameObject;
					bool appendGraphic = true;

					if (m_IgnoreReversedGraphics)
					{
						if (currentEventCamera == null)
						{
							// If we dont have a camera we know that we should always be facing forward
							var dir = go.transform.rotation * Vector3.forward;
							appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
						}
						else
						{
							// If we have a camera compare the direction against the cameras forward.
							var cameraForward = currentEventCamera.transform.rotation * Vector3.forward * currentEventCamera.nearClipPlane;
							appendGraphic = Vector3.Dot(go.transform.position - currentEventCamera.transform.position - cameraForward, go.transform.forward) >= 0;
						}
					}

					if (appendGraphic)
					{
						float distance = 0;
						Transform trans = go.transform;
						Vector3 transForward = trans.forward;

						if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
							distance = 0;
						else
						{
							// http://geomalgorithms.com/a06-_intersect-2.html
							distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

							// Check to see if the go is behind the camera.
							if (distance < 0)
								continue;
						}

						resultAppendList.Add(new RaycastResult
						{
							gameObject = go,
							module = this,
							distance = distance,
							screenPosition = screenPosition,
							displayIndex = displayIndex,
							index = resultAppendList.Count,
							depth = graphic.depth,
							sortingLayer = canvas.sortingLayerID,
							sortingOrder = canvas.sortingOrder,
							worldPosition = ray.origin + ray.direction * distance,
							worldNormal = -transForward
						});
					}
				}
			}
		}

		Vector3 hitScreenPos = Vector3.zero;
		Vector2 hitScreenPos2D = Vector2.zero;
		static readonly RaycastHit[] hits = new RaycastHit[255];
		protected virtual void PhysicsRaycast(Ray ray, Camera eventCamera, List<RaycastResult> resultAppendList)
		{
			var hitCount = Physics.RaycastNonAlloc(ray, hits, m_PhysicsCastDistance, m_PhysicsEventMask);

			for (int i = 0; i < hitCount; ++i)
			{
				hitScreenPos = eventCamera.WorldToScreenPoint(hits[i].point);
				hitScreenPos2D.x = hitScreenPos.x;
				hitScreenPos2D.y = hitScreenPos.y;

				resultAppendList.Add(new RaycastResult
				{
					gameObject = hits[i].collider.gameObject,
					module = this,
					distance = hits[i].distance,
					worldPosition = hits[i].point,
					worldNormal = hits[i].normal,
					screenPosition = hitScreenPos2D,
					index = resultAppendList.Count,
					sortingLayer = 0,
					sortingOrder = 0
				});
			}
		}
		#endregion

		#region Event
		private void CopyList(List<GameObject> src, List<GameObject> dst)
		{
			dst.Clear();
			for (int i = 0; i < src.Count; i++)
				dst.Add(src[i]);
		}

		private void ExitEnterHandler(ref List<GameObject> enterObjects, ref List<GameObject> exitObjects)
		{
			if (exitObjects.Count > 0)
			{
				for (int i = 0; i < exitObjects.Count; i++)
				{
					if (exitObjects[i] != null && !enterObjects.Contains(exitObjects[i]))
					{
						ExecuteEvents.Execute(exitObjects[i], pointerData, ExecuteEvents.pointerExitHandler);
						DEBUG("ExitEnterHandler() Exit: " + exitObjects[i]);
					}
				}
			}

			if (enterObjects.Count > 0)
			{
				for (int i = 0; i < enterObjects.Count; i++)
				{
					if (enterObjects[i] != null && !exitObjects.Contains(enterObjects[i]))
					{
						ExecuteEvents.Execute(enterObjects[i], pointerData, ExecuteEvents.pointerEnterHandler);
						DEBUG("ExitEnterHandler() Enter: " + enterObjects[i] + ", camera: " + pointerData.enterEventCamera);
					}
				}
			}

			CopyList(enterObjects, exitObjects);
		}
		private void HoverHandler()
		{
			if (raycastObject != null && (raycastObject == raycastObjectEx))
			{
				INTERVAL("HoverHandler() Hover: " + raycastObject.name);
				ExecuteEvents.ExecuteHierarchy(raycastObject, pointerData, RaycastEvents.pointerHoverHandler);
			}
		}
		private void DownHandler()
		{
			DEBUG("DownHandler()");
			if (raycastObject == null) { return; }

			pointerData.pressPosition = pointerData.position;
			pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
			pointerData.pointerPress =
				ExecuteEvents.ExecuteHierarchy(raycastObject, pointerData, ExecuteEvents.pointerDownHandler)
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(raycastObject);

			DEBUG("DownHandler() Down: " + pointerData.pointerPress + ", raycastObject: " + raycastObject.name);

			// If Drag Handler exists, send initializePotentialDrag event.
			pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(raycastObject);
			if (pointerData.pointerDrag != null)
			{
				DEBUG("DownHandler() Send initializePotentialDrag to " + pointerData.pointerDrag + ", current GameObject is " + raycastObject);
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
			}

			// press happened (even not handled) object.
			pointerData.rawPointerPress = raycastObject;
			// allow to send Pointer Click event
			pointerData.eligibleForClick = true;
			// reset the screen position of press, can be used to estimate move distance
			pointerData.delta = Vector2.zero;
			// current Down, reset drag state
			pointerData.dragging = false;
			pointerData.useDragThreshold = true;
			// record the count of Pointer Click should be processed, clean when Click event is sent.
			pointerData.clickCount = 1;
			// set clickTime to current time of Pointer Down instead of Pointer Click.
			// since Down & Up event should not be sent too closely. (< kClickInterval)
			pointerData.clickTime = Time.unscaledTime;
		}
		private void UpHandler()
		{
			if (!pointerData.eligibleForClick && !pointerData.dragging)
			{
				// 1. no pending click
				// 2. no dragging
				// Mean user has finished all actions and do NOTHING in current frame.
				return;
			}

			// raycastObject may be different with pointerData.pointerDrag so we don't check null

			if (pointerData.pointerPress != null)
			{
				// In the frame of button is pressed -> unpressed, send Pointer Up
				DEBUG("UpHandler() Send Pointer Up to " + pointerData.pointerPress);
				ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
			}

			if (pointerData.eligibleForClick)
			{
				GameObject objectToClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(raycastObject);
				if (objectToClick != null)
				{
					if (objectToClick == pointerData.pointerPress)
					{
						// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
						DEBUG("UpHandler() Send Pointer Click to " + pointerData.pointerPress);
						ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
					}
					else
					{
						DEBUG("UpHandler() pointer down object " + pointerData.pointerPress + " is different with click object " + objectToClick);
					}
				}
				else
				{
					if (pointerData.dragging)
					{
						GameObject _pointerDrop = ExecuteEvents.GetEventHandler<IDropHandler>(raycastObject);
						if (_pointerDrop == pointerData.pointerDrag)
						{
							// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
							DEBUG("UpHandler() Send Pointer Drop to " + pointerData.pointerDrag);
							ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dropHandler);
						}
						DEBUG("UpHandler() Send Pointer endDrag to " + pointerData.pointerDrag);
						ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);

						pointerData.dragging = false;
					}
				}
			}

			// initializePotentialDrag was sent when IDragHandler exists.
			pointerData.pointerDrag = null;
			// Down of pending Click object.
			pointerData.pointerPress = null;
			// press happened (even not handled) object.
			pointerData.rawPointerPress = null;
			// clear pending state.
			pointerData.eligibleForClick = false;
			// Click is processed, clearcount.
			pointerData.clickCount = 0;
			// Up is processed thus clear the time limitation of Down event.
			pointerData.clickTime = 0;
		}

		// After selecting an object over this duration, the drag action will be taken.
		const float kTimeToDrag = 0.2f;
		private void DragHandler()
		{
			if (Time.unscaledTime - pointerData.clickTime < kTimeToDrag) { return; }
			if (pointerData.pointerDrag == null) { return; }

			if (!pointerData.dragging)
			{
				DEBUG("DragHandler() Send BeginDrag to " + pointerData.pointerDrag);
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.beginDragHandler);
				pointerData.dragging = true;
			}
			else
			{
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
			}
		}

		private void SubmitHandler()
		{
			if (raycastObject == null) { return; }

			DEBUG("SubmitHandler() Submit: " + raycastObject.name);
			ExecuteEvents.ExecuteHierarchy(raycastObject, pointerData, ExecuteEvents.submitHandler);
		}

		// Do NOT allow event DOWN being sent multiple times during kClickInterval
		// since UI element of Unity needs time to perform transitions.
		const float kClickInterval = 0.2f;
		private void HandleRaycastEvent()
		{
			ExitEnterHandler(ref s_raycastObjects, ref s_raycastObjectsEx);
			HoverHandler();

			bool submit = OnSubmit();
			if (submit)
			{
				SubmitHandler();
				return;
			}

			bool down = OnDown();
			bool hold = OnHold();
			if (!down && hold)
			{
				// Hold means to Drag.
				DragHandler();
			}
			else if (Time.unscaledTime - pointerData.clickTime < kClickInterval)
			{
				// Delay new events until kClickInterval has passed.
			}
			else if (down && !pointerData.eligibleForClick)
			{
				// 1. Not Down -> Down
				// 2. No pending Click should be procced.
				DownHandler();
			}
			else if (!hold)
			{
				// 1. If Down before, send Up event and clear Down state.
				// 2. If Dragging, send Drop & EndDrag event and clear Dragging state.
				// 3. If no Down or Dragging state, do NOTHING.
				UpHandler();
			}
		}
		#endregion

		#region Actions
		protected virtual bool OnDown() { return false; }
		protected virtual bool OnHold() { return false; }
		protected virtual bool OnSubmit() { return false; }
		#endregion
	}
}
