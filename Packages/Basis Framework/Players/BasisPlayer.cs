using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Drivers;
using System;
using UnityEngine;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
namespace Basis.Scripts.BasisSdk.Players
{
public abstract class BasisPlayer : MonoBehaviour
{
    public bool IsLocal { get; set; }
    public string DisplayName;
    public string UUID;
    public BasisAvatar Avatar;
    public AddressableGenericResource AvatarAddressableGenericResource;
    public BasisAvatarStrainJiggleDriver BasisAvatarStrainJiggleDriver;
    public string AvatarUrl;
    public byte AvatarLoadMode;//0 downloading 1 local
    public bool HasAvatarDriver;
    public event Action OnAvatarSwitched;
    public event Action OnAvatarSwitchedFallBack;
    public ProgressReport ProgressReportAvatarLoad;
    public const byte LoadModeNetworkDownloadable = 0;
    public const byte LoadModeLocal = 1;
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
        if (BasisAvatarStrainJiggleDriver != null)
        {
            BasisAvatarStrainJiggleDriver.OnCalibration();
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
}