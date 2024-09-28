using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Assist/Acquisition Assist")]
    [Preserve]
    public class AcquisitionAssist : MonoBehaviour
    {
        public BlendshapeActuationDefinitionFile definitionFile;
        public AcquisitionService acquisitionService;
        public Dictionary<string, float> memory = new Dictionary<string, float>();

        private void OnAddressUpdated(string address, float value)
        {
            if (!isActiveAndEnabled) return;
            
            acquisitionService.Submit(address, value);
        }
    }
}