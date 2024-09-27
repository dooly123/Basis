using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Basis.Scripts.BasisSdk;
using DarkRift;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Avatar Comms")]
    public class HVRAvatarComms : MonoBehaviour
    {
        private const DeliveryMethod NegotiationDelivery = DeliveryMethod.ReliableOrdered;
        
        public const byte OurMessageIndex = 0xC0;
        private const int BytesPerGuid = 16;

        [SerializeField] private BasisAvatar avatar;
        [SerializeField] private FeatureNetworking featureNetworking;
        
        private bool _isWearer;
        private ushort _wearerNetId;
        private Guid[] _negotiatedGuids;
        private Dictionary<int, int> _fromTheirsToOurs;
        private Stopwatch _debugRetrySendNegotiation;
        private bool _alreadyInitialized;

        private void Awake()
        {
            if (avatar == null) avatar = GetComponentInParent<BasisAvatar>(true); // Defensive
            if (featureNetworking == null) featureNetworking = GetComponentInParent<FeatureNetworking>(true); // Defensive
            if (avatar == null || featureNetworking == null)
            {
                throw new InvalidOperationException("Broke assumption: Avatar and/or FeatureNetworking cannot be found.");
            }
            
            avatar.OnAvatarNetworkReady += OnAvatarNetworkReady;
        }

        private void OnDestroy()
        {
            avatar.OnAvatarNetworkReady -= OnAvatarNetworkReady;
        }

        private void OnAvatarNetworkReady()
        {
            if (_alreadyInitialized) return;
            _alreadyInitialized = true;
            
            _isWearer = avatar.IsOwnedLocally;
            _wearerNetId = avatar.LinkedPlayerID;

            featureNetworking.AssignGuids(_isWearer);

            avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
            if (_isWearer)
            {
                _debugRetrySendNegotiation = new Stopwatch();
                _debugRetrySendNegotiation.Restart();
            }
        }

        private void Update()
        {
            if (!_isWearer) return;

            // This is a hack: Send the negotiation packet every so often, because the negotiation packets are currently not going through properly.
            if (_debugRetrySendNegotiation.ElapsedMilliseconds > 5000)
            {
                _debugRetrySendNegotiation.Restart();
                avatar.NetworkMessageSend(OurMessageIndex, featureNetworking.GetNegotiationPacket(), DeliveryMethod.Sequenced); // Use Sequenced, because we know this delivery type works
            }
        }

        private void OnNetworkMessageReceived(ushort playerid, byte messageindex, byte[] unsafeBuffer, ushort[] recipients)
        {
            // Ignore all other messages first
            if (OurMessageIndex != messageindex) return;
            
            // Ignore all net messages as long as this is disabled
            if (!isActiveAndEnabled) return;
            
            // The sender cannot receive
            if (_isWearer) return;

            if (unsafeBuffer.Length == 0) return; // Protocol error: Missing sub-packet identifier
            
            var isSentByWearer = _wearerNetId == playerid;
            
            var theirs = unsafeBuffer[0];
            if (theirs == FeatureNetworking.NegotiationPacket)
            {
                // Only the wearer can send us this message
                if (!isSentByWearer) return;
                
                DecodeNegotiationPacket(SubBuffer(unsafeBuffer));
            }
            else if (theirs == FeatureNetworking.ReservedPacket)
            {
                // Reserved packets are not necessarily sent by the wearer.
                DecodeReservedPacket(SubBuffer(unsafeBuffer));
            }
            else // Transmission packet
            {
                // Only the wearer can send us this message
                if (!isSentByWearer) return;
                
                if (_fromTheirsToOurs == null) return; // Protocol error: No valid Networking packet was previously received
                
                if (_fromTheirsToOurs.TryGetValue(theirs, out var ours))
                {
                    featureNetworking.OnPacketReceived(ours, SubBuffer(unsafeBuffer));
                }
                else
                {
                    // Either:
                    // - Mismatching avatar structures, or
                    // - Protocol error: Remote has sent a GUID index greater or equal to the previously negotiated GUIDs. 
                }
            }
        }

        private static ArraySegment<byte> SubBuffer(byte[] unsafeBuffer)
        {
            return new ArraySegment<byte>(unsafeBuffer, 1, unsafeBuffer.Length - 1);
        }

        private bool DecodeNegotiationPacket(ArraySegment<byte> unsafeGuids)
        {
            if (unsafeGuids.Count % BytesPerGuid != 0) return false;
            
            // Safe after this point
            var safeGuids = unsafeGuids;
            
            var guidCount = safeGuids.Count / BytesPerGuid;
            _negotiatedGuids = new Guid[guidCount];
            _fromTheirsToOurs = new Dictionary<int, int>();
            if (guidCount == 0)
            {
                return true;
            }
            
            for (var guidIndex = 0; guidIndex < guidCount; guidIndex++)
            {
                var guid = new Guid(safeGuids.Slice(guidIndex * BytesPerGuid, BytesPerGuid));
                _negotiatedGuids[guidIndex] = guid;
            }

            var lookup = featureNetworking.GetOrderedGuids().ToList();

            for (var theirIndex = 0; theirIndex < _negotiatedGuids.Length; theirIndex++)
            {
                var theirGuid = _negotiatedGuids[theirIndex];
                var ourIndexOptional = lookup.IndexOf(theirGuid);
                if (ourIndexOptional != -1)
                {
                    _fromTheirsToOurs[theirIndex] = ourIndexOptional;
                }
            }

            return true;
        }

        private void DecodeReservedPacket(ArraySegment<byte> data)
        {
            if (data.Count == 0) return; // Protocol error: Missing data identifier

            var reservedPacketIdentifier = data.get_Item(0);

            // Reserved packets are not currently used.
        }
    }
}