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
}
