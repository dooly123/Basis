using System;
using UnityEngine;
public abstract class BasisPlayer : MonoBehaviour
{
    public bool IsLocal { get; set; }
    public string DisplayName;
    public string UUID;
    public BasisAvatar Avatar;
    public AddressableGenericResource AvatarAddressableGenericResource;
    public BasisAvatarStrainJiggleDriver BasisAvatarStrainJiggleDriver;
    public string AvatarUrl;
    public bool HasAvatarDriver;
    public const string FallBackAvatar = "LoadingAvatar";
    public event Action OnAvatarSwitched;
    public event Action OnAvatarSwitchedFallBack;
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
    public void AvatarSwitchedFallBack()
    {
        OnAvatarSwitchedFallBack?.Invoke();
    }
    public void AvatarSwitched()
    {
        OnAvatarSwitched?.Invoke();
    }
}