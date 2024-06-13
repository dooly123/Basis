using UnityEngine;
using UnityEngine.Rendering;

public class BasisRemoteAvatarWriter : MonoBehaviour
{
    public Animator Sender;
    private HumanPoseHandler SenderPoseHandler;
    public HumanPose Pose;
    public BasisAvatarData Target = new BasisAvatarData();
    private float updateInterval = 0.05f;
    private float timer = 0f;
    public BasisLocalAvatarReader LocalAvatarReader;
    public BasisRangedFloatData PositionRanged;
    public BasisRangedFloatData ScaleRanged;
    public void OnEnable()
    {
        SenderPoseHandler = new HumanPoseHandler(Sender.avatar, Sender.transform);
        if (Target.Muscles.IsCreated == false)
        {
            Target.Muscles.ResizeArray(95);
        }
        PositionRanged = new BasisRangedFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
        ScaleRanged = new BasisRangedFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
    }
    private void LateUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer -= updateInterval;
            UpdateAvatarData();
        }
    }
    private void UpdateAvatarData()
    {
        BasisNetworkAvatarCompressor.CompressAvatar(ref Target, Pose, SenderPoseHandler, Sender, out byte[] AvatarData,PositionRanged,ScaleRanged);
        if (LocalAvatarReader != null)
        {
            LocalAvatarReader.ReceiveAvatarUpdate(AvatarData);
        }
    }
}

