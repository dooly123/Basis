#if BASISNDMF_NDMF_IS_INSTALLED
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace HVR.Basis.NDMF
{
    [InitializeOnLoad]
    internal class BasisNDMFBuildHook
    {
        static BasisNDMFBuildHook()
        {
            BasisPrefabAssetBundleBuilder.preProcessor = BasisAvatarPrefabProcessor;
        }

        private static GameObject BasisAvatarPrefabProcessor(GameObject copy)
        {
            AvatarProcessor.ProcessAvatar(copy);
            return copy;
        }
    }
}
#endif