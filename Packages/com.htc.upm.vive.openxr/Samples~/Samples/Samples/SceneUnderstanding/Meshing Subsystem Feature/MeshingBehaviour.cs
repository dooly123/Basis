using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using VIVE.OpenXR.SceneUnderstanding;
namespace UnityEngine.XR.OpenXR.Samples.MeshingFeature
{
    public class MeshingBehaviour : MonoBehaviour
    {
        public GameObject emptyMeshPrefab_Default;
        public GameObject emptyMeshPrefab_Occlusion;
        public Material meshMat_Default;
        public Material meshMat_Occlusion;
        private GameObject emptyMeshPrefab;
        private Material meshesMat;
        public TextMesh textMesh;
        public Transform target;
        public bool isOcclusion = true;
        private XRMeshSubsystem s_MeshSubsystem;
        private List<MeshInfo> s_MeshInfos = new List<MeshInfo>();

        private Dictionary<MeshId, GameObject> m_MeshIdToGo = new Dictionary<MeshId, GameObject>();

#region OpenXR feature
        private MeshingTeapotFeature m_MeshingFeature;
#endregion

#region Scene compute consistency

        [Header("Scene compute consistency")]
        [Tooltip("Scene compute consistency can only be set before entering the play mode.")]
        public XrSceneComputeConsistencyMSFT m_SceneComputeConsistency =
            XrSceneComputeConsistencyMSFT.SnapshotIncompleteFast;
#endregion

#region Scene compute bound variables

        [Header("Scene compute bounds")]

        // Sphere bound.
        public bool m_EnableSphereBound;
        private bool m_PreviousEnableSphereBound = false;

        // A default sphere game object.
        public GameObject m_SphereBoundObject;

        // Box bound.
        public bool m_EnableBoxBound;
        private bool m_PreviousEnableBoxBound = false;

        // A default cube game object
        // Box bound is enabled by default.
        public GameObject m_BoxBoundObject;

        // Frustum bound.
        public bool m_EnableFrustumBound;
        private bool m_PreviousEnableFrustumBound = false;
        public Camera m_FrustumBoundCamera;
        public float m_FarDistance = 2.0f;
#endregion

#region Mesh compute lod variables
        [Header("Mesh compute lod")]
        public XrMeshComputeLodMSFT m_MeshComputeLod =
            XrMeshComputeLodMSFT.Coarse;
        private XrMeshComputeLodMSFT m_PrevMeshComputeLod =
            XrMeshComputeLodMSFT.Coarse;
#endregion
        void Start()
        {
            // Set the mesh prefab to default material.
            if(emptyMeshPrefab_Occlusion == null || emptyMeshPrefab_Default == null)
            {
                return;
            }
            emptyMeshPrefab = isOcclusion ? emptyMeshPrefab_Occlusion : emptyMeshPrefab_Default;

            m_MeshingFeature = OpenXRSettings.Instance.GetFeature<MeshingTeapotFeature>();
            if (m_MeshingFeature == null || m_MeshingFeature.enabled == false)
            {
                enabled = false;
                return;
            }

            var meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetInstances(meshSubsystems);
            if (meshSubsystems.Count == 1)
            {
                s_MeshSubsystem = meshSubsystems[0];
                textMesh.gameObject.SetActive(false);
            }
            else
            {
#if UNITY_EDITOR
                textMesh.text = "Failed to initialize MeshSubsystem.\nCheck `MeshingFeaturePlugin' folder be moved to the root 'Assets` folder\nTry reloading the Unity Editor";
                UnityEngine.Debug.LogWarning("The SceneUnderstanding Example requires the `MeshingFeaturePlugin' folder be moved to the root 'Assets` folder to run properly. Move the folder and try reloading the Unity Editor.");
#else
                textMesh.text = "Failed to initialize MeshSubsystem.";
#endif
                enabled = false;
            }

            if (m_FrustumBoundCamera == null)
            {
                m_FrustumBoundCamera = Camera.main;
            }

            // Set scene compute consistency at the start.
            m_MeshingFeature.SetSceneComputeConsistency(m_SceneComputeConsistency);

            // Set mesh compute lod.
            m_MeshingFeature.SetMeshComputeLod(m_MeshComputeLod);
        }

        void Update()
        {
            if (s_MeshSubsystem.running && s_MeshSubsystem.TryGetMeshInfos(s_MeshInfos))
            {
                foreach (var meshInfo in s_MeshInfos)
                {
                    switch (meshInfo.ChangeState)
                    {
                        case MeshChangeState.Added:
                        case MeshChangeState.Updated:
                            if (!m_MeshIdToGo.TryGetValue(meshInfo.MeshId, out var go))
                            {
                                go = Instantiate(emptyMeshPrefab, target, false);
                                m_MeshIdToGo[meshInfo.MeshId] = go;
                            }

                            var mesh = go.GetComponent<MeshFilter>().mesh;
                            var col = go.GetComponent<MeshCollider>();

                            s_MeshSubsystem.GenerateMeshAsync(meshInfo.MeshId, mesh, col, MeshVertexAttributes.None,
                                result =>
                                {
                                    result.Mesh.RecalculateNormals();
                                });
                            break;
                        case MeshChangeState.Removed:
                            if (m_MeshIdToGo.TryGetValue(meshInfo.MeshId, out var meshGo))
                            {
                                Destroy(meshGo);
                                m_MeshIdToGo.Remove(meshInfo.MeshId);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // Read keyboard input by the Input System package.
            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                // Toggle the sphere bound.
                m_EnableSphereBound ^= true;
            }
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                // Toggle the box bound.
                m_EnableBoxBound ^= true;
            }
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                // Toggle the frustum bound.
                m_EnableFrustumBound ^= true;
            }
            if (Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                // Set mesh lod to coarse.
                m_MeshComputeLod = XrMeshComputeLodMSFT.Coarse;
            }
            if (Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                // Set mesh lod to medium.
                m_MeshComputeLod = XrMeshComputeLodMSFT.Medium;
            }
            if (Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                // Set mesh lod to fine.
                m_MeshComputeLod = XrMeshComputeLodMSFT.Fine;
            }
            if (Keyboard.current.numpad4Key.wasPressedThisFrame)
            {
                // Set mesh lod to unlimited.
                m_MeshComputeLod = XrMeshComputeLodMSFT.Unlimited;
            }

            if (m_MeshingFeature == null) return;

            // Set scene computation sphere bound.
            if (m_EnableSphereBound)
            {
                 SetSceneComputeSphereBound();
            }
            else if (m_PreviousEnableSphereBound)
            {
                // The bound becomes disabled. Clear related data in the plugin.
                m_MeshingFeature.ClearSceneComputeBounds(XrSceneBoundType.Sphere);
            }
            m_PreviousEnableSphereBound = m_EnableSphereBound;

            // Set scene computation box bound.
            if (m_EnableBoxBound)
            {
                SetSceneComputeOrientedBoxBound();
            }
            else if (m_PreviousEnableBoxBound)
            {
                // The bound becomes disabled. Clear related data in the plugin.
                m_MeshingFeature.ClearSceneComputeBounds(XrSceneBoundType.OrientedBox);
            }
            m_PreviousEnableBoxBound = m_EnableBoxBound;

            // Set scene computation frustum bound.
            if (m_EnableFrustumBound)
            {
                SetSceneComputeFrustumBound();
            }
            else if (m_PreviousEnableFrustumBound)
            {
                // The bound becomes disabled. Clear related data in the plugin.
                m_MeshingFeature.ClearSceneComputeBounds(XrSceneBoundType.Frustum);
            }
            m_PreviousEnableFrustumBound = m_EnableFrustumBound;

            // Set mesh compute lod if updated.
            if (m_MeshComputeLod != m_PrevMeshComputeLod)
            {
                m_MeshingFeature.SetMeshComputeLod(m_MeshComputeLod);
                m_PrevMeshComputeLod = m_MeshComputeLod;
            }
        }

        void SetSceneComputeSphereBound()
        {
            if (m_SphereBoundObject == null) return;

            m_MeshingFeature.SetSceneComputeSphereBound(m_SphereBoundObject.transform.position,
                0.5f * m_SphereBoundObject.transform.localScale.x); // The radius of a default sphere is 0.5f.
        }

        void SetSceneComputeOrientedBoxBound()
        {
            if (m_BoxBoundObject == null) return;

            m_MeshingFeature.SetSceneComputeOrientedBoxBound(m_BoxBoundObject.transform,
                m_BoxBoundObject.transform.localScale); // The widths of a default cube is 1.0f.
        }

        void SetSceneComputeFrustumBound()
        {
            var camera = m_FrustumBoundCamera;
            if (camera == null) return;
            var halfVerticalFieldOfView = 0.5f * camera.fieldOfView * Mathf.Deg2Rad;
            var halfHorizontalFieldOfView = 0.5f * Camera.VerticalToHorizontalFieldOfView(
                camera.fieldOfView, camera.aspect) * Mathf.Deg2Rad;
            m_MeshingFeature.SetSceneComputeFrustumBound(camera.transform,
                halfVerticalFieldOfView, -1 * halfVerticalFieldOfView,
                halfHorizontalFieldOfView, -1 * halfHorizontalFieldOfView,
                m_FarDistance);
                // camera.farClipPlane);
        }

        public void SwitchMeshPrefab()
        {
            if(emptyMeshPrefab_Default != null && emptyMeshPrefab_Occlusion != null)
            {
                isOcclusion ^= true;
                Debug.Log("isOcclusion: " + isOcclusion);
                emptyMeshPrefab = isOcclusion ? emptyMeshPrefab_Occlusion : emptyMeshPrefab_Default;
                Renderer[] allChildern = target.transform.GetComponentsInChildren<Renderer>();
                foreach(Renderer child in allChildern)
                {
                    if(isOcclusion)
                    {
                        child.material = meshMat_Occlusion;
                    }
                    else
                    {
                        child.material = meshMat_Default;
                    }
                }
            }
        }
    }
}
