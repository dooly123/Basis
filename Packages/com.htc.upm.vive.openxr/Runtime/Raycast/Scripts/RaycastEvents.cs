// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.EventSystems;

namespace VIVE.OpenXR.Raycast
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
