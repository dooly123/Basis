using UnityEngine;

[System.Serializable]
public struct BasisCalibratedOffsetData
{
    public bool Use;
    public Quaternion OffsetRotation;
    public Vector3 OffsetPosition;
}