using System.IO;
using UnityEngine;

namespace Basis.Scripts.Common
{
    public static class BasisDataStore
    {
        // Method to save the string to a file using JSON
        public static void SaveString(string stringContents, string fileNameAndExtension)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            // Convert the string to a JSON string
            string json = JsonUtility.ToJson(new BasisSavedString(stringContents));

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);

            Debug.Log("String saved to " + filePath);
        }

        // Method to load the string from a file using JSON
        public static string LoadString(string fileNameAndExtension, string defaultValue)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Read the JSON string from the file
                string json = File.ReadAllText(filePath);

                // Convert the JSON string back to a string
                BasisSavedString stringWrapper = JsonUtility.FromJson<BasisSavedString>(json);
                string stringValue = stringWrapper.ToValue();

                Debug.Log("String loaded from " + filePath);
                return stringValue;
            }
            else
            {
                Debug.LogWarning("File not found at " + filePath);
                return defaultValue;
            }
        }

        // Method to save the avatar (string and byte) to a file using JSON
        public static void SaveAvatar(string avatarName, byte avatarData, string fileNameAndExtension)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            // Convert the avatar (string and byte) to a JSON string
            string json = JsonUtility.ToJson(new BasisSavedAvatar(avatarName, avatarData));

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);

            Debug.Log("Avatar saved to " + filePath);
        }

        // Method to load the avatar (string and byte) from a file using JSON
        public static (string, byte) LoadAvatar(string fileNameAndExtension, string defaultName, byte defaultData)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Read the JSON string from the file
                string json = File.ReadAllText(filePath);

                // Convert the JSON string back to an avatar (string and byte)
                BasisSavedAvatar avatarWrapper = JsonUtility.FromJson<BasisSavedAvatar>(json);
                (string avatarName, byte avatarData) = avatarWrapper.ToValue();
                if(string.IsNullOrEmpty(avatarName))
                {
                    avatarName = defaultName;
                }
                Debug.Log("Avatar loaded from " + filePath);
                return (avatarName, avatarData);
            }
            else
            {
                Debug.LogWarning("File not found at " + filePath);
                return (defaultName, defaultData);
            }
        }

        // Wrapper class for serializing and deserializing the string
        [System.Serializable]
        private class BasisSavedString
        {
            public string String;

            public BasisSavedString(string saveString)
            {
                String = saveString;
            }

            public string ToValue()
            {
                return String;
            }
        }

        // Wrapper class for serializing and deserializing the avatar (string and byte)
        [System.Serializable]
        private class BasisSavedAvatar
        {
            public string Name;
            public byte Data;

            public BasisSavedAvatar(string name, byte data)
            {
                Name = name;
                Data = data;
            }

            public (string, byte) ToValue()
            {
                return (Name, Data);
            }
        }
    }
}