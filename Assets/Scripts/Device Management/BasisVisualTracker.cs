using UnityEngine;
using UnityEngine.Events;

public class BasisVisualTracker : MonoBehaviour
{
    public MeshRenderer Renderer;
    public BasisInput BasisInput;
    public UnityEvent TrackedSetup = new UnityEvent();
    public void Initialization(BasisInput basisInput)
    {
        if(basisInput != null)
        {
            BasisInput = basisInput;
        }
    }
    public void OnDisable()
    {
    }
}
