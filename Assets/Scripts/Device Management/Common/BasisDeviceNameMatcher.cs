using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



//using UnityEditor;

using UnityEngine;

namespace Basis.Scripts.Device_Management
{
    [CreateAssetMenu(fileName = "BasisDeviceNameMatcher", menuName = "Basis/BasisDeviceNameMatcher", order = 1)]
    public class BasisDeviceNameMatcher : ScriptableObject
    {
        [SerializeField]
        public List<BasisDeviceMatchSettings> BasisDevice = new List<BasisDeviceMatchSettings>();
        public async Task<BasisDeviceMatchSettings> GetAssociatedDeviceMatchableNames(string nameToMatch)
        {
            foreach (BasisDeviceMatchSettings DeviceEntry in BasisDevice)
            {
                string[] Matched = DeviceEntry.MatchableDeviceIdsLowered().ToArray();
                if (Matched.Contains(nameToMatch.ToLower()))
                {
                    return DeviceEntry;
                }
            }
            BasisDeviceMatchSettings Settings = new BasisDeviceMatchSettings
            {
                VersionNumber = 1,
                DeviceID = nameToMatch,
                matchableDeviceIds = new string[] { nameToMatch },
                HasRayCastVisual = true,
                HasRayCastRedical = true,
                CanDisplayPhysicalTracker = false,
                HasRayCastSupport = true,
                HasTrackedRole = false
            };
            BasisDeviceManagement.Instance.BasisDeviceNameMatcher.BasisDevice.Add(Settings);
            Debug.LogError("Unable to find Configuration for device Generating " + nameToMatch);
            await BasisDeviceManagement.Instance.LoadAndOrSaveDefaultDeviceConfigs();
            return Settings;
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