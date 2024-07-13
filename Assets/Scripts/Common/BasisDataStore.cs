using System.IO;
using UnityEngine;

public static class BasisDataStore
{
    // Method to save the list of strings to a file using JSON
    public static void SaveString(string stringcontents, string FileNameAndExtension)
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileNameAndExtension);
        // Convert the list of strings to a JSON string
        string json = JsonUtility.ToJson(new BasisSavedString(stringcontents));

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);

        Debug.Log("List saved to " + filePath);
    }

    // Method to load the list of strings from a file using JSON
    public static string LoadString(string FileNameAndExtension,string DefaultValue)
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileNameAndExtension);
        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the JSON string from the file
            string json = File.ReadAllText(filePath);

            // Convert the JSON string back to a list of strings
            BasisSavedString stringListWrapper = JsonUtility.FromJson<BasisSavedString>(json);
            string stringList = stringListWrapper.ToValue();

            Debug.Log("List loaded from " + filePath);
            return stringList;
        }
        else
        {
            Debug.LogWarning("File not found at " + filePath);
            return DefaultValue;
        }
    }

    // Wrapper class for serializing and deserializing the list of strings
    [System.Serializable]
    private class BasisSavedString
    {
        public string String;

        public BasisSavedString(string savestring)
        {
            String = savestring;
        }

        public string ToValue()
        {
            return String;
        }
    }
}