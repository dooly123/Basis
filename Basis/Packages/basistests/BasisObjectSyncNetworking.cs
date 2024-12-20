using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using BasisSerializer.OdinSerializer;
using System;
using UnityEngine;

public class BasisObjectSyncNetworking : MonoBehaviour
{
    public ushort MessageIndex = 3321;
    public byte[] byteArray = new byte[28];
    public bool IsOwner = false;  // Ownership flag
    public float LerpMultiplier = 3f;
    private float sendInterval = 0.2f; // Send data every 0.2 seconds
    private float timeSinceLastSend = 0.0f;
    public BasisNetworkedPlayer LocalNetworkPlayer;  // Local player reference
    public Rigidbody Rigidbody;
    public BasisPositionRotationScale Storeddata = new BasisPositionRotationScale();
    public InteractableObject iObject;

    public string uniqueId = "1";
    private Transform currentOwner;

    // Subscribe to necessary network events
    public void Awake()
    {
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft += OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft += OnRemotePlayerLeft;
        BasisNetworkManagement.OnOwnershipTransfer += OnOwnershipTransfer;
    }

    // Unsubscribe when destroyed
    public void OnDestroy()
    {
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined -= OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined -= OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft -= OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft -= OnRemotePlayerLeft;
        BasisNetworkManagement.OnOwnershipTransfer -= OnOwnershipTransfer;
    }

    // Local player joins
    public void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        LocalNetworkPlayer = player1;  // Assign the local player
        RequestCurrentOwnership();  // Get the current owner of the object
    }

    // Remote player joins
    public void OnRemotePlayerJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
        RequestCurrentOwnership();  // Recheck ownership logic when remote player joins
    }

    // Player leaves the game
    public void OnLocalPlayerLeft(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        RequestCurrentOwnership();  // Recheck ownership if the local player leaves
    }

    public void OnRemotePlayerLeft(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
        RequestCurrentOwnership();  // Recheck ownership if remote player leaves
    }

    public void RequestOwnership()
    {
        BasisNetworkManagement.RequestOwnership(uniqueId, LocalNetworkPlayer.NetId);
    }
    private void RequestCurrentOwnership()
    {
        BasisNetworkManagement.RequestCurrentOwnership(uniqueId);
    }
    private void OnOwnershipTransfer(string UniqueEntityID, ushort NetIdNewOwner, bool IsOwner)
    {
        if (UniqueEntityID == uniqueId)
        {
            if (IsOwner)
            {
                currentOwner = LocalNetworkPlayer.transform;
                Debug.Log("Current Owner Assigned: " + currentOwner.name);
                if (!iObject.isHeld)
                {
                    iObject.PickUp(currentOwner);
                }
            }
            else
            {
                if (iObject.isHeld)
                {
                    iObject.Drop();
                }
            }
        }
    }

    public void Update()
    {
        // Exit if LocalNetworkPlayer is not assigned
        if (LocalNetworkPlayer == null)
        {
            Debug.LogWarning("LocalNetworkPlayer is not assigned!");
            return;
        }

        if (Rigidbody != null)
        {
            // Sync position, rotation, and scale to networked object
            timeSinceLastSend += Time.deltaTime;

            if (timeSinceLastSend >= sendInterval)
            {
                transform.GetPositionAndRotation(out Storeddata.Position, out Storeddata.Rotation);
                Storeddata.Scale = transform.localScale;

                // Send update to network if necessary
                byte[] byteArray = SerializationUtility.SerializeValue(Storeddata, DataFormat.Binary);

                // Reset the timer
                timeSinceLastSend = 0.0f;
            }
        }
    }

    // Handle network message reception
    public void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        if (MessageIndex == this.MessageIndex)
        {
            byteArray = buffer;
            Storeddata = SerializationUtility.DeserializeValue<BasisPositionRotationScale>(byteArray, DataFormat.Binary);
        }
    }
}