using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
public class BasisPointerEventData : PointerEventData
{
    public bool WasLastDown = false;
    public BasisPointerEventData(EventSystem eventSystem) : base(eventSystem)
    {
    }
}
}