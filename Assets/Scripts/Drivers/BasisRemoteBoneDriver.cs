using UnityEngine;

public class BasisRemoteBoneDriver : BaseBoneDriver
{
    public BasisBoneControl Head;
    public void Initialize()
    {
        FindBone(out Head, BasisBoneTrackedRole.Head);
        if (Head != null)
        {
            Head.HasTrackerPositionDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
            Head.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
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
            Head.LocalRawPosition = HeadAvatar.position - RemotePlayer.RemoteDriver.transform.position;
            Head.LocalRawRotation = HeadAvatar.rotation;
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