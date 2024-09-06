using System;
using UnityEngine;

namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public class JiggleRigBuilder : MonoBehaviour
    {
        public const float VERLET_TIME_STEP = 0.02f;
        public const float MAX_CATCHUP_TIME = VERLET_TIME_STEP * 4f;

        public JiggleRig[] jiggleRigs;

        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        [SerializeField]
        public Vector3 wind;

        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        [SerializeField]
        public JiggleRigSimpleLOD levelOfDetail;

        private double accumulation;
        private bool dirtyFromEnable = false;
        private bool wasLODActive = true;
        public int jiggleRigsCount;
        public Vector3 Gravity;
        // Cached variables to avoid repeated Unity API calls
        private Vector3 cachedPosition;
        public double currentFrame;
        public double previousFrame;
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

            double CurrentTime = Time.timeAsDouble;
            currentFrame = CurrentTime;
            previousFrame = CurrentTime;
            ComputeSquareVelvetTiming(VERLET_TIME_STEP);
            for (int JiggleCount = 0; JiggleCount < jiggleRigsCount; JiggleCount++)
            {
                jiggleRigs[JiggleCount].Initialize();
            }

            CachedSphereCollider.AddBuilder(this);
            dirtyFromEnable = true;
        }
        public float squaredDeltaTime;
        public void ComputeSquareVelvetTiming(float VelvetTiming)
        {
            // Precompute values outside the loop
            squaredDeltaTime = VelvetTiming * VelvetTiming;
        }

        private void CacheTransformData()
        {
            cachedPosition = transform.position;
        }

        public virtual void Advance(float deltaTime, double timeAsDouble, float VelvetTiming)
        {
            CacheTransformData();  // Cache the position at the start of Advance

            // Early exit if not active, avoiding unnecessary checks
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

            // Handle the transition from inactive to active
            if (!wasLODActive)
            {
                FinishTeleport(timeAsDouble);
            }

            CachedSphereCollider.StartPass();

            // Combine similar loops for cache-friendliness
            currentFrame = timeAsDouble;
            for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
            {
                jiggleRigs[jiggleIndex].PrepareBone(cachedPosition, levelOfDetail);
            }

            if (dirtyFromEnable)
            {
                LockFrame(timeAsDouble);
                for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
                {
                    jiggleRigs[jiggleIndex].FinishTeleport();
                }
                dirtyFromEnable = false;
            }

            // Cap accumulation to avoid too many iterations
            accumulation = Math.Min(accumulation + deltaTime, MAX_CATCHUP_TIME);
            double diff;
            diff = currentFrame - previousFrame;
            float Percentage = (float)((timeAsDouble - previousFrame) / diff);
            // Update within while loop only when necessary
            if (accumulation > VelvetTiming)
            {
                do
                {
                    accumulation -= VelvetTiming;
                    currentFrame = timeAsDouble;
                    // Update each jiggleRig in the same loop to reduce loop overhead
                    for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
                    {
                        jiggleRigs[jiggleIndex].Update(wind, VelvetTiming, squaredDeltaTime, Gravity, Percentage);
                    }
                } while (accumulation > VelvetTiming);
            }
            // Final pose loop
            for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
            {
                jiggleRigs[jiggleIndex].Pose(Percentage);
            }

            CachedSphereCollider.FinishedPass();
            wasLODActive = true;
        }
        private void LateUpdate()
        {
            Advance(Time.deltaTime, Time.timeAsDouble, VERLET_TIME_STEP);
        }

        public void PrepareTeleport()
        {
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].PrepareTeleport();
            }
        }

        public void FinishTeleport(double TimeASDouble)
        {
            LockFrame(TimeASDouble);
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                jiggleRigs[JiggleIndex].FinishTeleport();
            }
        }
        public void FinishTeleport()
        {
            FinishTeleport(Time.timeAsDouble);
        }
        public void LockFrame(double timeAsDouble)
        {
            previousFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME * 2f;
            currentFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME;
        }
    }
}