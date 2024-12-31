using BattlePhaze.SaveSystem;
using System.Collections.Generic;
using UnityEngine;
namespace BattlePhaze.SettingsManager
{
    public class SMSavePlayerPref : SMSaveModuleBase
    {
        public override string ModuleName()
        {
            return "PlayerPref";
        }
        public override bool Load(SettingsManager manager, SettingsManagerSaveSystem save)
        {
            int count = PlayerPrefs.GetInt(manager.ManagerSettings.FileName + "_Count", 0);
            return LoadInternal(count, manager.ManagerSettings.FileName, save);
        }

        public override bool Save(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            return SaveInternal(Save.OptionMapping, Manager.ManagerSettings.FileName);
        }
        public bool LoadInternal(int OptionIndex, string FileName, SettingsManagerSaveSystem save)
        {
            BasisDebug.Log($"Loading Data Player Pref: {FileName}");
            for (int i = 0; i < OptionIndex; i++)
            {
                string key = PlayerPrefs.GetString($"{FileName}_Key{i}", string.Empty);
                string value = PlayerPrefs.GetString($"{FileName}_Key{i}_Value", string.Empty);
                string comment = PlayerPrefs.GetString($"{FileName}_Key{i}_Comment", string.Empty);
                save.Set(key, value, comment);
            }
            return true;
        }
        public bool SaveInternal(Dictionary<string, SMOptionInformation> sMOptionInformation, string fileName)
        {
            BasisDebug.Log("Saving Data to Player Prefs");
            PlayerPrefs.SetInt(fileName + "_Count", sMOptionInformation.Count);
            foreach (var option in sMOptionInformation)
            {
                string key = option.Key;
                string value = option.Value.value;
                string comment = option.Value.comment;
                PlayerPrefs.SetString(fileName + "_Key" + key, key);
                PlayerPrefs.SetString(fileName + "_Key" + key + "_Value", value);
                PlayerPrefs.SetString(fileName + "_Key" + key + "_Comment", comment);
            }
            PlayerPrefs.Save();
            BasisDebug.Log("Player Prefs saved");
            return true;
        }
        public void NukePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        public override bool Delete(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            if (Manager != null && Save != null)
            {
                DeleteInternal(Save.OptionMapping, Manager.ManagerSettings.FileName);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool DeleteInternal(Dictionary<string, SMOptionInformation> sMOptionInformation, string fileName)
        {
            BasisDebug.Log("Saving Data to Player Prefs");
            PlayerPrefs.SetInt(fileName + "_Count", sMOptionInformation.Count);
            foreach (var option in sMOptionInformation)
            {
                string key = option.Key;
                PlayerPrefs.DeleteKey(fileName + "_Key" + key);
                PlayerPrefs.DeleteKey(fileName + "_Key" + key + "_Value");
                PlayerPrefs.DeleteKey(fileName + "_Key" + key + "_Comment");
            }
            PlayerPrefs.Save();
            BasisDebug.Log("Player Prefs Deleted");
            return true;
        }

        public override string Location(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            return string.Empty;
        }
        public override SaveSystemType Type()
        {
            return SaveSystemType.Normal;
        }
    }
}
