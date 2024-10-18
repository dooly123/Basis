using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public static class BasisDataStoreAvatarKeys
    {
        // The AvatarKey class stores the URL and Name fields
        public class AvatarKey
        {
            public string Url { get; set; }
            public string Pass { get; set; }

            // Optional: Override ToString for better Debug.Log output
            public override string ToString()
            {
                return $"Name: {Pass}, Url: {Url}";
            }
        }

        // File path to store the keys
        private static string FilePath = "VerySafePasswordStore.json";

        // A list to keep password keys in memory
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
            // Find key by matching Url or Name (up to your logic preference)
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
                string jsonString = await File.ReadAllTextAsync(FilePath);
                keys = JsonUtility.FromJson<List<AvatarKey>>(jsonString) ?? new List<AvatarKey>();
                Debug.Log("Keys loaded from file.");
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
            string jsonString = JsonUtility.ToJson(keys);
            await File.WriteAllTextAsync(FilePath, jsonString);
            Debug.Log("Keys saved to file.");
        }

        // Display all stored keys
        public static void DisplayKeys()
        {
            Debug.Log("Stored Keys:");
            foreach (var key in keys)
            {
                Debug.Log(key); // This will use the ToString() method for output
            }
        }
    }
}