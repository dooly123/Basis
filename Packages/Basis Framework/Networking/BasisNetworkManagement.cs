using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using JetBrains.Annotations;
using LiteNetLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public bool HasAuthenticated = false;
        public ReadyMessage readyMessage = new ReadyMessage();
        /// <summary>
        /// fire when ownership is changed for a unique string
        /// </summary>
        public static OnNetworkMessageReceiveOwnershipTransfer OnOwnershipTransfer;
        public static ConcurrentDictionary<ushort, BasisNetworkedPlayer> Players = new ConcurrentDictionary<ushort, BasisNetworkedPlayer>();
        public static ConcurrentDictionary<ushort, BasisNetworkReceiver> RemotePlayers = new ConcurrentDictionary<ushort, BasisNetworkReceiver>();
        public static HashSet<ushort> JoiningPlayers = new HashSet<ushort>();
        public static BasisNetworkReceiver[] ReceiverArray;
        public static int ReceiverCount = 0;
        public static ushort LocalPlayerID;
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
                Debug.Log("ReceiverCount was " + ReceiverCount);
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
        public bool TryToReconnectAutomatically = true;
        public bool HasInitalizedClient = false;
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
            }            // Initialize AvatarBuffer
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
                Connect(Port, Ip, ISServer);
            }
        }
        public void OnDestroy()
        {
            Players.Clear();
            BasisAvatarBufferPool.Clear();
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
        public void Connect()
        {
            Connect(Port, Ip, ISServer);
        }
        public void Connect(ushort Port, string IpString, bool IsHostMode)
        {
            Debug.Log("Connecting with Port " + Port + " IpString " + IpString + " Is Server = " + IsHostMode);
            if (IsHostMode)
            {
                BasisNetworkServer.StartServer();
            }
            HasAuthenticated = false;
            if (HasInitalizedClient == false)
            {
                Client.Initialize();
                Client.Disconnected += Disconnected;
                HasInitalizedClient = true;
            }
            if (HasAuthenticated == false)
            {
                HasAuthenticated = true;
                BasisNetworkClient.listener.NetworkReceiveEvent += MainThreadMessageReceived;
            }
            string result = BasisNetworkIPResolve.ResolveHosttoIP(IpString);
            Debug.Log($"DNS call: {IpString} resolves to {result}");
            Client.ConnectInBackground(BasisNetworkIPResolve.IpOutput(result.ToString()), Port, Callback);
        }
        public bool ISServer = false;
        private void Disconnected(object sender, DisconnectedEventArgs e)
        {
            Debug.LogError("Disconnected from Server " + e.Error);
            Disconnect();
            Players.Clear();
            if (TryToReconnectAutomatically)
            {
                Connect(Port, Ip, ISServer);
            }
            else
            {
                SceneManager.LoadScene(0, LoadSceneMode.Single);//reset
            }
            OwnershipPairing.Clear();
        }
        public void Disconnect()
        {
            if (HasAuthenticated)
            {
                Client.Client.MessageReceived -= MainThreadMessageReceived;
                HasAuthenticated = false;
            }
            Client.Close();
        }
        public void Callback([CanBeNull] Exception e)
        {
            if (e == null)
            {
            }
            else
            {
                Debug.LogError("Failed to connect: " + e.Message);
            }
        }
        private void MainThreadMessageReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            switch (e.Tag) // Use e.Tag instead of message.Tag
            {
                case BasisNetworkTag.AvatarMuscleUpdateTag:
                    await BasisNetworkHandleAvatar.HandleAvatarUpdate(e);
                    break;
                case BasisNetworkTag.AudioSegmentTag:
                    await BasisNetworkHandleVoice.HandleAudioUpdate(e);
                    break;

                case BasisNetworkTag.DisconnectTag:
                    BasisNetworkHandleRemoval.HandleDisconnection(reader);
                    break;

                case BasisNetworkTag.AvatarChangeMessage:
                    BasisNetworkHandleAvatar.HandleAvatarChangeMessage(reader);
                    break;

                case BasisNetworkTag.CreateRemotePlayerTag:
                    await BasisNetworkHandleRemote.HandleCreateRemotePlayer(reader, this.transform);
                    break;

                case BasisNetworkTag.CreateRemotePlayersTag:
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
                    using (LiteNetLib.NetPacketReader reader = message.GetReader())
                    {
                        BasisNetworkGenericMessages.HandleOwnershipResponse(reader);
                    }
                    break;

                case BasisNetworkTag.OwnershipTransfer:
                    using (LiteNetLib.NetPacketReader reader = message.GetReader())
                    {
                        BasisNetworkGenericMessages.HandleOwnershipTransfer(reader);
                    }
                    break;

                default:
                    Debug.Log("Unknown message tag: " + e.Tag);
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
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(OwnershipTransferMessage);
                using (Message serverOwnershipInitialize = Message.Create(BasisTags.OwnershipTransfer, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(serverOwnershipInitialize, DarkRift.Server.Plugins.Commands.BasisNetworking.EventsChannel, DeliveryMethod.ReliableSequenced);
                }
            }
        }
        public static void RequestCurrentOwnership(string UniqueNetworkId)
        {
            OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage
            {
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = BasisNetworkManagement.Instance.Client.ID
                },
                ownershipID = UniqueNetworkId
            };
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(OwnershipTransferMessage);
                using (Message serverOwnershipInitialize = Message.Create(BasisTags.OwnershipResponse, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(serverOwnershipInitialize, DarkRift.Server.Plugins.Commands.BasisNetworking.EventsChannel, DeliveryMethod.ReliableSequenced);
                }
            }
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