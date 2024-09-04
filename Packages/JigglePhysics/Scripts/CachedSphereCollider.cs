using System;
using System.Collections.Generic;
using UnityEngine;

namespace JigglePhysics
{
    public static class CachedSphereCollider
    {
        // Define constants for the layers you want to include and exclude
        public const int DefaultLayer = 0;  // Default layer index

        // Layer mask for including just the default and player layers
        public static readonly int IncludeDefaultPlayer = LayerMask.GetMask("Default");

        // Layer mask for excluding everything but default
        public static readonly int ExcludeDefaultPlayer = ~LayerMask.GetMask("Default");

        // You can add more layer masks as needed for other layers
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
        public static void RemoveBuilder(JiggleRigBuilder builder)
        {
            builders.Remove(builder);
        }
        public static void StartPass()
        {
            if ((remainingBuilders <= -1 || remainingBuilders >= builders.Count) && TryGet(out SphereCollider collider))
            {
                collider.includeLayers = 0;//include just default player 
                collider.excludeLayers = -1;//everything but default
                remainingBuilders = 0;
            }
        }
        public static void FinishedPass()
        {
            remainingBuilders++;
            if (remainingBuilders >= builders.Count && TryGet(out SphereCollider collider))
            {
                collider.includeLayers = -1;//everything but default
                collider.excludeLayers = 0;//include just default player
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