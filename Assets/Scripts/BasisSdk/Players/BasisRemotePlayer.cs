using UnityEngine;

public class BasisRemotePlayer : BasisPlayer
{
    public BasisRemoteBoneDriver RemoteDriver;
    public BasisRemoteAvatarDriver RemoteAvatarDriver;
    public GameObject AudioSourceGameobject;
    public BasisBoneControl MouthControl;
    public void RemoteInitialize()
    {
        IsLocal = false;
        RemoteDriver.CreateInitialArrays(RemoteDriver.transform);
        RemoteDriver.Initialize();
        RemoteAvatarDriver.CalibrationComplete.AddListener(RemoteCalibration);
        if (Avatar == null)
        {
            CreateAvatar();
        }
        RemoteDriver.FindBone(out MouthControl, BasisBoneTrackedRole.Mouth);
    }
    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        AudioSourceGameobject.transform.SetPositionAndRotation(position, rotation);
    }
    public async void CreateAvatar()
    {
        await BasisAvatarFactory.LoadAvatar(this, FallBackAvatar);
    }
    public void RemoteCalibration()
    {
        RemoteDriver.OnCalibration(this);
    }
}