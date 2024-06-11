using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static SerializableDarkRift;
/// <summary>
/// the goal of this script is to be the glue of consistent data between remote and local
/// </summary>
[DefaultExecutionOrder(15002)]
public abstract class BasisNetworkSendBase : MonoBehaviour
{
    public bool Ready;
    public BasisNetworkedPlayer NetworkedPlayer;
    public BasisAvatarData Target = new BasisAvatarData();
    public BasisAvatarData Output = new BasisAvatarData();
    public HumanPose HumanPose = new HumanPose();
    public LocalAvatarSyncMessage LASM = new LocalAvatarSyncMessage();
    public PlayerIdMessage NetworkNetID = new PlayerIdMessage();
    public DataJobs AvatarJobs;
    public HumanPoseHandler PoseHandler;
    [SerializeField]
    public BasisRangedFloatData PositionRanged;
    [SerializeField]
    public BasisRangedFloatData ScaleRanged;
    protected BasisNetworkSendBase()
    {
        LASM = new LocalAvatarSyncMessage()
        {
            array = new byte[0],
        };
        if (Target.Muscles.IsCreated == false)
        {
            Target.Muscles.ResizeArray(95);
        }
        if (Output.Muscles.IsCreated == false)
        {
            Output.Muscles.ResizeArray(95);
        }
        PositionRanged = new BasisRangedFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
        ScaleRanged = new BasisRangedFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
    }
    public void InitalizeAvatarStoredData(ref BasisAvatarData data, int VectorCount = 3, int QuaternionCount = 1, int MuscleCount = 95)
    {
        //data
        Debug.Log("Setting Up Position Rotation Muscles");
        data.Vectors = new NativeArray<Vector3>(VectorCount, Allocator.Persistent);
        data.Quaternions = new NativeArray<Quaternion>(QuaternionCount, Allocator.Persistent);
        data.Muscles = new NativeArray<float>(MuscleCount, Allocator.Persistent);
    }
    public void InitalizeDataJobs()
    {
        //jobs
        AvatarJobs.rotationJob = new UpdateAvatarRotationJob();
        AvatarJobs.positionJob = new UpdateAvatarPositionJob();
        AvatarJobs.muscleJob = new UpdateAvatarMusclesJob();
    }
    public abstract void Compute();
    public abstract void Initialize(BasisNetworkedPlayer NetworkedPlayer);
    public abstract void DeInitialize();
    public void OnAvatarCalibration()
    {
        if (NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.Avatar != null)
        {
            PoseHandler = new HumanPoseHandler(NetworkedPlayer.Player.Avatar.Animator.avatar, NetworkedPlayer.Player.Avatar.transform.transform);
        }
    }
}