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
using System.Net;
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

        public bool ForceConnect = false;
        public bool TryToReconnectAutomatically = true;
        public bool HasInitalizedClient = false;
        /// <summary>
        /// this occurs after the localplayer has been approved by the network and setup
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisLocalPlayer> OnLocalPlayerJoined;
        /// <summary>
        /// this occurs after a remote user has been authenticated and joined & spawned
        /// </summary>
        public static Action<BasisNetworkedPlayer, BasisRemotePlayer> OnRemotePlayerJoined;
        public static Action OnEnableInstanceCreate;
        public static BasisNetworkManagement Instance;
        public void OnEnable()
        {
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
            if(BasisScene.Instance != null)
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
                            await BasisNetworkCreateRemote.HandleCreateRemotePlayer(reader, this.transform);
                            break;
                        case BasisTags.CreateRemotePlayersTag:
                            await BasisNetworkCreateRemote.HandleCreateAllRemoteClients(reader, this.transform);
                            break;
                        case BasisTags.SceneGenericMessage:
                            BasisNetworkGenericMessages.HandleServerSceneDataMessage(reader);
                            break;
                        case BasisTags.AvatarGenericMessage:
                            BasisNetworkGenericMessages.HandleServerAvatarDataMessage(reader);
                            break;
                        default:
                            Debug.Log("Unknown message at " + message.Tag);
                            break;
                    }
                }
            }
        }
    }
}