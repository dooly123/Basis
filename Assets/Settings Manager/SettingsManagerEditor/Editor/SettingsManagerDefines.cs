namespace BattlePhaze.SettingsManager.EditorDefine
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using UnityEditor;
    public static class SettingsManagerDefines
    {
        public static void AddDefineIfNecessary(string define, BuildTargetGroup buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (string.IsNullOrEmpty(defines))
            {
                defines = define;
            }
            else if (!defines.Contains(define))
            {
                defines += ";" + define;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }

        public static bool CheckDefineExistence(string define, BuildTargetGroup buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return !string.IsNullOrEmpty(defines) && defines.Contains(define);
        }

        public static void RemoveDefineIfNecessary(string define, BuildTargetGroup buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (string.IsNullOrEmpty(defines)) return;

            var separator = new[] { ";" };
            var parts = defines.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var newParts = new List<string>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != define)
                {
                    newParts.Add(parts[i]);
                }
            }

            if (newParts.Count == 0)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Empty);
            }
            else
            {
                var newDefines = string.Join(";", newParts.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
            }
        }

        public static void DefineWarmup(SettingsManagerEnums.SupportedRenderPipelines renderPipeline)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            switch (renderPipeline)
            {
                case SettingsManagerEnums.SupportedRenderPipelines.HighDefinitionRenderPipeline:
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_LEGACY", buildTargetGroup);
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_UNIVERSAL", buildTargetGroup);
                    AddDefineIfNecessary("SETTINGS_MANAGER_HD", buildTargetGroup);
                    break;
                case SettingsManagerEnums.SupportedRenderPipelines.BuiltIn:
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_HD", buildTargetGroup);
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_UNIVERSAL", buildTargetGroup);
                    AddDefineIfNecessary("SETTINGS_MANAGER_LEGACY", buildTargetGroup);
                    break;
                case SettingsManagerEnums.SupportedRenderPipelines.UniversalRenderPipeline:
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_HD", buildTargetGroup);
                    RemoveDefineIfNecessary("SETTINGS_MANAGER_LEGACY", buildTargetGroup);
                    AddDefineIfNecessary("SETTINGS_MANAGER_UNIVERSAL", buildTargetGroup);
                    break;
            }
        }
        public static void IntegrationGenerate(string className, string defineName)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (defines.Contains(defineName))
            {
                DebugSystem.SettingsManagerDebug.Log($"{defineName} : Already exists.");
                return;
            }

            System.Type assemblyDomain = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
                                          from type in assembly.GetTypes()
                                          where type.Name == className
                                          select type).FirstOrDefault();

            if (assemblyDomain == null)
            {
                return;
            }

            AddDefineIfNecessary(defineName, EditorUserBuildSettings.selectedBuildTargetGroup);
            DebugSystem.SettingsManagerDebug.Log($"{defineName} : Was added to Defines!");
        }
    }
}