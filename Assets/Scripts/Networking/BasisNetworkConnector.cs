using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server.Plugins.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static SerializableDarkRift;
public class BasisNetworkConnector : MonoBehaviour
{
    public BasisLowLevelClient Client;
    public bool HasUnityClient = false;
    public static BasisNetworkConnector Instance;
    public string authenticationCode = "Default";
    public PlayerIdMessage PlayerID = new PlayerIdMessage();
    public ReadyMessage readyMessage = new ReadyMessage();
    public Dictionary<ushort, BasisNetworkedPlayer> Players = new Dictionary<ushort, BasisNetworkedPlayer>();
    public bool ForceConnect = false;
    public string Ip = "170.64.184.249";
    public ushort Port = 4296;
    public static string LocalHost = "localhost";
    public void OnEnable()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        this.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        if (ForceConnect)
        {
            Connect(Port, Ip);
        }
    }
    public void Connect(ushort Port, string IpString)
    {
        HasUnityClient = false;
        Client = BasisHelpers.GetOrAddComponent<BasisLowLevelClient>(this.gameObject);
        Client.Initialize();
        if (IpString.ToLower() == LocalHost)
        {
            string[] IpStrings = ResolveLocahost(IpString);
            IpString = IpStrings[0];
        }
        IPAddress Ip = IPAddress.Parse(IpString);
        Client.ConnectInBackground(Ip, Port, Callback);
        Client.Disconnected += Disconnected;
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
    }
    public void Disconnect()
    {
        if (Client != null)
        {
            Client.Close();
            Destroy(Client);
        }
    }
    public void Callback([CanBeNull] Exception e)
    {
        if (e == null)
        {
            Authentication();
        }
        else
        {
            Debug.LogError("Failed to connect: " + e.Message);
        }
    }
    public void Authentication()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            AuthenticationToServerMessage Auth = new AuthenticationToServerMessage
            {
                password = authenticationCode
            };
            writer.Write(Auth);
            using (Message msg = Message.Create(BasisTags.AuthTag, writer))
            {
                Client.SendMessage(msg, DeliveryMethod.ReliableOrdered);
                Debug.Log("sent Authentication");
                Client.MessageReceived += OnAuthResponse;
            }
        }
    }
    private async void OnAuthResponse(object sender, MessageReceivedEventArgs e)
    {
        await AsyncOnAuthResponse(sender, e);
    }
    private async Task AsyncOnAuthResponse(object sender, MessageReceivedEventArgs e)
    {
        try
        {
            Message message = e.GetMessage();

            if (message.Tag == BasisTags.AuthSuccess)
            {
                await HandleAuthenticationSuccess();
            }
            else
            {
                if (message.Tag == BasisTags.AuthFailure)
                {
                    Debug.LogError("Failed PassCode Check " + message.Tag);
                }
                else
                {
                    Debug.LogError("Unexpected message tag: " + message.Tag);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in AsyncOnAuthResponse: " + ex);
        }
        finally
        {
            Client.MessageReceived -= OnAuthResponse;
        }
    }
    private async Task HandleAuthenticationSuccess()
    {
        HasUnityClient = true;
        BasisNetworkedPlayer player = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(new InstantiationParameters(transform.position, transform.rotation, transform));
        ushort playerID = Client.ID;
        player.ReInitialize(BasisLocalPlayer.Instance, playerID);
        if (Players.TryAdd(playerID, player))
        {

        }
        else
        {
            Debug.LogError("Cant add " + playerID);
        }
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(readyMessage);
            Message ReadyMessage = Message.Create(BasisTags.ReadyStateTag, writer);
            Client.SendMessage(ReadyMessage, DeliveryMethod.ReliableOrdered);
        }
        Client.MessageReceived += MessageReceived;
    }
    private async void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                switch (message.Tag)
                {
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

                }
            }
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
        reader.Read(out AudioSegment AudioUpdate);
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
        reader.Read(out ServerSideSyncPlayerMessage sspm);
        await CreateRemotePlayer(sspm);
    }

    private async Task HandleCreateAllRemoteClients(DarkRiftReader reader)
    {
        reader.Read(out CreateAllRemoteMessage allRemote);
        int playerCount = (int)allRemote.playerCount;
        for (int PlayerIndex = 0; PlayerIndex < playerCount; PlayerIndex++)
        {
            ServerSideSyncPlayerMessage sspm = allRemote.serverSidePlayer[PlayerIndex];
            await CreateRemotePlayer(sspm);
        }
    }
    private async Task CreateRemotePlayer(ServerSideSyncPlayerMessage sspm)
    {
        InstantiationParameters instantiationParameters = new InstantiationParameters(Vector3.zero, Quaternion.identity, transform);
        BasisRemotePlayer remote = await BasisPlayerFactory.CreateRemotePlayer(instantiationParameters);
        BasisNetworkedPlayer networkedPlayer = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(instantiationParameters);
        networkedPlayer.ReInitialize(remote, sspm.playerIdMessage.playerID);
        if (Players.TryAdd(sspm.playerIdMessage.playerID, networkedPlayer))
        {

        }
        else
        {
            Debug.LogError("Cant add " + sspm.playerIdMessage.playerID);
        }
    }
#if UNITY_EDITOR
    [MenuItem("Basis/Spawn Fake Remote")]
    public static void SpawnRemoteTestClient()
    {
        BasisNetworkConnector NetworkConnector = BasisNetworkConnector.Instance;
        if (NetworkConnector != null)
        {
            ServerSideSyncPlayerMessage serverSideSyncPlayerMessage = new ServerSideSyncPlayerMessage();
            serverSideSyncPlayerMessage.playerIdMessage = new PlayerIdMessage();
            serverSideSyncPlayerMessage.playerIdMessage.playerID = (ushort)(NetworkConnector.Players.Count + 1);
            BasisNetworkTransmitter Transmitter = FindFirstObjectByType<BasisNetworkTransmitter>();
            if (Transmitter != null)
            {
                serverSideSyncPlayerMessage.avatarSerialization = Transmitter.LASM;
            }
            CreateTestRemotePlayer(serverSideSyncPlayerMessage);
        }
    }
    public async static void CreateTestRemotePlayer(ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage)
    {
        BasisNetworkConnector NetworkConnector = BasisNetworkConnector.Instance;
        await NetworkConnector.CreateRemotePlayer(ServerSideSyncPlayerMessage);
    }
#endif
    public void Host(ushort Port)
    {
        //  public NetworkServer Server;
        //   DarkRift2CommsNetwork.DarkRiftClient = Client;
        //    Server = Helpers.GetOrAddComponent<NetworkServer>(this.gameObject);
        //   Server.configuration = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/DarkRift/DarkRift/Plugins/Server/ExampleConfiguration.xml");
        //   Server.Create();
        //  if (Server != null)
        //  {
        //     Server.Close();
        //     Destroy(Server);
        // }
    }
}