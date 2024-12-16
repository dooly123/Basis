using Basis.Network.Core;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using DarkRift.Basis_Common.Serializable;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static BasisNetworkGenericMessages;
using static SerializableBasis;
namespace Basis.Scripts.Networking
{
    [DefaultExecutionOrder(15001)]
    public class BasisNetworkManagement : MonoBehaviour
    {
        public string Ip = "170.64.184.249";
        public ushort Port = 4296;
        /// <summary>
        /// fire when ownership is changed for a unique string
        /// </summary>
        public static OnNetworkMessageReceiveOwnershipTransfer OnOwnershipTransfer;
        public static ConcurrentDictionary<ushort, BasisNetworkedPlayer> Players = new ConcurrentDictionary<ushort, BasisNetworkedPlayer>();
        public static ConcurrentDictionary<ushort, BasisNetworkReceiver> RemotePlayers = new ConcurrentDictionary<ushort, BasisNetworkReceiver>();
        public static HashSet<ushort> JoiningPlayers = new HashSet<ushort>();
        public static BasisNetworkReceiver[] ReceiverArray;
        public static int ReceiverCount = 0;
        public static SynchronizationContext MainThreadContext;
        public static ushort LocalPlayerID;
        public static NetPeer LocalPlayerPeer;
        public static bool AddPlayer(BasisNetworkedPlayer NetPlayer)
        {
            if (Instance != null)
            {
                if (NetPlayer.Player != null && NetPlayer.Player.IsLocal == false)
                {
                    RemotePlayers.TryAdd(NetPlayer.NetId, (BasisNetworkReceiver)NetPlayer.NetworkSend);
                    ReceiverArray = RemotePlayers.Values.ToArray();
                    ReceiverCount = ReceiverArray.Length;
                    Debug.Log("ReceiverCount was " + ReceiverCount);
                }
                return Players.TryAdd(NetPlayer.NetId, NetPlayer);
            }
            return false;
        }
        public static bool RemovePlayer(ushort NetID)
        {
            if (Instance != null)
            {
                RemotePlayers.TryRemove(NetID,out BasisNetworkReceiver A);
                ReceiverArray = RemotePlayers.Values.ToArray();
                ReceiverCount = ReceiverArray.Length;
                //Debug.Log("ReceiverCount was " + ReceiverCount);
                return Players.Remove(NetID, out var B);
            }
            return false;
        }
        public static bool RemovePlayer(BasisNetworkedPlayer NetPlayer)
        {
            if (Instance != null)
            {
                if (NetPlayer.Player != null && NetPlayer.Player.IsLocal == false)
                {
                    RemotePlayers.Remove(NetPlayer.NetId, out BasisNetworkReceiver A);
                    ReceiverArray = RemotePlayers.Values.ToArray();
                    ReceiverCount = ReceiverArray.Length;
                    Debug.Log("ReceiverCount was " + ReceiverCount);
                }
                return Players.Remove(NetPlayer.NetId, out var B);
            }
            return false;
        }
        public bool ForceConnect = false;
        /// <summary>
        /// this occurs after the localplayer has been approved by the network and setup
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisLocalPlayer> OnLocalPlayerJoined;
        public static bool HasSentOnLocalPlayerJoin = false;
        /// <summary>
        /// this occurs after a remote user has been authenticated and joined & spawned
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisRemotePlayer> OnRemotePlayerJoined;
        /// <summary>
        /// this occurs after the localplayer has removed
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisLocalPlayer> OnLocalPlayerLeft;
        /// <summary>
        /// this occurs after a remote user has removed
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisRemotePlayer> OnRemotePlayerLeft;

        public static Action OnEnableInstanceCreate;
        public static BasisNetworkManagement Instance;
        public Dictionary<string, ushort> OwnershipPairing = new Dictionary<string, ushort>();
        // ConcurrentQueue to hold actions to be executed on the Unity main thread
        private static readonly ConcurrentQueue<Func<Task>> asyncActions = new ConcurrentQueue<Func<Task>>();
        // Method to enqueue a task
        public static void ScheduleOnMainThread(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            asyncActions.Enqueue(action);
        }
        // Method to process the queue in the Update method
        private async void Update()
        {
            while (asyncActions.TryDequeue(out Func<Task> action))
            {
                if (action != null)
                {
                    try
                    {
                        await action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error executing scheduled action: {ex.Message}");
                    }
                }
            }
        }
        public void OnEnable()
        {
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
            MainThreadContext = SynchronizationContext.Current;
            // Initialize AvatarBuffer
            BasisAvatarBufferPool.AvatarBufferPool(30);
            OwnershipPairing.Clear();
            if (BasisScene.Instance != null)
            {
                SetupSceneEvents(BasisScene.Instance);
            }
            BasisScene.Ready.AddListener(SetupSceneEvents);
            this.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            OnEnableInstanceCreate?.Invoke();
            if (ForceConnect)
            {
               Connect(Port, Ip);
            }
        }
        private void LogErrorOutput(string obj)
        {
           Debug.LogError(obj);
        }
        private void LogWarningOutput(string obj)
        {
            Debug.LogWarning(obj);
        }
        private void LogOutput(string obj)
        {
            Debug.Log(obj);
        }
        public void OnDestroy()
        {
            Players.Clear();
            BasisAvatarBufferPool.Clear();
            Disconnect();
            GameObject.Destroy(this.gameObject);
        }
        public void LateUpdate()
        {
            double TimeAsDouble = Time.timeAsDouble;
            for (int Index = 0; Index < ReceiverCount; Index++)
            {
                //schedule mulithreaded tasks
                ReceiverArray[Index].Compute(TimeAsDouble);
            }
            float deltaTime = Time.deltaTime;
            for (int Index = 0; Index < ReceiverCount; Index++)
            {
                //now that its all schedule start going through and
                //completing if they are not done already
                ReceiverArray[Index].Apply(TimeAsDouble, deltaTime);
            }
        }
        public static bool TryGetLocalPlayerID(out ushort LocalID)
        {
            if (Instance != null)
            {
                LocalID = LocalPlayerID;
                return true;
            }
            LocalID = 0;
            return false;
        }
        public void SetupSceneEvents(BasisScene BasisScene)
        {
            BasisScene.OnNetworkMessageSend += BasisNetworkGenericMessages.OnNetworkMessageSend;
        }
        public  void Connect()
        {
            Connect(Port, Ip);
        }
        public void Connect(ushort Port, string IpString)
        {
            BNL.LogOutput += LogOutput;
            BNL.LogWarningOutput += LogWarningOutput;
            BNL.LogErrorOutput += LogErrorOutput;
            Debug.Log("Connecting with Port " + Port + " IpString " + IpString);
            //   string result = BasisNetworkIPResolve.ResolveHosttoIP(IpString);
            //   Debug.Log($"DNS call: {IpString} resolves to {result}");
            BasisLocalPlayer BasisLocalPlayer = BasisLocalPlayer.Instance;
            byte[] Information = BasisBundleConversionNetwork.ConvertBasisLoadableBundleToBytes(BasisLocalPlayer.AvatarMetaData);
            ReadyMessage readyMessage = new ReadyMessage
            {
                localAvatarSyncMessage = BasisNetworkAvatarCompressor.InitalAvatarData(BasisLocalPlayer.Instance.Avatar.Animator),
                clientAvatarChangeMessage = new ClientAvatarChangeMessage
                {
                    byteArray = Information,
                    loadMode = BasisLocalPlayer.AvatarLoadMode,
                },
                playerMetaDataMessage = new PlayerMetaDataMessage
                {
                    playerUUID = BasisLocalPlayer.UUID,
                    playerDisplayName = BasisLocalPlayer.DisplayName
                }
            };
            Debug.Log("Network  Starting Client");
            LocalPlayerPeer = BasisNetworkClient.StartClient(IpString, Port, readyMessage);
            Debug.Log("Network Client Started");
            BasisNetworkClient.listener.PeerConnectedEvent += PeerConnectedEvent;
            BasisNetworkClient.listener.PeerDisconnectedEvent += PeerDisconnectedEvent;
            BasisNetworkClient.listener.NetworkReceiveEvent += NetworkReceiveEvent;
        }
        private async void PeerConnectedEvent(NetPeer peer)
        {
            await PeerConnectedEventAsync(peer);
        }
        private async Task PeerConnectedEventAsync(NetPeer peer)
        {
            Debug.Log("Success! Now setting up Networked Local Player");

            // Wrap the main logic in a task for thread safety and asynchronous execution.
            await Task.Run(() =>
            {
                BasisNetworkManagement.MainThreadContext.Post(async _ =>
                {
                    try
                    {
                        // Create the local networked player asynchronously.
                        this.transform.GetPositionAndRotation(out Vector3 Position,out Quaternion Rotation);
                        BasisNetworkedPlayer LocalNetworkedPlayer = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(
                            new InstantiationParameters(Position, Rotation, this.transform));

                        LocalPlayerID = (ushort)peer.Id;
                        LocalPlayerPeer = peer;

                        // Initialize the local networked player.
                        LocalNetworkedPlayer.LocalInitalize(BasisLocalPlayer.Instance, LocalPlayerID);

                        if (AddPlayer(LocalNetworkedPlayer))
                        {
                            Debug.Log($"Added local player {LocalPlayerID}");
                        }
                        else
                        {
                            Debug.LogError($"Cannot add player {LocalPlayerID}");
                        }

                        // Notify listeners about the local player joining.
                        OnLocalPlayerJoined?.Invoke(LocalNetworkedPlayer, BasisLocalPlayer.Instance);
                        HasSentOnLocalPlayerJoin = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error setting up the local player: {ex.Message}");
                    }
                }, null);
            });
        }
        private async void PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log($"Client disconnected from server [{peer.Id}]");
            if (peer == LocalPlayerPeer)
            {
                await Task.Run(() =>
                {
                    BasisNetworkManagement.MainThreadContext.Post(async _ =>
                {
                    if (BasisNetworkManagement.Players.TryGetValue((ushort)LocalPlayerPeer.Id, out BasisNetworkedPlayer NetworkedPlayer))
                    {
                        BasisNetworkManagement.OnLocalPlayerLeft?.Invoke(NetworkedPlayer, (Basis.Scripts.BasisSdk.Players.BasisLocalPlayer)NetworkedPlayer.Player);
                    }
                    if (disconnectInfo.Reason == DisconnectReason.RemoteConnectionClose)
                    {
                        if (disconnectInfo.AdditionalData.TryGetString(out string Reason))
                        {
                            Debug.LogError(Reason);
                        }
                    }
                    Debug.Log($"Client disconnected from server [{peer.Id}] [{disconnectInfo.Reason}]");
                    Players.Clear();
                    OwnershipPairing.Clear();
                    SceneManager.LoadScene(0, LoadSceneMode.Single);//reset
                    await Boot_Sequence.BootSequence.OnAddressablesInitializationComplete();
                }, null);
                });
            }
            else
            {
            }
        }
        public void Disconnect()
        {
            BasisNetworkClient.Disconnect();
        }
        private async void NetworkReceiveEvent(NetPeer peer, NetPacketReader Reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
        {
            switch (channel)
            {
                case BasisNetworkCommons.EventsChannel:
                    BasisMessageReceivedEventArgs e = new BasisMessageReceivedEventArgs
                    {
                        Tag = Reader.GetByte(),
                        SendMode = deliveryMethod,
                        ClientId = (ushort)peer.Id
                    };
                    ScheduleOnMainThread(async () =>
                    {
                        await NetworkReceiveEventTag(peer, Reader, e);
                    });
                    break;
                case BasisNetworkCommons.VoiceChannel:
                    await BasisNetworkHandleVoice.HandleAudioUpdate(Reader);
                    Reader.Recycle();
                    break;
                case BasisNetworkCommons.MovementChannel:
                    await BasisNetworkHandleAvatar.HandleAvatarUpdate(Reader);
                    Reader.Recycle();
                    break;
                case BasisNetworkCommons.SceneChannel:
                     e = new BasisMessageReceivedEventArgs
                    {
                        Tag = Reader.GetByte(),
                        SendMode = deliveryMethod,
                        ClientId = (ushort)peer.Id
                    };
                    ScheduleOnMainThread(async () =>
                    {
                        await NetworkReceiveEventTag(peer, Reader, e);
                    });
                    break;
                case BasisNetworkCommons.AvatarChannel:
                     e = new BasisMessageReceivedEventArgs
                    {
                        Tag = Reader.GetByte(),
                        SendMode = deliveryMethod,
                        ClientId = (ushort)peer.Id
                    };
                    ScheduleOnMainThread(async () =>
                    {
                        await NetworkReceiveEventTag(peer, Reader, e);
                    });
                    break;
                default:
                    BNL.LogError($"this Channel was not been implemented {channel}");
                    Reader.Recycle();
                    break;
            }
        }
        private async Task NetworkReceiveEventTag(NetPeer peer, NetPacketReader reader, BasisMessageReceivedEventArgs e)
        {
            switch (e.Tag) // Use e.Tag instead of message.Tag
            {
                case BasisNetworkTag.Disconnection:
                    BasisNetworkHandleRemoval.HandleDisconnection(reader);
                    break;
                case BasisNetworkTag.AvatarChangeMessage:
                    BasisNetworkHandleAvatar.HandleAvatarChangeMessage(reader);
                    break;
                case BasisNetworkTag.CreateRemotePlayer:
                    await BasisNetworkHandleRemote.HandleCreateRemotePlayer(reader, this.transform);
                    break;
                case BasisNetworkTag.CreateRemotePlayers:
                    await BasisNetworkHandleRemote.HandleCreateAllRemoteClients(reader, this.transform);
                    break;
                case BasisNetworkTag.SceneGenericMessage:
                    BasisNetworkGenericMessages.HandleServerSceneDataMessage(reader);
                    break;
                case BasisNetworkTag.SceneGenericMessage_NoRecipients:
                    BasisNetworkGenericMessages.HandleServerSceneDataMessage_NoRecipients(reader);
                    break;
                case BasisNetworkTag.SceneGenericMessage_NoRecipients_NoPayload:
                    BasisNetworkGenericMessages.HandleServerSceneDataMessage_NoRecipients_NoPayload(reader);
                    break;
                case BasisNetworkTag.AvatarGenericMessage:
                    BasisNetworkGenericMessages.HandleServerAvatarDataMessage(reader);
                    break;
                case BasisNetworkTag.AvatarGenericMessage_NoRecipients:
                    BasisNetworkGenericMessages.HandleServerAvatarDataMessage_NoRecipients(reader);
                    break;
                case BasisNetworkTag.AvatarGenericMessage_NoRecipients_NoPayload:
                    BasisNetworkGenericMessages.HandleServerAvatarDataMessage_NoRecipients_NoPayload(reader);
                    break;
                case BasisNetworkTag.AvatarGenericMessage_Recipients_NoPayload:
                    BasisNetworkGenericMessages.HandleServerAvatarDataMessage_Recipients_NoPayload(reader);
                    break;
                case BasisNetworkTag.SceneGenericMessage_Recipients_NoPayload:
                    BasisNetworkGenericMessages.HandleServerSceneDataMessage_Recipients_NoPayload(reader);
                    break;
                case BasisNetworkTag.OwnershipResponse:
                    BasisNetworkGenericMessages.HandleOwnershipResponse(reader);
                    break;
                case BasisNetworkTag.OwnershipTransfer:
                    BasisNetworkGenericMessages.HandleOwnershipTransfer(reader);
                    break;
                default:
                    Debug.Log("Unknown message tag: " + e.Tag);
                    break;
            }
            reader.Recycle();
        }
        public static void RequestOwnership(string UniqueNetworkId, ushort NewOwner)
        {
            OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage
            {
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = NewOwner
                },
                ownershipID = UniqueNetworkId
            };
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put(BasisNetworkTag.OwnershipTransfer);
            OwnershipTransferMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.EventsChannel, DeliveryMethod.ReliableSequenced);
        }
        public static void RequestCurrentOwnership(string UniqueNetworkId)
        {
            OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage
            {
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)BasisNetworkManagement.LocalPlayerPeer.Id,
                },
                ownershipID = UniqueNetworkId
            };
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put(BasisNetworkTag.OwnershipResponse);
            OwnershipTransferMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter,BasisNetworkCommons.EventsChannel, DeliveryMethod.ReliableSequenced);
        }
        public static bool AvatarToPlayer(BasisAvatar Avatar, out BasisPlayer BasisPlayer, out BasisNetworkedPlayer NetworkedPlayer)
        {
            if (Instance == null)
            {
                Debug.LogError("Network Not Ready!");
                NetworkedPlayer = null;
                BasisPlayer = null;
                return false;
            }
            if (Avatar == null)
            {
                Debug.LogError("Missing Avatar! Make sure your not sending in a null item");
                NetworkedPlayer = null;
                BasisPlayer = null;
                return false;
            }
            if (Avatar.TryGetLinkedPlayer(out ushort id))
            {
                BasisNetworkedPlayer output = Players[id];
                NetworkedPlayer = output;
                BasisPlayer = output.Player;
                return true;
            }
            else
            {
                Debug.LogError("the player was not assigned at this time!");
            }
            NetworkedPlayer = null;
            BasisPlayer = null;
            return false;
        }
        /// <summary>
        /// on the remote player this will only work...
        /// </summary>
        /// <param name="Avatar"></param>
        /// <param name="BasisPlayer"></param>
        /// <returns></returns>
        public static bool AvatarToPlayer(BasisAvatar Avatar, out BasisPlayer BasisPlayer)
        {
            if (Instance == null)
            {
                Debug.LogError("Network Not Ready!");
                BasisPlayer = null;
                return false;
            }
            if (Avatar == null)
            {
                Debug.LogError("Missing Avatar! Make sure your not sending in a null item");
                BasisPlayer = null;
                return false;
            }
            if (Avatar.TryGetLinkedPlayer(out ushort id))
            {
                BasisNetworkedPlayer output = Players[id];
                BasisPlayer = output.Player;
                return true;
            }
            else
            {
                Debug.LogError("the player was not assigned at this time!");
            }
            BasisPlayer = null;
            return false;
        }
        public static bool PlayerToNetworkedPlayer(BasisPlayer BasisPlayer, out BasisNetworkedPlayer NetworkedPlayer)
        {
            if (Instance == null)
            {
                Debug.LogError("Network Not Ready!");
                NetworkedPlayer = null;
                return false;
            }
            if (BasisPlayer == null)
            {
                Debug.LogError("Missing Player! make sure your not sending in a null item");
                NetworkedPlayer = null;
                return false;
            }
            int BasisPlayerInstance = BasisPlayer.GetInstanceID();
            foreach (BasisNetworkedPlayer NPlayer in Players.Values)
            {
                if (NPlayer == null)
                {
                    continue;
                }
                if (NPlayer.Player == null)
                {
                    continue;
                }
                if (NPlayer.Player.GetInstanceID() == BasisPlayerInstance)
                {
                    NetworkedPlayer = NPlayer;
                    return true;
                }
            }
            NetworkedPlayer = null;
            return false;
        }
        // API to get the oldest available ushort starting from 0
        public ushort GetOldestAvailablePlayerUshort()
        {
            ushort smallestValue = ushort.MaxValue; // Initialize with the maximum possible ushort value

            // Iterate over the dictionary's keys
            foreach (ushort key in Players.Keys)
            {
                if (key < smallestValue) // If a smaller key is found, update smallestValue
                {
                    smallestValue = key;
                }
            }

            return smallestValue;
        }
    }
}