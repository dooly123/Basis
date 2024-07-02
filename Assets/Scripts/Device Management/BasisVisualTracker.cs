using UnityEngine;
using UnityEngine.Events;

public class BasisVisualTracker : MonoBehaviour
{
    public BasisInput BasisInput;
    public UnityEvent TrackedSetup = new UnityEvent();
    public Quaternion ModelRotationOffset = Quaternion.identity;
    public Vector3 ModelPositionOffset = Vector3.zero;
    public void Initialization(BasisInput basisInput)
    {
        if(basisInput != null)
        {
            BasisInput = basisInput;
            UpdateVisualSizeAndOffset();
            BasisLocalPlayer.OnPlayersHeightChanged += UpdateVisualSizeAndOffset;
            BasisLocalPlayer.OnLocalAvatarChanged += UpdateVisualSizeAndOffset;
            TrackedSetup.Invoke();
        }
    }
    public void OnDestroy()
    {
        BasisLocalPlayer.OnLocalAvatarChanged -= UpdateVisualSizeAndOffset;
        BasisLocalPlayer.OnPlayersHeightChanged -= UpdateVisualSizeAndOffset;
    }
    public void UpdateVisualSizeAndOffset()
    {
       gameObject.transform.localScale = Vector3.one * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
       gameObject.transform.SetLocalPositionAndRotation(ModelPositionOffset * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale, ModelRotationOffset);
    }
}
