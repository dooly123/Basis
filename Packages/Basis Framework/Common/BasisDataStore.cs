using System.Globalization;
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
            string json = JsonUtility.ToJson(new BasisSavedString(stringContents));
            File.WriteAllText(filePath, json);
            Debug.Log("String saved to " + filePath);
        }

        // Method to load the string from a file using JSON
        public static string LoadString(string fileNameAndExtension, string defaultValue)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                BasisSavedString stringWrapper = JsonUtility.FromJson<BasisSavedString>(json);
                Debug.Log("String loaded from " + filePath);
                return stringWrapper.ToValue();
            }
            else
            {
                Debug.LogWarning("File not found at " + filePath);
                return defaultValue;
            }
        }

        // Method to save an int value to a file
        public static void SaveInt(int intValue, string fileNameAndExtension)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            string json = JsonUtility.ToJson(new BasisSavedInt(intValue));
            File.WriteAllText(filePath, json);
            Debug.Log("Int saved to " + filePath);
        }

        // Method to load an int value from a file
        public static int LoadInt(string fileNameAndExtension, int defaultValue)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                BasisSavedInt intWrapper = JsonUtility.FromJson<BasisSavedInt>(json);
                Debug.Log("Int loaded from " + filePath);
                return intWrapper.ToValue();
            }
            else
            {
                Debug.LogWarning("File not found at " + filePath);
                return defaultValue;
            }
        }

        // Method to save a float value to a file using invariant culture
        public static void SaveFloat(float floatValue, string fileNameAndExtension)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            string json = JsonUtility.ToJson(new BasisSavedFloat(floatValue.ToString(CultureInfo.InvariantCulture)));
            File.WriteAllText(filePath, json);
            Debug.Log("Float saved to " + filePath);
        }

        // Method to load a float value from a file using invariant culture
        public static bool LoadFloat(string fileNameAndExtension, float defaultValue,out float returningValue)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                BasisSavedFloat floatWrapper = JsonUtility.FromJson<BasisSavedFloat>(json);
                if (float.TryParse(floatWrapper.ToValue(), NumberStyles.Float, CultureInfo.InvariantCulture, out float loadedFloat))
                {
                    Debug.Log("Float loaded from " + filePath);
                    returningValue = loadedFloat;
                    return true;
                }
                else
                {
                    Debug.LogWarning("Failed to parse float, returning default value.");
                    returningValue = defaultValue;
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("File not found at " + filePath);
                returningValue = defaultValue;
                return false;
            }
        }

        // Method to save the avatar (string and byte) to a file using JSON
        public static void SaveAvatar(string avatarName, byte avatarData, string fileNameAndExtension)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            string json = JsonUtility.ToJson(new BasisSavedAvatar(avatarName, avatarData));
            File.WriteAllText(filePath, json);
            Debug.Log("Avatar saved to " + filePath);
        }

        // Method to load the avatar (string and byte) from a file using JSON
        public static (string, byte) LoadAvatar(string fileNameAndExtension, string defaultName, byte defaultData)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                BasisSavedAvatar avatarWrapper = JsonUtility.FromJson<BasisSavedAvatar>(json);
                (string avatarName, byte avatarData) = avatarWrapper.ToValue();
                if (string.IsNullOrEmpty(avatarName))
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

        // Wrapper class for serializing and deserializing the int
        [System.Serializable]
        private class BasisSavedInt
        {
            public int Value;

            public BasisSavedInt(int value)
            {
                Value = value;
            }

            public int ToValue()
            {
                return Value;
            }
        }

        // Wrapper class for serializing and deserializing the float using a string representation (to handle culture)
        [System.Serializable]
        private class BasisSavedFloat
        {
            public string Value;

            public BasisSavedFloat(string value)
            {
                Value = value;
            }

            public string ToValue()
            {
                return Value;
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