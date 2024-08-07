using UnityEngine;

namespace Assets.Scripts.Common
{
[System.Serializable]
public struct BasisCalibratedOffsetData
{
    public bool Use;
    public Quaternion rotation;
    public Vector3 position;
}
}