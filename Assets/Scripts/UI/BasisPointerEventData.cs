using UnityEngine;
using UnityEngine.EventSystems;
public class BasisPointerEventData : PointerEventData
{
    public bool WasLastDown = false;
    public BasisPointerEventData(EventSystem eventSystem) : base(eventSystem)
    {
    }
}
