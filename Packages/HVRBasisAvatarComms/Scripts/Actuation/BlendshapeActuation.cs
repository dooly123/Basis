using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Blendshape Actuation")]
    public class BlendshapeActuation : MonoBehaviour, ICommsNetworkable
    {
        private const int MaxAddresses = 256;
        private const float BlendshapeAtFullStrength = 100f;

        [SerializeField] private SkinnedMeshRenderer[] renderers;
        [SerializeField] private BlendshapeActuationDefinitionFile[] definitionFiles;
        [SerializeField] private BlendshapeActuationDefinition[] definitions;
        
        // TODO: These should be auto-filled by the system.
        [SerializeField] private FeatureNetworking featureNetworking;
        [SerializeField] private AcquisitionService acquisition;
        
        private Dictionary<string, int> _addressBase = new Dictionary<string, int>();
        private ComputedActuator[] _computedActuators;
        private ComputedActuator[][] _addressBaseIndexToActuators;
        private FeatureInterpolator featureInterpolator;
        private int _guidIndex;

        private void OnAddressUpdated(string address, float inRange)
        {
            if (!_addressBase.TryGetValue(address, out var index)) return;
            
            // TODO: Might need to queue and delay this change so that it executes on the Update loop.

            var actuatorsForThisAddress = _addressBaseIndexToActuators[index];
            if (actuatorsForThisAddress == null) return; // There may be no actuator for an address when it does not exist in the renderers.
            
            var lower = 0f;
            var upper = 0f;
            foreach (var actuator in actuatorsForThisAddress)
            {
                Actuate(actuator, inRange);
                lower = actuator.StreamedLower;
                upper = actuator.StreamedUpper;
            }
            
            var streamed01 = Mathf.InverseLerp(lower, upper, inRange);
            featureInterpolator.Store(index, streamed01);
        }

        private void OnInterpolatedDataChanged(float[] current)
        {
            foreach (var actuator in _computedActuators)
            {
                var streamed01 = current[actuator.AddressIndex];
                var inRange = Mathf.Lerp(actuator.StreamedLower, actuator.StreamedUpper, streamed01);
                
                Actuate(actuator, inRange);
            }
        }

        private static void Actuate(ComputedActuator actuator, float inRange)
        {
            var intermediate01 = Mathf.InverseLerp(actuator.InStart, actuator.InEnd, inRange);
            if (actuator.UseCurve)
            {
                intermediate01 = actuator.Curve.Evaluate(intermediate01);
            }
            var outputWild = Mathf.Lerp(actuator.OutStart, actuator.OutEnd, intermediate01);
            var output01 = Mathf.Clamp01(outputWild);
            var output0100 = output01 * BlendshapeAtFullStrength;
                
            foreach (var target in actuator.Targets)
            {
                foreach (var blendshapeIndex in target.BlendshapeIndices)
                {
                    target.Renderer.SetBlendShapeWeight(blendshapeIndex, output0100);
                }
            }
        }

        private void WhenNetworkIdAssigned()
        {
            var allDefinitions = definitions
                .Concat(definitionFiles.SelectMany(file => file.definitions))
                .ToArray();
            _addressBase = MakeIndexDictionary(allDefinitions.Select(definition => definition.address).Distinct().ToArray());
            
            if (_addressBase.Count > MaxAddresses)
            {
                Debug.LogError($"Exceeded max {MaxAddresses} addresses allowed in an actuator.");
                enabled = false;
                return;
            }

            var smrToBlendshapeNames = new Dictionary<SkinnedMeshRenderer, List<string>>();
            foreach (var smr in renderers)
            {
                var mesh = smr.sharedMesh;
                smrToBlendshapeNames.Add(smr, Enumerable.Range(0, mesh.blendShapeCount)
                    .Select(i => mesh.GetBlendShapeName(i))
                    .ToList());
            }

            // All streamed avatar feature values are between 0 and 1.
            // If we want to stream values outside of this range (i.e. [-1; 1]), we need to collect all
            // possible InStart and InEnd values in order to lerp in that range.
            var addressToStreamedLowerUpper = allDefinitions
                .GroupBy(definition => definition.address)
                .ToDictionary(grouping => grouping.Key, grouping =>
                {
                    var inValuesForThisAddress = grouping
                        // Reminder that InStart may be greater than InEnd.
                        // We want the lower bound, not the minimum of InStart.
                        .SelectMany(definition => new [] { definition.inStart, definition.inEnd })
                        .ToArray();
                    return (inValuesForThisAddress.Min(), inValuesForThisAddress.Max());
                });
            
            _computedActuators = allDefinitions.Select(definition =>
                {
                    var actuatorTargets = ComputeTargets(smrToBlendshapeNames, definition.blendshapes, definition.onlyFirstMatch);
                    if (actuatorTargets.Length == 0) return null;

                    var (lower, upper) = addressToStreamedLowerUpper[definition.address];
                    return new ComputedActuator
                    {
                        AddressIndex = _addressBase[definition.address],
                        InStart = definition.inStart,
                        InEnd = definition.inEnd,
                        OutStart = definition.outStart,
                        OutEnd = definition.outEnd,
                        StreamedLower = lower,
                        StreamedUpper = upper,
                        UseCurve = definition.useCurve,
                        Curve = definition.curve,
                        Targets = actuatorTargets
                    };
                })
                .Where(actuator => actuator != null)
                .ToArray();

            _addressBaseIndexToActuators = new ComputedActuator[_addressBase.Count][];
            foreach (var computedActuator in _computedActuators.GroupBy(actuator => actuator.AddressIndex, actuator => actuator))
            {
                _addressBaseIndexToActuators[computedActuator.Key] = computedActuator.ToArray();
            }

            acquisition.RegisterAddresses(_addressBase.Keys.ToArray(), OnAddressUpdated);
            featureInterpolator = featureNetworking.NewInterpolator(_guidIndex, _addressBase.Count, OnInterpolatedDataChanged);
        }

        private Dictionary<string, int> MakeIndexDictionary(string[] addressBase)
        {
            var dictionary = new Dictionary<string, int>();
            for (var index = 0; index < addressBase.Length; index++)
            {
                var se = addressBase[index];
                dictionary[se] = index;
            }

            return dictionary;
        }

        private void OnDisable()
        {
            acquisition.UnregisterAddresses(_addressBase.Keys.ToArray(), OnAddressUpdated);
            featureInterpolator.Unregister();
            featureInterpolator = null;
        }

        private ComputedActuatorTarget[] ComputeTargets(Dictionary<SkinnedMeshRenderer, List<string>> smrToBlendshapeNames, string[] definitionBlendshapes, bool onlyFirstMatch)
        {
            var actuatorTargets = new List<ComputedActuatorTarget>();
            foreach (var pair in smrToBlendshapeNames)
            {
                var indices = definitionBlendshapes
                    .Select(toFind => pair.Value.IndexOf(toFind))
                    .Where(i => i >= 0)
                    .ToArray();
            
                if (indices.Length > 0)
                {
                    if (onlyFirstMatch)
                    {
                        actuatorTargets.Add(new ComputedActuatorTarget
                        {
                            Renderer = pair.Key,
                            BlendshapeIndices = new[] { indices[0] }
                        });
                    }
                    else
                    {
                        actuatorTargets.Add(new ComputedActuatorTarget
                        {
                            Renderer = pair.Key,
                            BlendshapeIndices = indices
                        });
                    }
                }
            }

            return actuatorTargets.ToArray();
        }

        private class ComputedActuator
        {
            public int AddressIndex;
            public float StreamedLower;
            public float StreamedUpper;
            public float InStart;
            public float InEnd;
            public float OutStart;
            public float OutEnd;
            public bool UseCurve;
            public AnimationCurve Curve;
            public ComputedActuatorTarget[] Targets;
        }

        private class ComputedActuatorTarget
        {
            public SkinnedMeshRenderer Renderer;
            public int[] BlendshapeIndices;
        }

        public void OnGuidAssigned(int guidIndex, Guid guid)
        {
            _guidIndex = guidIndex;
            WhenNetworkIdAssigned();
        }
    }
}