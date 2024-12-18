namespace Basis.Network.Core
{
    public static class BasisNetworkCommons
    {
        public static int NetworkIntervalPoll = 10;
        //we should never use this for basis, leave it for internal
        public const byte DefaultChannel = 1;
        /// <summary>
        /// this is normally avatar movement only can be used once!
        /// </summary>
        public const byte MovementChannel = 2;
        /// <summary>
        /// this is voice data only can be used once!
        /// </summary>
        public const byte VoiceChannel = 3;
        /// <summary>
        /// this is what people to send data on the scene network
        /// </summary>
        public const byte SceneChannel = 14;
        /// <summary>
        /// this is what people use to send data on there avatar
        /// </summary>
        public const byte AvatarChannel = 15;

        public const byte CreateRemotePlayer = 6;
        public const byte CreateRemotePlayers = 7;
        public const byte AvatarChangeMessage = 8;
        public const byte OwnershipResponse = 9;
        public const byte OwnershipTransfer = 10;
        public const byte AudioRecipients = 11;
        public const byte Disconnection = 12;
    }
}