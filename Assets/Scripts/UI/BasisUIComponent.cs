using UnityEngine;
using UnityEngine.UI;
public class BasisUIComponent : MonoBehaviour
{
    public Canvas Canvas;
    public CanvasScaler CanvasScaler;
    public BasisGraphicUIRayCaster GraphicUIRayCaster;
    public void OnEnable()
    {
        Initalize();
    }
    public void Initalize()
    {
        Canvas.worldCamera = BasisLocalCameraDriver.Instance.Camera;
    }
}
