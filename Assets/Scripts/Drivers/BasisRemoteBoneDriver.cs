using UnityEngine;

public class BasisRemoteBoneDriver : BaseBoneDriver
{
    public BasisBoneControl Head;
    public void Initialize()
    {
        FindBone(out Head, BasisBoneTrackedRole.Head);
        if (Head != null)
        {
            Head.HasTrackerPositionDriver = BasisHasTracked.HasNoTracker;
            Head.HasTrackerRotationDriver = BasisHasTracked.HasNoTracker;
        }

    }
    public void Update()
    {
        if (Head.HasBone)
        {
            CalculateHeadBoneData();
        }
    }
    public void CalculateHeadBoneData()
    {
        if (HeadAvatar != null)
        {
            Head.RawLocalData.Position = HeadAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Head.RawLocalData.Rotation = HeadAvatar.rotation;
        }
    }
    public BasisRemotePlayer RemotePlayer;
    public Transform HeadAvatar;
    public void OnCalibration(BasisRemotePlayer RemotePlayer)
    {
        HeadAvatar = RemotePlayer.Avatar.Animator.GetBoneTransform(HumanBodyBones.Head);
        this.RemotePlayer = RemotePlayer;
    }
}