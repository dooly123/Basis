namespace Basis.Network.Core
{
    public static class BasisNetworkCommons
    {
        public static int NetworkIntervalPoll = 10;
        /// <summary>
        /// channel zero is only used for unreliable methods
        /// we fall it through to stop bugs
        /// </summary>
        public const byte FallChannel = 0;
        /// <summary>
        /// this is normally avatar movement only can be used once!
        /// </summary>
        public const byte MovementChannel = 1;
        /// <summary>
        /// this is what people use voice data only can be used once!
        /// </summary>
        public const byte VoiceChannel = 2;
        /// <summary>
        /// this is what peopl usee send data on the scene network
        /// </summary>
        public const byte SceneChannel = 3;
        /// <summary>
        /// this is what people use to send data on there avatar
        /// </summary>
        public const byte AvatarChannel = 4;

        public const byte CreateRemotePlayer = 5;
        public const byte CreateRemotePlayers = 6;
        public const byte AvatarChangeMessage = 7;
        public const byte OwnershipResponse = 8;
        public const byte OwnershipTransfer = 9;
        public const byte AudioRecipients = 10;
        public const byte Disconnection = 11;
    }
}