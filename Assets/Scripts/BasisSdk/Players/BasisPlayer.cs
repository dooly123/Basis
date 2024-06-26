using UnityEngine;
public abstract class BasisPlayer : MonoBehaviour
{
    public bool IsLocal { get; set; }
    public string DisplayName;
    public string UUID;
    public BasisAvatar Avatar;
    public AddressableGenericResource AvatarAddressableGenericResource;
    public string AvatarUrl;
    public bool HasAvatarDriver;
    public const string FallBackAvatar = "LoadingAvatar";
    public System.Action OnAvatarSwitched;
    public System.Action OnAvatarSwitchedFallBack;
    public void InitalizeIKCalibration(BasisAvatarDriver LocalAvatarDriver)
    {
        if (LocalAvatarDriver != null)
        {
            HasAvatarDriver = true;
        }
        else
        {
            Debug.LogError("Mising CharacterIKCalibration");
            HasAvatarDriver = false;
        }
    }
}