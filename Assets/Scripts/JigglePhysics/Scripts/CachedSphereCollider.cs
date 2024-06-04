using System;
using System.Collections.Generic;
using UnityEngine;

namespace JigglePhysics
{
    public static class CachedSphereCollider
    {
        private class DestroyListener : MonoBehaviour
        {
            void OnDestroy()
            {
                _hasSphere = false;
            }
        }
        private static int remainingBuilders = -1;
        private static bool _hasSphere = false;
        private static SphereCollider _sphereCollider;
        private static readonly HashSet<MonoBehaviour> builders = new HashSet<MonoBehaviour>();

        public static void AddBuilder(JiggleRigBuilder builder)
        {
            builders.Add(builder);
        }
        public static void AddSkin(JiggleSkin skin)
        {
            builders.Add(skin);
        }
        public static void RemoveSkin(JiggleSkin skin)
        {
            builders.Remove(skin);
        }
        public static void RemoveBuilder(JiggleRigBuilder builder)
        {
            builders.Remove(builder);
        }

        public static void StartPass()
        {
            if ((remainingBuilders <= -1 || remainingBuilders >= builders.Count) && TryGet(out SphereCollider collider))
            {
                collider.enabled = true;
                remainingBuilders = 0;
            }
        }

        public static void FinishedPass()
        {
            remainingBuilders++;
            if (remainingBuilders >= builders.Count && TryGet(out SphereCollider collider))
            {
                collider.enabled = false;
                remainingBuilders = -1;
            }
        }

        public static bool TryGet(out SphereCollider collider)
        {
            if (_hasSphere)
            {
                collider = _sphereCollider;
                return true;
            }
            try
            {
                var obj = new GameObject("JiggleBoneSphereCollider", typeof(SphereCollider), typeof(DestroyListener))
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(obj);
                }

                if (obj.TryGetComponent<SphereCollider>(out _sphereCollider))
                {
                    collider = _sphereCollider;
                    collider.enabled = false;
                    _hasSphere = true;
                    return true;
                }
                else
                {
                    new Exception("Missing Sphere Collider");
                    collider = null;
                    return false;
                }
            }
            catch
            {
                // Something went wrong! Try to clean up and try again next frame. Better throwing an expensive exception than spawning spheres every frame.
                if (_sphereCollider != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(_sphereCollider.gameObject);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(_sphereCollider.gameObject);
                    }
                }
                _hasSphere = false;
                collider = null;
                throw;
            }
        }
    }
}