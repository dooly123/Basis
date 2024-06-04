#include <openxr/openxr.h>
#include "IUnityInterface.h"
#include "XR/IUnityXRMeshing.h"
#include <map>
#include <memory>
#include <string>
#include <thread>
#include <fstream>
#include <vector>
static IUnityXRMeshInterface* s_Meshing = nullptr;
static XrInstance s_XrInstance = nullptr;
static XrSession s_XrSession = nullptr;

// XrSpace related function pointers
static PFN_xrEnumerateReferenceSpaces s_xrEnumerateReferenceSpaces = nullptr;
static PFN_xrCreateReferenceSpace s_xrCreateReferenceSpace = nullptr;
static PFN_xrDestroySpace s_xrDestroySpace = nullptr;

// XR_MSFT_scene_understanding function pointers
static PFN_xrEnumerateSceneComputeFeaturesMSFT s_xrEnumerateSceneComputeFeaturesMSFT = nullptr;
static PFN_xrCreateSceneObserverMSFT s_xrCreateSceneObserverMSFT = nullptr;
static PFN_xrDestroySceneObserverMSFT s_xrDestroySceneObserverMSFT = nullptr;
static PFN_xrCreateSceneMSFT s_xrCreateSceneMSFT = nullptr;
static PFN_xrDestroySceneMSFT s_xrDestroySceneMSFT = nullptr;
static PFN_xrComputeNewSceneMSFT s_xrComputeNewSceneMSFT = nullptr;
static PFN_xrGetSceneComputeStateMSFT s_xrGetSceneComputeStateMSFT = nullptr;
static PFN_xrGetSceneComponentsMSFT s_xrGetSceneComponentsMSFT = nullptr;
static PFN_xrLocateSceneComponentsMSFT s_xrLocateSceneComponentsMSFT = nullptr;
static PFN_xrGetSceneMeshBuffersMSFT s_xrGetSceneMeshBuffersMSFT = nullptr;

static XrReferenceSpaceType s_ReferenceSpaceType = XrReferenceSpaceType::XR_REFERENCE_SPACE_TYPE_STAGE;
static XrSpace s_XrSpace = nullptr;

static XrSceneObserverMSFT s_SceneObserver = nullptr;
static bool s_OpenXRReady = false;

static XrSceneComputeConsistencyMSFT s_SceneComputeConsistency = XrSceneComputeConsistencyMSFT::XR_SCENE_COMPUTE_CONSISTENCY_SNAPSHOT_INCOMPLETE_FAST_MSFT;

// User specified scene computation boundaries.

static std::vector<XrSceneSphereBoundMSFT> s_SceneSphereBounds;
static std::vector<XrSceneOrientedBoxBoundMSFT> s_SceneOrientedBoxBounds;
static std::vector<XrSceneFrustumBoundMSFT> s_SceneFrustumBounds;

static XrMeshComputeLodMSFT s_MeshComputeLod = XrMeshComputeLodMSFT::XR_MESH_COMPUTE_LOD_COARSE_MSFT;

void Log(const std::string& line)
{
    std::ofstream file("meshing_plugin.log", std::ios::app);
    if (!file.is_open()) return;
    file << line << "\n";
}

void CheckResult(XrResult result, const std::string& funcName)
{
    if (result != XrResult::XR_SUCCESS)
    {
        Log(funcName + " failure: " + std::to_string(result));
    }
}
/**
 * Make an XRSceneMSFT can be managed by shared pointers.
 * The containing XRSceneMSFT is destroyed in the destructor
 * by an OpenXR function, xrDestroySceneMSFT.
 */
class SharedOpenXRScene
{
public:
    /**
     * @param[in] scene A valid scene, which is created by xrCreateSceneMSFT.
     */
    SharedOpenXRScene(XrSceneMSFT scene) : m_Scene(scene) {}
    ~SharedOpenXRScene()
    {
        if (s_xrDestroySceneMSFT != nullptr && s_SceneObserver != nullptr)
        {
            CheckResult(s_xrDestroySceneMSFT(m_Scene), "xrDestroySceneMSFT");
        }
    }

    XrSceneMSFT GetScene() const { return m_Scene; };

private:
    XrSceneMSFT m_Scene;
};

/**
 * Store mesh data belonging to a UnityXRMeshId.
 */
class MeshData
{
public:
    MeshData() : m_UpdateTime(0) {}

    UnityXRMeshInfo m_UnityXRMeshInfo;

    /**
     * Point to a shared OpenXR scene.
     * When there is no mesh data pointing to a scene,
     * the scene will be destroyed by an OpenXR function (xrDestroySceneMSFT).
     */
    std::shared_ptr<SharedOpenXRScene> m_SharedOpenXRScene;

    XrSceneMeshMSFT m_OpenXRSceneMesh;

    long long m_UpdateTime;
};

static std::map<UnityXRMeshId, MeshData, MeshIdLessThanComparator> s_MeshDataByMeshId;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* interfaces)
{
    s_Meshing = interfaces->Get<IUnityXRMeshInterface>();
    if (s_Meshing == nullptr)
        return;

    UnityLifecycleProvider meshingLifecycleHandler{};
    meshingLifecycleHandler.Initialize = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        // Reset scene computation bounds.
        // Use an axis-aligned bounding box by default.
        s_SceneSphereBounds.clear();
        s_SceneOrientedBoxBounds.clear();
        s_SceneFrustumBounds.clear();

        s_MeshDataByMeshId.clear();

        UnityXRMeshProvider meshProvider{};
        meshProvider.GetMeshInfos = [](UnitySubsystemHandle handle, void* userData, UnityXRMeshInfoAllocator* allocator) -> UnitySubsystemErrorCode {
            if (s_SceneObserver == nullptr) return kUnitySubsystemErrorCodeFailure;

            // Set existing mesh infos as not updated.
            for (auto&& pair : s_MeshDataByMeshId)
            {
                pair.second.m_UnityXRMeshInfo.updated = false;
            }

            bool canComputeNewScene = false;

            // Check the scene compute state.
            XrSceneComputeStateMSFT computeState;
            CheckResult(s_xrGetSceneComputeStateMSFT(s_SceneObserver, &computeState), "xrGetSceneComputeStateMSFT");
            switch (computeState)
            {
            case XrSceneComputeStateMSFT::XR_SCENE_COMPUTE_STATE_NONE_MSFT:
            {
                // Compute a new scene at the end of the function.
                canComputeNewScene = true;
                break;
            }
            case XrSceneComputeStateMSFT::XR_SCENE_COMPUTE_STATE_UPDATING_MSFT:
                // Wait for scene computation.
                canComputeNewScene = false;
                break;
            case XrSceneComputeStateMSFT::XR_SCENE_COMPUTE_STATE_COMPLETED_MSFT:
            {
                // Compute a new scene at the end of the function.
                canComputeNewScene = true;

                // Create a scene of the computation result.
                XrSceneCreateInfoMSFT sceneCreateInfo;
                sceneCreateInfo.type = XrStructureType::XR_TYPE_SCENE_CREATE_INFO_MSFT;
                sceneCreateInfo.next = NULL;
                XrSceneMSFT scene;
                CheckResult(s_xrCreateSceneMSFT(s_SceneObserver, &sceneCreateInfo, &scene), "xrCreateSceneMSFT");

                // Create a shared scene to be stored in mesh data.
                auto sharedScene = std::make_shared<SharedOpenXRScene>(scene);

                // Stage 1: Get scene visual mesh components.

                XrSceneComponentsGetInfoMSFT sceneComponentsGetInfo;
                sceneComponentsGetInfo.type = XrStructureType::XR_TYPE_SCENE_COMPONENTS_GET_INFO_MSFT;
                sceneComponentsGetInfo.next = NULL;
                sceneComponentsGetInfo.componentType = XrSceneComponentTypeMSFT::XR_SCENE_COMPONENT_TYPE_VISUAL_MESH_MSFT;
                // First get the buffer capacity.
                XrSceneComponentsMSFT sceneComponents;
                sceneComponents.type = XrStructureType::XR_TYPE_SCENE_COMPONENTS_MSFT;
                sceneComponents.next = NULL;
                sceneComponents.componentCapacityInput = 0;
                sceneComponents.components = NULL;
                CheckResult(s_xrGetSceneComponentsMSFT(scene, &sceneComponentsGetInfo, &sceneComponents), "xrGetSceneComponentsMSFT");
                // Create scene components by the provided capacity.
                std::vector<XrSceneComponentMSFT> sceneComponentsVector(sceneComponents.componentCountOutput);
                sceneComponents.componentCapacityInput = sceneComponents.componentCountOutput;
                sceneComponents.components = sceneComponentsVector.data();
                // Also add an instance in the structure chain for getting scene visual mesh components.
                std::vector<XrSceneMeshMSFT> sceneMeshesVector(sceneComponents.componentCountOutput);
                XrSceneMeshesMSFT sceneMeshes;
                sceneMeshes.type = XrStructureType::XR_TYPE_SCENE_MESHES_MSFT;
                sceneMeshes.next = NULL;
                sceneMeshes.sceneMeshCount = sceneComponents.componentCountOutput;
                sceneMeshes.sceneMeshes = sceneMeshesVector.data();
                sceneComponents.next = &sceneMeshes;
                // Call xrGetSceneComponentsMSFT() again to fill out the scene components and scene visual mesh components.
                CheckResult(s_xrGetSceneComponentsMSFT(scene, &sceneComponentsGetInfo, &sceneComponents), "xrGetSceneComponentsMSFT");

                // Fill out mesh info from the scene visual mesh components.
                for (size_t componentIndex = 0; componentIndex < sceneComponents.componentCountOutput; ++componentIndex)
                {
                    auto& sceneComponent = sceneComponentsVector[componentIndex];
                    auto& sceneMesh = sceneMeshesVector[componentIndex];

                    // Create a Unity mesh id by the OpenXR component id.
                    // If OpenXR scene components of different time have the same component id,
                    // they represent the same physical object. Thus use component id
                    // as Unity mesh id.
                    UnityXRMeshId meshId;
                    memcpy(&meshId, &sceneComponent.id, sizeof(UnityXRMeshId));

                    // Prepare to store mesh data of the mesh id.
                    // The mesh data can be an existing one from the previous scene,
                    // or a new one of the current scene.
                    auto& meshData = s_MeshDataByMeshId[meshId];

                    // Set the mesh info of the mesh id.
                    // If the current update time is larger than the stored value,
                    // set the mesh as updated.
                    auto& meshInfo = meshData.m_UnityXRMeshInfo;
                    meshInfo.meshId = meshId;
                    meshInfo.updated = sceneComponent.updateTime > meshData.m_UpdateTime;
                    meshInfo.priorityHint = 0;

                    // Store the shared scene in order to manage the destruction of scene.
                    meshData.m_SharedOpenXRScene = sharedScene;

                    // Store the OpenXR scene mesh.
                    meshData.m_OpenXRSceneMesh = sceneMesh;

                    // Store the update time.
                    meshData.m_UpdateTime = sceneComponent.updateTime;
                }

                // After setting data of the current scene, remove mesh data
                // not belonging to the current scene.
                for (auto iterator = s_MeshDataByMeshId.cbegin(); iterator != s_MeshDataByMeshId.cend();)
                {
                    if (iterator->second.m_SharedOpenXRScene != sharedScene)
                    {
                        // The mesh data does not exist in the current scene.
                        // Erase it from the container, and get the iterator
                        // after the erased position.
                        iterator = s_MeshDataByMeshId.erase(iterator);
                    }
                    else
                    {
                        // The mesh data exist in the current scene.
                        // Do nothing and increment the iterator.
                        ++iterator;
                    }
                }
            }
                break;
            case XrSceneComputeStateMSFT::XR_SCENE_COMPUTE_STATE_COMPLETED_WITH_ERROR_MSFT:
                Log("Scene computation failed");
                // Compute a new scene at the end of the function.
                canComputeNewScene = true;
                break;
            default:
                Log("Invalid scene compute state: " + std::to_string(computeState));
                // Compute a new scene at the end of the function.
                canComputeNewScene = true;
                break;
            }

            if (canComputeNewScene)
            {
                // Compute a new scene.
                XrVisualMeshComputeLodInfoMSFT visualMeshComputeLodInfo;
                visualMeshComputeLodInfo.type = XrStructureType::XR_TYPE_VISUAL_MESH_COMPUTE_LOD_INFO_MSFT;
                visualMeshComputeLodInfo.next = NULL;
                visualMeshComputeLodInfo.lod = s_MeshComputeLod;
                std::vector<XrSceneComputeFeatureMSFT> sceneComputeFeatures = {XrSceneComputeFeatureMSFT::XR_SCENE_COMPUTE_FEATURE_VISUAL_MESH_MSFT};
                XrNewSceneComputeInfoMSFT newSceneComputeInfo;
                newSceneComputeInfo.type = XrStructureType::XR_TYPE_NEW_SCENE_COMPUTE_INFO_MSFT;
                newSceneComputeInfo.next = &visualMeshComputeLodInfo;
                newSceneComputeInfo.requestedFeatureCount = (uint32_t) sceneComputeFeatures.size();
                newSceneComputeInfo.requestedFeatures = sceneComputeFeatures.data();
                newSceneComputeInfo.consistency = s_SceneComputeConsistency;
                newSceneComputeInfo.bounds.sphereCount = (uint32_t) s_SceneSphereBounds.size();
                newSceneComputeInfo.bounds.spheres = s_SceneSphereBounds.data();
                newSceneComputeInfo.bounds.boxCount =  (uint32_t) s_SceneOrientedBoxBounds.size();
                newSceneComputeInfo.bounds.boxes = s_SceneOrientedBoxBounds.data();
                newSceneComputeInfo.bounds.frustumCount = (uint32_t) s_SceneFrustumBounds.size();
                newSceneComputeInfo.bounds.frustums = s_SceneFrustumBounds.data();
                CheckResult(s_xrComputeNewSceneMSFT(s_SceneObserver, &newSceneComputeInfo), "xrComputeNewSceneMSFT");
            }

            // Allocate an output array and copy mesh infos to it.
            auto pMeshInfos = s_Meshing->MeshInfoAllocator_Allocate(allocator, s_MeshDataByMeshId.size());
            size_t meshInfoIndex = 0;
            for (auto&& pair : s_MeshDataByMeshId)
            {
                pMeshInfos[meshInfoIndex] = pair.second.m_UnityXRMeshInfo;
                ++meshInfoIndex;
            }

            return kUnitySubsystemErrorCodeSuccess;
        };
        meshProvider.AcquireMesh = [](UnitySubsystemHandle handle, void* userData, const UnityXRMeshId* meshId, UnityXRMeshDataAllocator* allocator) -> UnitySubsystemErrorCode {
            // Get mesh data from the input mesh id.
            MeshData* pMeshData = nullptr;
            try
            {
                pMeshData = &s_MeshDataByMeshId.at(*meshId);
            }
            catch(const std::exception& e)
            {
                Log("Mesh id not found: " + std::string(e.what()));
                return UnitySubsystemErrorCode::kUnitySubsystemErrorCodeFailure;
            }

            // Check if the shared OpenXR scene is not null.
            if (pMeshData->m_SharedOpenXRScene == nullptr)
            {
                // Mesh data with null shared scene implies that a mesh
                // of the mesh data has been acquired before.
                return UnitySubsystemErrorCode::kUnitySubsystemErrorCodeFailure;
            }

            // Stage 2: Get mesh buffers of the first scene visual mesh component.

            XrSceneMeshBuffersGetInfoMSFT sceneMeshBuffersGetInfo;
            sceneMeshBuffersGetInfo.type = XrStructureType::XR_TYPE_SCENE_MESH_BUFFERS_GET_INFO_MSFT;
            sceneMeshBuffersGetInfo.next = NULL;
            sceneMeshBuffersGetInfo.meshBufferId = pMeshData->m_OpenXRSceneMesh.meshBufferId;
            // Create buffers on the structure chain of XrSceneMeshBuffersMSFT.
            // Set input capacity to zero to get buffer capacity.
            XrSceneMeshBuffersMSFT sceneMeshBuffers;
            sceneMeshBuffers.type = XrStructureType::XR_TYPE_SCENE_MESH_BUFFERS_MSFT;
            XrSceneMeshVertexBufferMSFT sceneMeshVerticesBuffer;
            sceneMeshVerticesBuffer.type = XrStructureType::XR_TYPE_SCENE_MESH_VERTEX_BUFFER_MSFT;
            sceneMeshVerticesBuffer.vertexCapacityInput = 0;
            sceneMeshVerticesBuffer.vertices = NULL;
            XrSceneMeshIndicesUint32MSFT sceneMeshIndicesUint32Buffer;
            sceneMeshIndicesUint32Buffer.type = XrStructureType::XR_TYPE_SCENE_MESH_INDICES_UINT32_MSFT;
            sceneMeshIndicesUint32Buffer.indexCapacityInput = 0;
            sceneMeshIndicesUint32Buffer.indices = NULL;
            // Chain the structure instances.
            sceneMeshBuffers.next = &sceneMeshVerticesBuffer;
            sceneMeshVerticesBuffer.next = &sceneMeshIndicesUint32Buffer;
            sceneMeshIndicesUint32Buffer.next = NULL;
            // Call xrGetSceneMeshBuffersMSFT() to get buffer capacity.
            CheckResult(s_xrGetSceneMeshBuffersMSFT(pMeshData->m_SharedOpenXRScene->GetScene(),
                &sceneMeshBuffersGetInfo, &sceneMeshBuffers), "xrGetSceneMeshBuffersMSFT");
            // Create buffers by the capacity.
            std::vector<XrVector3f> vertices(sceneMeshVerticesBuffer.vertexCountOutput);
            std::vector<uint32_t> indices(sceneMeshIndicesUint32Buffer.indexCountOutput);
            sceneMeshVerticesBuffer.vertexCapacityInput = sceneMeshVerticesBuffer.vertexCountOutput;
            sceneMeshVerticesBuffer.vertices = vertices.data();
            sceneMeshIndicesUint32Buffer.indexCapacityInput = sceneMeshIndicesUint32Buffer.indexCountOutput;
            sceneMeshIndicesUint32Buffer.indices = indices.data();
            // Call xrGetSceneMeshBuffersMSFT() again to fill out buffers.
            CheckResult(s_xrGetSceneMeshBuffersMSFT(pMeshData->m_SharedOpenXRScene->GetScene(),
                &sceneMeshBuffersGetInfo, &sceneMeshBuffers), "xrGetSceneMeshBuffersMSFT");

            // After getting OpenXR mesh buffers, reset the shared pointer to the scene
            // to release the scene. The scene will be destroyed when no shared pointers
            // pointing to it.
            pMeshData->m_SharedOpenXRScene = nullptr;

            // Test
            // sceneMeshVerticesBuffer.vertices[0] = {0, 0, 0};
            // sceneMeshVerticesBuffer.vertices[1] = {0, 0, 1};
            // sceneMeshVerticesBuffer.vertices[2] = {1, 0, 0};
            // sceneMeshIndicesUint32Buffer.indices[0] = 0;
            // sceneMeshIndicesUint32Buffer.indices[1] = 1;
            // sceneMeshIndicesUint32Buffer.indices[2] = 2;

            // Now the buffers are filled with mesh data.
            // Copy data to the Unity XR mesh descriptor.
            auto& verticesCount = sceneMeshVerticesBuffer.vertexCountOutput;
            auto& indicesCount = sceneMeshIndicesUint32Buffer.indexCountOutput;
            auto* meshDesc = s_Meshing->MeshDataAllocator_AllocateMesh(allocator,
                verticesCount, indicesCount, kUnityXRIndexFormat32Bit,
                (UnityXRMeshVertexAttributeFlags) 0,kUnityXRMeshTopologyTriangles);
            memcpy(meshDesc->positions, sceneMeshVerticesBuffer.vertices, verticesCount * sizeof(float) * 3);
            memcpy(meshDesc->indices32, sceneMeshIndicesUint32Buffer.indices, indicesCount * sizeof(uint32_t));

            // Convert meshes from right-handed to left-handed.
            for (size_t i = 0; i < verticesCount; ++i)
            {
                // Multiply the z value by -1.
                meshDesc->positions[i].z *= -1.0f;
            }
            for (size_t i = 0; i < indicesCount; i += 3)
            {
                // Swap the second and the third index in a triangle.
                std::swap(meshDesc->indices32[i + 1],  meshDesc->indices32[i + 2]);
            }

            // int numVerts = sizeof(positionsTeapot) / sizeof(float) / 3;
            // int numIndices = sizeof(indicesTeapot) / sizeof(uint16_t);
            // auto* meshDesc = s_Meshing->MeshDataAllocator_AllocateMesh(allocator, numVerts, numIndices, kUnityXRIndexFormat16Bit, (UnityXRMeshVertexAttributeFlags)(kUnityXRMeshVertexAttributeFlagsNormals | kUnityXRMeshVertexAttributeFlagsUvs), kUnityXRMeshTopologyTriangles);
            // memcpy(meshDesc->positions, positionsTeapot, numVerts * sizeof(float) * 3);
            // memcpy(meshDesc->normals, normalsTeapot, numVerts * sizeof(float) * 3);
            // memcpy(meshDesc->uvs, uvsTeapot, (numVerts * 2) * sizeof(float));
            // memcpy(meshDesc->indices16, indicesTeapot, numIndices * sizeof(uint16_t));
            return kUnitySubsystemErrorCodeSuccess;
        };
        meshProvider.ReleaseMesh = [](UnitySubsystemHandle handle, void* userData, const UnityXRMeshId* meshId, const UnityXRMeshDescriptor* mesh, void* pluginData) -> UnitySubsystemErrorCode {
            return kUnitySubsystemErrorCodeSuccess;
        };
        meshProvider.SetMeshDensity = [](UnitySubsystemHandle handle, void* userData, float density) -> UnitySubsystemErrorCode {
            return kUnitySubsystemErrorCodeSuccess;
        };
        meshProvider.SetBoundingVolume = [](UnitySubsystemHandle handle, void* userData, const UnityXRBoundingVolume* boundingVolume) -> UnitySubsystemErrorCode {
            return kUnitySubsystemErrorCodeSuccess;
        };
        s_Meshing->RegisterMeshProvider(handle, &meshProvider);
        return kUnitySubsystemErrorCodeSuccess;
    };
    meshingLifecycleHandler.Start = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        // Create a scene observer.
        XrSceneObserverCreateInfoMSFT sceneObserverCreateInfo;
        sceneObserverCreateInfo.type = XrStructureType::XR_TYPE_SCENE_OBSERVER_CREATE_INFO_MSFT;
        sceneObserverCreateInfo.next = NULL;
        CheckResult(s_xrCreateSceneObserverMSFT(s_XrSession, &sceneObserverCreateInfo, &s_SceneObserver), "xrCreateSceneObserverMSFT");

        return kUnitySubsystemErrorCodeSuccess;
    };
    meshingLifecycleHandler.Stop = [](UnitySubsystemHandle handle, void* userData) -> void {
        // Clear mesh data.
        // All pointed shared OpenXR scenes are also destroyed.
        s_MeshDataByMeshId.clear();

        // Destroy the scene observer.
        CheckResult(s_xrDestroySceneObserverMSFT(s_SceneObserver), "xrDestroySceneObserverMSFT");
    };
    meshingLifecycleHandler.Shutdown = [](UnitySubsystemHandle handle, void* userData) -> void {
        s_OpenXRReady = false;

        // Destroy the reference space.
        CheckResult(s_xrDestroySpace(s_XrSpace), "xrDestroySpace");
    };

    s_Meshing->RegisterLifecycleProvider("OpenXR Extension Sample", "Sample Meshing", &meshingLifecycleHandler);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginUnload()
{
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetOpenXRVariables(unsigned long long instance, unsigned long long session,
    void* xrEnumerateReferenceSpaces,
    void* xrCreateReferenceSpace,
    void* xrDestroySpace,
    void* xrEnumerateSceneComputeFeaturesMSFTptr,
    void* xrCreateSceneObserverMSFTptr,
    void* xrDestroySceneObserverMSFTptr,
    void* xrCreateSceneMSFTptr,
    void* xrDestroySceneMSFTptr,
    void* xrComputeNewSceneMSFTptr,
    void* xrGetSceneComputeStateMSFTptr,
    void* xrGetSceneComponentsMSFTptr,
    void* xrLocateSceneComponentsMSFTptr,
    void* xrGetSceneMeshBuffersMSFTptr)
{
    s_XrInstance = (XrInstance)instance;
    s_XrSession = (XrSession)session;
    
    s_xrEnumerateReferenceSpaces = (PFN_xrEnumerateReferenceSpaces)xrEnumerateReferenceSpaces;
    s_xrCreateReferenceSpace = (PFN_xrCreateReferenceSpace)xrCreateReferenceSpace;
    s_xrDestroySpace = (PFN_xrDestroySpace)xrDestroySpace;
    s_xrEnumerateSceneComputeFeaturesMSFT = (PFN_xrEnumerateSceneComputeFeaturesMSFT)xrEnumerateSceneComputeFeaturesMSFTptr;
    s_xrCreateSceneObserverMSFT = (PFN_xrCreateSceneObserverMSFT)xrCreateSceneObserverMSFTptr;
    s_xrDestroySceneObserverMSFT = (PFN_xrDestroySceneObserverMSFT)xrDestroySceneObserverMSFTptr;
    s_xrCreateSceneMSFT = (PFN_xrCreateSceneMSFT)xrCreateSceneMSFTptr;
    s_xrDestroySceneMSFT = (PFN_xrDestroySceneMSFT)xrDestroySceneMSFTptr;
    s_xrComputeNewSceneMSFT = (PFN_xrComputeNewSceneMSFT)xrComputeNewSceneMSFTptr;
    s_xrGetSceneComputeStateMSFT = (PFN_xrGetSceneComputeStateMSFT)xrGetSceneComputeStateMSFTptr;
    s_xrGetSceneComponentsMSFT = (PFN_xrGetSceneComponentsMSFT)xrGetSceneComponentsMSFTptr;
    s_xrLocateSceneComponentsMSFT = (PFN_xrLocateSceneComponentsMSFT)xrLocateSceneComponentsMSFTptr;
    s_xrGetSceneMeshBuffersMSFT = (PFN_xrGetSceneMeshBuffersMSFT)xrGetSceneMeshBuffersMSFTptr;
    s_XrInstance = (XrInstance) instance;
    s_XrSession = (XrSession) session;
 
    // Enumerate supported reference space types.
    std::vector<XrReferenceSpaceType> supportedReferenceSpaceTypes;
    uint32_t supportedReferenceSpaceTypeCount = 0;
    CheckResult(s_xrEnumerateReferenceSpaces(s_XrSession, 0, &supportedReferenceSpaceTypeCount,
        supportedReferenceSpaceTypes.data()), "xrEnumerateReferenceSpaces");
    supportedReferenceSpaceTypes.resize(supportedReferenceSpaceTypeCount);
    CheckResult(s_xrEnumerateReferenceSpaces(s_XrSession, (uint32_t) supportedReferenceSpaceTypes.size(), &supportedReferenceSpaceTypeCount,
        supportedReferenceSpaceTypes.data()), "xrEnumerateReferenceSpaces");
    // Get a supported reference space type. Prefer the stage space type.
    for (auto&& type : supportedReferenceSpaceTypes)
    {
        s_ReferenceSpaceType = type;
        if (type == XrReferenceSpaceType::XR_REFERENCE_SPACE_TYPE_STAGE)
        {
            break;
        }
    }
    // Create a reference space.
    XrReferenceSpaceCreateInfo referenceSpaceCreateInfo;
    referenceSpaceCreateInfo.type = XrStructureType::XR_TYPE_REFERENCE_SPACE_CREATE_INFO;
    referenceSpaceCreateInfo.next = NULL;
    referenceSpaceCreateInfo.referenceSpaceType = s_ReferenceSpaceType;
    referenceSpaceCreateInfo.poseInReferenceSpace.orientation = {0, 0, 0, 1};
    referenceSpaceCreateInfo.poseInReferenceSpace.position = {0, 0, 0};
    CheckResult(s_xrCreateReferenceSpace(s_XrSession, &referenceSpaceCreateInfo, &s_XrSpace), "xrCreateReferenceSpace");
    s_OpenXRReady = true;
}

/**
 * Set a scene compute sphere bound in a right-handed world space.
 * Existing sphere bound will be replaced.
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetSceneComputeSphereBound(XrVector3f center, float radius)
{
    s_SceneSphereBounds.resize(1);
    auto& bound = s_SceneSphereBounds[0];
    bound.center = center;
    bound.radius = radius;
}

/**
 * Set a scene compute oriented box bound in a right-handed world space.
 * Existing oriented box bound will be replaced.
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetSceneComputeOrientedBoxBound(XrQuaternionf orientation, XrVector3f position, XrVector3f extents)
{
    s_SceneOrientedBoxBounds.resize(1);
    auto& bound = s_SceneOrientedBoxBounds[0];
    bound.pose.orientation = orientation;
    bound.pose.position = position;
    bound.extents = extents;
}

/**
 * Set a scene compute oriented box bound in a right-handed world space.
 * Existing frustum bound will be replaced.
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetSceneComputeFrustumBound(XrQuaternionf orientation, XrVector3f position,
    float angleUp, float angleDown, float angleRight, float angleLeft, float farDistance)
{
    s_SceneFrustumBounds.resize(1);
    auto& bound = s_SceneFrustumBounds[0];
    bound.pose.orientation = orientation;
    bound.pose.position = position;
    bound.fov.angleUp = angleUp;
    bound.fov.angleDown = angleDown;
    bound.fov.angleRight = angleRight;
    bound.fov.angleLeft = angleLeft;
    bound.farDistance = farDistance;
}

enum XrSceneBoundType
{
    XR_SCENE_BOUND_SPHERE_TYPE = 1,
    XR_SCENE_BOUND_ORIENTED_BOX_TYPE = 2,
    XR_SCENE_BOUND_FRUSTUM_TYPE = 3,
    XR_SCENE_BOUND_MAX = 4
};

/**
 * Clear scene compute bounds of a specified type.
 */
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
ClearSceneComputeBounds(XrSceneBoundType type)
{
    switch (type)
    {
    case XrSceneBoundType::XR_SCENE_BOUND_SPHERE_TYPE:
        s_SceneSphereBounds.clear();
        break;
    case XrSceneBoundType::XR_SCENE_BOUND_ORIENTED_BOX_TYPE:
        s_SceneOrientedBoxBounds.clear();
        break;
    case XrSceneBoundType::XR_SCENE_BOUND_FRUSTUM_TYPE:
        s_SceneFrustumBounds.clear();
        break;
    default:
        break;
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetSceneComputeConsistency(XrSceneComputeConsistencyMSFT consistency)
{
    s_SceneComputeConsistency = consistency;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
SetMeshComputeLod(XrMeshComputeLodMSFT lod)
{
    s_MeshComputeLod = lod;
}
