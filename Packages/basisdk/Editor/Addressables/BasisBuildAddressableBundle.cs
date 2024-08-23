using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;

public static class BasisBuildAddressableBundle
{
    public static void ConfigureProfile(AddressableAssetSettings settings, BasisAddressableData Build)
    {
        try
        {
            Debug.Log($"Starting profile creation or retrieval for build: {Build.UniqueIdentifier}");

            string Value = settings.profileSettings.GetProfileName(Build.UniqueIdentifier);
            if (String.IsNullOrEmpty(Value))
            {
                Debug.Log($"Profile {Build.UniqueIdentifier} does not exist. Creating a new profile.");
                settings.activeProfileId = settings.profileSettings.AddProfile(Build.UniqueIdentifier, Build.DefaultProfileName);
            }
            else
            {
                Debug.Log($"Profile {Build.UniqueIdentifier} exists. Retrieving profile ID.");
                settings.activeProfileId = settings.profileSettings.GetProfileId(Value);
            }

            Build.ProfileSettings = settings.profileSettings;
            Build.AssetSettings = settings;

            settings.profileSettings.CreateValue(Build.RemoteBuildPath, Build.RemoteBuildLocation);
            settings.profileSettings.CreateValue(Build.RemoteLoadPath, Build.RemoteLoadLocation);
            settings.profileSettings.SetValue(settings.activeProfileId, Build.RemoteBuildPath, Build.RemoteBuildLocation);
            settings.profileSettings.SetValue(settings.activeProfileId, Build.RemoteLoadPath, Build.RemoteLoadLocation);

            Debug.Log($"Profile {Build.UniqueIdentifier} has been set successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in CreateOrGetSetProfile: {ex.Message}");
        }
    }

    public static void NukeCreateGroup(BasisAddressableData Build, AddressableAssetSettings settings)
    {
        try
        {
            Debug.Log($"Starting group creation for build: {Build.UniqueIdentifier}");

            Build.Group = settings.FindGroup(Build.UniqueIdentifier);
            if (Build.Group != null)
            {
                Debug.Log($"Group {Build.UniqueIdentifier} exists. Removing existing group.");
                settings.RemoveGroup(Build.Group);
            }

            Build.Group = settings.CreateGroup(Build.UniqueIdentifier, true, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            Debug.Log($"Group {Build.UniqueIdentifier} created.");

            for (int groupsIndex = 0; groupsIndex < settings.groups.Count; groupsIndex++)
            {
                if (Build.Group != settings.groups[groupsIndex])
                {
                    Debug.Log($"Removing group: {settings.groups[groupsIndex].Name}");
                    settings.RemoveGroup(settings.groups[groupsIndex]);
                }
            }

            Build.Group.Settings.BuildRemoteCatalog = true;
            Build.Group.Settings.OptimizeCatalogSize = true;
            Build.Group.Settings.RemoteCatalogBuildPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, Build.RemoteBuildPath);
            Build.Group.Settings.RemoteCatalogLoadPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, Build.RemoteLoadPath);

            var SchemeData = Build.Group.GetSchema<BundledAssetGroupSchema>();
            SchemeData.BuildPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, Build.RemoteBuildPath);
            SchemeData.LoadPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, Build.RemoteLoadPath);
            SchemeData.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            SchemeData.IncludeLabelsInCatalog = true;
            SchemeData.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
            SchemeData.ForceUniqueProvider = true;

            EditorUtility.SetDirty(SchemeData);
            Debug.Log($"Group {Build.UniqueIdentifier} configured successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in NukeCreateGroup: {ex.Message}");
        }
    }

    public static void BuildContent(AddressableAssetSettings settings, BasisAddressableData Build)
    {
        // try
        //{
        Debug.Log($"Starting content build for build: {Build.UniqueIdentifier}");

        IDataBuilder builder = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Build.BuildScript) as IDataBuilder;
        settings.ActivePlayerDataBuilderIndex = settings.DataBuilders.IndexOf((ScriptableObject)builder);
        AddressableAssetSettings.BuildPlayerContent(out Build.Result);

        if (string.IsNullOrEmpty(Build.Result.Error))
        {
            Debug.Log($"Content build successful for build: {Build.UniqueIdentifier}");
            List<string> ActiveList = new List<string>(Build.Result.FileRegistry.GetFilePaths());

            for (int FilesIndex = 0; FilesIndex < ActiveList.Count; FilesIndex++)
            {
                string Extension = Path.GetExtension(ActiveList[FilesIndex]);
                foreach (string FileName in BasisAddressableRenameExtensions.Renameable)
                {
                    if (Extension == FileName)
                    {
                        Debug.Log($"Renaming file: {ActiveList[FilesIndex]}");
                        Rename(ActiveList[FilesIndex], Build.UniqueIdentifier + Build.UniqueIdentifier);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Build content error: {Build.Result.Error}");
        }
        //  }
        //  catch (Exception ex)
        //  {
        //     Debug.LogError($"Error in BuildContent: {ex.Message}");
        // }
    }

    public static void Rename(string filePath, string newFileName)
    {
        try
        {
            string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName + Path.GetExtension(filePath));
            if (File.Exists(newFilePath))
            {
                Debug.LogWarning($"File {newFilePath} already exists. Deleting existing file.");
                File.Delete(newFilePath);
            }
            File.Move(filePath, newFilePath);
            Debug.Log($"File renamed from {filePath} to {newFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in Rename: {ex.Message}");
        }
    }

    public static void AddAssetToGroup(BasisAddressableData Build)
    {
        try
        {
            Debug.Log($"Adding assets to group: {Build.Group.Name}");

            for (int PathIndex = 0; PathIndex < Build.LocationFiles.Count; PathIndex++)
            {
                if (!string.IsNullOrEmpty(Build.LocationFiles[PathIndex]))
                {

                    if (BasisAddressableRenameExtensions.IsApproved(Build.LocationFiles[PathIndex]))
                    {
                        AddressableAssetEntry entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(Build.LocationFiles[PathIndex]), Build.Group, false, true);
                        if (entry != null)
                        {
                            SetLabelName(entry, Path.GetExtension(entry.AssetPath), true, Build);
                            Debug.Log($"Asset {Build.LocationFiles[PathIndex]} added to group with label {Path.GetExtension(entry.AssetPath)}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to add asset {Build.LocationFiles[PathIndex]} to group.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"File path at index {PathIndex} is empty or null.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in AddAssetToGroup: {ex.Message}");
        }
    }

    public static void SetLabelName(AddressableAssetEntry Entry, string Label, bool enabled, BasisAddressableData Build)
    {
        try
        {
            if (!Build.AssetSettings.GetLabels().Contains(Label))
            {
                Build.AssetSettings.AddLabel(Label);
                Debug.Log($"Label {Label} added to asset settings.");
            }
            Entry.SetLabel(Label, enabled, false, false);
            Debug.Log($"Label {Label} set on asset entry.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in SetLabelName: {ex.Message}");
        }
    }

    public static AddressableAssetSettings CreateOrGetDefaultObjectSettings()
    {
        try
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                Debug.Log("AddressableAssetSettings not found. Creating new settings.");
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
            }
            return AddressableAssetSettingsDefaultObject.Settings;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in CreateOrGetDefaultObjectSettings: {ex.Message}");
            return null;
        }
    }
}