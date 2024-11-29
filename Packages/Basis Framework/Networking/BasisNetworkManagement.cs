using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using DarkRift.Basis_Common.Serializable;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server.Plugins.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BasisNetworkGenericMessages;
using static SerializableDarkRift;
namespace Basis.Scripts.Networking
{
    public class BasisNetworkManagement : MonoBehaviour
    {
        public string Ip = "170.64.184.249";
        public ushort Port = 4296;
        public bool HasAuthenticated = false;

        public BasisLowLevelClient Client;
        public ReadyMessage readyMessage = new ReadyMessage();
        /// <summary>
        /// fire when ownership is changed for a unique string
        /// </summary>
        public static OnNetworkMessageReceiveOwnershipTransfer OnOwnershipTransfer;
        public static Dictionary<ushort, BasisNetworkedPlayer> Players = new Dictionary<ushort, BasisNetworkedPlayer>();
        public static Dictionary<ushort, BasisNetworkReceiver> RemotePlayers = new Dictionary<ushort, BasisNetworkReceiver>();
        public static HashSet<ushort> JoiningPlayers = new HashSet<ushort>();
        public static BasisNetworkReceiver[] ReceiverArray;
        public static int ReceiverCount = 0;
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
                RemotePlayers.Remove(NetID);
                return Players.Remove(NetID);
            }
            return false;
        }
        public static bool RemovePlayer(BasisNetworkedPlayer NetPlayer)
        {
            if (Instance != null)
            {
                if (NetPlayer.Player != null && NetPlayer.Player.IsLocal == false)
                {
                    RemotePlayers.Remove(NetPlayer.NetId);
                    ReceiverArray = RemotePlayers.Values.ToArray();
                    ReceiverCount = ReceiverArray.Length;
                    Debug.Log("ReceiverCount was " + ReceiverCount);
                }
                return Players.Remove(NetPlayer.NetId);
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
            BasisAvatarBufferPool.AvatarBufferPool();
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
        public static bool TryGetLocalPlayerID(out ushort LocalID)
        {
            if (Instance != null && Instance.Client != null)
            {
                LocalID = Instance.Client.ID;
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
        public void Connect(ushort Port, string IpString, bool StartServer)
        {
            Debug.Log("Connecting with Port " + Port + " IpString " + IpString + " Is Server = " + StartServer);
            if (StartServer)
            {
                BasisNetworkServer NetworkServer = BasisHelpers.GetOrAddComponent<BasisNetworkServer>(this.gameObject);
                NetworkServer.Create();
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
                Client.MessageReceived += MessageReceived;
            }
            Client.ConnectInBackground(BasisNetworkIPResolve.IpOutput(IpString), Port, Callback);
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
                Client.MessageReceived -= MessageReceived;
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
        private async void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case BasisTags.AuthSuccess:
                            await BasisNetworkLocalCreation.HandleAuthSuccess(this.transform);
                            break;

                        case BasisTags.AvatarMuscleUpdateTag:
                            BasisNetworkHandleAvatar.HandleAvatarUpdate(reader);
                            break;

                        case BasisTags.AudioSegmentTag:
                            BasisNetworkHandleVoice.HandleAudioUpdate(reader);
                            break;

                        case BasisTags.DisconnectTag:
                            BasisNetworkHandleRemoval.HandleDisconnection(reader);
                            break;

                        case BasisTags.AvatarChangeMessage:
                            BasisNetworkHandleAvatar.HandleAvatarChangeMessage(reader);
                            break;

                        case BasisTags.CreateRemotePlayerTag:
                            await BasisNetworkHandleRemote.HandleCreateRemotePlayer(reader, this.transform);
                            break;

                        case BasisTags.CreateRemotePlayersTag:
                            await BasisNetworkHandleRemote.HandleCreateAllRemoteClients(reader, this.transform);
                            break;

                        case BasisTags.SceneGenericMessage:
                            BasisNetworkGenericMessages.HandleServerSceneDataMessage(reader);
                            break;

                        case BasisTags.SceneGenericMessage_NoRecipients:
                            BasisNetworkGenericMessages.HandleServerSceneDataMessage_NoRecipients(reader);
                            break;

                        case BasisTags.SceneGenericMessage_NoRecipients_NoPayload:
                            BasisNetworkGenericMessages.HandleServerSceneDataMessage_NoRecipients_NoPayload(reader);
                            break;

                        case BasisTags.AvatarGenericMessage:
                            BasisNetworkGenericMessages.HandleServerAvatarDataMessage(reader);
                            break;

                        case BasisTags.AvatarGenericMessage_NoRecipients:
                            BasisNetworkGenericMessages.HandleServerAvatarDataMessage_NoRecipients(reader);
                            break;

                        case BasisTags.AvatarGenericMessage_NoRecipients_NoPayload:
                            BasisNetworkGenericMessages.HandleServerAvatarDataMessage_NoRecipients_NoPayload(reader);
                            break;

                        case BasisTags.AvatarGenericMessage_Recipients_NoPayload:
                            BasisNetworkGenericMessages.HandleServerAvatarDataMessage_Recipients_NoPayload(reader);
                            break;

                        case BasisTags.SceneGenericMessage_Recipients_NoPayload:
                            BasisNetworkGenericMessages.HandleServerSceneDataMessage_Recipients_NoPayload(reader);
                            break;


                        case BasisTags.OwnershipResponse:
                            BasisNetworkGenericMessages.HandleOwnershipResponse(reader);
                            break;

                        case BasisTags.OwnershipTransfer:
                            BasisNetworkGenericMessages.HandleOwnershipTransfer(reader);
                            break;

                        default:
                            Debug.Log("Unknown message tag: " + message.Tag);
                            break;
                    }
                }
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