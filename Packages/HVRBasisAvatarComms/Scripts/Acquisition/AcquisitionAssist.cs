#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Assist/Acquisition Assist")]
    public class AcquisitionAssist : MonoBehaviour
    {
        public BlendshapeActuationDefinitionFile definitionFile;
        public AcquisitionService acquisitionService;
        internal Dictionary<string, float> memory = new Dictionary<string, float>();

        private void OnAddressUpdated(string address, float value)
        {
            if (!isActiveAndEnabled) return;
            
            acquisitionService.Submit(address, value);
        }
    }

    [CustomEditor(typeof(AcquisitionAssist))]
    public class AcquisitionAssistEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var assist = (AcquisitionAssist)target;
            var groupings = assist.definitionFile.definitions.GroupBy(definition => definition.address, definition => definition);
            foreach (var definitions in groupings)
            {
                var address = definitions.First().address;
                assist.memory.TryGetValue(address, out var value);
                var allInEnds = definitions.Select(definition => definition.inStart).Concat(definitions.Select(definition => definition.inEnd)).ToArray();
                var min = allInEnds.Min();
                var max = allInEnds.Max();
                var newValue = EditorGUILayout.Slider(address, value, min, max);
                if (value != newValue)
                {
                    assist.memory[address] = newValue;
                    assist.acquisitionService.Submit(address, newValue);
                } 
            }
        }
    }
}
#endif