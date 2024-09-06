using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig
    {
        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        public Transform[] ignoredTransforms;
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        public Transform rootTransform;
        public List<Transform> RawTransforms;
        [SerializeField]
        public InitalizationData PreInitalData;

        //what is still used in the update
        public SphereCollider sphereCollider;
        public Collider[] colliders;
        [SerializeField]
        public JiggleBone[] JiggleBones;
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
    }
    public struct JiggleRigRuntime
    {
        [SerializeField]
        public JiggleSettingsData jiggleSettingsdata;
        public UpdateParticleSignalsJob SignalJob;
        public ExtrapolationJob extrapolationJob;
        [SerializeField]
        public RuntimeData Runtimedata;
        public Transform[] TransformAccessArray;
    }
}