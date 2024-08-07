using Assets.Scripts.BasisSdk.Players;
using Assets.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Assets.Scripts.Drivers
{
public class BasisRemoteBoneDriver : BaseBoneDriver
{
    public BasisRemotePlayer RemotePlayer;
    public Transform HeadAvatar;
    public Transform HipsAvatar;
    public BasisBoneControl Head;
    public BasisBoneControl Hips;
    public bool HasEvent = false;
    public void Initialize()
    {
        FindBone(out Head, BasisBoneTrackedRole.Head);
        FindBone(out Hips, BasisBoneTrackedRole.Hips);
        if (Head != null)
        {
            Head.HasTracked = BasisHasTracked.HasNoTracker;
        }
        if (Hips != null)
        {
            Hips.HasTracked = BasisHasTracked.HasNoTracker;
        }
        if (HasEvent == false)
        {
            OnSimulate += CalculateHeadBoneData;
            HasEvent = true;
        }
    }
    public void OnDestroy()
    {
        if (HasEvent)
        {
            OnSimulate -= CalculateHeadBoneData;
            HasEvent = false;
        }
    }
    public void CalculateHeadBoneData()
    {
        if (Head.HasBone && HeadAvatar != null)
        {
            Head.OutGoingData.position = HeadAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Head.OutGoingData.rotation = HeadAvatar.rotation;
        }
        if (Hips.HasBone && HipsAvatar != null)
        {
            Hips.OutGoingData.position = HipsAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Hips.OutGoingData.rotation = HipsAvatar.rotation;
        }
    }
    public void OnCalibration(BasisRemotePlayer remotePlayer)
    {
        HeadAvatar = RemotePlayer.Avatar.Animator.GetBoneTransform(HumanBodyBones.Head);
        HipsAvatar = RemotePlayer.Avatar.Animator.GetBoneTransform(HumanBodyBones.Hips);
        this.RemotePlayer = remotePlayer;
    }
}
}