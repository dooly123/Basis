using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class BasisUIComponent : MonoBehaviour
{
    public Canvas Canvas;
    public TrackedDeviceGraphicRaycaster TrackedDeviceGraphicRaycaster;
    public GraphicRaycaster GraphicRaycaster;
    public CanvasScaler CanvasScaler;
    public void OnEnable()
    {
        if (Canvas == null)
        {
            Canvas = BasisHelpers.GetOrAddComponent<Canvas>(this.gameObject);
        }
        if (TrackedDeviceGraphicRaycaster == null)
        {
            TrackedDeviceGraphicRaycaster = BasisHelpers.GetOrAddComponent<TrackedDeviceGraphicRaycaster>(this.gameObject);
            TrackedDeviceGraphicRaycaster.checkFor3DOcclusion = true;
        }
        if (GraphicRaycaster == null)
        {
            GraphicRaycaster = BasisHelpers.GetOrAddComponent<GraphicRaycaster>(this.gameObject);
        }
        if (CanvasScaler == null)
        {
            CanvasScaler = BasisHelpers.GetOrAddComponent<CanvasScaler>(this.gameObject);
        }
        Initalize();
    }
    public void Initalize()
    {
        Canvas.worldCamera = BasisLocalCameraDriver.Instance.Camera;
    }
}
