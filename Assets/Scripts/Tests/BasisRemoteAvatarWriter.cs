using UnityEngine;
using UnityEngine.Rendering;
using static SerializableDarkRift;

public class BasisRemoteAvatarWriter : MonoBehaviour
{
    public Animator Sender;
    private HumanPoseHandler SenderPoseHandler;
    public HumanPose Pose;
    public BasisAvatarData Target = new BasisAvatarData();
    private float updateInterval = 0.05f;
    private float timer = 0f;
    public BasisLocalAvatarReader LocalAvatarReader;
    public BasisRangedUshortFloatData PositionRanged;
    public BasisRangedUshortFloatData ScaleRanged;
    public LocalAvatarSyncMessage LocalAvatarSyncMessage;
    public void OnEnable()
    {
        SenderPoseHandler = new HumanPoseHandler(Sender.avatar, Sender.transform);
        if (Target.Muscles.IsCreated == false)
        {
            Target.Muscles.ResizeArray(95);
        }
        PositionRanged = new BasisRangedUshortFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
        ScaleRanged = new BasisRangedUshortFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
        LocalAvatarSyncMessage.array = new byte[224];
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
        BasisNetworkAvatarCompressor.CompressAvatar(ref Target,ref Pose, SenderPoseHandler, Sender, ref LocalAvatarSyncMessage, PositionRanged, ScaleRanged);
        if (LocalAvatarReader != null)
        {
            LocalAvatarReader.ReceiveAvatarUpdate(LocalAvatarSyncMessage.array);
        }
    }
}