using UnityEngine;
using UnityEngine.Events;

public class BasisVisualTracker : MonoBehaviour
{
    public MeshRenderer Renderer;
    public BasisInput BasisInput;
    public UnityEvent TrackedSetup = new UnityEvent();
    public Quaternion ModelRotationOffset = Quaternion.identity;
    public Vector3 ModelPositionOffset = Vector3.zero;
    public void Initialization(BasisInput basisInput)
    {
        if(basisInput != null)
        {
            BasisInput = basisInput;
            gameObject.transform.SetLocalPositionAndRotation(ModelPositionOffset, ModelRotationOffset);
            TrackedSetup.Invoke();
        }
    }
}
