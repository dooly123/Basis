using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Drivers
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
            Head.HasTracked = BasisHasTracked.HasTracker;
        }
        if (Hips != null)
        {
            Hips.HasTracked = BasisHasTracked.HasTracker;
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
        DeInitalzeGizmos();
    }
    public void CalculateHeadBoneData()
    {
        if (Head.HasBone && HeadAvatar != null)
        {
            Head.IncomingData.position = HeadAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Head.IncomingData.rotation = HeadAvatar.rotation;
        }
        if (Hips.HasBone && HipsAvatar != null)
        {
            Hips.IncomingData.position = HipsAvatar.position - RemotePlayer.RemoteBoneDriver.transform.position;
            Hips.IncomingData.rotation = HipsAvatar.rotation;
        }
    }
    public void OnCalibration(BasisRemotePlayer remotePlayer)
    {
        HeadAvatar = RemotePlayer.BasisAvatar.Animator.GetBoneTransform(HumanBodyBones.Head);
        HipsAvatar = RemotePlayer.BasisAvatar.Animator.GetBoneTransform(HumanBodyBones.Hips);
        this.RemotePlayer = remotePlayer;
    }
}
}