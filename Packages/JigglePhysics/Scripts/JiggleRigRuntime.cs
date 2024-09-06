using System;
using UnityEngine;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRigRuntime
    {
        public bool NeedsCollisions;
        public int collidersCount;
        public int simulatedPointsCount;

        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        public JiggleSettingsData jiggleSettingsdata;
        public UpdateParticleSignalsJob SignalJob;
        public ExtrapolationJob extrapolationJob;
        [SerializeField]
        public InitalizationData PreInitalData;
        [SerializeField]
        public RuntimeData Runtimedata;

        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        public Transform[] ignoredTransforms;
        public Collider[] colliders;
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        public Transform rootTransform;
        public SphereCollider sphereCollider;
        public JiggleRigLOD JiggleRigLOD;
        public JiggleBone[] JiggleBones;
        public Transform[] ComputedTransforms;
    }

}