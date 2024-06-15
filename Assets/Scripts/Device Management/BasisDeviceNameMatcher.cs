using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasisDeviceNameMatcher", menuName = "Basis/BasisDeviceNameMatcher", order = 1)]
public class BasisDeviceNameMatcher : ScriptableObject
{
    public List<BasisDeviceMatchableNames> BasisDevice = new List<BasisDeviceMatchableNames>();
    public bool GetAssociatedDeviceID(string nameToMatch,out string DeviceID)
    {
        foreach (var DeviceEntry in BasisDevice)
        {
            if (DeviceEntry.MatchableDeviceIds.Contains(nameToMatch.ToLower()))
            {
                DeviceID= DeviceEntry.DeviceID;
                return true; 
            }
        }
        DeviceID = null;
        return false;
    }
    public bool GetAssociatedPivotOffset(string nameToMatch,out Vector3 pivotOffset)
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
    public bool GetAssociatedRotationOffset(string nameToMatch,out Vector3 rotationOffset)
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
}
