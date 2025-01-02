using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIMovementDriver : MonoBehaviour
    {
        public Vector3 WorldOffset = new Vector3(0, 0, 0.5f);
        public bool hasLocalCreationEvent = false;
        public Vector3 Position;
        public Quaternion Rotation;
        public void OnEnable()
        {
            if (BasisLocalPlayer.Instance != null)
            {
                LocalPlayerGenerated();
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
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += StartWaitAndSetUILocation;
            SetUILocation();
        }
        public void OnDisable()
        {
            BasisLocalPlayer.Instance.OnPlayersHeightChanged -= StartWaitAndSetUILocation;
            if (hasLocalCreationEvent)
            {
                BasisLocalPlayer.OnLocalPlayerCreated -= LocalPlayerGenerated;
                hasLocalCreationEvent = false;
            }
        }
        public void StartWaitAndSetUILocation()
        {
            BasisDebug.Log("StartWaitAndSetUILocation");
            SetUILocation();
        }
        public void SetUILocation()
        {
            // Get the current position and rotation from the BasisLocalCameraDriver
            BasisLocalCameraDriver.GetPositionAndRotation(out Position, out Rotation);

            // Log the current scale for debugging purposes
            BasisDebug.Log("Scale was " + BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale);

            // Extract the yaw (rotation around the vertical axis) and ignore pitch and roll
            Vector3 eulerRotation = Rotation.eulerAngles;
         //   eulerRotation.x = 0f; // Remove pitch
            eulerRotation.z = 0f; // Remove roll

            // Create a new quaternion with the adjusted rotation
            Quaternion horizontalRotation = Quaternion.Euler(eulerRotation);

            // Set the position and the adjusted horizontal rotation
            transform.SetPositionAndRotation(Position + (horizontalRotation * (WorldOffset * BasisLocalPlayer.Instance.EyeRatioPlayerToDefaultScale)), horizontalRotation);
        }
    }
}