using UnityEngine;

public class BasisRemoteBoneDriver : BaseBoneDriver
{
    public BasisRemotePlayer RemotePlayer;
    public Transform HeadAvatar;
    public Transform HipsAvatar;
    public BasisBoneControl Head;
    public BasisBoneControl Hips;
    public void Initialize()
    {
        FindBone(out Head, BasisBoneTrackedRole.Head);
        FindBone(out Hips, BasisBoneTrackedRole.Hips);
        if (Head != null)
        {
            Head.HasTrackerPositionDriver = BasisHasTracked.HasNoTracker;
            Head.HasTrackerRotationDriver = BasisHasTracked.HasNoTracker;
        }
        if (Hips != null)
        {
            Hips.HasTrackerPositionDriver = BasisHasTracked.HasNoTracker;
            Hips.HasTrackerRotationDriver = BasisHasTracked.HasNoTracker;
        }
        OnSimulate += CalculateHeadBoneData;
    }
    public void OnDestroy()
    {
        OnSimulate -= CalculateHeadBoneData;
    }
    public void CalculateHeadBoneData()
    {
        if (Head.HasBone && HeadAvatar != null)
        {
            Head.RawLocalData.position = HeadAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Head.RawLocalData.rotation = HeadAvatar.rotation;
        }
        if (Hips.HasBone && HipsAvatar != null)
        {
            Hips.RawLocalData.position = HipsAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Hips.RawLocalData.rotation = HipsAvatar.rotation;
        }
    }
    public void OnCalibration(BasisRemotePlayer remotePlayer)
    {
        HeadAvatar = RemotePlayer.Avatar.Animator.GetBoneTransform(HumanBodyBones.Head);
        HipsAvatar = RemotePlayer.Avatar.Animator.GetBoneTransform(HumanBodyBones.Hips);
        this.RemotePlayer = remotePlayer;
    }
}