/// <summary>
/// Network Tags assigned sequentially from 0 upwards.
/// </summary>
public static class BasisNetworkTag
{
    // Core Player Management
    public const byte CreateRemotePlayer = 0;
    public const byte CreateRemotePlayers = 1;
    // Avatar Communication
    public const byte AvatarChangeMessage = 2;
    // Ownership Management
    public const byte OwnershipResponse = 3;
    public const byte OwnershipTransfer = 4;
    // Audio Communication
    public const byte AudioRecipients = 5;
    // player disconnect
    public const byte Disconnection = 6;
}