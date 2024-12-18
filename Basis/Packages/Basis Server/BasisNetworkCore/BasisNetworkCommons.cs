namespace Basis.Network.Core
{
    public static class BasisNetworkCommons
    {
        public static int NetworkIntervalPoll = 10;
        //we should never use this for basis, leave it for internal
        public const byte DefaultChannel = 0;
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
        /// <summary>
        /// this is for anything basis related
        /// </summary>
        public const byte BasisChannel = 5;
    }
}