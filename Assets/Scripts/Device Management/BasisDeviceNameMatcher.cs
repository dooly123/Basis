using System.Collections.Generic;
using UnityEngine;

namespace Basis.Scripts.Device_Management
{
[CreateAssetMenu(fileName = "BasisDeviceNameMatcher", menuName = "Basis/BasisDeviceNameMatcher", order = 1)]
public class BasisDeviceNameMatcher : ScriptableObject
{
    public List<BasisDeviceMatchableNames> BasisDevice = new List<BasisDeviceMatchableNames>();
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
}