#if BASISNDMF_NDMF_IS_INSTALLED
using Basis.Scripts.BasisSdk;
using HVR.Basis.NDMF;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

[assembly: ExportsPlugin(typeof(BasisNDMFCreateTestAvatarPlugin))]
namespace HVR.Basis.NDMF
{
    public class BasisNDMFCreateTestAvatarPlugin : Plugin<BasisNDMFCreateTestAvatarPlugin>
    {
        private const string NameOfAddressableForTestAvatar = "TestLocalAvatar";
        private const string TestLocalAvatarPrefabLocation = "Assets/TestLocalAvatar.prefab";
        public override string QualifiedName => "HVR.Basis.NDMF.BasisNDMFCreateTestAvatarPlugin";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("nadena.dev.ndmf.InternalPasses")
                .Run("Basis NDMF: Make Addressable if Play Mode", MakeAddressableIfPlayMode);
        }

        private void MakeAddressableIfPlayMode(BuildContext context)
        {
            if (!Application.isPlaying) return;
            if (!context.AvatarRootObject.GetComponent<BasisAvatar>()) return;

            var asset = AssetDatabase.LoadAssetAtPath<Object>(TestLocalAvatarPrefabLocation);
            if (asset)
            {
                Object.Destroy(asset);
            }

            var completedAvatar = context.AvatarRootObject;
            
            // We need to create the addressable prefab only after the NDMF BuildContext finishes,
            // because the prefab needs to reference serialized assets.
            // NDMF does not support post-processors that run after the serialization of the avatar,
            // so we're delaying the call as a hack.
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying) return;
                
                StoreAvatarAsAddressable(completedAvatar);
            };
        }

        private static void StoreAvatarAsAddressable(GameObject completedAvatar)
        {
            // We don't want the AvatarActivator to trigger again when copying the avatar.
            completedAvatar.SetActive(false);
            var copy = Object.Instantiate(completedAvatar);
            
            try
            {
                var monoBehaviours = copy.GetComponents<MonoBehaviour>();
                foreach (var monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour && monoBehaviour.GetType().FullName == "nadena.dev.ndmf.runtime.AvatarActivator")
                    {
                        Object.Destroy(monoBehaviour);
                    }
                }

                var prefab = PrefabUtility.SaveAsPrefabAsset(copy, TestLocalAvatarPrefabLocation);
                
                var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                addressableSettings.CreateAssetReference(guid);
                var addressableAsset = addressableSettings.FindAssetEntry(guid);
                addressableAsset.address = NameOfAddressableForTestAvatar;
            }
            finally
            {
                Object.Destroy(copy);
            }
            
            completedAvatar.SetActive(true);
        }
    }
}
#endif