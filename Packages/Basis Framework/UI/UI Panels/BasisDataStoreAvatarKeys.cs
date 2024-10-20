using BasisSerializer.OdinSerializer;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public static class BasisDataStoreAvatarKeys
    {
        // The AvatarKey class stores the URL and Pass fields
        [System.Serializable]
        public class AvatarKey
        {
            public string Url;
            public string Pass;
        }

        // File path to store the keys, now using Unity's persistentDataPath
        private static string FilePath => Path.Combine(Application.persistentDataPath, "VerySafePasswordStore.json");

        // A list to keep password keys in memory
        [SerializeField]
        private static List<AvatarKey> keys = new List<AvatarKey>();

        // Add a new key to the list and save to disk
        public static async Task AddNewKey(AvatarKey newKey)
        {
            keys.Add(newKey);
            await SaveKeysToFile();
            Debug.Log($"Key added: {newKey}");
        }

        // Remove a key from the list and save changes to disk
        public static async Task RemoveKey(AvatarKey keyToRemove)
        {
            // Find key by matching Url and Pass
            var key = keys.Find(k => k.Url == keyToRemove.Url && k.Pass == keyToRemove.Pass);
            if (key != null)
            {
                keys.Remove(key);
                await SaveKeysToFile();
                Debug.Log($"Key removed: {keyToRemove}");
            }
            else
            {
                Debug.Log("Key not found.");
            }
        }

        // Load keys from the file into memory
        public static async Task LoadKeys()
        {
            if (File.Exists(FilePath))
            {
                // Read the JSON from the file and deserialize into the keys list
                byte[] ByteData = await File.ReadAllBytesAsync(FilePath);
                keys = SerializationUtility.DeserializeValue<List<AvatarKey>>(ByteData, DataFormat.Binary);
                Debug.Log("Keys loaded from file. with a count of " + keys.Count);
            }
            else
            {
                Debug.Log("No key file found. Starting fresh.");
            }
        }

        // Save the current keys to a JSON file
        private static async Task SaveKeysToFile()
        {
            // Serialize the keys list to a JSON string and write to file
            byte[] ByteData = SerializationUtility.SerializeValue<List<AvatarKey>>(keys, DataFormat.Binary);
            await File.WriteAllBytesAsync(FilePath, ByteData);
            Debug.Log($"Keys saved to file at: {FilePath}");
        }

        // Display all stored keys
        public static List<AvatarKey> DisplayKeys()
        {
            return keys;
        }
    }
}