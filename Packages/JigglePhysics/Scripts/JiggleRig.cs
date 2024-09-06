using System;
using UnityEngine;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig
    {
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        public JiggleSettingsData jiggleSettingsdata;
        public bool NeedsCollisions;
        public int collidersCount;
        public Vector3 Zero = Vector3.zero;
        public UpdateParticleSignalsJob SignalJob;
        public ExtrapolationJob extrapolationJob;
        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        public Transform[] ignoredTransforms;
        public Collider[] colliders;
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        public Transform rootTransform;
        public SphereCollider sphereCollider;
        public JiggleRigLOD JiggleRigLOD;

        public InitalizationData PreInitalData = new InitalizationData();
        public RuntimeData Runtimedata = new RuntimeData();
        public JiggleBone[] JiggleBones;
        public Transform[] ComputedTransforms;
        public int simulatedPointsCount;
        public void Initialize(JiggleRigLOD jiggleRigLOD)
        {
            JiggleRigLOD = jiggleRigLOD;
            InitalizeLists(this);
            CreateSimulatedPoints(this, ignoredTransforms, rootTransform, null);
            JiggleRigHelper.InitalizeIndexes(this);
            simulatedPointsCount = JiggleBones.Length;

            // Precompute normalized indices in a single pass
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                JiggleBone test = JiggleBones[SimulatedIndex];
                int distanceToRoot = 0, distanceToChild = 0;

                // Calculate distance to root
                while (test.JiggleParentIndex != -1)
                {
                    test = JiggleBones[test.JiggleParentIndex];
                    distanceToRoot++;
                }
                test = JiggleBones[SimulatedIndex];
                // Calculate distance to child
                while (test.childIndex != -1)
                {
                    test = JiggleBones[test.childIndex];
                    distanceToChild++;
                }
                int max = distanceToRoot + distanceToChild;
                PreInitalData.normalizedIndex[SimulatedIndex] = (float)distanceToRoot / max;
            }
            JiggleRigHelper.InitializeNativeArrays(this);
            jiggleSettingsdata = jiggleSettings.GetData();
            NeedsCollisions = colliders.Length != 0;
            if (NeedsCollisions)
            {
                if (!CachedSphereCollider.TryGet(out sphereCollider))
                {
                    Debug.LogError("Missing Sphere Collider Bailing!");
                    return;  // No need to proceed if there's no valid sphereCollider
                }
            }
            SignalJob = new UpdateParticleSignalsJob
            {
                workingPosition = Runtimedata.workingPosition,
                particleSignalCurrent = Runtimedata.particleSignalCurrent,
                particleSignalPrevious = Runtimedata.particleSignalPrevious
            };
            extrapolationJob = new ExtrapolationJob
            {
                ParticleSignalCurrent = Runtimedata.particleSignalCurrent,
                ParticleSignalPrevious = Runtimedata.particleSignalPrevious,
                ExtrapolatedPosition = Runtimedata.extrapolatedPosition
            };
        }
    }
}