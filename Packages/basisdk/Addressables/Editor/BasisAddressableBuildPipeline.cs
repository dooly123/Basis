using System.IO;
using System;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BasisAddressableBuildPipeline
{
    private const string TempStorageLocation = "Packages/com.basis.basisdk/TemporaryStorage/";
    private const string UnityExtension = ".unity";
    private const string PrefabExtension = ".prefab";
    private const string BasisPrefix = "basis://";
    private const string AddressablesPath = "Addressables/";
    private const string ServerDataPath = "ServerData/";

    public static BasisAddressableData CreateBasisAddressable()
    {
        return new BasisAddressableData
        {
            UniqueIdentifier = CreateUniqueIdentifier(),
        };
    }

    private static string CreateUniqueIdentifier()
    {
        var guid = Guid.NewGuid();
        var timestamp = DateTime.UtcNow.Ticks;
        var random = new System.Random().Next(100000, 999999);
        return $"{guid}-{timestamp}-{random}";
    }

    // Method for handling scenes
    public static void CreateAddressableForScene(BuildTarget target, Scene scene)
    {
        if (scene == null)
        {
            throw new ArgumentNullException(nameof(scene), "Scene cannot be null.");
        }

        var addressableData = CreateBasisAddressable();
        var addressableAssetSettings = BasisBuildAddressableBundle.CreateOrGetDefaultObjectSettings();

        SaveCurrentScene(scene);
        ProcessAddressable(addressableAssetSettings, addressableData, target, scene.path, 1);
    }

    // Method for handling prefabs
    public static void CreateAddressableForPrefab(BuildTarget target, BasisContentBase data, GameObject prefab)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
        }

        var addressableData = CreateBasisAddressable();
        var addressableAssetSettings = BasisBuildAddressableBundle.CreateOrGetDefaultObjectSettings();

        CreateTemporaryPrefab(data, prefab, out var projectReference, out string assetLocal);
        ProcessAddressable(addressableAssetSettings, addressableData, target, assetLocal, 0);
        AssetDatabase.DeleteAsset(assetLocal);
    }

    private static void ProcessAddressable(AddressableAssetSettings addressableAssetSettings, BasisAddressableData addressableData, BuildTarget target,string assetLocal, int maxScenes)
    {
        AddBuildContext(addressableAssetSettings, addressableData, target);
        AddFiles(addressableData, assetLocal);
        RemoveAdditionalScenes(addressableData, maxScenes);
        BuildContent(addressableData);
    }

    private static void SaveCurrentScene(Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void CreateTemporaryPrefab(BasisContentBase data, GameObject originalRef, out GameObject projectReference, out string assetLocal)
    {
        assetLocal = AssetDatabase.GenerateUniqueAssetPath(TempStorageLocation + data.name + PrefabExtension);
        AssetDatabase.DeleteAsset(assetLocal);

        if (PrefabUtility.IsPartOfAnyPrefab(originalRef))
        {
            originalRef = GameObject.Instantiate(originalRef);
            projectReference = PrefabUtility.SaveAsPrefabAsset(originalRef, assetLocal, out _);
            GameObject.DestroyImmediate(originalRef);
        }
        else
        {
            projectReference = PrefabUtility.SaveAsPrefabAsset(originalRef, assetLocal, out _);
        }
    }

    private static void AddFiles(BasisAddressableData build, string assetLocal)
    {
        FileUtil.DeleteFileOrDirectory(build.RemoteBuildLocation);
        build.LocationFiles.Add(assetLocal);
        build.LocationFiles.AddRange(AssetDatabase.GetDependencies(assetLocal, true));
    }

    private static void RemoveAdditionalScenes(BasisAddressableData build, int maxScenes)
    {
        build.LocationFiles.RemoveAll(path => Path.GetExtension(path) == UnityExtension && build.LocationFiles.IndexOf(path) >= maxScenes);
    }

    private static void AddBuildContext(AddressableAssetSettings addressableAssetSettings, BasisAddressableData data, BuildTarget target)
    {
        data.RemoteBuildLocation = $"{ServerDataPath}{target}/";
        data.RemoteLoadLocation = $"{BasisPrefix}{AddressablesPath}{data.UniqueIdentifier}";
        BasisBuildAddressableBundle.ConfigureProfile(addressableAssetSettings, data);
        BasisBuildAddressableBundle.NukeCreateGroup(data, data.AssetSettings);
    }

    private static void BuildContent(BasisAddressableData data)
    {
        BasisBuildAddressableBundle.AddAssetToGroup(data);
        MarkAssetsDirty(data);
        BasisBuildAddressableBundle.BuildContent(data.AssetSettings, data);
    }

    private static void MarkAssetsDirty(BasisAddressableData data)
    {
        EditorUtility.SetDirty(data.Group);
        EditorUtility.SetDirty(AddressableAssetSettingsDefaultObject.Settings);
        AssetDatabase.SaveAssets();
    }
}