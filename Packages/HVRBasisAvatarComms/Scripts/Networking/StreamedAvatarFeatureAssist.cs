using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace HVR.Basis.Comms
{
    [AddComponentMenu("HVR.Basis/Comms/Assist/Streamed Avatar Feature Assist")]
    [Preserve]
    public class StreamedAvatarFeatureAssist : MonoBehaviour
    {
        public float deltaTime = 0.1f;
        public StreamedAvatarFeature[] features;
    }
}