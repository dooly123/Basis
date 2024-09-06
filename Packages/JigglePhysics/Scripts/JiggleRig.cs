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
    }
}