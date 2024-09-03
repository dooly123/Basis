using System;
using System.Collections.Generic;
using UnityEngine;
namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public partial class JiggleRigBuilder : MonoBehaviour
    {
        public static float maxCatchupTime => Time.fixedDeltaTime * 4;
        public List<JiggleRig> jiggleRigs;
        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        [SerializeField]
        public Vector3 wind;
        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        [SerializeField]
        public JiggleRigSimpleLOD levelOfDetail;
        [Tooltip("Draws some simple lines to show what the simulation is doing. Generally this should be disabled.")]
        [SerializeField] public bool debugDraw;
        private double accumulation;
        private bool dirtyFromEnable = false;
        private bool wasLODActive = true;
        private void Awake()
        {
            Initialize();
        }
        void OnEnable()
        {
            CachedSphereCollider.AddBuilder(this);
            dirtyFromEnable = true;
        }
        void OnDisable()
        {
            CachedSphereCollider.RemoveBuilder(this);
            foreach (var rig in jiggleRigs)
            {
                rig.PrepareTeleport();
            }
        }
        public void Initialize()
        {
            accumulation = 0f;
            jiggleRigs ??= new List<JiggleRig>();
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.Initialize();
            }
        }
        public virtual void Advance(float deltaTime)
        {
            if (levelOfDetail != null && !levelOfDetail.CheckActive(transform.position))
            {
                if (wasLODActive) PrepareTeleport();
                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                wasLODActive = false;
                return;
            }
            if (!wasLODActive) FinishTeleport();
            CachedSphereCollider.StartPass();
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.PrepareBone(transform.position, levelOfDetail);
            }
            if (dirtyFromEnable)
            {
                foreach (var rig in jiggleRigs)
                {
                    rig.FinishTeleport();
                }
                dirtyFromEnable = false;
            }
            accumulation = Math.Min(accumulation + deltaTime, maxCatchupTime);
            while (accumulation > Time.fixedDeltaTime)
            {
                accumulation -= Time.fixedDeltaTime;
                double time = Time.timeAsDouble - accumulation;
                foreach (JiggleRig rig in jiggleRigs)
                {
                    rig.Update(wind, time);
                }
            }
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.Pose(debugDraw);
            }
            CachedSphereCollider.FinishedPass();
            wasLODActive = true;
        }
        private void LateUpdate()
        {
            Advance(Time.deltaTime);
        }
        public void PrepareTeleport()
        {
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.PrepareTeleport();
            }
        }
        public void FinishTeleport()
        {
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.FinishTeleport();
            }
        }
        private void OnDrawGizmos()
        {
            if (jiggleRigs == null)
            {
                return;
            }
            foreach (var rig in jiggleRigs)
            {
                rig.OnRenderObject();
            }
        }
        private void OnValidate()
        {
            if (Application.isPlaying || jiggleRigs == null) return;
            foreach (JiggleRig rig in jiggleRigs)
            {
                rig.Initialize();
            }
        }
    }
}