using UnityEngine;
public abstract class BasisPlayer : MonoBehaviour
{
    public bool IsLocal { get; set; }
    public BasisAvatar Avatar;
    public bool HasAvatarDriver;
    public const string FallBackAvatar = "LoadingAvatar";
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