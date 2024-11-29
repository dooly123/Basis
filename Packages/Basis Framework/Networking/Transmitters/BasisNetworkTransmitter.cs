using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management.Devices.Desktop;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.Transmitters
{
    [DefaultExecutionOrder(15001)]
    public partial class BasisNetworkTransmitter : BasisNetworkSendBase
    {
        public bool HasEvents = false;
        public float timer = 0f;
        public float interval = 0.0333333333333333f;
        public float activeDistance;
        [SerializeField]
        public BasisAudioTransmission AudioTransmission = new BasisAudioTransmission();
        private NativeArray<float3> targetPositions;  // NativeArray for multithreading.
        private NativeArray<float> distances;         // Distance results.
        public float UnClampedInterval;

        public static float DefaultInterval = 0.0333333333333333f;
        public static float BaseMultiplier = 1f; // Starting multiplier.
        public static float IncreaseRate = 0.0075f; // Rate of increase per unit distance.

        public int InitalizedLength = -1;
        public DistanceCalculationJob distanceJob = new DistanceCalculationJob();
        public ClosestTransformJob closestJob = new ClosestTransformJob();
        public JobHandle distanceJobHandle;
        public JobHandle closestJobHandle;
        public override void Compute()
        {
            if (Ready && NetworkedPlayer.Player.Avatar != null)
            {
                BasisNetworkAvatarCompressor.Compress(this, NetworkedPlayer.Player.Avatar.Animator);
            }
        }
        void SendOutLatest()
        {
            timer += Time.deltaTime;
            if (timer >= interval) // Trigger Compute at specified intervals
            {
                ScheduleCheck();
                Compute();
                distanceJobHandle.Complete();

                closestJob.distances = distanceJob.distances;
                closestJob.Length = distances.Length;

                closestJobHandle = closestJob.Schedule(closestJobHandle);
                closestJobHandle.Complete();

                activeDistance = closestJob.result[0];
                //this is a int miliseconds but i need to know if that will work for the timer check

                UnClampedInterval = DefaultInterval * (BaseMultiplier + (activeDistance * IncreaseRate));
                interval = math.clamp(UnClampedInterval, 0.005f, 5);
                timer = 0;
            }
        }
        public void OnDestroy()
        {
            DeInitialize();
        }
        public override void Initialize(BasisNetworkedPlayer networkedPlayer)
        {
            if (Ready == false)
            {
                NetworkedPlayer = networkedPlayer;
                AudioTransmission.OnEnable(networkedPlayer);
                OnAvatarCalibration();
                if (HasEvents == false)
                {
                    NetworkedPlayer.Player.OnAvatarSwitchedFallBack += OnAvatarCalibration;
                    NetworkedPlayer.Player.OnAvatarSwitched += OnAvatarCalibration;
                    NetworkedPlayer.Player.OnAvatarSwitched += SendOutLatestAvatar;
                    BasisLocalInputActions.AfterAvatarChanges += SendOutLatest;
                    BasisNetworkManagement.OnRemotePlayerJoined += OnRemoteJoined;
                    BasisNetworkManagement.OnRemotePlayerLeft += OnRemoteLeft;
                    ResizeOrCreateArrayData(BasisNetworkManagement.ReceiverCount);
                    HasEvents = true;
                }
                Ready = true;
            }
            else
            {
                Debug.Log("Already Ready");
            }
        }

        private void OnRemoteLeft(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
        {
            ResizeOrCreateArrayData(BasisNetworkManagement.ReceiverCount);
        }

        private void OnRemoteJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
        {
            ResizeOrCreateArrayData(BasisNetworkManagement.ReceiverCount);
        }
        public void ScheduleCheck()
        {
            distanceJob.referencePosition = NetworkedPlayer.MouthBone.OutgoingWorldData.position;
            for (int Index = 0; Index < BasisNetworkManagement.ReceiverCount; Index++)
            {
                targetPositions[Index] = BasisNetworkManagement.ReceiverArray[Index].NetworkedPlayer.MouthBone.OutgoingWorldData.position;
            }
            distanceJobHandle = distanceJob.Schedule(targetPositions.Length, 64);
        }
        public NativeArray<float> result;
        public void ResizeOrCreateArrayData(int TotalUserCount)
        {
            if (InitalizedLength != TotalUserCount)
            {
                if (closestJobHandle.IsCompleted == false)
                {
                    closestJobHandle.Complete();
                }
                if (targetPositions.IsCreated)
                {
                    targetPositions.Dispose();
                }
                if (distances.IsCreated)
                {
                    distances.Dispose();
                }
                if (result.IsCreated)
                {
                    result.Dispose();
                }
                result = new NativeArray<float>(1, Allocator.TempJob);
                targetPositions = new NativeArray<float3>(TotalUserCount, Allocator.Persistent);
                distances = new NativeArray<float>(TotalUserCount, Allocator.Persistent);
                InitalizedLength = TotalUserCount;

                // Step 2: Find closest index in the next frame
                closestJob.distances = distances;
                distanceJob.distances = distances;

                distanceJob.targetPositions = targetPositions;

                closestJob.result = result;
            }
        }
        public override void DeInitialize()
        {
            if (Ready)
            {
                AudioTransmission.OnDisable();
            }
            if (HasEvents)
            {
                NetworkedPlayer.Player.OnAvatarSwitchedFallBack -= OnAvatarCalibration;
                NetworkedPlayer.Player.OnAvatarSwitched -= OnAvatarCalibration;
                NetworkedPlayer.Player.OnAvatarSwitched -= SendOutLatestAvatar;
                BasisLocalInputActions.AfterAvatarChanges -= SendOutLatest;
                BasisNetworkManagement.OnRemotePlayerJoined -= OnRemoteJoined;
                BasisNetworkManagement.OnRemotePlayerLeft -= OnRemoteLeft;
                if (targetPositions.IsCreated) targetPositions.Dispose();
                if (distances.IsCreated) distances.Dispose();
                if (result.IsCreated)
                {
                    result.Dispose();
                }
                HasEvents = false;
            }
        }
        public void SendOutLatestAvatar()
        {
            byte[] LAI = BasisBundleConversionNetwork.ConvertBasisLoadableBundleToBytes(NetworkedPlayer.Player.AvatarMetaData);
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                ClientAvatarChangeMessage ClientAvatarChangeMessage = new ClientAvatarChangeMessage
                {
                    byteArray = LAI,
                    loadMode = NetworkedPlayer.Player.AvatarLoadMode,
                };
                writer.Write(ClientAvatarChangeMessage);
                using (var msg = Message.Create(BasisTags.AvatarChangeMessage, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.EventsChannel, DeliveryMethod.ReliableOrdered);
                }
            }
        }
       // [BurstCompile]
        public struct DistanceCalculationJob : IJobParallelFor
        {
            [ReadOnly]
            public float3 referencePosition;
            [ReadOnly] 
            public NativeArray<float3> targetPositions;
            [WriteOnly]
            public NativeArray<float> distances;

            public void Execute(int index)
            {
                Vector3 diff = targetPositions[index] - referencePosition;
                distances[index] = diff.sqrMagnitude;
                //Debug.Log(" distances " + index + " | " + distances[index] + " Refer " + referencePosition + " To " + targetPositions[index]);
            }
        }

        [BurstCompile]
        public struct ClosestTransformJob : IJob
        {
            public int Length;
            [ReadOnly] public NativeArray<float> distances;
            public NativeArray<float> result; // Shared writable array for results

            public void Execute()
            {
                float smallestDistance = float.MaxValue;

                for (int Index = 0; Index < Length; Index++)
                {
                    if (distances[Index] < smallestDistance)
                    {
                        smallestDistance = distances[Index];
                    }
                }

                // Store the result
                result[0] = smallestDistance;
            }
        }
    }
}