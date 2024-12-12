namespace Basis.Network.Core
{
    public static class BasisNetworkCommons
    {
        public static int NetworkIntervalPoll = 10;
        //channels 0 to 6
        public const byte EventsChannel = 0;
        /// <summary>
        /// this is normally avatar movement only can be used once!
        /// </summary>
        public const byte MovementChannel = 1;
        /// <summary>
        /// this is voice data only can be used once!
        /// </summary>
        public const byte VoiceChannel = 2;
        /// <summary>
        /// this is what people to send data on the scene network
        /// </summary>
        public const byte SceneChannel = 3;
        /// <summary>
        /// this is what people use to send data on there avatar
        /// </summary>
        public const byte AvatarChannel = 4;
    }
}