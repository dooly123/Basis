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
    // Generic Messaging
    public const byte SceneGenericMessage = 3;
    public const byte AvatarGenericMessage = 4;
    // No Recipients, No Payload Variants
    public const byte AvatarGenericMessage_NoRecipients_NoPayload = 5;
    public const byte AvatarGenericMessage_NoRecipients = 6;
    public const byte SceneGenericMessage_NoRecipients_NoPayload = 7;
    public const byte SceneGenericMessage_NoRecipients = 8;
    // Ownership Management
    public const byte OwnershipResponse = 9;
    public const byte OwnershipTransfer = 10;
    // Recipients, No Payload Variants
    public const byte AvatarGenericMessage_Recipients_NoPayload = 11;
    public const byte SceneGenericMessage_Recipients_NoPayload = 12;
    // Audio Communication
    public const byte AudioRecipients = 13;
    // player disconnect
    public const byte Disconnection = 14;
}