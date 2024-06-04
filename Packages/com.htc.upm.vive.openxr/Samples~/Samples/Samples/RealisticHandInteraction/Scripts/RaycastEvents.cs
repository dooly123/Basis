// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.EventSystems;

namespace Wave.Generic.Sample
{
	public class RaycastEventData : PointerEventData
	{
		/// <summary> The actor sends an event. </summary>
		public GameObject Actor { get { return m_Actor; } }
		private GameObject m_Actor = null;

		public RaycastEventData(EventSystem eventSystem, GameObject actor)
			: base(eventSystem)
		{
			m_Actor = actor;
		}
	}

	/// <summary>
	/// The object which receives events should implement this interface.
	/// </summary>
	public interface IHoverHandler : IEventSystemHandler
	{
		void OnHover(RaycastEventData eventData);
	}

	/// <summary>
	/// Objects will use
	/// ExecuteEvents.Execute (GameObject, BaseEventData, RayastEvents.pointerXXXXHandler)
	/// to send XXXX events.
	/// </summary>
	public static class RaycastEvents
	{
		#region Event Executor of Hover
		/// Use ExecuteEvents.Execute (GameObject, BaseEventData, RaycastEvents.pointerHoverHandler)
		private static void HoverExecutor(IHoverHandler handler, BaseEventData eventData)
		{
			handler.OnHover(ExecuteEvents.ValidateEventData<RaycastEventData>(eventData));
		}
		public static ExecuteEvents.EventFunction<IHoverHandler> pointerHoverHandler
		{
			get { return HoverExecutor; }
		}
		#endregion
	}
}
