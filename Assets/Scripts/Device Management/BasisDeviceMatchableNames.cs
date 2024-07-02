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
    public bool HasRayCastVisual;
    public bool HasRayCastRedical;

    public Vector3 PivotRaycastOffset;
    public Vector3 RotationRaycastOffset;
    //useful for things like the avatars hands
    public Vector3 AvatarPositionOffset;
    public Vector3 AvatarRotationOffset;
}