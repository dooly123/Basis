using System;
using System.Collections.Generic;
using System.Linq;
using Basis.Scripts.BasisSdk;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Feature Networking")]
    public class FeatureNetworking : MonoBehaviour
    {
        public const byte NegotiationPacket = 255;
        public const byte ReservedPacket = 254;

        public delegate void InterpolatedDataChanged(float[] current);

        [SerializeField] private FeatureNetPairing[] netPairings = new FeatureNetPairing[0]; // Unsafe: May contain malformed GUIDs, or null components, or non-networkable components.
        [HideInInspector] [SerializeField] private BasisAvatar avatar;

        private Dictionary<Guid, ICommsNetworkable> _guidToNetworkable;
        private Guid[] _orderedGuids;
        private byte[] _negotiationPacket;
        
        private FeatureInterpolator[] _featureHandles;
        private GameObject _holder;
        private bool _isWearer;

        private void Awake()
        {
            if (avatar == null) avatar = CommsUtil.GetAvatar(this);
            if (avatar.GetComponentInChildren<HVRAvatarComms>(true) == null)
            {
                avatar.gameObject.AddComponent<HVRAvatarComms>();
            }
            
            var rand = new System.Random();
            var safeNetPairings = netPairings
                .Where(pairing => Guid.TryParse(pairing.guid, out _))
                .Where(pairing => pairing.component != null)
                .Select(pairing =>
                {
                    if (pairing.component is ICommsNetworkable) return pairing;

                    // Be lenient if the user has dragged the Transform into this.
                    var lookingForNetworkable = pairing.component.GetComponents<Component>();
                    foreach (var candidate in lookingForNetworkable)
                    {
                        if (candidate is ICommsNetworkable)
                        {
                            return new FeatureNetPairing
                            {
                                guid = pairing.guid,
                                component = candidate
                            };
                        }
                    }

                    return pairing; // Will not go through the following .Where predicate
                })
                .Where(pairing => pairing.component is ICommsNetworkable)
                // Shuffling the array makes sure we catch implementation mistakes early.
                // The order of the list of pairings should not matter between clients because of the Negotiation packet.
                .OrderBy(_ => rand.Next())
                .ToArray();
                
            _guidToNetworkable = safeNetPairings.ToDictionary(pairing => new Guid(pairing.guid), pairing => (ICommsNetworkable)pairing.component);
            _orderedGuids = safeNetPairings.Select(pairing => new Guid(pairing.guid)).ToArray();
            _negotiationPacket = new [] { NegotiationPacket }
                .Concat(safeNetPairings.SelectMany(pairing => new Guid(pairing.guid).ToByteArray()))
                .ToArray();

            _featureHandles = new FeatureInterpolator[safeNetPairings.Length];
        }

        public void OnPacketReceived(int guidIndex, ArraySegment<byte> arraySegment)
        {
            var handleOptional = _featureHandles[guidIndex];
            if (handleOptional != null)
            {
                handleOptional.OnPacketReceived(arraySegment);
            }
        }

        public Guid[] GetOrderedGuids()
        {
            return _orderedGuids;
        }

        public FeatureInterpolator NewInterpolator(int guidIndex, int count, InterpolatedDataChanged interpolatedDataChanged)
        {
            _holder = new GameObject($"Streamed-{guidIndex}")
            {
                transform = { parent = transform }
            };
            _holder.SetActive(false);
            var streamed = _holder.AddComponent<StreamedAvatarFeature>();
            streamed.avatar = avatar;
            streamed.valueArraySize = (byte)count; // TODO: Sanitize count to be within bounds
            _holder.SetActive(true);
            
            var handle = new FeatureInterpolator(this, guidIndex, streamed, interpolatedDataChanged);
            streamed.OnInterpolatedDataChanged += handle.OnInterpolatedDataChanged;
            streamed.SetEncodingInfo(_isWearer, (byte)guidIndex); // TODO: Make sure upstream that guidIndex is within limits
            _featureHandles[guidIndex] = handle;
            return handle;
        }

        internal void Unregister(int guidIndex)
        {
            _featureHandles[guidIndex] = null;
            Destroy(_holder);
        }

        public byte[] GetNegotiationPacket()
        {
            return _negotiationPacket;
        }

        public void AssignGuids(bool isWearer)
        {
            _isWearer = isWearer;
            for (var index = 0; index < _orderedGuids.Length; index++)
            {
                var guid = _orderedGuids[index];
                var networkable = _guidToNetworkable[guid];
                
                networkable.OnGuidAssigned(index, guid);
            }
        }

        public bool TryAddPairingIfNotExists(Component networkable)
        {
            if (netPairings.All(pairing => pairing.component != networkable))
            {
                netPairings = netPairings.Concat(new[]
                {
                    new FeatureNetPairing
                    {
                        guid = Guid.NewGuid().ToString(),
                        component = networkable
                    }
                }).ToArray();
                return true;
            }

            return false;
        }
    }

    public class FeatureInterpolator
    {
        private readonly FeatureNetworking _featureNetworking;
        private readonly int guidIndex;
        private readonly StreamedAvatarFeature _streamed;
        private readonly FeatureNetworking.InterpolatedDataChanged interpolatedDataChanged;

        internal FeatureInterpolator(FeatureNetworking featureNetworking, int guidIndex, StreamedAvatarFeature streamed, FeatureNetworking.InterpolatedDataChanged interpolatedDataChanged)
        {
            _featureNetworking = featureNetworking;
            this.guidIndex = guidIndex;
            _streamed = streamed;
            this.interpolatedDataChanged = interpolatedDataChanged;
        }

        public void Store(int value, float streamed01)
        {
            _streamed.Store(value, streamed01);
        }

        public void Unregister()
        {
            _featureNetworking.Unregister(guidIndex);
        }

        public void OnPacketReceived(ArraySegment<byte> data)
        {
            _streamed.OnPacketReceived(data);
        }

        public void OnInterpolatedDataChanged(float[] current)
        {
            interpolatedDataChanged.Invoke(current);
        }
    }

    [Serializable]
    public class FeatureNetPairing
    {
        public Component component;
        public string guid;
    }
}