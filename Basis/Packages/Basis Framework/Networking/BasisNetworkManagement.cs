using Basis.Network.Core;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Profiler;
using DarkRift.Basis_Common.Serializable;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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
        [HideInInspector]
        public string Password = "default_password";
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
        public static NetPeer LocalPlayerPeer;
        public static bool AddPlayer(BasisNetworkedPlayer NetPlayer)
        {
            if (Instance != null)
            {
                if (NetPlayer.Player != null)
                {
                    if (NetPlayer.Player.IsLocal == false)
                    {
                        RemotePlayers.TryAdd(NetPlayer.NetId, (BasisNetworkReceiver)NetPlayer.NetworkSend);
                        ReceiverArray = RemotePlayers.Values.ToArray();
                        ReceiverCount = ReceiverArray.Length;
                        Debug.Log("ReceiverCount was " + ReceiverCount);
                    }
                }
                else
                {
                    Debug.LogError("Player was Null!");
                }
                return Players.TryAdd(NetPlayer.NetId, NetPlayer);
            }
            else
            {
                Debug.LogError("No network Instance Existed!");
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
            BasisNetworkClient.Disconnect();
        }
        public void Update()
        {
            double TimeAsDouble = Time.timeAsDouble;

            // Schedule multithreaded tasks
            for (int Index = 0; Index < ReceiverCount; Index++)
            {
                try
                {
                    if (ReceiverArray[Index] != null)
                    {
                        ReceiverArray[Index].Compute(TimeAsDouble);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and continue with the next iteration
                    Debug.LogError($"Error in Compute at index {Index}: {ex.Message} {ex.StackTrace}");
                }
            }
        }
        public void LateUpdate()
        {
            double TimeAsDouble = Time.timeAsDouble;
            float deltaTime = Time.deltaTime;

            // Complete tasks and apply results
            for (int Index = 0; Index < ReceiverCount; Index++)
            {
                try
                {
                    if (ReceiverArray[Index] != null)
                    {
                        ReceiverArray[Index].Apply(TimeAsDouble, deltaTime);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and continue with the next iteration
                    Debug.LogError($"Error in Apply at index {Index}: {ex.Message} {ex.StackTrace}");
                }
            }
        }
        public static bool TryGetLocalPlayerID(out ushort LocalID)
        {
            if (Instance != null)
            {
                LocalID = (ushort)LocalPlayerPeer.RemoteId;
                return true;
            }
            LocalID = 0;
            return false;
        }
        public void SetupSceneEvents(BasisScene BasisScene)
        {
            BasisScene.OnNetworkMessageSend += BasisNetworkGenericMessages.OnNetworkMessageSend;
        }
        public void Connect()
        {
            Connect(Port, Ip, Password);
        }
        public void Connect(ushort Port, string IpString,string PrimitivePassword)
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
            Debug.Log("Network Starting Client");
            BasisNetworkClient.AuthenticationMessage = new Network.Core.Serializable.SerializableBasis.AuthenticationMessage
            {
                Message = Encoding.UTF8.GetBytes(PrimitivePassword)
            };
           // Debug.Log("Size is " + BasisNetworkClient.AuthenticationMessage.Message.Length);
            LocalPlayerPeer = BasisNetworkClient.StartClient(IpString, Port, readyMessage);
            Debug.Log("Network Client Started " + LocalPlayerPeer.RemoteId);
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
                BasisNetworkManagement.MainThreadContext.Post(_ =>
                {
                    try
                    {
                        LocalPlayerPeer = peer;
                        ushort LocalPlayerID = (ushort)peer.RemoteId;
                        // Create the local networked player asynchronously.
                        this.transform.GetPositionAndRotation(out Vector3 Position, out Quaternion Rotation);
                        BasisNetworkedPlayer LocalNetworkedPlayer = new BasisNetworkedPlayer();
                        Debug.Log("Network Id Updated " + LocalPlayerPeer.RemoteId);

                        LocalNetworkedPlayer.ProvideNetworkKey(LocalPlayerID);
                        // Initialize the local networked player.
                        LocalNetworkedPlayer.LocalInitalize(BasisLocalPlayer.Instance);
                        if (AddPlayer(LocalNetworkedPlayer))
                        {
                            Debug.Log($"Added local player {LocalPlayerID}");
                        }
                        else
                        {
                            Debug.LogError($"Cannot add player {LocalPlayerID}");
                        }
                        LocalNetworkedPlayer.InitalizeNetwork();
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
        public bool IsRunning = true;
        private async void PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log($"Client disconnected from server [{peer.Id}]");
            if (peer == LocalPlayerPeer)
            {
                await Task.Run(() =>
                {
                    BasisNetworkManagement.MainThreadContext.Post(async _ =>
                {
                    if (BasisNetworkManagement.Players.TryGetValue((ushort)LocalPlayerPeer.RemoteId, out BasisNetworkedPlayer NetworkedPlayer))
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
                    Players.Clear();
                    OwnershipPairing.Clear();
                    ReceiverCount = 0;
                    Debug.Log($"Client disconnected from server [{peer.RemoteId}] [{disconnectInfo.Reason}]");
                    SceneManager.LoadScene(0, LoadSceneMode.Single);//reset
                    await Boot_Sequence.BootSequence.OnAddressablesInitializationComplete();
                }, null);
                });
            }
        }
        private async void NetworkReceiveEvent(NetPeer peer, NetPacketReader Reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
        {
            switch (channel)
            {
                case BasisNetworkCommons.FallChannel:
                    if (deliveryMethod == DeliveryMethod.Unreliable)
                    {
                        if (Reader.TryGetByte(out byte Byte))
                        {
                            NetworkReceiveEvent(peer, Reader, Byte, deliveryMethod);
                        }
                        else
                        {
                            BNL.LogError($"Unknown channel no data remains: {channel} " + Reader.AvailableBytes);
                            Reader.Recycle();
                        }
                    }
                    else
                    {
                        BNL.LogError($"Unknown channel: {channel} " + Reader.AvailableBytes);
                        Reader.Recycle();
                    }
                    break;
                case BasisNetworkCommons.Disconnection:
                    BasisNetworkHandleRemoval.HandleDisconnection(Reader);
                    Reader.Recycle();
                    break;
                case BasisNetworkCommons.AvatarChangeMessage:
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkHandleAvatar.HandleAvatarChangeMessage(Reader);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.CreateRemotePlayer:
                    BasisNetworkManagement.MainThreadContext.Post(async _ =>
                    {
                        await BasisNetworkHandleRemote.HandleCreateRemotePlayer(Reader, this.transform);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.CreateRemotePlayers:
                    BasisNetworkManagement.MainThreadContext.Post(async _ =>
                    {
                        await BasisNetworkHandleRemote.HandleCreateAllRemoteClients(Reader, this.transform);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.OwnershipResponse:
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkGenericMessages.HandleOwnershipResponse(Reader);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.OwnershipTransfer:
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkGenericMessages.HandleOwnershipTransfer(Reader);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.VoiceChannel:
                    await BasisNetworkHandleVoice.HandleAudioUpdate(Reader);
                    Reader.Recycle();
                    break;
                case BasisNetworkCommons.MovementChannel:
                     BasisNetworkHandleAvatar.HandleAvatarUpdate(Reader);
                    Reader.Recycle();
                    break;
                case BasisNetworkCommons.SceneChannel:
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkGenericMessages.HandleServerSceneDataMessage(Reader,deliveryMethod);
                        Reader.Recycle();
                    }, null);
                    break;
                case BasisNetworkCommons.AvatarChannel:
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkGenericMessages.HandleServerAvatarDataMessage(Reader, deliveryMethod);
                        Reader.Recycle();
                    }, null);
                    break;
                default:
                    BNL.LogError($"this Channel was not been implemented {channel}");
                    Reader.Recycle();
                    break;
            }
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
            OwnershipTransferMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.OwnershipTransfer, DeliveryMethod.ReliableSequenced);
            BasisNetworkProfiler.OwnershipTransferMessageCounter.Sample(netDataWriter.Length);
        }
        public static void RequestCurrentOwnership(string UniqueNetworkId)
        {
            OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage
            {
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)BasisNetworkManagement.LocalPlayerPeer.RemoteId,
                },
                ownershipID = UniqueNetworkId
            };
            NetDataWriter netDataWriter = new NetDataWriter();
            OwnershipTransferMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter,BasisNetworkCommons.OwnershipResponse, DeliveryMethod.ReliableSequenced);
            BasisNetworkProfiler.RequestOwnershipTransferMessageCounter.Sample(netDataWriter.Length);
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
                if (Players.TryGetValue(id, out BasisNetworkedPlayer player))
                {
                    BasisNetworkedPlayer output = Players[id];
                    BasisPlayer = output.Player;
                    return true;
                }
                else
                {
                    if(JoiningPlayers.Contains(id))
                    {
                        Debug.LogError("Player was still Connecting when this was called!");
                    }
                    else
                    {
                        Debug.LogError("Player was not found, this also includes joining list, something is very wrong!");
                    }
                }
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
