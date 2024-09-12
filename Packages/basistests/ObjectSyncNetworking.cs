using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using System;
using UnityEngine;
public class ObjectSyncNetworking : MonoBehaviour
{
    public ushort MessageIndex = 3321;
    public byte[] byteArray = new byte[28];
    public Vector3 OutputPosition;
    public Quaternion OutputRotation;
    public bool IsOwner = false;
    public float LerpMultiplier = 3;
    private float sendInterval = 0.2f; // Send data every 0.2 seconds
    private float timeSinceLastSend = 0.0f;
    public BasisNetworkedPlayer LocalNetworkPlayer;
    public DeliveryMethod DeliveryMethod = DarkRift.DeliveryMethod.Unreliable;
    public Vector3 previousPosition;
    public Quaternion previousRotation;
    public bool NeedsAUpdate = false;
    public Rigidbody Rigidbody;
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
        if (IsOwner)
        {
            Rigidbody.isKinematic = false;
        }
        else
        {
            Rigidbody.isKinematic = true;
        }
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
                // Get current position and rotation
                transform.GetPositionAndRotation(out OutputPosition, out OutputRotation);

                // Check if position or rotation has changed since last sync
                if (NeedsAUpdate || HasPositionOrRotationChanged())
                {
                    // Reset the timer
                    timeSinceLastSend = 0.0f;

                    // Convert to byte array (assuming this is a method you have defined)
                    byte[] byteArray = ConvertToByteArray(OutputRotation, OutputPosition);

                    // Send network message with position and rotation data
                    BasisScene.NetworkMessageSend(MessageIndex, byteArray, DarkRift.DeliveryMethod.Unreliable);

                    // Reset previous values to the current ones
                    previousPosition = OutputPosition;
                    previousRotation = OutputRotation;
                    NeedsAUpdate = false;
                }

                // Check if the object is below the respawn height and reset position if necessary
                if (OutputPosition.y < BasisSceneFactory.Instance.BasisScene.RespawnHeight)
                {
                    this.transform.position = BasisSceneFactory.Instance.BasisScene.SpawnPoint.position;
                }
            }
        }
        else
        {
            // Get the current position and rotation
            transform.GetPositionAndRotation(out Vector3 CurrentPosition, out Quaternion CurrentRotation);
            float DeltaTime = Time.deltaTime;
            float CurrentLerp = LerpMultiplier * DeltaTime;

            // Interpolate position and rotation for smooth transitions
            Vector3 DesiredCurrentPosition = Vector3.Lerp(CurrentPosition, OutputPosition, CurrentLerp);
            Quaternion DesiredCurrentRotation = Quaternion.Slerp(CurrentRotation, OutputRotation, CurrentLerp);

            transform.SetPositionAndRotation(DesiredCurrentPosition, DesiredCurrentRotation);
        }
    }
    private bool HasPositionOrRotationChanged()
    {
        // Check if the position or rotation has changed
        bool positionChanged = previousPosition != OutputPosition;
        bool rotationChanged = previousRotation != OutputRotation;

        // Return true if either has changed
        return positionChanged || rotationChanged;
    }

    public void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        // Check if this is the correct message
        if (MessageIndex == this.MessageIndex)
        {
            // Convert received byte array back to Quaternion and Vector3
            ConvertFromByteArray(buffer, out OutputRotation, out OutputPosition);
        }
    }
    // Converts Quaternion and Vector3 to a byte array
    public byte[] ConvertToByteArray(Quaternion quaternion, Vector3 vector)
    {
        // Convert Quaternion to bytes
        Buffer.BlockCopy(BitConverter.GetBytes(quaternion.x), 0, byteArray, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(quaternion.y), 0, byteArray, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(quaternion.z), 0, byteArray, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(quaternion.w), 0, byteArray, 12, 4);

        // Convert Vector3 to bytes
        Buffer.BlockCopy(BitConverter.GetBytes(vector.x), 0, byteArray, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(vector.y), 0, byteArray, 20, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(vector.z), 0, byteArray, 24, 4);

        return byteArray;
    }
    // Converts a byte array back to Quaternion and Vector3
    public void ConvertFromByteArray(byte[] byteArray, out Quaternion quaternion, out Vector3 vector)
    {
        // Extract Quaternion from bytes
        float qx = BitConverter.ToSingle(byteArray, 0);
        float qy = BitConverter.ToSingle(byteArray, 4);
        float qz = BitConverter.ToSingle(byteArray, 8);
        float qw = BitConverter.ToSingle(byteArray, 12);
        quaternion = new Quaternion(qx, qy, qz, qw);

        // Extract Vector3 from bytes
        float vx = BitConverter.ToSingle(byteArray, 16);
        float vy = BitConverter.ToSingle(byteArray, 20);
        float vz = BitConverter.ToSingle(byteArray, 24);
        vector = new Vector3(vx, vy, vz);
    }
}