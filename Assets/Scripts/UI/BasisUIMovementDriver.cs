using UnityEngine;

public class BasisUIMovementDriver : MonoBehaviour
{
    public BasisLocalPlayer LocalPlayer;
    public BasisLocalCameraDriver CameraDriver;
    public Vector3 WorldOffset;
    public void OnEnable()
    {
        LocalPlayer = BasisLocalPlayer.Instance;
        CameraDriver = BasisLocalCameraDriver.Instance;
    }
    void Update()
    {
        // Get the camera's position in world space
        Vector3 cameraPosition = CameraDriver.Camera.transform.position;

        // Set the UI's position to be at the bottom of the camera's view
        transform.position =  new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z) + CameraDriver.Camera.transform.rotation * WorldOffset;

        // Make the UI face the same direction as the camera
        transform.rotation = CameraDriver.Camera.transform.rotation;

    }
}
