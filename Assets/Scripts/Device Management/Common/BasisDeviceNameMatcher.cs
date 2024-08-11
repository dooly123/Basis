using System.Collections.Generic;
using System.Linq;


//using UnityEditor;

using UnityEngine;

namespace Basis.Scripts.Device_Management
{
    [CreateAssetMenu(fileName = "BasisDeviceNameMatcher", menuName = "Basis/BasisDeviceNameMatcher", order = 1)]
    public class BasisDeviceNameMatcher : ScriptableObject
    {
        [SerializeField]
        public List<BasisDeviceMatchSettings> BasisDevice = new List<BasisDeviceMatchSettings>();
        public bool GetAssociatedDeviceMatchableNames(string nameToMatch, out BasisDeviceMatchSettings BasisDeviceMatchableNames)
        {
            foreach (BasisDeviceMatchSettings DeviceEntry in BasisDevice)
            {
                string[] Matched = DeviceEntry.MatchableDeviceIdsLowered().ToArray();
                if (Matched.Contains(nameToMatch.ToLower()))
                {
                    BasisDeviceMatchableNames = DeviceEntry;
                    return true;
                }
            }
            Debug.LogError("Unable to find Configuration for device " + nameToMatch);
            BasisDeviceMatchableNames = null;
            return false;
        }
    }
    /*
    [CustomEditor(typeof(BasisDeviceNameMatcher))]
    public class BasisDeviceNameMatcherEditor : Editor
    {
        public override async void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BasisDeviceNameMatcher script = (BasisDeviceNameMatcher)target;

            if (GUILayout.Button("Save Devices"))
            {
                string directoryPath = EditorUtility.OpenFolderPanel("Select Directory to Save Devices", "", "");
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Debug.Log("directoryPath " + directoryPath);
                    await BasisDeviceLoaderAndSaver.SaveDevices(directoryPath, script.BasisDevice);
                }
            }
            if (GUILayout.Button("Load Devices"))
            {
                string directoryPath = EditorUtility.OpenFolderPanel("Select Directory to Save Devices", "", "");
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Debug.Log("directoryPath " + directoryPath);
                    script.BasisDevice = await BasisDeviceLoaderAndSaver.LoadDeviceAsync(directoryPath);
                }
            }
        }
    }
    */
}