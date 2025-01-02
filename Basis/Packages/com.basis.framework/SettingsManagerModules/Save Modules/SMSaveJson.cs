using BattlePhaze.SaveSystem;
using BattlePhaze.SettingsManager.DebugSystem;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace BattlePhaze.SettingsManager
{
    public class SMSaveJson : SMSaveModuleBase
    {
        private string FileExtension = ".json";
        public string GetCurrentFilePath(SettingsManager Manager)
        {
            return Path.Combine(Application.persistentDataPath, Manager.ManagerSettings.FileName + FileExtension);
        }
        public override bool Load(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            string filePath = GetCurrentFilePath(Manager);
            if (File.Exists(filePath))
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        Save.Clear();
                        OptionMappings Mappings = JsonUtility.FromJson<OptionMappings>(reader.ReadToEnd());
                        if (Mappings.Information != null)
                        {
                            for (int MappingIndex = 0; MappingIndex < Mappings.Information.Length; MappingIndex++)
                            {
                                Save.Set(Mappings.Information[MappingIndex].key, Mappings.Information[MappingIndex].value, Mappings.Information[MappingIndex].comment);
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    BasisDebug.Log("Impossible to open file nuking : " + filePath);
                    BasisDebug.Log(e.StackTrace);
                    Delete(Manager, Save);
                    return false;
                }
            }
          //  BasisDebug.Log("Loading Data json");
            return true;
        }

        public override string ModuleName()
        {
            return "JSON";
        }
        [System.Serializable]
        public struct OptionMappings
        {
            [SerializeField]
            public SMOptionInformation[] Information;
        }
        public override bool Save(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            Encoding encoding = Encoding.UTF8;
            BasisDebug.Log("Saving Data JSON");
            string filePath = GetCurrentFilePath(Manager);
            try
            {
                OptionMappings mappings = new OptionMappings();
                using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
                {
                    try
                    {
                        mappings.Information = Save.OptionMapping.Values.ToArray();
                        string Data = JsonUtility.ToJson(mappings);
                        writer.Write(Data);
                    }
                    catch (Exception ex)
                    {
                        BasisDebug.LogError("Error saving file: " + ex.Message);
                    }
                }
            }
            catch (IOException e)
            {
                BasisDebug.LogError("Impossible to save file: " + GetCurrentFilePath(Manager));
                Debug.LogWarning(e);
                return false;
            }
            return true;
        }

        public override bool Delete(SettingsManager Manager, SettingsManagerSaveSystem Save)
        {
            if (Manager != null && Save != null)
            {
                File.Delete(GetCurrentFilePath(Manager));
            }
            else
            {
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
