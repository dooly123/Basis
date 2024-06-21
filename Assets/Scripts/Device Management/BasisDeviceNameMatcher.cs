using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasisDeviceNameMatcher", menuName = "Basis/BasisDeviceNameMatcher", order = 1)]
public class BasisDeviceNameMatcher : ScriptableObject
{
    public List<BasisDeviceMatchableNames> BasisDevice = new List<BasisDeviceMatchableNames>();
    public bool GetAssociatedDeviceID(string nameToMatch, out string DeviceID)
    {
        foreach (var DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                DeviceID = DeviceEntry.DeviceID;
                return true;
            }
        }
        DeviceID = null;
        return false;
    }
    public bool GetAssociatedDeviceID(string nameToMatch, out string DeviceID,out bool ShowVisual)
    {
        foreach (var DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                DeviceID = DeviceEntry.DeviceID;
                ShowVisual = DeviceEntry.HasRepresentation;
                return true;
            }
        }
        ShowVisual = false;
        DeviceID = null;
        return false;
    }
    public bool GetAssociatedPivotOffset(string nameToMatch, out Vector3 pivotOffset)
    {
        foreach (var DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                pivotOffset = DeviceEntry.PivotOffset;
                return true;
            }
        }
        pivotOffset = Vector3.zero;
        return false;
    }
    public bool GetAssociatedRotationOffset(string nameToMatch, out Vector3 rotationOffset)
    {
        foreach (var DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                rotationOffset = DeviceEntry.RotationOffset;
                return true;
            }
        }
        rotationOffset = Vector3.zero;
        return false;
    }
    public bool GetAssociatedPivotAndRotationOffset(string nameToMatch, out Vector3 rotationOffset, out Vector3 pivotOffset)
    {
        foreach (BasisDeviceMatchableNames DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                rotationOffset = DeviceEntry.RotationOffset;
                pivotOffset = DeviceEntry.PivotOffset;
                return true;
            }
        }
        rotationOffset = Vector3.zero;
        pivotOffset = Vector3.zero;
        return false;
    }
    public bool GetAssociatedDeviceMatchableNames(string nameToMatch, out BasisDeviceMatchableNames BasisDeviceMatchableNames)
    {
        foreach (BasisDeviceMatchableNames DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                BasisDeviceMatchableNames = DeviceEntry;
                return true;
            }
        }
        BasisDeviceMatchableNames = null;
        return false;
    }
}
