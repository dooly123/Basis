using UnityEngine;

public class BasisRemotePlayer : BasisPlayer
{
    public BasisRemoteBoneDriver RemoteBoneDriver;
    public BasisRemoteAvatarDriver RemoteAvatarDriver;
    public GameObject AudioSourceGameobject;
    public BasisBoneControl MouthControl;
    public async void RemoteInitialize()
    {
        IsLocal = false;
        RemoteBoneDriver.CreateInitialArrays(RemoteBoneDriver.transform);
        RemoteBoneDriver.Initialize();
        RemoteAvatarDriver.CalibrationComplete.AddListener(RemoteCalibration);
        if (Avatar == null)
        {
            CreateAvatar();
        }
        RemoteBoneDriver.FindBone(out MouthControl, BasisBoneTrackedRole.Mouth);
        await BasisRemoteNamePlate.LoadRemoteNamePlate(this);
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
        RemoteBoneDriver.OnCalibration(this);
    }
}