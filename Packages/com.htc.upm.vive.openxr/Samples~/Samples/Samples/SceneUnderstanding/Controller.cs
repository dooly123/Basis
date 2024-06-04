using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_ActionReferenceTrigger;
    public InputActionReference actionReferenceTrigger { get => m_ActionReferenceTrigger ; set => m_ActionReferenceTrigger=value; }
    [SerializeField]
    private InputActionReference m_ActionReferenceGrip;
    public InputActionReference actionReferenceGrip { get => m_ActionReferenceGrip ; set => m_ActionReferenceGrip=value; }
    public GameObject MeshSubSystem;
    public Transform spawnPoint;
    public GameObject sphere;
    public float shootVelocity = 5f;

    Type lastActiveType_Trigger = null;
    Type lastActiveType_Grip = null;
    bool isSpawned = false;
    bool isSwitchedMeshPrefab = false;
    // Update is called once per frame
    void Update()
    {
        if ( actionReferenceTrigger != null && actionReferenceTrigger.action != null 
            && actionReferenceTrigger.action.enabled && actionReferenceTrigger.action.controls.Count > 0
            && actionReferenceGrip != null && actionReferenceGrip.action != null
            && actionReferenceGrip.action.enabled && actionReferenceGrip.action.controls.Count > 0)
        {
            // Grip
            Type typeToUse_Grip = null;
            if(actionReferenceGrip.action.activeControl != null)
            {
                typeToUse_Grip = actionReferenceGrip.action.activeControl.valueType;
            }
            else
            {
                typeToUse_Grip = lastActiveType_Grip;
            }

            if(typeToUse_Grip == typeof(float))
            {
                lastActiveType_Grip = typeof(float);
                float value = actionReferenceGrip.action.ReadValue<float>();
                if(value > 0.5)
                {
                    if(!isSwitchedMeshPrefab)
                    {
                        isSwitchedMeshPrefab = true;
                        Debug.Log("Press Button B.");
                        if(MeshSubSystem != null)
                        {
                            MeshSubSystem.GetComponent<UnityEngine.XR.OpenXR.Samples.MeshingFeature.MeshingBehaviour>().SwitchMeshPrefab(); 
                        }
                    }
                }
                else
                {
                    isSwitchedMeshPrefab = false;
                }
            }

            // Trigger
            Type typeToUse_Trigger = null;
            if(actionReferenceTrigger.action.activeControl != null)
            {
                typeToUse_Trigger = actionReferenceTrigger.action.activeControl.valueType;
            }
            else
            {
                typeToUse_Trigger = lastActiveType_Trigger;
            }

            if(typeToUse_Trigger == typeof(float))
            {
                lastActiveType_Trigger = typeof(float);
                float value = actionReferenceTrigger.action.ReadValue<float>();
                if(value > 0.5)
                {
                    if(!isSpawned)
                    {
                        isSpawned = true;
                        SpawnTarget();
                    }
                }
                else
                {
                    isSpawned = false;
                }
            }
        }
    }

    private void SpawnTarget()
    {
        GameObject ball = Instantiate(sphere, spawnPoint);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = ball.transform.parent.forward * shootVelocity;
        rb.isKinematic = false;
        ball.transform.parent = null;
    }
}
