using System;
using System.Collections;
using UnityEngine;

namespace VIVE.OpenXR.SceneUnderstanding
{
    public enum XrSceneComputeConsistencyMSFT
    {
        SnapshotComplete = 1,
        SnapshotIncompleteFast = 2,
        OcclusionOptimized = 3,
    }

    public enum XrSceneBoundType
    {
        Sphere = 1,
        OrientedBox = 2,
        Frustum = 3,
    }

    public enum XrSceneComputeFeatureMSFT
    {
        XR_SCENE_COMPUTE_FEATURE_PLANE_MSFT = 1,
        XR_SCENE_COMPUTE_FEATURE_PLANE_MESH_MSFT = 2,
        XR_SCENE_COMPUTE_FEATURE_VISUAL_MESH_MSFT = 3,
        XR_SCENE_COMPUTE_FEATURE_COLLIDER_MESH_MSFT = 4,
        XR_SCENE_COMPUTE_FEATURE_SERIALIZE_SCENE_MSFT = 1000098000,
        XR_SCENE_COMPUTE_FEATURE_OCCLUSION_HINT_MSFT = 1000099000,
        XR_SCENE_COMPUTE_FEATURE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    public enum XrMeshComputeLodMSFT
    {
        Coarse = 1,
        Medium = 2,
        Fine = 3,
        Unlimited = 4,
    }

    public enum XrSceneComponentTypeMSFT
    {
        XR_SCENE_COMPONENT_TYPE_INVALID_MSFT = -1,
        XR_SCENE_COMPONENT_TYPE_OBJECT_MSFT = 1,
        XR_SCENE_COMPONENT_TYPE_PLANE_MSFT = 2,
        XR_SCENE_COMPONENT_TYPE_VISUAL_MESH_MSFT = 3,
        XR_SCENE_COMPONENT_TYPE_COLLIDER_MESH_MSFT = 4,
        XR_SCENE_COMPONENT_TYPE_SERIALIZED_SCENE_FRAGMENT_MSFT = 1000098000,
        XR_SCENE_COMPONENT_TYPE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    public enum XrSceneObjectTypeMSFT
    {
        XR_SCENE_OBJECT_TYPE_UNCATEGORIZED_MSFT = -1,
        XR_SCENE_OBJECT_TYPE_BACKGROUND_MSFT = 1,
        XR_SCENE_OBJECT_TYPE_WALL_MSFT = 2,
        XR_SCENE_OBJECT_TYPE_FLOOR_MSFT = 3,
        XR_SCENE_OBJECT_TYPE_CEILING_MSFT = 4,
        XR_SCENE_OBJECT_TYPE_PLATFORM_MSFT = 5,
        XR_SCENE_OBJECT_TYPE_INFERRED_MSFT = 6,
        XR_SCENE_OBJECT_TYPE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    public enum XrScenePlaneAlignmentTypeMSFT
    {
        XR_SCENE_PLANE_ALIGNMENT_TYPE_NON_ORTHOGONAL_MSFT = 0,
        XR_SCENE_PLANE_ALIGNMENT_TYPE_HORIZONTAL_MSFT = 1,
        XR_SCENE_PLANE_ALIGNMENT_TYPE_VERTICAL_MSFT = 2,
        XR_SCENE_PLANE_ALIGNMENT_TYPE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    public enum XrSceneComputeStateMSFT
    {
        XR_SCENE_COMPUTE_STATE_NONE_MSFT = 0,
        XR_SCENE_COMPUTE_STATE_UPDATING_MSFT = 1,
        XR_SCENE_COMPUTE_STATE_COMPLETED_MSFT = 2,
        XR_SCENE_COMPUTE_STATE_COMPLETED_WITH_ERROR_MSFT = 3,
        XR_SCENE_COMPUTE_STATE_MAX_ENUM_MSFT = 0x7FFFFFFF
    }

    public struct XrUuidMSFT
    {
        public byte byte0;
        public byte byte1;
        public byte byte2;
        public byte byte3;
        public byte byte4;
        public byte byte5;
        public byte byte6;
        public byte byte7;
        public byte byte8;
        public byte byte9;
        public byte byte10;
        public byte byte11;
        public byte byte12;
        public byte byte13;
        public byte byte14;
        public byte byte15;
        public byte byte16;

    }

    public struct XrSceneObserverCreateInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
    }

    public struct XrSceneCreateInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
    }

    public struct XrSceneSphereBoundMSFT
    {
        public XrVector3f center;
        public float radius;
    }

    public struct XrSceneOrientedBoxBoundMSFT
    {
        public XrPosef pose;
        public XrVector3f extents;
    }

    public struct XrSceneFrustumBoundMSFT
    {
        public XrPosef pose;
        public XrFovf fov;
        public float farDistance;
    }

    public struct XrSceneBoundsMSFT
    {
        public ulong space;
        public long time;
        public uint sphereCount;

        // XrSceneSphereBoundMSFT
        public IntPtr spheres;
        public uint boxCount;

        // XrSceneOrientedBoxBoundMSFT
        public IntPtr boxes;
        public uint frustumCount;

        // XrSceneFrustumBoundMSFT
        public IntPtr frustums;
    }

    public struct XrNewSceneComputeInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint requestedFeatureCount;

        // XrSceneComputeFeatureMSFT array
        public IntPtr requestedFeatures;
        public uint disableInferredSceneObjects;
        public XrSceneBoundsMSFT bounds;
    }

    // XrVisualMeshComputeLodInfoMSFT extends XrNewSceneComputeInfoMSFT
    public struct XrVisualMeshComputeLodInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public XrMeshComputeLodMSFT lod;
    }

    public struct XrSceneComponentMSFT
    {
        public XrSceneComponentTypeMSFT componentType;
        public XrUuidMSFT componentId;
        public XrUuidMSFT parentObjectId;
        public long updateTime;
    }

    public struct XrSceneComponentsMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint componentCapacityInput;
        public uint componentCountOutput;

        // XrSceneComponentMSFT array
        public IntPtr components;
    }

    public struct XrSceneComponentsGetInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public XrSceneComponentTypeMSFT componentType;
    }

    public struct XrSceneComponentLocationMSFT
    {
        public ulong flags;
        public XrPosef pose;
    }

    public struct XrSceneComponentLocationsMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint locationCount;

        // XrSceneComponentLocationMSFT array
        public IntPtr locations;
    }

    public struct XrSceneComponentsLocateInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;

        // XrSpace
        public ulong baseSpace;

        // XrTime
        public long time;
        public uint idCount;

        // XrUuidMSFT array
        public IntPtr ids;
    }

    public struct XrSceneObjectMSFT
    {
        public XrSceneObjectTypeMSFT objectType;
    }

    // XrSceneObjectsMSFT extends XrSceneComponentsMSFT
    public struct XrSceneObjectsMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint sceneObjectCount;

        // XrSceneObjectMSFT array
        public IntPtr sceneObjects;
    }

    // XrSceneComponentParentFilterInfoMSFT extends XrSceneComponentsGetInfoMSFT
    public struct XrSceneComponentParentFilterInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public XrUuidMSFT parentObjectId;
    }

    // XrSceneObjectTypesFilterInfoMSFT extends XrSceneComponentsGetInfoMSFT
    public struct XrSceneObjectTypesFilterInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint objectTypeCount;

        // XrSceneObjectTypeMSFT array
        public IntPtr objectTypes;
    }

    public struct XrScenePlaneMSFT
    {
        public XrScenePlaneAlignmentTypeMSFT alignment;
        public XrExtent2Df size;
        public ulong meshBufferId;

        // XrBool32
        public uint supportsIndicesUint16;
    }

    // XrScenePlanesMSFT extends XrSceneComponentsMSFT
    public struct XrScenePlanesMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint scenePlaneCount;

        // XrScenePlaneMSFT array
        public IntPtr scenePlanes;
    }

    // XrScenePlaneAlignmentFilterInfoMSFT extends XrSceneComponentsGetInfoMSFT
    public struct XrScenePlaneAlignmentFilterInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint alignmentCount;

        // XrScenePlaneAlignmentTypeMSFT array
        public IntPtr alignments;
    }

    public struct XrSceneMeshMSFT
    {
        public ulong meshBufferId;

        // XrBool32
        public uint supportsIndicesUint16;
    }

    // XrSceneMeshesMSFT extends XrSceneComponentsMSFT
    public struct XrSceneMeshesMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint sceneMeshCount;

        // XrSceneMeshMSFT array
        public IntPtr sceneMeshes;
    }

    public struct XrSceneMeshBuffersGetInfoMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public ulong meshBufferId;
    }

    public struct XrSceneMeshBuffersMSFT
    {
        public XrStructureType type;
        public IntPtr next;
    }

    public struct XrSceneMeshVertexBufferMSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint vertexCapacityInput;
        public uint vertexCountOutput;

        // XrVector3f array
        public IntPtr vertices;
    }

    public struct XrSceneMeshIndicesUint32MSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint indexCapacityInput;
        public uint indexCountOutput;

        // uint32_t array
        public IntPtr indices;
    }

    public struct XrSceneMeshIndicesUint16MSFT
    {
        public XrStructureType type;
        public IntPtr next;
        public uint indexCapacityInput;
        public uint indexCountOutput;

        // uint16_t array
        public IntPtr indices;
    }

    public struct XrSystemPassThroughCameraInfoHTC
    {
        public float focalLengthX;
        public float focalLengthY;
        public float opticalCenterX;
        public float opticalCenterY;
        public uint imageWidth;
        public uint imageHeight;
        public uint imageChannelCount;
    }
    public struct XrSystemPassThroughPropertiesHTC
    {
        public XrStructureType type;
        public IntPtr next;
        public uint supportsPassThrough;
        XrSystemPassThroughCameraInfoHTC leftCameraInfo;
        XrSystemPassThroughCameraInfoHTC rightCameraInfo;
        public int deviceType;
        public long format;
    }

    delegate int xrGetInstanceProcDelegate(ulong instance, string name, out IntPtr function);
    public static class ViveSceneUnderstandingHelper
    {

    }
}