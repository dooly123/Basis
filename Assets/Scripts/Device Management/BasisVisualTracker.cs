using Assets.Scripts.BasisSdk.Players;
using Assets.Scripts.Device_Management.Devices;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Device_Management
{
public class BasisVisualTracker : MonoBehaviour
{
    public BasisInput BasisInput;
    public UnityEvent TrackedSetup = new UnityEvent();
    public Quaternion ModelRotationOffset = Quaternion.identity;
    public Vector3 ModelPositionOffset = Vector3.zero;
    public bool HasEvents = false;
    public void Initialization(BasisInput basisInput)
    {
        if(basisInput != null)
        {
            BasisInput = basisInput;
            UpdateVisualSizeAndOffset();
            if (HasEvents == false)
            {
                BasisLocalPlayer.Instance.OnPlayersHeightChanged += UpdateVisualSizeAndOffset;
                BasisLocalPlayer.Instance.OnLocalAvatarChanged += UpdateVisualSizeAndOffset;
                HasEvents = true;
            }
            TrackedSetup.Invoke();
        }
    }
    public void OnDestroy()
    {
        if (HasEvents)
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= UpdateVisualSizeAndOffset;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged -= UpdateVisualSizeAndOffset;
            HasEvents = false;
        }
    }
    public void UpdateVisualSizeAndOffset()
    {
       gameObject.transform.localScale = Vector3.one * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
       gameObject.transform.SetLocalPositionAndRotation(ModelPositionOffset * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale, ModelRotationOffset);
    }
}
}