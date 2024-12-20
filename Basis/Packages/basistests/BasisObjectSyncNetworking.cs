using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using BasisSerializer.OdinSerializer;
using LiteNetLib;
using UnityEngine;
public class BasisObjectSyncNetworking : MonoBehaviour
{
    public ushort MessageIndex = 3321;
    public byte[] byteArray = new byte[28];
    public bool IsOwner = false;
    public float LerpMultiplier = 3;
    private float sendInterval = 0.2f; // Send data every 0.2 seconds
    private float timeSinceLastSend = 0.0f;
    public BasisNetworkedPlayer LocalNetworkPlayer;
    public DeliveryMethod DeliveryMethod = DeliveryMethod.Unreliable;
    public Vector3 previousPosition;
    public Quaternion previousRotation;
    public Vector3 previousScale;
    public bool NeedsAUpdate = false;
    public Rigidbody Rigidbody;
    public BasisPositionRotationScale Storeddata = new BasisPositionRotationScale();
    public void Awake()
    {
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft += OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft += OnRemotePlayerLeft;
    }
    public void OnDestroy()
    {
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisNetworkManagement.OnLocalPlayerJoined -= OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined -= OnRemotePlayerJoined;
        BasisNetworkManagement.OnLocalPlayerLeft -= OnLocalPlayerLeft;
        BasisNetworkManagement.OnRemotePlayerLeft -= OnRemotePlayerLeft;
    }
    public void OnRemotePlayerLeft(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
        // Handle the remote player leaving
        ComputeCurrentOwner();
    }

    public void OnLocalPlayerLeft(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        // Handle the local player leaving
        ComputeCurrentOwner();
    }

    public void OnRemotePlayerJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {
        // Handle a remote player joining
        ComputeCurrentOwner();
        NeedsAUpdate = true;
    }

    public void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        LocalNetworkPlayer = player1;
        // Handle the local player joining
        ComputeCurrentOwner();
    }

    public void ComputeCurrentOwner()
    {
        ushort OldestPlayerInInstance = BasisNetworkManagement.Instance.GetOldestAvailablePlayerUshort();
        IsOwner = OldestPlayerInInstance == LocalNetworkPlayer.NetId;
        Rigidbody.isKinematic = !IsOwner;
    }

    public void Update()
    {
        if (IsOwner)
        {
            // Accumulate time since last send
            timeSinceLastSend += Time.deltaTime;

            // Only send data if the time since last send exceeds the interval
            if (timeSinceLastSend >= sendInterval)
            {
                // Get current position, rotation, and scale
                transform.GetPositionAndRotation(out Storeddata.Position, out Storeddata.Rotation);
                Storeddata.Scale = transform.localScale;

                // Check if position, rotation, or scale has changed since last sync
                if (NeedsAUpdate || HasPositionRotationOrScaleChanged())
                {
                    // Reset the timer
                    timeSinceLastSend = 0.0f;

                    // Convert to byte array
                    byte[] byteArray = SerializationUtility.SerializeValue(Storeddata, DataFormat.Binary);
                    // Send network message with position, rotation, and scale data
                    BasisScene.NetworkMessageSend(MessageIndex, byteArray, DeliveryMethod);

                    // Reset previous values to the current ones
                    previousPosition = Storeddata.Position;
                    previousRotation = Storeddata.Rotation;
                    previousScale = Storeddata.Scale;
                    NeedsAUpdate = false;
                }

                // Check if the object is below the respawn height and reset position if necessary
                if (Storeddata.Position.y < BasisSceneFactory.Instance.BasisScene.RespawnHeight)
                {
                    this.transform.position = BasisSceneFactory.Instance.BasisScene.SpawnPoint.position;
                }
            }
        }
        else
        {
            // Get the current position, rotation, and scale
            transform.GetPositionAndRotation(out Vector3 CurrentPosition, out Quaternion CurrentRotation);
            Vector3 CurrentScale = transform.localScale;
            float DeltaTime = Time.deltaTime;
            float CurrentLerp = LerpMultiplier * DeltaTime;

            // Interpolate position, rotation, and scale for smooth transitions
            Vector3 DesiredCurrentPosition = Vector3.Lerp(CurrentPosition, Storeddata.Position, CurrentLerp);
            Quaternion DesiredCurrentRotation = Quaternion.Slerp(CurrentRotation, Storeddata.Rotation, CurrentLerp);
            Vector3 DesiredCurrentScale = Vector3.Lerp(CurrentScale, Storeddata.Scale, CurrentLerp);

            transform.SetPositionAndRotation(DesiredCurrentPosition, DesiredCurrentRotation);
            transform.localScale = DesiredCurrentScale;
        }
    }

    private bool HasPositionRotationOrScaleChanged()
    {
        // Check if the position, rotation, or scale has changed
        bool positionChanged = previousPosition != Storeddata.Position;
        bool rotationChanged = previousRotation != Storeddata.Rotation;
        bool scaleChanged = previousScale != Storeddata.Scale;

        // Return true if any have changed
        return positionChanged || rotationChanged || scaleChanged;
    }

    public void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, DeliveryMethod DeliveryMethod = DeliveryMethod.ReliableSequenced)
    {
        // Check if this is the correct message
        if (MessageIndex == this.MessageIndex)
        {
            byteArray = buffer;
            Storeddata = SerializationUtility.DeserializeValue<BasisPositionRotationScale>(byteArray, DataFormat.Binary);
        }
    }
}
