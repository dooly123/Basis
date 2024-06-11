using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SerializableDarkRift;

[DefaultExecutionOrder(15002)]
[System.Serializable]
public partial class BasisNetworkReceiver : BasisNetworkSendBase
{
    public Vector3 VisualOffset;
    public Vector3 ScaleOffset;
    public Vector3 PlayerPosition;

    private float lerpTimeSpeedMovement = 0;
    private float lerpTimeSpeedRotation = 0;
    private float lerpTimeSpeedMuscles = 0;
    private int dataSize = 1920;
    private int silentDataSize = 5760;
    public float[] silentData;
    public BasisAvatarLerpDataSettings Settings;

    [SerializeField]
    public BasisAudioReceiver AudioReceiverModule = new BasisAudioReceiver();

    public BasisRemotePlayer RemotePlayer;

    public override void Compute()
    {
        if (!IsAbleToUpdate())
            return;

        float deltaTime = Time.deltaTime;
        lerpTimeSpeedMovement = deltaTime * Settings.LerpSpeedMovement;
        lerpTimeSpeedRotation = deltaTime * Settings.LerpSpeedRotation;
        lerpTimeSpeedMuscles = deltaTime * Settings.LerpSpeedMuscles;

        BasisAvatarLerp.UpdateAvatar(ref Output, Target,AvatarJobs, lerpTimeSpeedMovement, lerpTimeSpeedRotation, lerpTimeSpeedMuscles, Settings.TeleportDistance);

        ApplyPoseData(NetworkedPlayer.Player.Avatar.Animator, Output, ref HumanPose);
        PoseHandler.SetHumanPose(ref HumanPose);

        RemotePlayer.RemoteDriver.Simulate();
        RemotePlayer.RemoteDriver.ApplyMovement();
        RemotePlayer.UpdateTransform(RemotePlayer.MouthControl.BoneTransform.position, RemotePlayer.MouthControl.BoneTransform.rotation);
    }

    public void LateUpdate()
    {
        if (Ready)
        {
            Compute();
            AudioReceiverModule.LateUpdate();
        }
    }

    public bool IsAbleToUpdate()
    {
        return NetworkedPlayer != null && NetworkedPlayer.Player != null && NetworkedPlayer.Player.Avatar != null;
    }

    public void ApplyPoseData(Animator animator, BasisAvatarData output, ref HumanPose pose)
    {
        pose.bodyPosition = output.Vectors[1];
        pose.bodyRotation = output.Quaternions[0];
        if (pose.muscles == null || pose.muscles.Length != output.Muscles.Length)
        {
            pose.muscles = output.Muscles.ToArray();
        }
        else
        {
            output.Muscles.CopyTo(pose.muscles);
        }
        PlayerPosition = Output.Vectors[0];

        animator.transform.localScale = Output.Vectors[2];

        //we scale the position 
        ScaleOffset = Output.Vectors[2] - Vector3.one;
        PlayerPosition.Scale(ScaleOffset);
        animator.transform.position = -PlayerPosition + VisualOffset;
    }

    public void ReceiveNetworkAudio(AudioSegment audioSegment)
    {
        if (AudioReceiverModule.decoder != null)
        {
            AudioReceiverModule.decoder.OnEncoded(audioSegment.audioSegmentData.buffer);
        }
    }

    public void ReceiveSilentNetworkAudio(AudioSilentSegmentData audioSilentSegment)
    {
        if (AudioReceiverModule.decoder != null)
        {
            AudioReceiverModule.OnDecodedSilence(silentData, dataSize);
        }
    }

    public void ReceiveNetworkAvatarData(ServerSideSyncPlayerMessage serverSideSyncPlayerMessage)
    {
        BasisNetworkAvatarDecompressor.DeCompress(this, serverSideSyncPlayerMessage);
    }

    public override async void Initialize(BasisNetworkedPlayer networkedPlayer)
    {
        if (!Ready)
        {
            InitalizeDataJobs();
            InitalizeAvatarStoredData(ref Target);
            InitalizeAvatarStoredData(ref Output);
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BasisAvatarLerpDataSettings> handle = Addressables.LoadAssetAsync<BasisAvatarLerpDataSettings>(BasisAvatarLerp.Settings);
            await handle.Task;
            Settings = handle.Result;
            Ready = true;
            NetworkedPlayer = networkedPlayer;
            RemotePlayer = (BasisRemotePlayer)NetworkedPlayer.Player;
            AudioReceiverModule.OnEnable(networkedPlayer, gameObject);
            OnAvatarCalibration();
            RemotePlayer.RemoteAvatarDriver.CalibrationComplete.AddListener(OnCalibration);
        }
        silentData = new float[silentDataSize];
        Array.Fill(silentData, 0f);
    }
    public void OnDestroy()
    {
        Target.Vectors.Dispose();
        Target.Quaternions.Dispose();
        Target.Muscles.Dispose();

        Output.Vectors.Dispose();
        Output.Quaternions.Dispose();
        Output.Muscles.Dispose();
    }
    public void OnCalibration()
    {
        AudioReceiverModule.OnCalibration(NetworkedPlayer);
    }

    public override void DeInitialize()
    {
        AudioReceiverModule.OnDisable();
    }
}