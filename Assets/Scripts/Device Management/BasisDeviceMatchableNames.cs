using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasisDeviceMatchableNames
{
    public string DeviceID;
    public List<string> MatchableDeviceIds = new List<string>();
    public Vector3 PivotOffset;
    public Vector3 RotationOffset;
}