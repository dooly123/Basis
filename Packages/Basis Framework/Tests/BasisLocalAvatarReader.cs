using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.Smoothing;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Basis.Scripts.Tests
{
    public class BasisLocalAvatarReader : MonoBehaviour
    {
        public Animator Receiver;
        public HumanPoseHandler ReceiverPoseHandler;
        public HumanPose HumanPose;
        public BasisAvatarData Target = new BasisAvatarData();

        public BasisAvatarData Output = new BasisAvatarData();
        private float LerpTimeSpeedMovement = 0;
        private float LerpTimeSpeedRotation = 0;
        private float LerpTimeSpeedMuscles = 0;
        public Vector3 ScaleOffset;
        public Vector3 PlayerPosition;
        public BasisAvatarLerpDataSettings Settings;
        public bool Ready = false;
        public BasisRangedUshortFloatData PositionRanged;
        public BasisRangedUshortFloatData ScaleRanged;
        public BasisDataJobs AvatarJobs;
        public async void OnEnable()
        {
            InitalizeDataJobs();
            ReceiverPoseHandler = new HumanPoseHandler(Receiver.avatar, Receiver.transform);
            Receiver.enabled = false;
            InitalizeAvatarStoredData(Target);
            InitalizeAvatarStoredData(Output);
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BasisAvatarLerpDataSettings> Handle = Addressables.LoadAssetAsync<BasisAvatarLerpDataSettings>(BasisAvatarLerp.Settings);
            await Handle.Task;
            Settings = Handle.Result;
            Ready = true;
            PositionRanged = new BasisRangedUshortFloatData(-BasisNetworkConstants.MaxPosition, BasisNetworkConstants.MaxPosition, BasisNetworkConstants.PositionPrecision);
            ScaleRanged = new BasisRangedUshortFloatData(BasisNetworkConstants.MinimumScale, BasisNetworkConstants.MaximumScale, BasisNetworkConstants.ScalePrecision);
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
        public void InitalizeAvatarStoredData(BasisAvatarData data, int VectorCount = 3, int QuaternionCount = 1, int MuscleCount = 95)
        {
            //data
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
        public void ReceiveAvatarUpdate(byte[] AvatarData)
        {
            BasisNetworkAvatarDecompressor.DecompressAvatar(ref Target, AvatarData, PositionRanged, ScaleRanged);
        }
        public void LateUpdate()
        {
            if (Ready)
            {
                float DeltaTime = Time.deltaTime;
                LerpTimeSpeedMovement = DeltaTime * Settings.LerpSpeedMovement;
                LerpTimeSpeedRotation = DeltaTime * Settings.LerpSpeedRotation;
                LerpTimeSpeedMuscles = DeltaTime * Settings.LerpSpeedMuscles;
                BasisAvatarLerp.UpdateAvatar(ref Output, Target, AvatarJobs, LerpTimeSpeedMovement, LerpTimeSpeedRotation, LerpTimeSpeedMuscles, Settings.TeleportDistance);
                ApplyPoseData(Receiver, Output, ref HumanPose);

                ReceiverPoseHandler.SetHumanPose(ref HumanPose);
            }
        }
        public void ApplyPoseData(Animator Animator, BasisAvatarData Output, ref HumanPose Pose)
        {
            Pose.bodyPosition = Output.Vectors[1];
            Pose.bodyRotation = Output.Quaternions[0];
            Output.Muscles.CopyTo(Pose.muscles);
            PlayerPosition = Output.Vectors[0];

            Animator.transform.localScale = Output.Vectors[3];

            //we scale the position 
            ScaleOffset = Output.Vectors[3] - Vector3.one;
            PlayerPosition.Scale(ScaleOffset);
            Animator.transform.position = -PlayerPosition;
        }
    }
}