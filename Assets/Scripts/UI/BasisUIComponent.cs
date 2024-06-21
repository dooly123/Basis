using UnityEngine;
using UnityEngine.UI;
public class BasisUIComponent : MonoBehaviour
{
    public Canvas Canvas;
    public CanvasScaler CanvasScaler;
    public BasisGraphicUIRayCaster GraphicUIRayCaster;
    public void OnEnable()
    {
        if (Canvas == null)
        {
            Canvas = BasisHelpers.GetOrAddComponent<Canvas>(this.gameObject);
        }
        if (GraphicUIRayCaster == null)
        {
            GraphicUIRayCaster = BasisHelpers.GetOrAddComponent<BasisGraphicUIRayCaster>(this.gameObject);
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
