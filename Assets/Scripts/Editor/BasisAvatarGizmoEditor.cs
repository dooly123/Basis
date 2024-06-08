using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public class BasisAvatarGizmoEditor : MonoBehaviour
{
    public static void UpdateGizmos(BasisAvatarSDKInspector inspector, BasisAvatar avatar)
    {
        if (inspector == null)
        {
            Debug.LogError("Inspector was null!");
            return;
        }

        if (avatar == null || avatar.Animator == null)
        {
            Debug.LogError("Avatar or its Animator was null!");
            return;
        }

        if (BasisHelpers.TryGetFloor(avatar.Animator, out Vector3 bottom))
        {
            Vector2 previousAvatarEyePosition = avatar.AvatarEyePosition;
            Vector2 previousAvatarMouthPosition = avatar.AvatarMouthPosition;
            UpdateAvatarPosition(ref avatar.AvatarEyePosition, inspector.AvatarEyePositionState, avatar.transform.rotation, bottom, previousAvatarEyePosition, AvatarPathConstants.avatarEyePositionField, inspector.uiElementsRoot, avatar);
            UpdateAvatarPosition(ref avatar.AvatarMouthPosition, inspector.AvatarMouthPositionState, avatar.transform.rotation, bottom, previousAvatarMouthPosition, AvatarPathConstants.avatarMouthPositionField, inspector.uiElementsRoot, avatar);
        }
    }

    private static void UpdateAvatarPosition(ref Vector2 avatarPosition, bool positionState, Quaternion rotation, Vector3 bottom, Vector2 previousPosition, string positionField, VisualElement uiElementsRoot, BasisAvatar avatar)
    {
        if (!positionState)
        {
            return;
        }

        Vector3 ConvertedToVector3 = BasisHelpers.AvatarPositionConversion(avatarPosition);

        Vector3 worldSpaceAvatarPosition = BasisHelpers.ConvertFromLocalSpace(ConvertedToVector3, bottom);
        BasisHelpersGizmo.PositionHandler(ref worldSpaceAvatarPosition, rotation);
        Vector3 convertedPosition = BasisHelpers.ConvertToLocalSpace(worldSpaceAvatarPosition, bottom);
        avatarPosition = BasisHelpers.AvatarPositionConversion(convertedPosition);
        if (avatarPosition != previousPosition)
        {
            BasisHelpersGizmo.SetValueVector2Field(uiElementsRoot, positionField, avatarPosition);
            EditorUtility.SetDirty(avatar);
        }
    }
}