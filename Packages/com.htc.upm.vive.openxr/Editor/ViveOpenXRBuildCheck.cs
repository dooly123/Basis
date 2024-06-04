// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management.Metadata;

namespace VIVE.OpenXR.Editor
{
	[InitializeOnLoad]
	public static class CheckIfVIVEEnabled
	{
		const string LOG_TAG = "VIVE.OpenXR.Editor.CheckIfVIVEEnabled";
		static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		const string VERSION_DEFINE_OPENXR = "USE_VIVE_OPENXR_1_0_0";
		internal struct ScriptingDefinedSettings
		{
			public string[] scriptingDefinedSymbols;
			public BuildTargetGroup[] targetGroups;

			public ScriptingDefinedSettings(string[] symbols, BuildTargetGroup[] groups)
			{
				scriptingDefinedSymbols = symbols;
				targetGroups = groups;
			}
		}
		static readonly ScriptingDefinedSettings m_ScriptDefineSettingOpenXRAndroid = new ScriptingDefinedSettings(
			new string[] { VERSION_DEFINE_OPENXR, },
			new BuildTargetGroup[] { BuildTargetGroup.Android, }
		);
		const string XR_LOADER_OPENXR_NAME = "UnityEngine.XR.OpenXR.OpenXRLoader";
		internal static bool ViveOpenXRAndroidAssigned { get { return XRPackageMetadataStore.IsLoaderAssigned(XR_LOADER_OPENXR_NAME, BuildTargetGroup.Android); } }
		static void AddScriptingDefineSymbols(ScriptingDefinedSettings setting)
		{
			for (int group_index = 0; group_index < setting.targetGroups.Length; group_index++)
			{
				var group = setting.targetGroups[group_index];
				string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				List<string> allDefines = definesString.Split(';').ToList();
				for (int symbol_index = 0; symbol_index < setting.scriptingDefinedSymbols.Length; symbol_index++)
				{
					if (!allDefines.Contains(setting.scriptingDefinedSymbols[symbol_index]))
					{
						DEBUG("AddDefineSymbols() " + setting.scriptingDefinedSymbols[symbol_index] + " to group " + group);
						allDefines.Add(setting.scriptingDefinedSymbols[symbol_index]);
					}
					else
					{
						DEBUG("AddDefineSymbols() " + setting.scriptingDefinedSymbols[symbol_index] + " already existed.");
					}
				}
				PlayerSettings.SetScriptingDefineSymbolsForGroup(
					group,
					string.Join(";", allDefines.ToArray())
				);
			}
		}
		static void RemoveScriptingDefineSymbols(ScriptingDefinedSettings setting)
		{
			for (int group_index = 0; group_index < setting.targetGroups.Length; group_index++)
			{
				var group = setting.targetGroups[group_index];
				string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				List<string> allDefines = definesString.Split(';').ToList();
				for (int symbol_index = 0; symbol_index < setting.scriptingDefinedSymbols.Length; symbol_index++)
				{
					if (allDefines.Contains(setting.scriptingDefinedSymbols[symbol_index]))
					{
						DEBUG("RemoveDefineSymbols() " + setting.scriptingDefinedSymbols[symbol_index] + " from group " + group);
						allDefines.Remove(setting.scriptingDefinedSymbols[symbol_index]);
					}
					else
					{
						DEBUG("RemoveDefineSymbols() " + setting.scriptingDefinedSymbols[symbol_index] + " already existed.");
					}
				}
				PlayerSettings.SetScriptingDefineSymbolsForGroup(
					group,
					string.Join(";", allDefines.ToArray())
				);
			}
		}
		static bool HasScriptingDefineSymbols(ScriptingDefinedSettings setting)
		{
			for (int group_index = 0; group_index < setting.targetGroups.Length; group_index++)
			{
				var group = setting.targetGroups[group_index];
				string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				List<string> allDefines = definesString.Split(';').ToList();
				for (int symbol_index = 0; symbol_index < setting.scriptingDefinedSymbols.Length; symbol_index++)
				{
					if (!allDefines.Contains(setting.scriptingDefinedSymbols[symbol_index]))
					{
						return false;
					}
				}
			}

			return true;
		}
		static void CheckScriptingDefineSymbols()
		{
			// Adds the script symbol if Vive OpenXR Plugin - Android is imported and assigned in XR Plugin-in Management.
			if (ViveOpenXRAndroidAssigned)
			{
				if (!HasScriptingDefineSymbols(m_ScriptDefineSettingOpenXRAndroid))
				{
					DEBUG("OnUpdate() Adds m_ScriptDefineSettingOpenXRAndroid.");
					AddScriptingDefineSymbols(m_ScriptDefineSettingOpenXRAndroid);
				}
			}
			// Removes the script symbol if Vive OpenXR Plugin - Android is uninstalled.
			else
			{
				if (HasScriptingDefineSymbols(m_ScriptDefineSettingOpenXRAndroid))
				{
					DEBUG("OnUpdate() Removes m_ScriptDefineSettingOpenXRAndroid.");
					RemoveScriptingDefineSymbols(m_ScriptDefineSettingOpenXRAndroid);
				}
			}
		}
		static void OnUpdate()
		{
			//CheckScriptingDefineSymbols();
		}
		static CheckIfVIVEEnabled()
		{
			EditorApplication.update += OnUpdate;
		}
	}
}
#endif