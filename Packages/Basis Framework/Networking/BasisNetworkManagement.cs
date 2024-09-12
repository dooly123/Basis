using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server.Plugins.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public Dictionary<ushort, BasisNetworkedPlayer> Players = new Dictionary<ushort, BasisNetworkedPlayer>();
        public static bool AddPlayer(BasisNetworkedPlayer Player)
        {
            if (Instance != null)
            {
                return Instance.Players.TryAdd(Player.NetId, Player);
            }
            return false;
        }
        public static bool RemovePlayer(ushort NetID)
        {
            if (Instance != null)
            {
                return Instance.Players.Remove(NetID);
            }
            return false;
        }
        public static bool RemovePlayer(BasisNetworkedPlayer BasisNetworkedPlayer)
        {
            if (Instance != null)
            {
                return Instance.Players.Remove(BasisNetworkedPlayer.NetId);
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
        public void OnEnable()
        {
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
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
        public void SetupSceneEvents(BasisScene BasisScene)
        {
            BasisScene.OnNetworkMessageSend += BasisNetworkGenericMessages.OnNetworkMessageSend;
        }
        public void Connect()
        {
            Connect(Port, Ip);
        }
        public void Connect(ushort Port, string IpString)
        {
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
        private void Disconnected(object sender, DisconnectedEventArgs e)
        {
            Debug.LogError("Disconnected from Server " + e.Error);
            Disconnect();
            if (TryToReconnectAutomatically)
            {
                Connect(Port, Ip);
            }
            else
            {
                SceneManager.LoadScene(0, LoadSceneMode.Single);//reset
            }
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

                        default:
                            Debug.Log("Unknown message tag: " + message.Tag);
                            break;
                    }
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
            int AvatarInstance = Avatar.GetInstanceID();
            foreach (BasisNetworkedPlayer NPlayer in Instance.Players.Values)
            {
                if (NPlayer == null)
                {
                    continue;
                }
                if (NPlayer.Player == null)
                {
                    continue;
                }
                if (NPlayer.Player.Avatar == null)
                {
                    continue;
                }
                if (NPlayer.Player.Avatar.GetInstanceID() == AvatarInstance)
                {
                    NetworkedPlayer = NPlayer;
                    BasisPlayer = NPlayer.Player;
                    return true;
                }
            }
            NetworkedPlayer = null;
            BasisPlayer = null;
            return false;
        }
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
            foreach (BasisNetworkedPlayer NPlayer in Instance.Players.Values)
            {
                if (NPlayer == null)
                {
                    Debug.LogError("Network Player was null!");
                    continue;
                }
                if (NPlayer.Player == null)
                {
                    Debug.LogError("Player was null!");
                    continue;
                }
                if (NPlayer.Player.Avatar == null)
                {
                    Debug.LogError("Avatar was null!");
                    continue;
                }
                if (Avatar == NPlayer.Player.Avatar)
                {
                    BasisPlayer = NPlayer.Player;
                    return true;
                }
            }
            Debug.LogError("Avatar was not found on any player that is known");
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
            foreach (BasisNetworkedPlayer NPlayer in Instance.Players.Values)
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