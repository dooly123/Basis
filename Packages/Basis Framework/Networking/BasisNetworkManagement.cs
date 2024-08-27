using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Player;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Plugins.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
//using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking
{
    public class BasisNetworkManagement : MonoBehaviour
    {
        public BasisLowLevelClient Client;
        public bool HasAuthenticated = false;
        public static BasisNetworkManagement Instance;
        public PlayerIdMessage PlayerID = new PlayerIdMessage();
        public ReadyMessage readyMessage = new ReadyMessage();
        public Dictionary<ushort, BasisNetworkedPlayer> Players = new Dictionary<ushort, BasisNetworkedPlayer>();
        public bool ForceConnect = false;
        public string Ip = "170.64.184.249";
        public ushort Port = 4296;
        public static string LocalHost = "localhost";
        public bool TryToReconnectAutomatically = true;
        public bool HasInitalizedClient = false;
        public static Action<BasisNetworkedPlayer, BasisLocalPlayer> OnLocalPlayerJoined;
        public static Action<BasisNetworkedPlayer, BasisRemotePlayer> OnRemotePlayerJoined;
        public static Action OnExists;
        public void OnEnable()
        {
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
            if (Client == null)
            {
                Client = GetComponent<BasisLowLevelClient>();
            }
            this.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            OnExists?.Invoke();
            if (ForceConnect)
            {
                Connect(Port, Ip);
            }
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
            if (IpString.ToLower() == LocalHost)
            {
                string[] IpStrings = ResolveLocahost(IpString);
                IpString = IpStrings[0];
            }
            IPAddress Ip = IPAddress.Parse(IpString);
            if (HasAuthenticated == false)
            {
                HasAuthenticated = true;
                Client.MessageReceived += MessageReceived;
            }
            Client.ConnectInBackground(Ip, Port, Callback);
        }
        public string[] ResolveLocahost(string localhost)
        {
            string[] addresses = ResolveLocalhostToIP(localhost);
            if (addresses != null)
            {
            }
            else
            {
                Debug.LogError("Failed to resolve localhost to IP address.");
            }
            return addresses;
        }
        string[] ResolveLocalhostToIP(string hostname)
        {
            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(hostname);
                if (ips != null && ips.Length > 0)
                {
                    string[] addresses = new string[ips.Length];
                    for (int Index = 0; Index < ips.Length; Index++)
                    {
                        addresses[Index] = ips[Index].ToString();
                    }
                    return addresses;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to resolve localhost to IP address: " + ex.Message);
            }
            return null;
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
        private async void MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case BasisTags.AuthSuccess:
                            BasisNetworkedPlayer player = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(new InstantiationParameters(transform.position, transform.rotation, transform));
                            ushort playerID = Client.ID;
                            BasisLocalPlayer BasisLocalPlayer = BasisLocalPlayer.Instance;
                            player.ReInitialize(BasisLocalPlayer.Instance, playerID);
                            if (Players.TryAdd(playerID, player))
                            {

                                Debug.Log("added local Player " + Client.ID);
                            }
                            else
                            {
                                Debug.LogError("Cant add " + playerID);
                            }
                            using (DarkRiftWriter writer = DarkRiftWriter.Create())
                            {
                                BasisNetworkAvatarCompressor.CompressIntoSendBase(player.NetworkSend, BasisLocalPlayer.Avatar.Animator);
                                readyMessage.localAvatarSyncMessage = player.NetworkSend.LASM;
                                readyMessage.clientAvatarChangeMessage = new ClientAvatarChangeMessage
                                {
                                    avatarID = BasisLocalPlayer.AvatarUrl
                                };
                                readyMessage.playerMetaDataMessage = new PlayerMetaDataMessage
                                {
                                    playerUUID = BasisLocalPlayer.UUID,
                                    playerDisplayName = BasisLocalPlayer.DisplayName
                                };
                                writer.Write(readyMessage);
                                Message ReadyMessage = Message.Create(BasisTags.ReadyStateTag, writer);
                                Client.SendMessage(ReadyMessage, BasisNetworking.EventsChannel, DeliveryMethod.ReliableOrdered);
                                OnLocalPlayerJoined?.Invoke(player, BasisLocalPlayer);
                            }
                            break;
                        case BasisTags.AvatarMuscleUpdateTag:
                            HandleAvatarUpdate(reader);
                            break;
                        case BasisTags.CreateRemotePlayerTag:
                            await HandleCreateRemotePlayer(reader);
                            break;
                        case BasisTags.CreateRemotePlayersTag:
                            await HandleCreateAllRemoteClients(reader);
                            break;
                        case BasisTags.AudioSegmentTag:
                            HandleAudioUpdate(reader);
                            break;
                        case BasisTags.DisconnectTag:
                            HandleDisconnection(reader);
                            break;
                        case BasisTags.AvatarChangeMessage:
                            HandleAvatarChangeMessage(reader);
                            break;
                        default:
                            Debug.Log("Unknown message at " + message.Tag);
                            break;
                    }
                }
            }
        }
        private void HandleAvatarChangeMessage(DarkRiftReader reader)
        {
            reader.Read(out ServerAvatarChangeMessage ServerAvatarChangeMessage);
            ushort PlayerID = ServerAvatarChangeMessage.uShortPlayerId.playerID;
            if (Players.TryGetValue(PlayerID, out BasisNetworkedPlayer Player))
            {
                BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)Player.NetworkSend;
                networkReceiver.ReceiveAvatarChangeRequest(ServerAvatarChangeMessage);
            }
            else
            {
                Debug.Log("Missing Player For Message " + ServerAvatarChangeMessage.uShortPlayerId.playerID);
            }
        }
        private void HandleDisconnection(DarkRiftReader reader)
        {
            if (Players.TryGetValue(reader.ReadUInt16(), out BasisNetworkedPlayer player))
            {
                GameObject.Destroy(player.gameObject);
            }
        }
        private void HandleAvatarUpdate(DarkRiftReader reader)
        {
            reader.Read(out ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage);
            if (Players.TryGetValue(ServerSideSyncPlayerMessage.playerIdMessage.playerID, out BasisNetworkedPlayer player))
            {
                BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)player.NetworkSend;
                networkReceiver.ReceiveNetworkAvatarData(ServerSideSyncPlayerMessage);
            }
            else
            {
                Debug.Log("Missing Player For Message " + ServerSideSyncPlayerMessage.playerIdMessage.playerID);
            }
        }
        private void HandleAudioUpdate(DarkRiftReader reader)
        {
            reader.Read(out AudioSegmentMessage AudioUpdate);
            if (Players.TryGetValue(AudioUpdate.playerIdMessage.playerID, out BasisNetworkedPlayer player))
            {
                BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)player.NetworkSend;
                if (AudioUpdate.wasSilentData)
                {
                    networkReceiver.ReceiveSilentNetworkAudio(AudioUpdate.silentData);
                }
                else
                {
                    networkReceiver.ReceiveNetworkAudio(AudioUpdate);
                }
            }
            else
            {
                Debug.Log("Missing Player For Message " + AudioUpdate.playerIdMessage.playerID);
            }
        }
        private async Task HandleCreateRemotePlayer(DarkRiftReader reader)
        {
            reader.Read(out ServerReadyMessage SRM);
            await CreateRemotePlayer(SRM);
        }
        private async Task HandleCreateAllRemoteClients(DarkRiftReader reader)
        {
            reader.Read(out CreateAllRemoteMessage allRemote);
            int RemoteLength = allRemote.serverSidePlayer.Length;
            for (int PlayerIndex = 0; PlayerIndex < RemoteLength; PlayerIndex++)
            {
                await CreateRemotePlayer(allRemote.serverSidePlayer[PlayerIndex]);
            }
        }
        public async Task CreateRemotePlayer(ServerReadyMessage ServerReadyMessage)
        {
            InstantiationParameters instantiationParameters = new InstantiationParameters(Vector3.zero, Quaternion.identity, transform);
            string avatarID = ServerReadyMessage.LocalReadyMessage.clientAvatarChangeMessage.avatarID;
            if (string.IsNullOrEmpty(avatarID))
            {
                Debug.Log("bad! empty avatar for " + ServerReadyMessage.playerIdMessage.playerID);
            }
            else
            {
                //   Debug.Log("requesting Avatar load " + avatarID);
            }
            BasisRemotePlayer remote = await BasisPlayerFactory.CreateRemotePlayer(instantiationParameters, avatarID, ServerReadyMessage.LocalReadyMessage.playerMetaDataMessage);
            BasisNetworkedPlayer networkedPlayer = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(instantiationParameters);
            networkedPlayer.ReInitialize(remote, ServerReadyMessage.playerIdMessage.playerID, ServerReadyMessage.LocalReadyMessage.localAvatarSyncMessage);
            if (Players.TryAdd(ServerReadyMessage.playerIdMessage.playerID, networkedPlayer))
            {
                Debug.Log("added Player " + ServerReadyMessage.playerIdMessage.playerID);
            }
            else
            {
                Debug.LogError("Cant add " + ServerReadyMessage.playerIdMessage.playerID);
            }
            BasisNetworkAvatarDecompressor.DeCompress(networkedPlayer.NetworkSend, ServerReadyMessage.LocalReadyMessage.localAvatarSyncMessage);
            OnRemotePlayerJoined?.Invoke(networkedPlayer, remote);
        }

        public NetworkServerConnection Server;
        /*
        [MenuItem("Host/Host")]
        public static void Host()
        {
            ServerSpawnData ServerSpawnData = new ServerSpawnData();
            ServerSpawnData.Data = new ServerSpawnData.DataSettings();
            ServerSpawnData.Data.Directory = "Data/";

            ServerSpawnData.Metrics = new ServerSpawnData.MetricsSettings();
            ServerSpawnData.Metrics.EnablePerMessageMetrics = false;
            //  ServerSpawnData.Metrics.MetricsWriter = new ServerSpawnData.MetricsSettings.MetricsWriterSettings();
            //  ServerSpawnData.Metrics.MetricsWriter.

            ServerSpawnData.Listeners = new ServerSpawnData.ListenersSettings();

            ServerSpawnData.Cache = new ServerSpawnData.CacheSettings();

            ServerSpawnData.EventsFromDispatcher = true;

            ServerSpawnData.Server = new ServerSpawnData.ServerSettings();

            DarkRiftServer server = new DarkRiftServer(ServerSpawnData);

            server.StartServer();
            while (!server.Disposed)
            {
                server.DispatcherWaitHandle.WaitOne();
                server.ExecuteDispatcherTasks();
            }
        }
        */
    }
}