using System.Collections;
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
        StartCoroutine(WaitAndSetUILocation());
        BasisLocalPlayer.OnLocalAvatarChanged += OnLocalAvatarChanged;
        BasisLocalPlayer.OnPlayersHeightChanged += OnPlayersHeightChanged;
    }

    public void OnDisable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged -= OnLocalAvatarChanged;
        BasisLocalPlayer.OnPlayersHeightChanged -= OnPlayersHeightChanged;
    }

    private void OnPlayersHeightChanged()
    {
        StartCoroutine(WaitAndSetUILocation());
    }

    private void OnLocalAvatarChanged()
    {
        StartCoroutine(WaitAndSetUILocation());
    }

    private IEnumerator WaitAndSetUILocation()
    {
        // Wait for the end of frame to ensure all camera updates are complete
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetUILocation();
    }

    public void SetUILocation()
    {
        // Get the camera's position in world space
        if (CameraDriver != null && CameraDriver.Camera != null)
        {
            Vector3 cameraPosition = CameraDriver.Camera.transform.position;
            transform.SetPositionAndRotation(cameraPosition + CameraDriver.Camera.transform.rotation * WorldOffset, CameraDriver.Camera.transform.rotation);
        }
    }
}