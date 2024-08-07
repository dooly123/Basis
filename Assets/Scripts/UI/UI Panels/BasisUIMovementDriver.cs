using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using System.Collections;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
public class BasisUIMovementDriver : MonoBehaviour
{
    public BasisLocalPlayer LocalPlayer;
    public BasisLocalCameraDriver CameraDriver;
    public Vector3 WorldOffset;
    public bool hasLocalCreationEvent = false;
    public void OnEnable()
    {
        LocalPlayer = BasisLocalPlayer.Instance;
        CameraDriver = BasisLocalCameraDriver.Instance;
        if (BasisLocalPlayer.Instance != null)
        {
            LocalPlayerGenerated();
            StartCoroutine(WaitAndSetUILocation());
        }
        else
        {
            if (hasLocalCreationEvent == false)
            {
                BasisLocalPlayer.OnLocalPlayerCreated += LocalPlayerGenerated;
                hasLocalCreationEvent = true;
            }
        }
    }
    public void LocalPlayerGenerated()
    {
        BasisLocalPlayer.Instance.OnLocalAvatarChanged += OnLocalAvatarChanged;
        BasisLocalPlayer.Instance.OnPlayersHeightChanged += OnPlayersHeightChanged;
    }
    public void OnDisable()
    {
        BasisLocalPlayer.Instance.OnLocalAvatarChanged -= OnLocalAvatarChanged;
        BasisLocalPlayer.Instance.OnPlayersHeightChanged -= OnPlayersHeightChanged;
        if(hasLocalCreationEvent)
        {
            BasisLocalPlayer.OnLocalPlayerCreated -= LocalPlayerGenerated;
            hasLocalCreationEvent = false;
        }
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
        yield return null;
        SetUILocation();
    }

    public void SetUILocation()
    {
        // Get the camera's position in world space
        if (CameraDriver != null && CameraDriver.Camera != null)
        {
            Vector3 cameraPosition = CameraDriver.Camera.transform.position;
            Vector3 Rotation = CameraDriver.Camera.transform.eulerAngles;
            Rotation = new Vector3(Rotation.x, Rotation.y, 0);
            Quaternion Rot = Quaternion.Euler(Rotation);
            transform.SetPositionAndRotation(cameraPosition + CameraDriver.Camera.transform.rotation * WorldOffset, Rot);
        }
    }
}
}