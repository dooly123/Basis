using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasisDeviceMatchableNames
{
    public string DeviceID;
    public List<string> MatchableDeviceIds = new List<string>();
    public Vector3 PivotOffset;
    public Vector3 RotationOffset;
    public bool HasRayCastSupport;
    public bool HasRepresentation = false;
    public bool ShowRayCast;
    public Vector3 PivotRaycastOffset;
    public Vector3 RotationRaycastOffset;
}