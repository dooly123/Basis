using Basis.Scripts.BasisSdk;
using UnityEngine;

namespace HVR.Basis.Comms
{
    internal class CommsUtil
    {
        internal static T GetOrCreateSceneInstance<T>(ref T instance) where T : Component
        {
            if (instance != null) return instance;

            var go = new GameObject($"HVR.{typeof(T).Name}");
            Object.DontDestroyOnLoad(go);
            instance = go.AddComponent<T>();

            return instance;
        }

        internal static BasisAvatar GetAvatar(Component component)
        {
            return component.GetComponentInParent<BasisAvatar>(true);
        }

        internal static FeatureNetworking FeatureNetworkingFromAvatar(BasisAvatar avatar)
        {
            return avatar.GetComponentInChildren<FeatureNetworking>(true);
        }
    }
}