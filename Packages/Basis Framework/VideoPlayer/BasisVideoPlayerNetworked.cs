using System.Security.Policy;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using BasisSerializer.OdinSerializer;
using DarkRift;
using UnityEngine;
using UnityEngine.Serialization;

public class BasisVideoPlayerNetworked : MonoBehaviour
{
    public BasisVideoPlayer VideoPlayer;

    public const ushort UrlMessageId = 420;

    public string Url { get => VideoPlayer.Url; set => VideoPlayer.Url = value; }

    public ushort OwnerId { get; private set; } = 0;
    public bool IsOwner { get; private set; }
    public string OwnershipContextId { get; private set; }

    public void Awake()
    {
        if (VideoPlayer == null)
        {
            Error("Missing the video player!");
            this.enabled = false;
            return;
        }

        OwnershipContextId = GetUniqueContextId();
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft += OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft += OnRemotePlayerLeft;
        BasisNetworkManagement.OnOwnershipTransfer += OnOwnershipTransfer;
    }

    public void OnDestroy()
    {
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined -= OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined -= OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft -= OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft -= OnRemotePlayerLeft;
        BasisNetworkManagement.OnOwnershipTransfer -= OnOwnershipTransfer;
    }

    public void Play()
    {
        Log($"Playing from cache: {Url ?? "null"}");
        Play(Url);
    }

    public void Play(string url)
    {
        Log($"Playing: {url}");
        Url = url;
        VideoPlayer.Play(url);
        SendUrl(url);
    }

    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        if (MessageIndex != UrlMessageId) return;
        if (!TryGetUrl(buffer, out string url)) return;
        Log($"Playing from network: {url}");
        Url = url;
        VideoPlayer.Play(url);
    }

    public void OnLocalPlayerJoined(BasisNetworkedPlayer playerNetwork, BasisLocalPlayer playerSystem)
    {
        BasisNetworkManagement.RequestCurrentOwnership(OwnershipContextId);
    }

    public void OnLocalPlayerLeft(BasisNetworkedPlayer playerNetwork, BasisLocalPlayer playerSystem) { }

    public void OnRemotePlayerJoined(BasisNetworkedPlayer playerNetwork, BasisRemotePlayer playerSystem)
    {
        if (IsOwner) SendUrl(Url, playerNetwork.NetId);
    }

    public void OnRemotePlayerLeft(BasisNetworkedPlayer playerNetwork, BasisRemotePlayer playerSystem)
    {
        // if prior owner has left, query for the new current owner data
        if (playerNetwork.NetId == OwnerId) 
            BasisNetworkManagement.RequestCurrentOwnership(OwnershipContextId);
    }

    private void OnOwnershipTransfer(string EntityId, ushort NetworkOwner, bool isOwner)
    {
        if (EntityId == OwnershipContextId)
        {
            OwnerId = NetworkOwner;
            IsOwner = isOwner;
            Log($"Updated Owner to {NetworkOwner} (am I the owner? {isOwner}) for '{EntityId}'");
        }
    }

    private void SendUrl(string url, params ushort[] recipients)
    {
        if (recipients.Length == 0) recipients = null;
        Log($"Sending URL to {(recipients == null ? "all" : recipients)} other clients: {url}");
        BasisScene.NetworkMessageSend(UrlMessageId, SerializeData(url), DeliveryMethod.ReliableSequenced, recipients);
    }

    private bool TryGetUrl(byte[] buffer, out string url)
    {
        return TryDeserializeData(buffer, out url);
    }

    private byte[] SerializeData(string data)
    {
        return SerializationUtility.SerializeValue($"{OwnershipContextId}\0{data}", DataFormat.Binary);
    }

    private bool TryDeserializeData(byte[] buffer, out string data)
    {
        data = null;
        string[] raw = SerializationUtility.DeserializeValue<string>(buffer, DataFormat.Binary).Split('\0', 2);
        if (raw.Length != 2) return false; // invalid format, not from this script
        if (raw[0] != OwnershipContextId) return false; // invalid id, not from this instance
        data = raw[1];
        return true;
    }

    private string GetUniqueContextId()
    {
        int componentIndex = System.Array.IndexOf(GetComponents<Component>(), this);
        return $"{GetHierarchyPath(transform)}#{nameof(BasisVideoPlayerNetworked)}:{componentIndex}";
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = "";
        while (t != null)
        {
            path = $"/{t.name.Replace(" ", "")}:{t.GetSiblingIndex()}";
            t = t.parent;
        }

        return path;
    }


    private static void Log(string message)
    {
        Debug.Log($"[<color=#00ffff>{nameof(BasisVideoPlayerNetworked)}</color>] {message}");
    }

    private static void Error(string message)
    {
        Debug.LogError($"[<color=#00ffff>{nameof(BasisVideoPlayerNetworked)}</color>] {message}");
    }
}