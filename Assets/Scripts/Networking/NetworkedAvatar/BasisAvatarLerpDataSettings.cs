using UnityEngine;
[CreateAssetMenu(fileName = "NewAvatarLerpData", menuName = "Avatar Lerp Data")]
public class BasisAvatarLerpDataSettings : ScriptableObject
{
    public float TeleportDistance = 15;
    public float LerpSpeedMovement = 20;
    public float LerpSpeedRotation = 20;
    public float LerpSpeedMuscles = 20;
}