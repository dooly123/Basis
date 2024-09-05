using JigglePhysics;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class JiggleRigConstruction
{
    public struct InitalizationData
    {
        public List<Quaternion> boneRotationChangeCheck;
        public List<Quaternion> lastValidPoseBoneRotation;
        public List<Vector3> currentFixedAnimatedBonePosition;
        public List<Vector3> bonePositionChangeCheck;
        public List<Vector3> lastValidPoseBoneLocalPosition;
        public List<Vector3> workingPosition;
        public List<Vector3> preTeleportPosition;
        public List<Vector3> extrapolatedPosition;
        public List<bool> hasTransform;
        public List<float> normalizedIndex;
        public List<PositionSignal> targetAnimatedBoneSignal;
        public List<PositionSignal> particleSignal;
    }
    public static void CreateSimulatedPoints(JiggleRig JiggleRig, Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
    {
        // Recursive function to create simulated points using a list
        void CreateSimulatedPointsInternal(JiggleRig JiggleRig, Transform[] ignored, Transform current, JiggleBone parent)
        {
            // Create a new JiggleBone and add it to the list
            JiggleBone newJiggleBone = JiggleRigConstruction.JiggleBone(JiggleRig, current, parent);
            // Check if the currentTransform has no children
            if (current.childCount == 0)
            {
                // Handle the case where newJiggleBone has no parent
                if (newJiggleBone.JiggleParentIndex == -1)
                {
                    if (JiggleRig.ComputedTransforms[newJiggleBone.boneIndex].parent == null)
                    {
                        throw new UnityException("Can't have a singular jiggle bone with no parents. That doesn't even make sense!");
                    }
                    else
                    {
                        // Add an extra virtual JiggleBone
                        JiggleBone ExtraBone = JiggleBone(JiggleRig, null, newJiggleBone);
                        return;
                    }
                }
                // Add another virtual JiggleBone
                JiggleBone virtualBone = JiggleBone(JiggleRig, null, newJiggleBone);
                return;
            }
            // Iterate through child transforms
            int childCount = current.childCount;
            for (int ChildIndex = 0; ChildIndex < childCount; ChildIndex++)
            {
                Transform child = current.GetChild(ChildIndex);
                // Check if the child is in the ignoredTransforms array
                if (Array.Exists(ignored, t => t == child))
                {
                    continue;
                }
                // Recursively create simulated points for child transforms
                CreateSimulatedPointsInternal(JiggleRig, ignored, child, newJiggleBone);
            }
        }
        // Call the internal recursive method
        CreateSimulatedPointsInternal(JiggleRig, ignoredTransforms, currentTransform, parentJiggleBone);
    }
    public static JiggleBone[] AddToArray(JiggleBone[] originalArray, JiggleBone newItem)
    {
        // Create a new array with one extra slot
        JiggleBone[] newArray;
        if (originalArray == null)
        {
            originalArray = new JiggleBone[] { };
        }
        newArray = new JiggleBone[originalArray.Length + 1];

        // Copy the original array into the new array
        for (int i = 0; i < originalArray.Length; i++)
        {
            newArray[i] = originalArray[i];
        }

        // Add the new item to the end of the new array
        newArray[originalArray.Length] = newItem;

        return newArray;
    }
    public static JiggleBone JiggleBone(JiggleRig JiggleRig, Transform transform, JiggleBone parent)
    {
        JiggleBone JiggleBone = new JiggleBone
        {
            JiggleParentIndex = -1,
            childIndex = -1
        };
        JiggleRig.JiggleBoneIndexes = AddToArray(JiggleRig.JiggleBoneIndexes, JiggleBone);
        int ParentIndex = Array.IndexOf(JiggleRig.JiggleBoneIndexes, parent);
        JiggleBone.boneIndex = Array.IndexOf(JiggleRig.JiggleBoneIndexes, JiggleBone);
        JiggleBone.JiggleParentIndex = ParentIndex;
        JiggleRig.ComputedTransforms.Add(transform);
        JiggleRig.PreInitalData.boneRotationChangeCheck.Add(Quaternion.identity);
        JiggleRig.PreInitalData.currentFixedAnimatedBonePosition.Add(Vector3.zero);
        JiggleRig.PreInitalData.bonePositionChangeCheck.Add(Vector3.zero);
        JiggleRig.PreInitalData.workingPosition.Add(Vector3.zero);
        JiggleRig.PreInitalData.preTeleportPosition.Add(Vector3.zero);
        JiggleRig.PreInitalData.extrapolatedPosition.Add(Vector3.zero);
        JiggleRig.PreInitalData.normalizedIndex.Add(0);
        Vector3 position;
        if (transform != null)
        {
            transform.GetLocalPositionAndRotation(out Vector3 Position, out Quaternion Rotation);
            position = transform.position;
            JiggleRig.PreInitalData.lastValidPoseBoneRotation.Add(Rotation);
            JiggleRig.PreInitalData.lastValidPoseBoneLocalPosition.Add(Position);
        }
        else
        {
            JiggleRig.PreInitalData.lastValidPoseBoneRotation.Add(Quaternion.identity);
            JiggleRig.PreInitalData.lastValidPoseBoneLocalPosition.Add(Vector3.zero);
            if (JiggleBone.JiggleParentIndex != -1)
            {
                position = JiggleRig.GetProjectedPosition(JiggleBone, JiggleBone.JiggleParentIndex);
            }
            else
            {
                position = Vector3.zero;
            }
        }
        double timeAsDouble = Time.timeAsDouble;
        JiggleRig.PreInitalData.targetAnimatedBoneSignal.Add(new PositionSignal(position, timeAsDouble));
        JiggleRig.PreInitalData.particleSignal.Add(new PositionSignal(position, timeAsDouble));
        JiggleRig.PreInitalData.hasTransform.Add(transform != null);
        if (parent == null)
        {
            return JiggleBone;
        }
        int childIndex = Array.IndexOf(JiggleRig.JiggleBoneIndexes, JiggleBone);

        int SParentIndex = JiggleBone.JiggleParentIndex;
        JiggleRig.JiggleBoneIndexes[SParentIndex].childIndex = childIndex;
        return JiggleBone;
    }
}