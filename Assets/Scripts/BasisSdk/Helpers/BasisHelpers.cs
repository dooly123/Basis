using UnityEngine;

public static class BasisHelpers
{
    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        if (!gameObject.TryGetComponent(out T component))
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public static bool CheckInstance<T>(T component) where T : Component
    {
        if (component != null)
        {
            Debug.LogError("Instance already exists of " + typeof(T).Name);
            return false;
        }
        return true;
    }

    public static Vector3 ScaleVector(Vector3 vector, float scaleFactor = 1.6f)
    {
        return vector * scaleFactor;
    }

    public static bool TryCheckOrAttempt<T>(GameObject gameObject, ref T component) where T : Component
    {
        if (component != null)
        {
            Debug.Log("Already found component " + component.GetType().Name);
            return true;
        }
        if (gameObject.TryGetComponent(out component))
        {
            return true;
        }
        return false;
    }
    public static bool TryGetTransformBone(Animator animator, HumanBodyBones bone, out Transform boneTransform)
    {
        boneTransform = animator.GetBoneTransform(bone);
        return boneTransform != null;
    }

    public static Vector3 ConvertToLocalSpace(Vector3 notFloorPosition, Vector3 floorPosition)
    {
        return notFloorPosition - floorPosition;
    }

    public static Vector3 ConvertFromLocalSpace(Vector3 notFloorPosition, Vector3 floorPosition)
    {
        return notFloorPosition + floorPosition;
    }

    public static bool TryGetFloor(Animator animator, out Vector3 bottom)
    {
        if (TryGetVector3Bone(animator, HumanBodyBones.LeftFoot, out Vector3 leftFoot) && TryGetVector3Bone(animator, HumanBodyBones.RightFoot, out Vector3 rightFoot))
        {
            bottom = Vector3.Lerp(leftFoot, rightFoot, 0.5f);
            return true;
        }
        else
        {
            bottom = Vector3.zero;
            return false;
        }
    }
    public static bool TryGetVector3Bone(Animator animator, HumanBodyBones bone, out Vector3 position)
    {
        if (animator.avatar != null && animator.avatar.isHuman)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                position = boneTransform.position;
                return true;
            }
            else
            {
                position = Vector3.zero;
                return false;
            }
        }
        position = Vector3.zero;
        return false;
    }

    public static Vector3 AvatarPositionConversion(Vector2 input)
    {
        return new Vector3(0, input.x, input.y);
    }

    public static Vector2 AvatarPositionConversion(Vector3 input)
    {
        return new Vector2(input.y, input.z);
    }
}