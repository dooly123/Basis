using UnityEngine;

public static class BasisAvatarLerp
{
    public static string Settings = "Assets/ScriptableObjects/Avatar Lerp Data.asset";
    public static void UpdateAvatar(ref BasisAvatarData Output, BasisAvatarData Target, float PositionLerp, float RotationLerp, float MuscleLerp, float teleportThreshold)
    {
        UpdateAvatarPosition(ref Output, Target, PositionLerp, teleportThreshold);
        UpdateAvatarRotation(ref Output, Target, RotationLerp);
        UpdateAvatarMuscles(ref Output, Target, MuscleLerp);
    }
    public static void UpdateAvatarPosition(ref BasisAvatarData Output, BasisAvatarData Target, float LerpTime, float teleportThreshold)
    {
        // Calculate the distance between current and target positions
        float distance = Vector3.Distance(Output.PlayerPosition, Target.PlayerPosition);

        // If distance is larger than threshold, teleport the avatar
        if (distance > teleportThreshold)
        {
            Output.PlayerPosition = Target.PlayerPosition;
            Output.BodyPosition = Target.BodyPosition;
            Output.Scale = Target.Scale;
        }
        else
        {
            // Otherwise, perform interpolation
            Output.PlayerPosition = Vector3.Lerp(Output.PlayerPosition, Target.PlayerPosition, LerpTime);
            Output.BodyPosition = Vector3.Lerp(Output.BodyPosition, Target.BodyPosition, LerpTime);
            Output.Scale = Vector3.Lerp(Output.Scale, Target.Scale, LerpTime);
        }
    }

    public static void UpdateAvatarRotation(ref BasisAvatarData Output, BasisAvatarData Target, float LerpTime)
    {
        Output.Rotation = Quaternion.Slerp(Output.Rotation, Target.Rotation, LerpTime);
    }
    public static void UpdateAvatarMuscles(ref BasisAvatarData Output, BasisAvatarData Target,float LerpTime)
    {
        for (int index = 0; index < 95; index++)
        {
            Output.Muscles[index] = Mathf.Lerp(Output.Muscles[index], Target.Muscles[index], LerpTime);
        }
    }
}
