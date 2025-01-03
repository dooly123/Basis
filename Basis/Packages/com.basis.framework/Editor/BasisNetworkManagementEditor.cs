using UnityEngine;
using UnityEditor;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Networking;

[CustomEditor(typeof(BasisNetworkManagement))]
public class BasisNetworkManagementEditor : Editor
{
    private bool receiverArrayFoldout = false; // Foldout toggle
    private int selectedReceiverIndex = 0; // Slider for receivers

    public override void OnInspectorGUI()
    {
        // Reference to the target script
        BasisNetworkManagement networkManager = (BasisNetworkManagement)target;

        // Display base inspector elements
        //DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Network Statistics", EditorStyles.boldLabel);

        // Display stats
        networkManager.Ip = EditorGUILayout.TextField("IP Address:", networkManager.Ip);
        networkManager.Port = (ushort)EditorGUILayout.IntField("Port:", networkManager.Port);
        EditorGUILayout.LabelField("Total Players:", BasisNetworkManagement.Players.Count.ToString());
        EditorGUILayout.LabelField("Remote Players:", BasisNetworkManagement.RemotePlayers.Count.ToString());
        EditorGUILayout.LabelField("Joining Players:", BasisNetworkManagement.JoiningPlayers.Count.ToString());
        EditorGUILayout.LabelField("Receiver Count:", BasisNetworkManagement.ReceiverCount.ToString());



        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transmitter Details", EditorStyles.boldLabel);
        if (BasisNetworkManagement.Instance.Transmitter != null)
        {
            // Display key properties
            EditorGUILayout.LabelField("Ready:", BasisNetworkManagement.Instance.Transmitter.Ready.ToString());
            EditorGUILayout.LabelField("Has Events:", BasisNetworkManagement.Instance.Transmitter.HasEvents.ToString());
            EditorGUILayout.LabelField("Timer:", BasisNetworkManagement.Instance.Transmitter.timer.ToString("F3"));
            EditorGUILayout.LabelField("Interval:", BasisNetworkManagement.Instance.Transmitter.interval.ToString("F3"));
            EditorGUILayout.LabelField("Smallest Distance to Another Player:", BasisNetworkManagement.Instance.Transmitter.SmallestDistanceToAnotherPlayer.ToString("F3"));
            EditorGUILayout.Space();
            // Native Array Details
            nativeArrayFoldout = EditorGUILayout.Foldout(nativeArrayFoldout, "Native Arrays", true);
            if (nativeArrayFoldout)
            {
                EditorGUILayout.LabelField("Target Positions:", BasisNetworkManagement.Instance.Transmitter.targetPositions.IsCreated ? "Created" : "Not Created");
                EditorGUILayout.LabelField("Distances:", BasisNetworkManagement.Instance.Transmitter.distances.IsCreated ? "Created" : "Not Created");
                EditorGUILayout.LabelField("Distance Results:", BasisNetworkManagement.Instance.Transmitter.DistanceResults.IsCreated ? "Created" : "Not Created");
                EditorGUILayout.LabelField("Hearing Results:", BasisNetworkManagement.Instance.Transmitter.HearingResults.IsCreated ? "Created" : "Not Created");
                EditorGUILayout.LabelField("Avatar Results:", BasisNetworkManagement.Instance.Transmitter.AvatarResults.IsCreated ? "Created" : "Not Created");
            }

            EditorGUILayout.Space();

            // Debugging Foldout
            debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debugging", true);
            if (debugFoldout)
            {
                EditorGUILayout.LabelField("Microphone Range Index:", BasisNetworkManagement.Instance.Transmitter.MicrophoneRangeIndex != null ? BasisNetworkManagement.Instance.Transmitter.MicrophoneRangeIndex.Length.ToString() : "NULL");
                EditorGUILayout.LabelField("Hearing Index:", BasisNetworkManagement.Instance.Transmitter.HearingIndex != null ? BasisNetworkManagement.Instance.Transmitter.HearingIndex.Length.ToString() : "NULL");
                EditorGUILayout.LabelField("Avatar Index:", BasisNetworkManagement.Instance.Transmitter.AvatarIndex != null ? BasisNetworkManagement.Instance.Transmitter.AvatarIndex.Length.ToString() : "NULL");
            }
        }
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();

        // Foldout for Receiver Array Data
        receiverArrayFoldout = EditorGUILayout.Foldout(receiverArrayFoldout, "Receiver Array Data", true);
        if (receiverArrayFoldout)
        {
            if (BasisNetworkManagement.ReceiverArray != null && BasisNetworkManagement.ReceiverArray.Length > 0)
            {
                EditorGUILayout.LabelField($"Receiver Array Length: {BasisNetworkManagement.ReceiverArray.Length}");

                // Slider to select receiver
                selectedReceiverIndex = EditorGUILayout.IntSlider("Select Receiver", selectedReceiverIndex, 0, BasisNetworkManagement.ReceiverArray.Length - 1);

                // Get selected receiver
                var receiver = BasisNetworkManagement.ReceiverArray[selectedReceiverIndex];
                if (receiver != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Receiver {selectedReceiverIndex + 1}", EditorStyles.boldLabel);

                    // Display properties of the selected receiver
                    DisplayReceiverProperties(receiver);

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.LabelField($"Receiver {selectedReceiverIndex + 1}: NULL");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Receiver Array is empty or null.");
            }
        }

        // Apply changes if properties were modified
        if (GUI.changed)
        {
            EditorUtility.SetDirty(networkManager);
        }
    }
    private bool nativeArrayFoldout = false; // Foldout for NativeArray details
    private bool debugFoldout = false; // Foldout for debugging data
    private void DisplayReceiverProperties(BasisNetworkReceiver receiver)
    {
        // Use reflection to dynamically display fields
        var fields = receiver.GetType().GetFields();
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(receiver);
            EditorGUILayout.LabelField($"{field.Name}:", fieldValue != null ? fieldValue.ToString() : "null");
        }

        // Use reflection to dynamically display properties
        var properties = receiver.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                var propertyValue = property.GetValue(receiver, null);
                EditorGUILayout.LabelField($"{property.Name}:", propertyValue != null ? propertyValue.ToString() : "null");
            }
        }
    }
}
