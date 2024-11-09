using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
[CreateAssetMenu(fileName = "NewAvatarLerpData", menuName = "Avatar Lerp Data")]
public class BasisAvatarLerpDataSettings : ScriptableObject
{
    public float TeleportDistanceSquared = 15;
    public float LerpSpeedMovement = 20;
    public float LerpSpeedRotation = 20;
    public float LerpSpeedMuscles = 20;
}
}