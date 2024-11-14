using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.BasisSdk.Helpers
{
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

        public static Vector3 ConvertToLocalSpace(float3 notFloorPosition, float3 floorPosition)
        {
            return notFloorPosition - floorPosition;
        }

        public static Vector3 ConvertFromLocalSpace(float3 notFloorPosition, float3 floorPosition)
        {
            return notFloorPosition + floorPosition;
        }

        public static bool TryGetFloor(Animator animator, out float3 bottom)
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
        public static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
        // Extended sign: returns -1, 0 or 1 based on sign of a
        public static float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }
        // taken from http://www.terathon.com/code/oblique.html
        public static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4
            (sgn(clipPlane.x),
                sgn(clipPlane.y),
                1.0f,
                1.0f);

            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            // 第三行=剪切平面-第四行（third row = clip plane - fourth row）
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];
        }
        public static Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float ClipOffset, float sideSign = 1.0f)
        {
            Vector3 offsetPos = pos + normal.normalized * ClipOffset;
            Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
            Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal) * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}