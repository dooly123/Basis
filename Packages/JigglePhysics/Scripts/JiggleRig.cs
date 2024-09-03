using System;
using System.Collections.Generic;
using UnityEngine;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig
    {
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        private Transform rootTransform;
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        private Transform[] ignoredTransforms;
        public Collider[] colliders;
        [SerializeField]
        public JiggleSettingsData data;
        private bool initialized;
        public Transform GetRootTransform() => rootTransform;
        public int simulatedPointsCount;
        private bool NeedsCollisions => colliders.Length != 0;
        [HideInInspector]
        protected JiggleBone[] simulatedPoints;
        public int collidersCount;
        public Vector3 Zero;
        public JiggleRig(Transform rootTransform, JiggleSettingsBase jiggleSettings, Transform[] ignoredTransforms, Collider[] colliders)
        {
            this.rootTransform = rootTransform;
            this.jiggleSettings = jiggleSettings;
            this.ignoredTransforms = ignoredTransforms;
            this.colliders = colliders;
            this.collidersCount = colliders.Length;
            Zero = Vector3.zero;
            Initialize();
        }
        public void PrepareBone(Vector3 position, JiggleRigLOD jiggleRigLOD, double timeAsDouble)
        {
            if (!initialized)
            {
                throw new UnityException("JiggleRig was never initialized. Please call JiggleRig.Initialize() if you're going to manually timestep.");
            }
            for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
            {
                simulatedPoints[PointIndex].PrepareBone(timeAsDouble);
            }
            data = jiggleSettings.GetData();
            data = jiggleRigLOD != null ? jiggleRigLOD.AdjustJiggleSettingsData(position, data) : data;
        }
        public void Update(Vector3 wind, double time, float fixedDeltaTime,Vector3 Gravity)
        {
            float squaredDeltaTime = fixedDeltaTime * fixedDeltaTime;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].VerletPass(data, wind, time, fixedDeltaTime, squaredDeltaTime, Gravity);//this one multi
            }
            if (NeedsCollisions)
            {
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    simulatedPoints[Index].CollisionPreparePass(data);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].ConstraintPass(data);//this one multi
            }
            if (NeedsCollisions)
            {
                for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
                {
                    simulatedPoints[SimulatedIndex].CollisionPass(jiggleSettings, colliders, collidersCount);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].SignalWritePosition(time);//this one multi
            }
        }
        public void Initialize()
        {
            if (rootTransform == null)
            {
                return;
            }
            CreateSimulatedPoints(ref simulatedPoints, ignoredTransforms, rootTransform, null);
            this.simulatedPointsCount = simulatedPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].CalculateNormalizedIndex();
            }
            initialized = true;
        }
        public void DeriveFinalSolve(double timeAsDouble)
        {
            Vector3 virtualPosition = simulatedPoints[0].DeriveFinalSolvePosition(Zero, timeAsDouble);
            Vector3 offset = simulatedPoints[0].transform.position - virtualPosition;
            int simulatedPointsLength = simulatedPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].DeriveFinalSolvePosition(offset, timeAsDouble);
            }
        }
        public void Pose(bool debugDraw, float deltaTime,double timeAsDouble)
        {
            DeriveFinalSolve(timeAsDouble);
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                simulatedPoints[SimulatedIndex].PoseBone(data.blend, deltaTime);

                if (debugDraw)
                {
                    simulatedPoints[SimulatedIndex].DebugDraw(Color.red, Color.blue, true);
                }
            }
        }
        public void PrepareTeleport()
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                simulatedPoints[PointsIndex].PrepareTeleport();
            }
        }
        public void FinishTeleport(double timeAsDouble,float FixedDeltaTime)
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                simulatedPoints[PointsIndex].FinishTeleport(timeAsDouble, FixedDeltaTime);
            }
        }
        public void OnRenderObject(double TimeAsDouble)
        {
            if (!initialized || simulatedPoints == null)
            {
                Initialize();
            }
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                simulatedPoints[PointsIndex].OnDrawGizmos(jiggleSettings, TimeAsDouble);
            }
        }
        protected virtual void CreateSimulatedPoints(ref JiggleBone[] outputPoints, Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
        {
            // Use a list to store the JiggleBones
            List<JiggleBone> jiggleBoneList = new List<JiggleBone>(outputPoints ?? new JiggleBone[0]);
            // Recursive function to create simulated points using a list
            void CreateSimulatedPointsInternal(List<JiggleBone> list, Transform[] ignored, Transform current, JiggleBone parent)
            {
                // Create a new JiggleBone and add it to the list
                JiggleBone newJiggleBone = new JiggleBone(current, parent);
                list.Add(newJiggleBone);
                // Check if the currentTransform has no children
                if (current.childCount == 0)
                {
                    // Handle the case where newJiggleBone has no parent
                    if (newJiggleBone.JiggleParent == null)
                    {
                        if (newJiggleBone.transform.parent == null)
                        {
                            throw new UnityException("Can't have a singular jiggle bone with no parents. That doesn't even make sense!");
                        }
                        else
                        {
                            // Add an extra virtual JiggleBone
                            list.Add(new JiggleBone(null, newJiggleBone));
                            return;
                        }
                    }
                    // Add another virtual JiggleBone
                    list.Add(new JiggleBone(null, newJiggleBone));
                    return;
                }
                // Iterate through child transforms
                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = current.GetChild(i);
                    // Check if the child is in the ignoredTransforms array
                    if (Array.Exists(ignored, t => t == child))
                    {
                        continue;
                    }
                    // Recursively create simulated points for child transforms
                    CreateSimulatedPointsInternal(list, ignored, child, newJiggleBone);
                }
            }
            // Call the internal recursive method
            CreateSimulatedPointsInternal(jiggleBoneList, ignoredTransforms, currentTransform, parentJiggleBone);
            // Convert the list back to an array and assign it to outputPoints
            outputPoints = jiggleBoneList.ToArray();
        }
    }
}