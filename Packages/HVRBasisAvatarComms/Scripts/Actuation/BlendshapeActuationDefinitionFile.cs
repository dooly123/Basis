using HVR.Basis.Comms;
using UnityEngine;
using UnityEngine.Scripting;

[CreateAssetMenu(fileName = "BlendshapeActuationAsset", menuName = "HVR.Basis/Comms/Blendshape Actuation Definition FIle")]
[Preserve]
public class BlendshapeActuationDefinitionFile : ScriptableObject
{
    [TextArea] public string comment; 
    public BlendshapeActuationDefinition[] definitions;
}