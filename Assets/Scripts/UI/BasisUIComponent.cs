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
        BasisLocalCameraDriver.InstanceExists += Initalize;
    }
    public void OnDisable()
    {
        BasisLocalCameraDriver.InstanceExists -= Initalize;
    }
    public void Initalize()
    {
        if (BasisLocalCameraDriver.Instance != null)
        {
            Canvas.worldCamera = BasisLocalCameraDriver.Instance.Camera;
        }
    }
}
