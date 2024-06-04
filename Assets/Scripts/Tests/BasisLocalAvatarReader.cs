using UnityEngine;
using UnityEngine.AddressableAssets;

public class BasisLocalAvatarReader : MonoBehaviour
{
    public Animator Receiver;
    public HumanPoseHandler ReceiverPoseHandler;
    public HumanPose HumanPose;
    public BasisAvatarData Target = new BasisAvatarData();

    public BasisAvatarData Output = new BasisAvatarData();

    public Vector3 VisualOffset;
    private float LerpTimeSpeedMovement = 0;
    private float LerpTimeSpeedRotation = 0;
    private float LerpTimeSpeedMuscles = 0;
    public Vector3 ScaleOffset;
    public Vector3 PlayerPosition;
    public BasisAvatarLerpDataSettings Settings;
    public bool Ready = false;
    public BasisRangedFloatData PositionRanged;
    public BasisRangedFloatData ScaleRanged;
    public async void OnEnable()
    {
        ReceiverPoseHandler = new HumanPoseHandler(Receiver.avatar, Receiver.transform);
        Receiver.enabled = false;
        Target.Muscles = new float[95];
        Output.Muscles = new float[95];
        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BasisAvatarLerpDataSettings> Handle = Addressables.LoadAssetAsync<BasisAvatarLerpDataSettings>(BasisAvatarLerp.Settings);
        await Handle.Task;
        Settings = Handle.Result;
        Ready = true;
        PositionRanged = new BasisRangedFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
        ScaleRanged = new BasisRangedFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
    }
    public void RecieveAvatarUpdate(byte[] AvatarData)
    {
        BasisNetworkAvatarDecompressor.DecompressAvatar(ref Target, AvatarData, PositionRanged,ScaleRanged);
    }
    public void LateUpdate()
    {
        if (Ready)
        {
            float DeltaTime = Time.deltaTime;
            LerpTimeSpeedMovement = DeltaTime * Settings.LerpSpeedMovement;
            LerpTimeSpeedRotation = DeltaTime * Settings.LerpSpeedRotation;
            LerpTimeSpeedMuscles = DeltaTime * Settings.LerpSpeedMuscles;
            BasisAvatarLerp.UpdateAvatar(ref Output, Target, LerpTimeSpeedMovement, LerpTimeSpeedRotation, LerpTimeSpeedMuscles, Settings.TeleportDistance);
            ApplyPoseData(Receiver, Output, ref HumanPose);

            ReceiverPoseHandler.SetHumanPose(ref HumanPose);
        }
    }
    public void ApplyPoseData(Animator Animator, BasisAvatarData Output, ref HumanPose Pose)
    {
        Pose.bodyPosition = Output.BodyPosition;
        Pose.bodyRotation = Output.Rotation;
        Pose.muscles = Output.Muscles;
        PlayerPosition = Output.PlayerPosition;

        Animator.transform.localScale = Output.Scale;

        //we scale the position 
        ScaleOffset = Output.Scale - Vector3.one;
        PlayerPosition.Scale(ScaleOffset);
        Animator.transform.position = -PlayerPosition + VisualOffset;
    }
}