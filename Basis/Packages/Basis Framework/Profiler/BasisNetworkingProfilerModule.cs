using Unity.Profiling;
using UnityEngine;

namespace Basis.Scripts.Profiler
{
    public class BasisNetworkProfiler : MonoBehaviour
    {
        public static readonly ProfilerCategory Category = ProfilerCategory.Network;
        // Message-specific profiling counters
        public static readonly ProfilerCounter<int> AudioSegmentDataMessageCounter = new ProfilerCounter<int>(Category, AudioSegmentDataMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> AuthenticationMessageCounter = new ProfilerCounter<int>(Category, AuthenticationMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> AvatarDataMessageCounter = new ProfilerCounter<int>(Category, AvatarDataMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> CreateAllRemoteMessageCounter = new ProfilerCounter<int>(Category, CreateAllRemoteMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> CreateSingleRemoteMessageCounter = new ProfilerCounter<int>(Category, CreateSingleRemoteMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> LocalAvatarSyncMessageCounter = new ProfilerCounter<int>(Category, LocalAvatarSyncMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> OwnershipTransferMessageCounter = new ProfilerCounter<int>(Category, OwnershipTransferMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> RequestOwnershipTransferMessageCounter = new ProfilerCounter<int>(Category, RequestOwnershipTransferMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> PlayerIdMessageCounter = new ProfilerCounter<int>(Category, PlayerIdMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> PlayerMetaDataMessageCounter = new ProfilerCounter<int>(Category, PlayerMetaDataMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> ReadyMessageCounter = new ProfilerCounter<int>(Category, ReadyMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> SceneDataMessageCounter = new ProfilerCounter<int>(Category, SceneDataMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> ServerAudioSegmentMessageCounter = new ProfilerCounter<int>(Category, ServerAudioSegmentMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> ServerAvatarChangeMessageCounter = new ProfilerCounter<int>(Category, ServerAvatarChangeMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> ServerSideSyncPlayerMessageCounter = new ProfilerCounter<int>(Category, ServerSideSyncPlayerMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> AudioRecipientsMessageCounter = new ProfilerCounter<int>(Category, AudioRecipientsMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> AvatarChangeMessageCounter = new ProfilerCounter<int>(Category, AvatarChangeMessageText, ProfilerMarkerDataUnit.Bytes);
        public static readonly ProfilerCounter<int> ServerAvatarDataMessageCounter = new ProfilerCounter<int>(Category, ServerAvatarDataMessageText, ProfilerMarkerDataUnit.Bytes);

        public const string AudioSegmentDataMessageText = "Audio Segment Data Message";
        public const string AuthenticationMessageText = "Authentication Message";
        public const string AvatarDataMessageText = "Avatar Data Message";
        public const string CreateAllRemoteMessageText = "Create All Remote Message";
        public const string CreateSingleRemoteMessageText = "Create Single Remote Message";
        public const string LocalAvatarSyncMessageText = "Local Avatar Sync Message";
        public const string OwnershipTransferMessageText = "Ownership Transfer Message";
        public const string RequestOwnershipTransferMessageText = "Request Ownership Transfer Message";
        public const string PlayerIdMessageText = "Player ID Message";
        public const string PlayerMetaDataMessageText = "Player Metadata Message";
        public const string ReadyMessageText = "Ready Message";
        public const string SceneDataMessageText = "Scene Data Message";
        public const string ServerAudioSegmentMessageText = "Server Audio Segment Message";
        public const string ServerAvatarChangeMessageText = "Server Avatar Change Message";
        public const string ServerSideSyncPlayerMessageText = "Server Side Sync Player Message";
        public const string AudioRecipientsMessageText = "Audio Recipients Message";
        public const string AvatarChangeMessageText = "Avatar Change Message";
        public const string ServerAvatarDataMessageText = "Server Avatar Data Message";
        private void Update()
        {

            // Sample message-specific counters
            AudioSegmentDataMessageCounter.Sample(0);
            AuthenticationMessageCounter.Sample(0);
            AvatarDataMessageCounter.Sample(0);
            CreateAllRemoteMessageCounter.Sample(0);
            CreateSingleRemoteMessageCounter.Sample(0);
            LocalAvatarSyncMessageCounter.Sample(0);
            OwnershipTransferMessageCounter.Sample(0);
            PlayerIdMessageCounter.Sample(0);
            PlayerMetaDataMessageCounter.Sample(0);
            ReadyMessageCounter.Sample(0);
            SceneDataMessageCounter.Sample(0);


            ServerAudioSegmentMessageCounter.Sample(0);
            ServerAvatarChangeMessageCounter.Sample(0);
            ServerSideSyncPlayerMessageCounter.Sample(0);
            ServerAvatarDataMessageCounter.Sample(0);
        }
    }
}
