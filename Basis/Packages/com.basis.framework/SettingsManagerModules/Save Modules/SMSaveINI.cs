using BattlePhaze.SaveSystem;
using BattlePhaze.SettingsManager.DebugSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BattlePhaze.SettingsManager
{
    public class SMSaveINI : SMSaveModuleBase
    {
        private string FileExtension = ".ini";
        public string GetCurrentFilePath(SettingsManager Manager)
        {
            return Path.Combine(Application.persistentDataPath, Manager.ManagerSettings.FileName + FileExtension);
        }
        public override bool Delete(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            if (Manager != null && Save != null)
            {
                File.Delete(GetCurrentFilePath(Manager));
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Load(SettingsManager manager, SettingsManagerSaveSystem save)
        {
            string filePath = GetCurrentFilePath(manager);
            if (!File.Exists(filePath))
            {
                return false;
            }
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    save.Clear();
                    string currentComment = string.Empty;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line.StartsWith("["))
                        {
                            int endIndex = line.IndexOf("]", StringComparison.Ordinal);

                            if (endIndex > 0)
                            {
                                save.CurrentGroup = line.Substring(1, endIndex - 1) + "/";
                            }
                            else
                            {
                                BasisDebug.LogError("Trailing ']' character not found in line: " + line);
                            }
                        }
                        else if (line.StartsWith("*") || line.StartsWith("#"))
                        {
                            currentComment = line.Substring(1).Trim();
                        }
                        else if (line.Contains("="))
                        {
                            string[] parts = line.Split('=');
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (value.Length >= 2 && value[0] == '\"' && value[value.Length - 1] == '\"')
                            {
                                value = value.Substring(1, value.Length - 2);
                            }

                            save.Set(key, value, currentComment);
                            currentComment = string.Empty;
                        }
                    }

                    save.CurrentGroup = string.Empty;
                }
            }
            catch (IOException e)
            {
                BasisDebug.LogError("Impossible to open file: " + manager.ManagerSettings.FileName + ".ini");
                Debug.LogWarning(e);
                return false;
            }

            BasisDebug.Log("Loading Data INI");
            return true;
        }

        public override string ModuleName()
        {
            return "INI";
        }
        public override bool Save(SettingsManager manager, SettingsManagerSaveSystem save)
        {
            Encoding encoding = Encoding.UTF8;
            string filePath = GetCurrentFilePath(manager);
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
                {
                    string currentGroup = string.Empty;
                    foreach (KeyValuePair<string, SMOptionInformation> pair in save.OptionMapping)
                    {
                        string key = pair.Key;
                        SMOptionInformation info = pair.Value;
                        string group = string.Empty;
                        int index = key.LastIndexOf("/");
                        if (index >= 0)
                        {
                            group = key.Substring(0, index);
                            key = key.Substring(index + 1);
                        }
                        if (currentGroup != group)
                        {
                            currentGroup = group;
                            writer.WriteLine();
                            writer.WriteLine($"[{currentGroup}]");
                        }
                        writer.WriteLine($"* {info.comment}");
                        string value = info.value;
                        if (value.StartsWith(" ") || value.StartsWith("\t") || value.EndsWith(" ") || value.EndsWith("\t"))
                        {
                            value = "\\ + value +";
                        }
                        writer.WriteLine($"{key} = {value}");
                    }
                }
            }
            catch (IOException e)
            {
                BasisDebug.LogError($"Failed to save settings file: {filePath}");
                Debug.LogWarning(e);
                return false;
            }
            return true;
        }
        public override string Location(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            return GetCurrentFilePath(Manager);
        }
        public override SaveSystemType Type()
        {
            return SaveSystemType.Normal;
        }
    }
}
