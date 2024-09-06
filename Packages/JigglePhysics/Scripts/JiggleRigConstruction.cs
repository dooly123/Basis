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

        public List<Vector3> targetAnimatedBoneSignalCurrent;
        public List<Vector3> particleSignalCurrent;

        public List<Vector3> targetAnimatedBoneSignalPrevious;
        public List<Vector3> particleSignalPrevious;
    }
    public static void InitalizeLists(ref JiggleRig JiggleRig)
    {
        JiggleRig.RawTransforms = new List<Transform>();
        JiggleRig.PreInitalData.boneRotationChangeCheck = new List<Quaternion>();
        JiggleRig.PreInitalData.lastValidPoseBoneRotation = new List<Quaternion>();
        JiggleRig.PreInitalData.currentFixedAnimatedBonePosition = new List<Vector3>();
        JiggleRig.PreInitalData.bonePositionChangeCheck = new List<Vector3>();
        JiggleRig.PreInitalData.lastValidPoseBoneLocalPosition = new List<Vector3>();
        JiggleRig.PreInitalData.workingPosition = new List<Vector3>();
        JiggleRig.PreInitalData.preTeleportPosition = new List<Vector3>();
        JiggleRig.PreInitalData.extrapolatedPosition = new List<Vector3>();
        JiggleRig.PreInitalData.hasTransform = new List<bool>();
        JiggleRig.PreInitalData.normalizedIndex = new List<float>();

        JiggleRig.PreInitalData.targetAnimatedBoneSignalCurrent = new List<Vector3>();
        JiggleRig.PreInitalData.particleSignalCurrent = new List<Vector3>();

        JiggleRig.PreInitalData.targetAnimatedBoneSignalPrevious = new List<Vector3>();
        JiggleRig.PreInitalData.particleSignalPrevious = new List<Vector3>();
    }
    public static void CreateSimulatedPoints(ref JiggleRig JiggleRig, Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
    {
        // Recursive function to create simulated points using a list
        void CreateSimulatedPointsInternal(JiggleRig JiggleRig, Transform[] ignored, Transform current, JiggleBone parent)
        {
            // Create a new JiggleBone and add it to the list
            JiggleBone newJiggleBone = JiggleBone( JiggleRig, current, parent);
            // Check if the currentTransform has no children
            if (current.childCount == 0)
            {
                // Handle the case where newJiggleBone has no parent
                if (newJiggleBone.JiggleParentIndex == -1)
                {
                   int Index = Array.IndexOf(JiggleRig.JiggleBones, newJiggleBone);
                    if (JiggleRig.RawTransforms[Index].parent == null)
                    {
                        throw new UnityException("Can't have a singular jiggle bone with no parents. That doesn't even make sense!");
                    }
                    else
                    {
                        // Add an extra virtual JiggleBone
                        JiggleBone ExtraBone = JiggleBone( JiggleRig, null, newJiggleBone);
                        return;
                    }
                }
                // Add another virtual JiggleBone
                JiggleBone virtualBone = JiggleBone( JiggleRig, null, newJiggleBone);
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
        // If the original array is null, initialize it with a single element array
        if (originalArray == null)
        {
            return new JiggleBone[] { newItem };
        }

        // Resize the array to have one additional slot
        Array.Resize(ref originalArray, originalArray.Length + 1);

        // Add the new item to the end of the resized array
        originalArray[originalArray.Length - 1] = newItem;

        return originalArray;
    }
    public static Transform[] AddToArray(Transform[] originalArray, Transform newItem)
    {
        // If the original array is null, initialize it with a single element array
        if (originalArray == null)
        {
            return new Transform[] { newItem };
        }

        // Resize the array to have one additional slot
        Array.Resize(ref originalArray, originalArray.Length + 1);

        // Add the new item to the end of the resized array
        originalArray[originalArray.Length - 1] = newItem;

        return originalArray;
    }
    public static JiggleBone JiggleBone(JiggleRig JiggleRig,Transform transform, JiggleBone parent)
    {
        JiggleBone JiggleBone = new JiggleBone
        {
            JiggleParentIndex = -1,
            childIndex = -1
        };
        JiggleRig.JiggleBones = AddToArray(JiggleRig.JiggleBones, JiggleBone);
        int ParentIndex = Array.IndexOf(JiggleRig.JiggleBones, parent);
       // JiggleBone.boneIndex = Array.IndexOf(JiggleRig.JiggleBones, JiggleBone);
        JiggleBone.JiggleParentIndex = ParentIndex;
        JiggleRig.RawTransforms.Add(transform);

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
                int Index = Array.IndexOf(JiggleRig.JiggleBones, JiggleBone);
                position = JiggleRigHelper.GetProjectedPosition(Index, JiggleBone.JiggleParentIndex,ref JiggleRig);
            }
            else
            {
                position = Vector3.zero;
            }
        }
        JiggleRig.PreInitalData.targetAnimatedBoneSignalCurrent.Add(position);
        JiggleRig.PreInitalData.particleSignalCurrent.Add(position);

        JiggleRig.PreInitalData.targetAnimatedBoneSignalPrevious.Add(position);
        JiggleRig.PreInitalData.particleSignalPrevious.Add(position);



        JiggleRig.PreInitalData.hasTransform.Add(transform != null);
        if (parent == null)
        {
            return JiggleBone;
        }
        int childIndex = Array.IndexOf(JiggleRig.JiggleBones, JiggleBone);

        int SParentIndex = JiggleBone.JiggleParentIndex;
        JiggleRig.JiggleBones[SParentIndex].childIndex = childIndex;
        return JiggleBone;
    }
}