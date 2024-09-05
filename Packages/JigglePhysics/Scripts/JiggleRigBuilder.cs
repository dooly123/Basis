using System;
using UnityEngine;

namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public class JiggleRigBuilder : MonoBehaviour
    {
        public static float GetmaxCatchupTime(float fixedDeltaTime)
        {
            return fixedDeltaTime * 4;
        }

        public JiggleRig[] jiggleRigs;

        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        [SerializeField]
        public Vector3 wind;

        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        [SerializeField]
        public JiggleRigSimpleLOD levelOfDetail;

        [Tooltip("Draws some simple lines to show what the simulation is doing. Generally this should be disabled.")]
        [SerializeField]
        public bool debugDraw;

        private double accumulation;
        private bool dirtyFromEnable = false;
        private bool wasLODActive = true;
        public int jiggleRigsCount;
        public Vector3 Gravity;
        // Cached variables to avoid repeated Unity API calls
        private Vector3 cachedPosition;
        void OnDisable()
        {
            CachedSphereCollider.RemoveBuilder(this);

            for (int JiggleCount = 0; JiggleCount < jiggleRigsCount; JiggleCount++)
            {
                jiggleRigs[JiggleCount].PrepareTeleport();
            }
        }

        public void Initialize()
        {
            Gravity = Physics.gravity;
            accumulation = 0f;
            jiggleRigsCount = jiggleRigs.Length;
            CacheTransformData();

            for (int JiggleCount = 0; JiggleCount < jiggleRigsCount; JiggleCount++)
            {
                jiggleRigs[JiggleCount].Initialize();
            }

            CachedSphereCollider.AddBuilder(this);
            dirtyFromEnable = true;
        }

        private void CacheTransformData()
        {
            cachedPosition = transform.position;
        }

        public virtual void Advance(float deltaTime, double TimeASDouble, float FixedDeltaTime)
        {
            CacheTransformData();  // Cache the position at the start of Advance
            if (!levelOfDetail.CheckActive(cachedPosition))
            {
                if (wasLODActive)
                {
                    PrepareTeleport();
                }

                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                wasLODActive = false;
                return;
            }

            if (!wasLODActive)
            {
                FinishTeleport(TimeASDouble, FixedDeltaTime);
            }

            CachedSphereCollider.StartPass();

            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].PrepareBone(cachedPosition, levelOfDetail,TimeASDouble);
            }

            if (dirtyFromEnable)
            {
                for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
                {
                    jiggleRigs[JiggleIndex].FinishTeleport(TimeASDouble, FixedDeltaTime);
                }
                dirtyFromEnable = false;
            }

            accumulation = Math.Min(accumulation + deltaTime, GetmaxCatchupTime(FixedDeltaTime));

            while (accumulation > FixedDeltaTime)
            {
                accumulation -= FixedDeltaTime;
                double time = TimeASDouble - accumulation;

                for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
                {
                    jiggleRigs[JiggleIndex].Update(wind, time, FixedDeltaTime, Gravity);
                }
            }

            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].Pose(debugDraw, TimeASDouble);
            }

            CachedSphereCollider.FinishedPass();
            wasLODActive = true;
        }

        private void LateUpdate()
        {
            Advance(Time.deltaTime, Time.timeAsDouble,Time.fixedDeltaTime);
        }

        public void PrepareTeleport()
        {
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].PrepareTeleport();
            }
        }

        public void FinishTeleport(double TimeASDouble, float FixedDeltaTime)
        {
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].FinishTeleport(TimeASDouble, FixedDeltaTime);
            }
        }
        public void FinishTeleport()
        {
            double TimeASDouble = Time.timeAsDouble;
            float FixedDeltaTime = Time.fixedDeltaTime;
            FinishTeleport(TimeASDouble, FixedDeltaTime);
        }

        private void OnRenderObject()
        {
            if (debugDraw)
            {
                double TimeAsDouble = Time.timeAsDouble;
                for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
                {
                    jiggleRigs[JiggleIndex].OnRenderObject(TimeAsDouble);
                }
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying || jiggleRigs == null) return;

            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].Initialize();
            }
        }
    }
}