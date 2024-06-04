// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace VIVE.OpenXR
{
	public struct XrFoveationConfigurationHTC
	{
		public XrFoveationLevelHTC level;
		public float clearFovDegree;
		public XrVector2f focalCenterOffset;
	}
	public enum XrFoveationModeHTC
	{
		XR_FOVEATION_MODE_DISABLE_HTC = 0,
		XR_FOVEATION_MODE_FIXED_HTC = 1,
		XR_FOVEATION_MODE_DYNAMIC_HTC = 2,
		XR_FOVEATION_MODE_CUSTOM_HTC = 3,
		XR_FOVEATION_MODE_MAX_ENUM_HTC = 0x7FFFFFFF
	}
	public enum XrFoveationLevelHTC
	{
		XR_FOVEATION_LEVEL_NONE_HTC = 0,
		XR_FOVEATION_LEVEL_LOW_HTC = 1,
		XR_FOVEATION_LEVEL_MEDIUM_HTC = 2,
		XR_FOVEATION_LEVEL_HIGH_HTC = 3,
		XR_FOVEATION_LEVEL_MAX_ENUM_HTC = 0x7FFFFFFF
	}
    /// <summary>
    /// Defines values, each of which corresponds to a specific OpenXR handle type. These values can be used to associate debug information with a particular type of object through one or more extensions.
    /// </summary>
    public enum XrObjectType
    {
        XR_OBJECT_TYPE_UNKNOWN = 0,
        XR_OBJECT_TYPE_INSTANCE = 1,
        XR_OBJECT_TYPE_SESSION = 2,
        XR_OBJECT_TYPE_SWAPCHAIN = 3,
        XR_OBJECT_TYPE_SPACE = 4,
        XR_OBJECT_TYPE_ACTION_SET = 5,
        XR_OBJECT_TYPE_ACTION = 6,
        XR_OBJECT_TYPE_DEBUG_UTILS_MESSENGER_EXT = 1000019000,
        XR_OBJECT_TYPE_SPATIAL_ANCHOR_MSFT = 1000039000,
        XR_OBJECT_TYPE_HAND_TRACKER_EXT = 1000051000,
        XR_OBJECT_TYPE_SCENE_OBSERVER_MSFT = 1000097000,
        XR_OBJECT_TYPE_SCENE_MSFT = 1000097001,
        XR_OBJECT_TYPE_FACIAL_TRACKER_HTC = 1000104000,
        XR_OBJECT_TYPE_FOVEATION_PROFILE_FB = 1000114000,
        XR_OBJECT_TYPE_TRIANGLE_MESH_FB = 1000117000,
        XR_OBJECT_TYPE_PASSTHROUGH_FB = 1000118000,
        XR_OBJECT_TYPE_PASSTHROUGH_LAYER_FB = 1000118002,
        XR_OBJECT_TYPE_GEOMETRY_INSTANCE_FB = 1000118004,
        XR_OBJECT_TYPE_SPATIAL_ANCHOR_STORE_CONNECTION_MSFT = 1000142000,
        XR_OBJECT_TYPE_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// All return codes in the OpenXR Core API are reported via XrResult return values.
    /// </summary>
    public enum XrResult
    {
        XR_SUCCESS = 0,
        XR_TIMEOUT_EXPIRED = 1,
        XR_SESSION_LOSS_PENDING = 3,
        XR_EVENT_UNAVAILABLE = 4,
        XR_SPACE_BOUNDS_UNAVAILABLE = 7,
        XR_SESSION_NOT_FOCUSED = 8,
        XR_FRAME_DISCARDED = 9,
        XR_ERROR_VALIDATION_FAILURE = -1,
        XR_ERROR_RUNTIME_FAILURE = -2,
        XR_ERROR_OUT_OF_MEMORY = -3,
        XR_ERROR_API_VERSION_UNSUPPORTED = -4,
        XR_ERROR_INITIALIZATION_FAILED = -6,
        XR_ERROR_FUNCTION_UNSUPPORTED = -7,
        XR_ERROR_FEATURE_UNSUPPORTED = -8,
        XR_ERROR_EXTENSION_NOT_PRESENT = -9,
        XR_ERROR_LIMIT_REACHED = -10,
        XR_ERROR_SIZE_INSUFFICIENT = -11,
        XR_ERROR_HANDLE_INVALID = -12,
        XR_ERROR_INSTANCE_LOST = -13,
        XR_ERROR_SESSION_RUNNING = -14,
        XR_ERROR_SESSION_NOT_RUNNING = -16,
        XR_ERROR_SESSION_LOST = -17,
        XR_ERROR_SYSTEM_INVALID = -18,
        XR_ERROR_PATH_INVALID = -19,
        XR_ERROR_PATH_COUNT_EXCEEDED = -20,
        XR_ERROR_PATH_FORMAT_INVALID = -21,
        XR_ERROR_PATH_UNSUPPORTED = -22,
        XR_ERROR_LAYER_INVALID = -23,
        XR_ERROR_LAYER_LIMIT_EXCEEDED = -24,
        XR_ERROR_SWAPCHAIN_RECT_INVALID = -25,
        XR_ERROR_SWAPCHAIN_FORMAT_UNSUPPORTED = -26,
        XR_ERROR_ACTION_TYPE_MISMATCH = -27,
        XR_ERROR_SESSION_NOT_READY = -28,
        XR_ERROR_SESSION_NOT_STOPPING = -29,
        XR_ERROR_TIME_INVALID = -30,
        XR_ERROR_REFERENCE_SPACE_UNSUPPORTED = -31,
        XR_ERROR_FILE_ACCESS_ERROR = -32,
        XR_ERROR_FILE_CONTENTS_INVALID = -33,
        XR_ERROR_FORM_FACTOR_UNSUPPORTED = -34,
        XR_ERROR_FORM_FACTOR_UNAVAILABLE = -35,
        XR_ERROR_API_LAYER_NOT_PRESENT = -36,
        XR_ERROR_CALL_ORDER_INVALID = -37,
        XR_ERROR_GRAPHICS_DEVICE_INVALID = -38,
        XR_ERROR_POSE_INVALID = -39,
        XR_ERROR_INDEX_OUT_OF_RANGE = -40,
        XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED = -41,
        XR_ERROR_ENVIRONMENT_BLEND_MODE_UNSUPPORTED = -42,
        XR_ERROR_NAME_DUPLICATED = -44,
        XR_ERROR_NAME_INVALID = -45,
        XR_ERROR_ACTIONSET_NOT_ATTACHED = -46,
        XR_ERROR_ACTIONSETS_ALREADY_ATTACHED = -47,
        XR_ERROR_LOCALIZED_NAME_DUPLICATED = -48,
        XR_ERROR_LOCALIZED_NAME_INVALID = -49,
        XR_ERROR_GRAPHICS_REQUIREMENTS_CALL_MISSING = -50,
        XR_ERROR_RUNTIME_UNAVAILABLE = -51,
        XR_ERROR_ANDROID_THREAD_SETTINGS_ID_INVALID_KHR = -1000003000,
        XR_ERROR_ANDROID_THREAD_SETTINGS_FAILURE_KHR = -1000003001,
        XR_ERROR_CREATE_SPATIAL_ANCHOR_FAILED_MSFT = -1000039001,
        XR_ERROR_SECONDARY_VIEW_CONFIGURATION_TYPE_NOT_ENABLED_MSFT = -1000053000,
        XR_ERROR_CONTROLLER_MODEL_KEY_INVALID_MSFT = -1000055000,
        XR_ERROR_REPROJECTION_MODE_UNSUPPORTED_MSFT = -1000066000,
        XR_ERROR_COMPUTE_NEW_SCENE_NOT_COMPLETED_MSFT = -1000097000,
        XR_ERROR_SCENE_COMPONENT_ID_INVALID_MSFT = -1000097001,
        XR_ERROR_SCENE_COMPONENT_TYPE_MISMATCH_MSFT = -1000097002,
        XR_ERROR_SCENE_MESH_BUFFER_ID_INVALID_MSFT = -1000097003,
        XR_ERROR_SCENE_COMPUTE_FEATURE_INCOMPATIBLE_MSFT = -1000097004,
        XR_ERROR_SCENE_COMPUTE_CONSISTENCY_MISMATCH_MSFT = -1000097005,
        XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB = -1000101000,
        XR_ERROR_COLOR_SPACE_UNSUPPORTED_FB = -1000108000,
        XR_ERROR_SPATIAL_ANCHOR_NAME_NOT_FOUND_MSFT = -1000142001,
        XR_ERROR_SPATIAL_ANCHOR_NAME_INVALID_MSFT = -1000142002,
        XR_RESULT_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// Any parameter that is a structure containing a type member must have a value of type which is a valid XrStructureType value matching the type of the structure.
    /// </summary>
    public enum XrStructureType
    {
        XR_TYPE_UNKNOWN = 0,
        XR_TYPE_API_LAYER_PROPERTIES = 1,
        XR_TYPE_EXTENSION_PROPERTIES = 2,
        XR_TYPE_INSTANCE_CREATE_INFO = 3,
        XR_TYPE_SYSTEM_GET_INFO = 4,
        XR_TYPE_SYSTEM_PROPERTIES = 5,
        XR_TYPE_VIEW_LOCATE_INFO = 6,
        XR_TYPE_VIEW = 7,
        XR_TYPE_SESSION_CREATE_INFO = 8,
        XR_TYPE_SWAPCHAIN_CREATE_INFO = 9,
        XR_TYPE_SESSION_BEGIN_INFO = 10,
        XR_TYPE_VIEW_STATE = 11,
        XR_TYPE_FRAME_END_INFO = 12,
        XR_TYPE_HAPTIC_VIBRATION = 13,
        XR_TYPE_EVENT_DATA_BUFFER = 16,
        XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING = 17,
        XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED = 18,
        XR_TYPE_ACTION_STATE_BOOLEAN = 23,
        XR_TYPE_ACTION_STATE_FLOAT = 24,
        XR_TYPE_ACTION_STATE_VECTOR2F = 25,
        XR_TYPE_ACTION_STATE_POSE = 27,
        XR_TYPE_ACTION_SET_CREATE_INFO = 28,
        XR_TYPE_ACTION_CREATE_INFO = 29,
        XR_TYPE_INSTANCE_PROPERTIES = 32,
        XR_TYPE_FRAME_WAIT_INFO = 33,
        XR_TYPE_COMPOSITION_LAYER_PROJECTION = 35,
        XR_TYPE_COMPOSITION_LAYER_QUAD = 36,
        XR_TYPE_REFERENCE_SPACE_CREATE_INFO = 37,
        XR_TYPE_ACTION_SPACE_CREATE_INFO = 38,
        XR_TYPE_EVENT_DATA_REFERENCE_SPACE_CHANGE_PENDING = 40,
        XR_TYPE_VIEW_CONFIGURATION_VIEW = 41,
        XR_TYPE_SPACE_LOCATION = 42,
        XR_TYPE_SPACE_VELOCITY = 43,
        XR_TYPE_FRAME_STATE = 44,
        XR_TYPE_VIEW_CONFIGURATION_PROPERTIES = 45,
        XR_TYPE_FRAME_BEGIN_INFO = 46,
        XR_TYPE_COMPOSITION_LAYER_PROJECTION_VIEW = 48,
        XR_TYPE_EVENT_DATA_EVENTS_LOST = 49,
        XR_TYPE_INTERACTION_PROFILE_SUGGESTED_BINDING = 51,
        XR_TYPE_EVENT_DATA_INTERACTION_PROFILE_CHANGED = 52,
        XR_TYPE_INTERACTION_PROFILE_STATE = 53,
        XR_TYPE_SWAPCHAIN_IMAGE_ACQUIRE_INFO = 55,
        XR_TYPE_SWAPCHAIN_IMAGE_WAIT_INFO = 56,
        XR_TYPE_SWAPCHAIN_IMAGE_RELEASE_INFO = 57,
        XR_TYPE_ACTION_STATE_GET_INFO = 58,
        XR_TYPE_HAPTIC_ACTION_INFO = 59,
        XR_TYPE_SESSION_ACTION_SETS_ATTACH_INFO = 60,
        XR_TYPE_ACTIONS_SYNC_INFO = 61,
        XR_TYPE_BOUND_SOURCES_FOR_ACTION_ENUMERATE_INFO = 62,
        XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO = 63,
        XR_TYPE_COMPOSITION_LAYER_CUBE_KHR = 1000006000,
        XR_TYPE_INSTANCE_CREATE_INFO_ANDROID_KHR = 1000008000,
        XR_TYPE_COMPOSITION_LAYER_DEPTH_INFO_KHR = 1000010000,
        XR_TYPE_VULKAN_SWAPCHAIN_FORMAT_LIST_CREATE_INFO_KHR = 1000014000,
        XR_TYPE_EVENT_DATA_PERF_SETTINGS_EXT = 1000015000,
        XR_TYPE_COMPOSITION_LAYER_CYLINDER_KHR = 1000017000,
        XR_TYPE_COMPOSITION_LAYER_EQUIRECT_KHR = 1000018000,
        XR_TYPE_DEBUG_UTILS_OBJECT_NAME_INFO_EXT = 1000019000,
        XR_TYPE_DEBUG_UTILS_MESSENGER_CALLBACK_DATA_EXT = 1000019001,
        XR_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT = 1000019002,
        XR_TYPE_DEBUG_UTILS_LABEL_EXT = 1000019003,
        XR_TYPE_GRAPHICS_BINDING_OPENGL_WIN32_KHR = 1000023000,
        XR_TYPE_GRAPHICS_BINDING_OPENGL_XLIB_KHR = 1000023001,
        XR_TYPE_GRAPHICS_BINDING_OPENGL_XCB_KHR = 1000023002,
        XR_TYPE_GRAPHICS_BINDING_OPENGL_WAYLAND_KHR = 1000023003,
        XR_TYPE_SWAPCHAIN_IMAGE_OPENGL_KHR = 1000023004,
        XR_TYPE_GRAPHICS_REQUIREMENTS_OPENGL_KHR = 1000023005,
        XR_TYPE_GRAPHICS_BINDING_OPENGL_ES_ANDROID_KHR = 1000024001,
        XR_TYPE_SWAPCHAIN_IMAGE_OPENGL_ES_KHR = 1000024002,
        XR_TYPE_GRAPHICS_REQUIREMENTS_OPENGL_ES_KHR = 1000024003,
        XR_TYPE_GRAPHICS_BINDING_VULKAN_KHR = 1000025000,
        XR_TYPE_SWAPCHAIN_IMAGE_VULKAN_KHR = 1000025001,
        XR_TYPE_GRAPHICS_REQUIREMENTS_VULKAN_KHR = 1000025002,
        XR_TYPE_GRAPHICS_BINDING_D3D11_KHR = 1000027000,
        XR_TYPE_SWAPCHAIN_IMAGE_D3D11_KHR = 1000027001,
        XR_TYPE_GRAPHICS_REQUIREMENTS_D3D11_KHR = 1000027002,
        XR_TYPE_GRAPHICS_BINDING_D3D12_KHR = 1000028000,
        XR_TYPE_SWAPCHAIN_IMAGE_D3D12_KHR = 1000028001,
        XR_TYPE_GRAPHICS_REQUIREMENTS_D3D12_KHR = 1000028002,
        XR_TYPE_SYSTEM_EYE_GAZE_INTERACTION_PROPERTIES_EXT = 1000030000,
        XR_TYPE_EYE_GAZE_SAMPLE_TIME_EXT = 1000030001,
        XR_TYPE_VISIBILITY_MASK_KHR = 1000031000,
        XR_TYPE_EVENT_DATA_VISIBILITY_MASK_CHANGED_KHR = 1000031001,
        XR_TYPE_SESSION_CREATE_INFO_OVERLAY_EXTX = 1000033000,
        XR_TYPE_EVENT_DATA_MAIN_SESSION_VISIBILITY_CHANGED_EXTX = 1000033003,
        XR_TYPE_COMPOSITION_LAYER_COLOR_SCALE_BIAS_KHR = 1000034000,
        XR_TYPE_SPATIAL_ANCHOR_CREATE_INFO_MSFT = 1000039000,
        XR_TYPE_SPATIAL_ANCHOR_SPACE_CREATE_INFO_MSFT = 1000039001,
        XR_TYPE_COMPOSITION_LAYER_IMAGE_LAYOUT_FB = 1000040000,
        XR_TYPE_COMPOSITION_LAYER_ALPHA_BLEND_FB = 1000041001,
        XR_TYPE_VIEW_CONFIGURATION_DEPTH_RANGE_EXT = 1000046000,
        XR_TYPE_GRAPHICS_BINDING_EGL_MNDX = 1000048004,
        XR_TYPE_SPATIAL_GRAPH_NODE_SPACE_CREATE_INFO_MSFT = 1000049000,
        XR_TYPE_SYSTEM_HAND_TRACKING_PROPERTIES_EXT = 1000051000,
        XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT = 1000051001,
        XR_TYPE_HAND_JOINTS_LOCATE_INFO_EXT = 1000051002,
        XR_TYPE_HAND_JOINT_LOCATIONS_EXT = 1000051003,
        XR_TYPE_HAND_JOINT_VELOCITIES_EXT = 1000051004,
        XR_TYPE_SYSTEM_HAND_TRACKING_MESH_PROPERTIES_MSFT = 1000052000,
        XR_TYPE_HAND_MESH_SPACE_CREATE_INFO_MSFT = 1000052001,
        XR_TYPE_HAND_MESH_UPDATE_INFO_MSFT = 1000052002,
        XR_TYPE_HAND_MESH_MSFT = 1000052003,
        XR_TYPE_HAND_POSE_TYPE_INFO_MSFT = 1000052004,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_SESSION_BEGIN_INFO_MSFT = 1000053000,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_STATE_MSFT = 1000053001,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_FRAME_STATE_MSFT = 1000053002,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_FRAME_END_INFO_MSFT = 1000053003,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_LAYER_INFO_MSFT = 1000053004,
        XR_TYPE_SECONDARY_VIEW_CONFIGURATION_SWAPCHAIN_CREATE_INFO_MSFT = 1000053005,
        XR_TYPE_CONTROLLER_MODEL_KEY_STATE_MSFT = 1000055000,
        XR_TYPE_CONTROLLER_MODEL_NODE_PROPERTIES_MSFT = 1000055001,
        XR_TYPE_CONTROLLER_MODEL_PROPERTIES_MSFT = 1000055002,
        XR_TYPE_CONTROLLER_MODEL_NODE_STATE_MSFT = 1000055003,
        XR_TYPE_CONTROLLER_MODEL_STATE_MSFT = 1000055004,
        XR_TYPE_VIEW_CONFIGURATION_VIEW_FOV_EPIC = 1000059000,
        XR_TYPE_HOLOGRAPHIC_WINDOW_ATTACHMENT_MSFT = 1000063000,
        XR_TYPE_COMPOSITION_LAYER_REPROJECTION_INFO_MSFT = 1000066000,
        XR_TYPE_COMPOSITION_LAYER_REPROJECTION_PLANE_OVERRIDE_MSFT = 1000066001,
        XR_TYPE_ANDROID_SURFACE_SWAPCHAIN_CREATE_INFO_FB = 1000070000,
        XR_TYPE_COMPOSITION_LAYER_SECURE_CONTENT_FB = 1000072000,
        XR_TYPE_INTERACTION_PROFILE_ANALOG_THRESHOLD_VALVE = 1000079000,
        XR_TYPE_HAND_JOINTS_MOTION_RANGE_INFO_EXT = 1000080000,
        XR_TYPE_LOADER_INIT_INFO_ANDROID_KHR = 1000089000,
        XR_TYPE_VULKAN_INSTANCE_CREATE_INFO_KHR = 1000090000,
        XR_TYPE_VULKAN_DEVICE_CREATE_INFO_KHR = 1000090001,
        XR_TYPE_VULKAN_GRAPHICS_DEVICE_GET_INFO_KHR = 1000090003,
        XR_TYPE_COMPOSITION_LAYER_EQUIRECT2_KHR = 1000091000,
        XR_TYPE_SCENE_OBSERVER_CREATE_INFO_MSFT = 1000097000,
        XR_TYPE_SCENE_CREATE_INFO_MSFT = 1000097001,
        XR_TYPE_NEW_SCENE_COMPUTE_INFO_MSFT = 1000097002,
        XR_TYPE_VISUAL_MESH_COMPUTE_LOD_INFO_MSFT = 1000097003,
        XR_TYPE_SCENE_COMPONENTS_MSFT = 1000097004,
        XR_TYPE_SCENE_COMPONENTS_GET_INFO_MSFT = 1000097005,
        XR_TYPE_SCENE_COMPONENT_LOCATIONS_MSFT = 1000097006,
        XR_TYPE_SCENE_COMPONENTS_LOCATE_INFO_MSFT = 1000097007,
        XR_TYPE_SCENE_OBJECTS_MSFT = 1000097008,
        XR_TYPE_SCENE_COMPONENT_PARENT_FILTER_INFO_MSFT = 1000097009,
        XR_TYPE_SCENE_OBJECT_TYPES_FILTER_INFO_MSFT = 1000097010,
        XR_TYPE_SCENE_PLANES_MSFT = 1000097011,
        XR_TYPE_SCENE_PLANE_ALIGNMENT_FILTER_INFO_MSFT = 1000097012,
        XR_TYPE_SCENE_MESHES_MSFT = 1000097013,
        XR_TYPE_SCENE_MESH_BUFFERS_GET_INFO_MSFT = 1000097014,
        XR_TYPE_SCENE_MESH_BUFFERS_MSFT = 1000097015,
        XR_TYPE_SCENE_MESH_VERTEX_BUFFER_MSFT = 1000097016,
        XR_TYPE_SCENE_MESH_INDICES_UINT32_MSFT = 1000097017,
        XR_TYPE_SCENE_MESH_INDICES_UINT16_MSFT = 1000097018,
        XR_TYPE_SERIALIZED_SCENE_FRAGMENT_DATA_GET_INFO_MSFT = 1000098000,
        XR_TYPE_SCENE_DESERIALIZE_INFO_MSFT = 1000098001,
        XR_TYPE_EVENT_DATA_DISPLAY_REFRESH_RATE_CHANGED_FB = 1000101000,
        XR_TYPE_SYSTEM_FACIAL_TRACKING_PROPERTIES_HTC = 1000104000,
        XR_TYPE_FACIAL_TRACKER_CREATE_INFO_HTC = 1000104001,
        XR_TYPE_FACIAL_EXPRESSIONS_HTC = 1000104002,
        XR_TYPE_SYSTEM_COLOR_SPACE_PROPERTIES_FB = 1000108000,
        XR_TYPE_FOVEATION_PROFILE_CREATE_INFO_FB = 1000114000,
        XR_TYPE_SWAPCHAIN_CREATE_INFO_FOVEATION_FB = 1000114001,
        XR_TYPE_SWAPCHAIN_STATE_FOVEATION_FB = 1000114002,
        XR_TYPE_FOVEATION_LEVEL_PROFILE_CREATE_INFO_FB = 1000115000,
        XR_TYPE_BINDING_MODIFICATIONS_KHR = 1000120000,
        XR_TYPE_VIEW_LOCATE_FOVEATED_RENDERING_VARJO = 1000121000,
        XR_TYPE_FOVEATED_VIEW_CONFIGURATION_VIEW_VARJO = 1000121001,
        XR_TYPE_SYSTEM_FOVEATED_RENDERING_PROPERTIES_VARJO = 1000121002,
        XR_TYPE_COMPOSITION_LAYER_DEPTH_TEST_VARJO = 1000122000,
        XR_TYPE_SPATIAL_ANCHOR_PERSISTENCE_INFO_MSFT = 1000142000,
        XR_TYPE_SPATIAL_ANCHOR_FROM_PERSISTED_ANCHOR_CREATE_INFO_MSFT = 1000142001,
        XR_TYPE_SWAPCHAIN_IMAGE_FOVEATION_VULKAN_FB = 1000160000,
        XR_TYPE_SWAPCHAIN_STATE_ANDROID_SURFACE_DIMENSIONS_FB = 1000161000,
        XR_TYPE_SWAPCHAIN_STATE_SAMPLER_OPENGL_ES_FB = 1000162000,
        XR_TYPE_SWAPCHAIN_STATE_SAMPLER_VULKAN_FB = 1000163000,
        XR_TYPE_GRAPHICS_BINDING_VULKAN2_KHR = XR_TYPE_GRAPHICS_BINDING_VULKAN_KHR,
        XR_TYPE_SWAPCHAIN_IMAGE_VULKAN2_KHR = XR_TYPE_SWAPCHAIN_IMAGE_VULKAN_KHR,
        XR_TYPE_GRAPHICS_REQUIREMENTS_VULKAN2_KHR = XR_TYPE_GRAPHICS_REQUIREMENTS_VULKAN_KHR,
		XR_TYPE_PLANE_DETECTOR_CREATE_INFO_EXT = 1000429001,
		XR_TYPE_PLANE_DETECTOR_BEGIN_INFO_EXT = 1000429002,
		XR_TYPE_PLANE_DETECTOR_GET_INFO_EXT = 1000429003,
		XR_TYPE_PLANE_DETECTOR_LOCATIONS_EXT = 1000429004,
		XR_TYPE_PLANE_DETECTOR_LOCATION_EXT = 1000429005,
		XR_TYPE_PLANE_DETECTOR_POLYGON_BUFFER_EXT = 1000429006,
        XR_TYPE_SYSTEM_PLANE_DETECTION_PROPERTIES_EXT = 1000429007,
        XR_TYPE_SYSTEM_ANCHOR_PROPERTIES_HTC = 1000319000,
        XR_TYPE_SPATIAL_ANCHOR_CREATE_INFO_HTC = 1000319001,
        XR_STRUCTURE_TYPE_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// Runtimes implement well-known reference spaces from XrReferenceSpaceType if they support tracking of that kind.
    /// </summary>
    public enum XrReferenceSpaceType
    {
        /// <summary>
        /// The VIEW space tracks the view origin used to generate view transforms for the primary viewer (or centroid of view origins if stereo), with +Y up, +X to the right, and -Z forward. This space points in the forward direction for the viewer without incorporating the user’s eye orientation, and is not gravity-aligned.
        /// 
        /// VIEW space is primarily useful when projecting from the user’s perspective into another space to obtain a targeting ray, or when rendering small head-locked content such as a reticle. Content rendered in VIEW space will stay at a fixed point on head-mounted displays and may be uncomfortable to view if too large. To obtain the ideal view and projection transforms to use each frame for rendering world content, applications should call <see cref="xrLocateViews">xrLocateViews</see> instead of using this space.
        /// 
        /// Runtimes must support this reference space.
        /// </summary>
        XR_REFERENCE_SPACE_TYPE_VIEW = 1,
        /// <summary>
        /// The LOCAL reference space establishes a world-locked origin, gravity-aligned to exclude pitch and roll, with +Y up, +X to the right, and -Z forward. This space locks in both its initial position and orientation, which the runtime may define to be either the initial position at application launch or some other calibrated zero position.
        /// 
        /// LOCAL space is useful when an application needs to render seated-scale content that is not positioned relative to the physical floor.
        /// 
        /// When a user needs to recenter LOCAL space, a runtime may offer some system-level recentering interaction that is transparent to the application, but which causes the current leveled head space to become the new LOCAL space. When such a recentering occurs, the runtime must queue the <see cref="XrEventDataReferenceSpaceChangePending">XrEventDataReferenceSpaceChangePending</see> event, with the recentered LOCAL space origin only taking effect for <see cref="xrLocateSpace">xrLocateSpace</see> or <see cref="xrLocateViews">xrLocateViews</see> calls whose <see cref="XrTime">XrTime</see> parameter is greater than or equal to the changeTime provided in that event.
        /// 
        /// When views, controllers or other spaces experience tracking loss relative to the LOCAL space, runtimes should continue to provide inferred or last-known position and orientation values. These inferred poses can, for example, be based on neck model updates, inertial dead reckoning, or a last-known position, so long as it is still reasonable for the application to use that pose. While a runtime is providing position data, it must continue to set XR_SPACE_LOCATION_POSITION_VALID_BIT and XR_VIEW_STATE_POSITION_VALID_BIT but it can clear XR_SPACE_LOCATION_POSITION_TRACKED_BIT and XR_VIEW_STATE_POSITION_TRACKED_BIT to indicate that the position is inferred or last-known in this way.
        /// 
        /// When tracking is recovered, runtimes should snap the pose of other spaces back into position relative to the LOCAL space’s original origin.
        /// 
        /// Runtimes must support this reference space.
        /// </summary>
        XR_REFERENCE_SPACE_TYPE_LOCAL = 2,
        /// <summary>
        /// The STAGE reference space is a runtime-defined flat, rectangular space that is empty and can be walked around on. The origin is on the floor at the center of the rectangle, with +Y up, and the X and Z axes aligned with the rectangle edges. The runtime may not be able to locate spaces relative to the STAGE reference space if the user has not yet defined one within the runtime-specific UI. Applications can use <see cref="xrGetReferenceSpaceBoundsRect">xrGetReferenceSpaceBoundsRect</see> to determine the extents of the STAGE reference space’s XZ bounds rectangle, if defined.
        /// 
        /// STAGE space is useful when an application needs to render standing-scale content (no bounds) or room-scale content (with bounds) that is relative to the physical floor.
        /// 
        /// When the user redefines the origin or bounds of the current STAGE space, or the runtime otherwise switches to a new STAGE definition, the runtime must queue the <see cref="XrEventDataReferenceSpaceChangePending">XrEventDataReferenceSpaceChangePending</see> event, with the new STAGE space origin only taking effect for <see cref="xrLocateSpace">xrLocateSpace</see> or <see cref="xrLocateViews">xrLocateViews</see> calls whose <see cref="XrTime">XrTime</see> parameter is greater than or equal to the changeTime provided in that event.
        /// 
        /// When views, controllers or other spaces experience tracking loss relative to the STAGE space, runtimes should continue to provide inferred or last-known position and orientation values. These inferred poses can, for example, be based on neck model updates, inertial dead reckoning, or a last-known position, so long as it is still reasonable for the application to use that pose. While a runtime is providing position data, it must continue to set XR_SPACE_LOCATION_POSITION_VALID_BIT and XR_VIEW_STATE_POSITION_VALID_BIT but it can clear XR_SPACE_LOCATION_POSITION_TRACKED_BIT and XR_VIEW_STATE_POSITION_TRACKED_BIT to indicate that the position is inferred or last-known in this way.
        /// 
        /// When tracking is recovered, runtimes should snap the pose of other spaces back into position relative to the STAGE space’s original origin.
        /// </summary>
        XR_REFERENCE_SPACE_TYPE_STAGE = 3,
        XR_REFERENCE_SPACE_TYPE_UNBOUNDED_MSFT = 1000038000,
        XR_REFERENCE_SPACE_TYPE_COMBINED_EYE_VARJO = 1000121000,
        XR_REFERENCE_SPACE_TYPE_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// The XrEyeVisibility enum selects which of the viewer’s eyes to display a layer.
    /// </summary>
    public enum XrEyeVisibility
    {
        /// <summary>
        /// Displays the layer to both eyes.
        /// </summary>
        XR_EYE_VISIBILITY_BOTH = 0,
        /// <summary>
        /// Displays the layer to the viewer’s physical left eye.
        /// </summary>
        XR_EYE_VISIBILITY_LEFT = 1,
        /// <summary>
        /// Displays the layer to the viewer’s physical right eye.
        /// </summary>
        XR_EYE_VISIBILITY_RIGHT = 2,
        XR_EYE_VISIBILITY_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// The possible blend modes are specified by the XrEnvironmentBlendMode enumeration.
    /// </summary>
    public enum XrEnvironmentBlendMode
    {
        /// <summary>
        /// The composition layers will be displayed with no view of the physical world behind them. The composited image will be interpreted as an RGB image, ignoring the composited alpha channel. This is the typical mode for VR experiences, although this mode can also be supported on devices that support video passthrough.
        /// </summary>
        XR_ENVIRONMENT_BLEND_MODE_OPAQUE = 1,
        /// <summary>
        /// The composition layers will be additively blended with the real world behind the display. The composited image will be interpreted as an RGB image, ignoring the composited alpha channel during the additive blending. This will cause black composited pixels to appear transparent. This is the typical mode for an AR experience on a see-through headset with an additive display, although this mode can also be supported on devices that support video passthrough.
        /// </summary>
        XR_ENVIRONMENT_BLEND_MODE_ADDITIVE = 2,
        /// <summary>
        /// The composition layers will be alpha-blended with the real world behind the display. The composited image will be interpreted as an RGBA image, with the composited alpha channel determining each pixel’s level of blending with the real world behind the display. This is the typical mode for an AR experience on a phone or headset that supports video passthrough.
        /// </summary>
        XR_ENVIRONMENT_BLEND_MODE_ALPHA_BLEND = 3,
        XR_ENVIRONMENT_BLEND_MODE_MAX_ENUM = 0x7FFFFFFF
    }
    /// <summary>
    /// The XrSessionState enumerates the possible session lifecycle states.
    /// </summary>
    public enum XrSessionState
    {
        /// <summary>
        /// An unknown state. The runtime must not return this value in an <see cref="XrEventDataSessionStateChanged">XrEventDataSessionStateChanged</see> event.
        /// </summary>
        XR_SESSION_STATE_UNKNOWN = 0,
        /// <summary>
        /// The initial state after calling <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see> or returned to after calling <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.
        /// </summary>
        XR_SESSION_STATE_IDLE = 1,
        /// <summary>
        /// The application is ready to call <see cref="xrBeginSession">xrBeginSession</see> and <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#sync_frame_loop">sync its frame loop with the runtime</see>.
        /// </summary>
        XR_SESSION_STATE_READY = 2,
        /// <summary>
        /// The application has synced its frame loop with the runtime but is not visible to the user.
        /// </summary>
        XR_SESSION_STATE_SYNCHRONIZED = 3,
        /// <summary>
        /// The application has <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#sync_frame_loop">synced its frame loop with the runtime</see> and is visible to the user but cannot receive XR input.
        /// </summary>
        XR_SESSION_STATE_VISIBLE = 4,
        /// <summary>
        /// The application has <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#sync_frame_loop">synced its frame loop with the runtime</see>, is visible to the user and can receive XR input.
        /// </summary>
        XR_SESSION_STATE_FOCUSED = 5,
        /// <summary>
        /// The application should exit its frame loop and call <see cref="xrEndSession">xrEndSession</see>.
        /// </summary>
        XR_SESSION_STATE_STOPPING = 6,
        /// <summary>
        /// The session is in the process of being lost. The application should destroy the current session and can optionally recreate it.
        /// </summary>
        XR_SESSION_STATE_LOSS_PENDING = 7,
        /// <summary>
        /// The application should end its XR experience and not automatically restart it.
        /// </summary>
        XR_SESSION_STATE_EXITING = 8,
        XR_SESSION_STATE_MAX_ENUM = 0x7FFFFFFF
    }
	public struct XrVector2f
	{
		public float x;
		public float y;
		public XrVector2f(float in_w, float in_y)
		{
			x = in_w;
			y = in_y;
		}
	}
    /// <summary>
    /// A three-dimensional vector is defined by the XrVector3f structure.
    /// </summary>
    public struct XrVector3f
    {
        /// <summary>
        /// The x coordinate of the vector.
        /// </summary>
        public float x;
        /// <summary>
        /// The y coordinate of the vector.
        /// </summary>
        public float y;
        /// <summary>
        /// The z coordinate of the vector.
        /// </summary>
        public float z;
        /// <param name="in_x">The x coordinate of the vector.</param>
        /// <param name="in_y">The y coordinate of the vector.</param>
        /// <param name="in_z">The z coordinate of the vector.</param>
        public XrVector3f(float in_x, float in_y, float in_z)
        {
            x = in_x;
            y = in_y;
            z = in_z;
        }
        public static XrVector3f Zero => new XrVector3f(0, 0, 0);
        public static XrVector3f One => new XrVector3f(1, 1, 1);
		public static XrVector3f Up => new XrVector3f(0, 1, 0);
        public static XrVector3f Forward => new XrVector3f(0, 0, 1);
        public static XrVector3f Right => new XrVector3f(1, 0, 0);

	}
    /// <summary>
    /// Rotation is represented by a unit quaternion defined by the XrQuaternionf structure.
    /// </summary>
    public struct XrQuaternionf
    {
        /// <summary>
        /// The x coordinate of the quaternion.
        /// </summary>
        public float x;
        /// <summary>
        /// The y coordinate of the quaternion.
        /// </summary>
        public float y;
        /// <summary>
        /// The z coordinate of the quaternion.
        /// </summary>
        public float z;
        /// <summary>
        /// The w coordinate of the quaternion.
        /// </summary>
        public float w;
        /// <param name="in_x">The x coordinate of the quaternion.</param>
        /// <param name="in_y">The y coordinate of the quaternion.</param>
        /// <param name="in_z">The z coordinate of the quaternion.</param>
        /// <param name="in_w">The w coordinate of the quaternion.</param>
        public XrQuaternionf(float in_x, float in_y, float in_z, float in_w)
        {
            x = in_x;
            y = in_y;
            z = in_z;
            w = in_w;
        }
        public static XrQuaternionf Identity => new XrQuaternionf(0, 0, 0, 1);
    }
    /// <summary>
    /// Unless otherwise specified, colors are encoded as linear (not with sRGB nor other gamma compression) values with individual components being in the range of 0.0 through 1.0, and without the RGB components being premultiplied by the alpha component.
    /// 
    /// If color encoding is specified as being premultiplied by the alpha component, the RGB components are set to zero if the alpha component is zero.
    /// </summary>
    public struct XrColor4f
    {
        /// <summary>
        /// The red component of the color.
        /// </summary>
        public float r;
        /// <summary>
        /// The green component of the color.
        /// </summary>
        public float g;
        /// <summary>
        /// The blue component of the color.
        /// </summary>
        public float b;
        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        public float a;
        /// <param name="in_r">The red component of the color.</param>
        /// <param name="in_g">The green component of the color.</param>
        /// <param name="in_b">The blue component of the color.</param>
        /// <param name="in_a">The alpha component of the color.</param>
        public XrColor4f(float in_r, float in_g, float in_b, float in_a)
        {
            r = in_r;
            g = in_g;
            b = in_b;
            a = in_a;
        }
    }
    /// <summary>
    /// A two-dimensional floating-point extent is defined by XrExtent2Df.
    /// </summary>
    public struct XrExtent2Df
    {
        /// <summary>
        /// The floating-point width of the extent.
        /// </summary>
        public float width;
        /// <summary>
        /// The floating-point height of the extent.
        /// </summary>
        public float height;
        /// <param name="in_width">The floating-point width of the extent.</param>
        /// <param name="in_height">The floating-point height of the extent.</param>
        public XrExtent2Df(float in_width, float in_height)
        {
            width = in_width;
            height = in_height;
        }
    }
    /// <summary>
    /// A rectangle with integer values is defined by the XrRect2Di.
    /// </summary>
    public struct XrRect2Di
    {
        /// <summary>
        /// The <see cref="XrOffset2Di">XrOffset2Di</see> specifying the integer rectangle offset.
        /// </summary>
        public XrOffset2Di offset;
        /// <summary>
        /// The <see cref="XrExtent2Di">XrExtent2Di</see> specifying the integer rectangle extent.
        /// </summary>
        public XrExtent2Di extent;
        /// <param name="in_offset">The <see cref="XrOffset2Di">XrOffset2Di</see> specifying the integer rectangle offset.</param>
        /// <param name="in_extent">The <see cref="XrExtent2Di">XrExtent2Di</see> specifying the integer rectangle extent.</param>
        public XrRect2Di(XrOffset2Di in_offset, XrExtent2Di in_extent)
        {
            offset = in_offset;
            extent = in_extent;
        }
    }
    /// <summary>
    /// A two-dimensional integer extent is defined by the XrExtent2Di.
    /// </summary>
    public struct XrExtent2Di
    {
        /// <summary>
        /// The integer width of the extent.
        /// </summary>
        public int width;
        /// <summary>
        /// The integer height of the extent.
        /// </summary>
        public int height;
        /// <param name="in_width">The integer width of the extent.</param>
        /// <param name="in_height">The integer height of the extent.</param>
        public XrExtent2Di(int in_width, int in_height)
        {
            width = in_width;
            height = in_height;
        }
    }
    /// <summary>
    /// An integer offset is defined by the XrOffset2Di.
    /// </summary>
    public struct XrOffset2Di
    {
        /// <summary>
        /// The integer offset in the x direction.
        /// </summary>
        public int x;
        /// <summary>
        /// The integer offset in the y direction.
        /// </summary>
        public int y;
        /// <param name="in_x">The integer offset in the x direction.</param>
        /// <param name="in_y">The integer offset in the y direction.</param>
        public XrOffset2Di(int in_x, int in_y)
        {
            x = in_x;
            y = in_y;
        }
    }
    /// <summary>
    /// A pose is defined by the XrPosef structure.
    /// </summary>
    public struct XrPosef
    {
        /// <summary>
        /// An <see cref="XrQuaternionf">XrQuaternionf</see> representing the orientation within a space.
        /// </summary>
        public XrQuaternionf orientation;
        /// <summary>
        /// An <see cref="XrVector3f">XrVector3f</see> representing position within a space.
        /// </summary>
        public XrVector3f position;
        public XrPosef(XrQuaternionf in_orientation, XrVector3f in_position)
        {
			orientation = in_orientation;
			position = in_position;
		}
        public static XrPosef Identity => new XrPosef(XrQuaternionf.Identity, XrVector3f.Zero);
    }

    /// <summary>
    /// Field of view (FoV) is defined by the XrFovf structure.
    /// </summary>
    public struct XrFovf
    {
        public float angleLeft;
        public float angleRight;
        public float angleUp;
        public float angleDown;
    }

    /// <summary>
    /// Boolean values used by OpenXR are of type XrBool32 and are 32-bits wide as suggested by the name.
    /// </summary>
    public struct XrBool32 : IEquatable<UInt32>
    {
        private readonly UInt32 value;

        public XrBool32(UInt32 u)
        {
            value = u;
        }

        public static implicit operator UInt32(XrBool32 equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrBool32(UInt32 u)
        {
            return new XrBool32(u);
        }
        public static implicit operator bool(XrBool32 equatable)
		{
            return equatable.value > 0;
		}
        public static implicit operator XrBool32(bool b)
		{
            return b ? new XrBool32(1) : new XrBool32(0);
		}

        public bool Equals(XrBool32 other)
        {
            return value == other.value;
        }
        public bool Equals(UInt32 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrBool32 && Equals((XrBool32)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrBool32 a, XrBool32 b) { return a.Equals(b); }
        public static bool operator !=(XrBool32 a, XrBool32 b) { return !a.Equals(b); }
        public static bool operator >=(XrBool32 a, XrBool32 b) { return a.value >= b.value; }
        public static bool operator <=(XrBool32 a, XrBool32 b) { return a.value <= b.value; }
        public static bool operator >(XrBool32 a, XrBool32 b) { return a.value > b.value; }
        public static bool operator <(XrBool32 a, XrBool32 b) { return a.value < b.value; }
        public static XrBool32 operator +(XrBool32 a, XrBool32 b) { return a.value + b.value; }
        public static XrBool32 operator -(XrBool32 a, XrBool32 b) { return a.value - b.value; }
        public static XrBool32 operator *(XrBool32 a, XrBool32 b) { return a.value * b.value; }
        public static XrBool32 operator /(XrBool32 a, XrBool32 b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// Bitmasks are passed to many functions and structures to compactly represent options and are stored in memory defined by the XrFlags64 type. But the API does not use the XrFlags64 type directly. Instead, a Xr*Flags type is used which is an alias of the XrFlags64 type. The API also defines a set of constant bit definitions used to set the bitmasks.
    /// </summary>
    public struct XrFlags64 : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrFlags64(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrFlags64 equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrFlags64(UInt64 u)
        {
            return new XrFlags64(u);
        }

        public bool Equals(XrFlags64 other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrFlags64 && Equals((XrFlags64)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrFlags64 a, XrFlags64 b) { return a.Equals(b); }
        public static bool operator !=(XrFlags64 a, XrFlags64 b) { return !a.Equals(b); }
        public static bool operator >=(XrFlags64 a, XrFlags64 b) { return a.value >= b.value; }
        public static bool operator <=(XrFlags64 a, XrFlags64 b) { return a.value <= b.value; }
        public static bool operator >(XrFlags64 a, XrFlags64 b) { return a.value > b.value; }
        public static bool operator <(XrFlags64 a, XrFlags64 b) { return a.value < b.value; }
        public static XrFlags64 operator +(XrFlags64 a, XrFlags64 b) { return a.value + b.value; }
        public static XrFlags64 operator -(XrFlags64 a, XrFlags64 b) { return a.value - b.value; }
        public static XrFlags64 operator *(XrFlags64 a, XrFlags64 b) { return a.value * b.value; }
        public static XrFlags64 operator /(XrFlags64 a, XrFlags64 b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// An OpenXR instance is an object that allows an OpenXR application to communicate with an OpenXR runtime. The application accomplishes this communication by calling <see cref="xrCreateInstance">xrCreateInstance</see> and receiving a handle to the resulting XrInstance object.
    /// 
    /// The XrInstance object stores and tracks OpenXR-related application state, without storing any such state in the application’s global address space. This allows the application to create multiple instances as well as safely encapsulate the application’s OpenXR state since this object is opaque to the application. OpenXR runtimes may limit the number of simultaneous XrInstance objects that may be created and used, but they must support the creation and usage of at least one XrInstance object per process.
    /// </summary>
    public struct XrInstance : IEquatable<ulong>
    {
        private readonly ulong value;

        public XrInstance(ulong u)
        {
            value = u;
        }

        public static implicit operator ulong(XrInstance equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrInstance(ulong u)
        {
            return new XrInstance(u);
        }

        public bool Equals(XrInstance other)
        {
            return value == other.value;
        }
        public bool Equals(ulong other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrInstance && Equals((XrInstance)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrInstance a, XrInstance b) { return a.Equals(b); }
        public static bool operator !=(XrInstance a, XrInstance b) { return !a.Equals(b); }
        public static bool operator >=(XrInstance a, XrInstance b) { return a.value >= b.value; }
        public static bool operator <=(XrInstance a, XrInstance b) { return a.value <= b.value; }
        public static bool operator >(XrInstance a, XrInstance b) { return a.value > b.value; }
        public static bool operator <(XrInstance a, XrInstance b) { return a.value < b.value; }
        public static XrInstance operator +(XrInstance a, XrInstance b) { return a.value + b.value; }
        public static XrInstance operator -(XrInstance a, XrInstance b) { return a.value - b.value; }
        public static XrInstance operator *(XrInstance a, XrInstance b) { return a.value * b.value; }
        public static XrInstance operator /(XrInstance a, XrInstance b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// A session represents an application’s intention to display XR content to the user.
    /// 
    /// Refer to <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#session-lifecycle">Session Lifecycle</see> for more details.
    /// </summary>
    public struct XrSession : IEquatable<ulong>
    {
        private readonly ulong value;

        public XrSession(ulong u)
        {
            value = u;
        }

        public static implicit operator ulong(XrSession equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrSession(ulong u)
        {
            return new XrSession(u);
        }

        public bool Equals(XrSession other)
        {
            return value == other.value;
        }
        public bool Equals(ulong other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrSession && Equals((XrSession)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrSession a, XrSession b) { return a.Equals(b); }
        public static bool operator !=(XrSession a, XrSession b) { return !a.Equals(b); }
        public static bool operator >=(XrSession a, XrSession b) { return a.value >= b.value; }
        public static bool operator <=(XrSession a, XrSession b) { return a.value <= b.value; }
        public static bool operator >(XrSession a, XrSession b) { return a.value > b.value; }
        public static bool operator <(XrSession a, XrSession b) { return a.value < b.value; }
        public static XrSession operator +(XrSession a, XrSession b) { return a.value + b.value; }
        public static XrSession operator -(XrSession a, XrSession b) { return a.value - b.value; }
        public static XrSession operator *(XrSession a, XrSession b) { return a.value * b.value; }
        public static XrSession operator /(XrSession a, XrSession b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// Across both virtual reality and augmented reality, XR applications have a core need to map the location of virtual objects to the corresponding real-world locations where they will be rendered. Spaces allow applications to explicitly create and specify the frames of reference in which they choose to track the real world, and then determine how those frames of reference move relative to one another over time.
    /// 
    /// Spaces are represented by XrSpace handles, which the application creates and then uses in API calls. Whenever an application calls a function that returns coordinates, it provides an XrSpace to specify the frame of reference in which those coordinates will be expressed. Similarly, when providing coordinates to a function, the application specifies which XrSpace the runtime should use to interpret those coordinates.
    /// 
    /// OpenXR defines a set of well-known reference spaces that applications use to bootstrap their spatial reasoning. These reference spaces are: VIEW, LOCAL and STAGE. Each reference space has a well-defined meaning, which establishes where its origin is positioned and how its axes are oriented.
    /// 
    /// Runtimes whose tracking systems improve their understanding of the world over time may track spaces independently. For example, even though a LOCAL space and a STAGE space each map their origin to a static position in the world, a runtime with an inside-out tracking system may introduce slight adjustments to the origin of each space on a continuous basis to keep each origin in place.
    /// 
    /// Beyond well-known reference spaces, runtimes expose other independently-tracked spaces, such as a pose action space that tracks the pose of a motion controller over time.
    /// 
    /// When one or both spaces are tracking a dynamic object, passing in an updated time to <see cref="xrLocateSpace">xrLocateSpace</see> each frame will result in an updated relative pose. For example, the location of the left hand’s pose action space in the STAGE reference space will change each frame as the user’s hand moves relative to the stage’s predefined origin on the floor. In other XR APIs, it is common to report the "pose" of an object relative to some presumed underlying global space. This API is careful to not explicitly define such an underlying global space, because it does not apply to all systems. Some systems will support no STAGE space, while others may support a STAGE space that switches between various physical stages with dynamic availability. To satisfy this wide variability, "poses" are always described as the relationship between two spaces.
    /// 
    /// Some devices improve their understanding of the world as the device is used. The location returned by <see cref="xrLocateSpace">xrLocateSpace</see> in later frames may change over time, even for spaces that track static objects, as either the target space or base space adjusts its origin.
    /// 
    /// Composition layers submitted by the application include an XrSpace for the runtime to use to position that layer over time. Composition layers whose XrSpace is relative to the VIEW reference space are implicitly "head-locked", even if they may not be "display-locked" for non-head-mounted form factors.
    /// </summary>
    public struct XrSpace : IEquatable<ulong>
    {
        private readonly ulong value;

        public XrSpace(ulong u)
        {
            value = u;
        }

        public static implicit operator ulong(XrSpace equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrSpace(ulong u)
        {
            return new XrSpace(u);
        }

        public bool Equals(XrSpace other)
        {
            return value == other.value;
        }
        public bool Equals(ulong other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrSpace && Equals((XrSpace)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrSpace a, XrSpace b) { return a.Equals(b); }
        public static bool operator !=(XrSpace a, XrSpace b) { return !a.Equals(b); }
        public static bool operator >=(XrSpace a, XrSpace b) { return a.value >= b.value; }
        public static bool operator <=(XrSpace a, XrSpace b) { return a.value <= b.value; }
        public static bool operator >(XrSpace a, XrSpace b) { return a.value > b.value; }
        public static bool operator <(XrSpace a, XrSpace b) { return a.value < b.value; }
        public static XrSpace operator +(XrSpace a, XrSpace b) { return a.value + b.value; }
        public static XrSpace operator -(XrSpace a, XrSpace b) { return a.value - b.value; }
        public static XrSpace operator *(XrSpace a, XrSpace b) { return a.value * b.value; }
        public static XrSpace operator /(XrSpace a, XrSpace b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// The XrPath is an atom that connects an application with a single path, within the context of a single instance. There is a bijective mapping between well-formed path strings and atoms in use. This atom is used — in place of the path name string it corresponds to — to retrieve state and perform other operations.
    /// 
    /// As an XrPath is only shorthand for a well-formed path string, they have no explicit life cycle.
    /// 
    /// Lifetime is implicitly managed by the XrInstance.An XrPath must not be used unless it is received at execution time from the runtime in the context of a particular XrInstance. Therefore, with the exception of <see cref="OpenXRHelper.XR_NULL_PATH">XR_NULL_PATH</see>, XrPath values must not be specified as constant values in applications: the corresponding path string should be used instead.During the lifetime of a given XrInstance, the XrPath associated with that instance with any given well-formed path must not vary, and similarly the well-formed path string that corresponds to a given XrPath in that instance must not vary.An XrPath that is received from one XrInstance may not be used with another. Such an invalid use may be detected and result in an error being returned, or it may result in undefined behavior.
    /// 
    /// Well-written applications should typically use a small, bounded set of paths in practice.However, the runtime should support looking up the XrPath for a large number of path strings for maximum compatibility. Runtime implementers should keep in mind that applications supporting diverse systems may look up path strings in a quantity exceeding the number of non-empty entities predicted or provided by any one runtime’s own path tree model, and this is not inherently an error. However, system resources are finite and thus runtimes may signal exhaustion of resources dedicated to these associations under certain conditions.
    /// 
    /// When discussing the behavior of runtimes at these limits, a new XrPath refers to an XrPath value that, as of some point in time, has neither been received by the application nor tracked internally by the runtime.In this case, since an application has not yet received the value of such an XrPath, the runtime has not yet made any assertions about its association with any path string. In this context, new only refers to the fact that the mapping has not necessarily been made constant for a given value/path string pair for the remaining life of the associated instance by being revealed to the application.It does not necessarily imply creation of the entity, if any, referred to by such a path.Similarly, it does not imply the absence of such an entity prior to that point. Entities in the path tree have varied lifetime that is independent from the duration of the mapping from path string to XrPath.
    /// 
    /// For flexibility, the runtime may internally track or otherwise make constant, in instance or larger scope, any mapping of a path string to an XrPath value even before an application would otherwise receive that value, thus making it no longer new by the above definition.
    /// 
    /// When the runtime’s resources to track the path string-XrPath mapping are exhausted, and the application makes an API call that would have otherwise retrieved a new XrPath as defined above, the runtime must return XR_ERROR_PATH_COUNT_EXCEEDED.This includes both explicit calls to xrStringToPath as well as other calls that retrieve an XrPath in any other way.
    /// </summary>
    public struct XrPath : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrPath(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrPath equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrPath(UInt64 u)
        {
            return new XrPath(u);
        }

        public bool Equals(XrPath other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrPath && Equals((XrPath)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrPath a, XrPath b) { return a.Equals(b); }
        public static bool operator !=(XrPath a, XrPath b) { return !a.Equals(b); }
        public static bool operator >=(XrPath a, XrPath b) { return a.value >= b.value; }
        public static bool operator <=(XrPath a, XrPath b) { return a.value <= b.value; }
        public static bool operator >(XrPath a, XrPath b) { return a.value > b.value; }
        public static bool operator <(XrPath a, XrPath b) { return a.value < b.value; }
        public static XrPath operator +(XrPath a, XrPath b) { return a.value + b.value; }
        public static XrPath operator -(XrPath a, XrPath b) { return a.value - b.value; }
        public static XrPath operator *(XrPath a, XrPath b) { return a.value * b.value; }
        public static XrPath operator /(XrPath a, XrPath b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// Action sets are application-defined collections of actions. They are attached to a given XrSession with a xrAttachSessionActionSets call. They are enabled or disabled by the application via xrSyncActions depending on the current application context. For example, a game may have one set of actions that apply to controlling a character and another set for navigating a menu system. When these actions are grouped into two XrActionSet handles they can be selectively enabled and disabled using a single function call.
    /// </summary>
    public struct XrActionSet : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrActionSet(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrActionSet equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrActionSet(UInt64 u)
        {
            return new XrActionSet(u);
        }

        public bool Equals(XrActionSet other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrActionSet && Equals((XrActionSet)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrActionSet a, XrActionSet b) { return a.Equals(b); }
        public static bool operator !=(XrActionSet a, XrActionSet b) { return !a.Equals(b); }
        public static bool operator >=(XrActionSet a, XrActionSet b) { return a.value >= b.value; }
        public static bool operator <=(XrActionSet a, XrActionSet b) { return a.value <= b.value; }
        public static bool operator >(XrActionSet a, XrActionSet b) { return a.value > b.value; }
        public static bool operator <(XrActionSet a, XrActionSet b) { return a.value < b.value; }
        public static XrActionSet operator +(XrActionSet a, XrActionSet b) { return a.value + b.value; }
        public static XrActionSet operator -(XrActionSet a, XrActionSet b) { return a.value - b.value; }
        public static XrActionSet operator *(XrActionSet a, XrActionSet b) { return a.value * b.value; }
        public static XrActionSet operator /(XrActionSet a, XrActionSet b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// Action handles are used to refer to individual actions when retrieving action data, creating action spaces, or sending haptic events.
    /// </summary>
    public struct XrAction : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrAction(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrAction equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrAction(UInt64 u)
        {
            return new XrAction(u);
        }

        public bool Equals(XrAction other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrAction && Equals((XrAction)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrAction a, XrAction b) { return a.Equals(b); }
        public static bool operator !=(XrAction a, XrAction b) { return !a.Equals(b); }
        public static bool operator >=(XrAction a, XrAction b) { return a.value >= b.value; }
        public static bool operator <=(XrAction a, XrAction b) { return a.value <= b.value; }
        public static bool operator >(XrAction a, XrAction b) { return a.value > b.value; }
        public static bool operator <(XrAction a, XrAction b) { return a.value < b.value; }
        public static XrAction operator +(XrAction a, XrAction b) { return a.value + b.value; }
        public static XrAction operator -(XrAction a, XrAction b) { return a.value - b.value; }
        public static XrAction operator *(XrAction a, XrAction b) { return a.value * b.value; }
        public static XrAction operator /(XrAction a, XrAction b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }


    /// <summary>
    /// Flag bits for <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrSpaceLocationFlags">XrSpaceLocationFlags</see>.
    /// </summary>
    [Flags]
    public enum XrSpaceLocationFlags : UInt64
    {
        /// <summary>
        /// XrSpaceLocationFlags bitmask 0x00000001 indicates the XrSpace's orientation is valid.
        /// </summary>
        XR_SPACE_LOCATION_ORIENTATION_VALID_BIT = 0x00000001,
        /// <summary>
        /// XrSpaceLocationFlags bitmask 0x00000002 indicates the XrSpace's position is valid.
        /// </summary>
        XR_SPACE_LOCATION_POSITION_VALID_BIT = 0x00000002,
        /// <summary>
        /// XrSpaceLocationFlags bitmask 0x00000004 indicates the XrSpace's orientation is tracked.
        /// </summary>
        XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT = 0x00000004,
        /// <summary>
        /// XrSpaceLocationFlags bitmask 0x00000008 indicates the XrSpace's position is tracked
        /// </summary>
        XR_SPACE_LOCATION_POSITION_TRACKED_BIT = 0x00000008,
    }

    public struct XrSpaceLocation
    {
        public XrStructureType type;
        public System.IntPtr next;
        public XrSpaceLocationFlags locationFlags;
        public XrPosef pose;
    }

    [Flags]
    public enum XrInputSourceLocalizedNameFlags : UInt64
	{
        /// <summary>
        /// XrInputSourceLocalizedNameFlags bitmask 0x00000001 indicates that the runtime must include the user path portion of the string in the result, if available. E.g. Left Hand.
        /// </summary>
        XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT = 0x00000001,
        /// <summary>
        /// XrInputSourceLocalizedNameFlags bitmask 0x00000002 indicates that the runtime must include the interaction profile portion of the string in the result, if available. E.g. Vive Controller.
        /// </summary>
        XR_INPUT_SOURCE_LOCALIZED_NAME_INTERACTION_PROFILE_BIT = 0x00000002,
        /// <summary>
        /// XrInputSourceLocalizedNameFlags bitmask 0x00000004 indicates that the runtime must include the input component portion of the string in the result, if available. E.g. Trigger.
        /// </summary>
        XR_INPUT_SOURCE_LOCALIZED_NAME_COMPONENT_BIT = 0x00000004,

        XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC = 0x1000000000000000,
    }

    public enum XrActionType : UInt64
    {
        XR_ACTION_TYPE_BOOLEAN_INPUT = 1,
        XR_ACTION_TYPE_FLOAT_INPUT = 2,
        XR_ACTION_TYPE_VECTOR2F_INPUT = 3,
        XR_ACTION_TYPE_POSE_INPUT = 4,
        XR_ACTION_TYPE_VIBRATION_OUTPUT = 100,
        XR_ACTION_TYPE_MAX_ENUM = 0x7FFFFFFF
    }

    /// <summary>
    /// An XrSystemId is an opaque atom used by the runtime to identify a system. The value <see cref="OpenXRHelper.XR_NULL_SYSTEM_ID">XR_NULL_SYSTEM_ID</see> is considered an invalid system.
    /// </summary>
    public struct XrSystemId : IEquatable<ulong>
    {
        private readonly ulong value;

        public XrSystemId(ulong u)
        {
            value = u;
        }

        public static implicit operator ulong(XrSystemId equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrSystemId(ulong u)
        {
            return new XrSystemId(u);
        }

        public bool Equals(XrSystemId other)
        {
            return value == other.value;
        }
        public bool Equals(ulong other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrSystemId && Equals((XrSystemId)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrSystemId a, XrSystemId b) { return a.Equals(b); }
        public static bool operator !=(XrSystemId a, XrSystemId b) { return !a.Equals(b); }
        public static bool operator >=(XrSystemId a, XrSystemId b) { return a.value >= b.value; }
        public static bool operator <=(XrSystemId a, XrSystemId b) { return a.value <= b.value; }
        public static bool operator >(XrSystemId a, XrSystemId b) { return a.value > b.value; }
        public static bool operator <(XrSystemId a, XrSystemId b) { return a.value < b.value; }
        public static XrSystemId operator +(XrSystemId a, XrSystemId b) { return a.value + b.value; }
        public static XrSystemId operator -(XrSystemId a, XrSystemId b) { return a.value - b.value; }
        public static XrSystemId operator *(XrSystemId a, XrSystemId b) { return a.value * b.value; }
        public static XrSystemId operator /(XrSystemId a, XrSystemId b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// XrTime is a base value type that represents time as a signed 64-bit integer, representing the monotonically-increasing count of nanoseconds that have elapsed since a runtime-chosen epoch. XrTime always represents the time elasped since that constant epoch, rather than a duration or a time point relative to some moving epoch such as vsync time, etc.
    /// </summary>
    public struct XrTime : IEquatable<Int64>
    {
        private readonly Int64 value;

        public XrTime(Int64 u)
        {
            value = u;
        }

        public static implicit operator Int64(XrTime equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrTime(Int64 u)
        {
            return new XrTime(u);
        }

        public bool Equals(XrTime other)
        {
            return value == other.value;
        }
        public bool Equals(Int64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrTime && Equals((XrTime)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrTime a, XrTime b) { return a.Equals(b); }
        public static bool operator !=(XrTime a, XrTime b) { return !a.Equals(b); }
        public static bool operator >=(XrTime a, XrTime b) { return a.value >= b.value; }
        public static bool operator <=(XrTime a, XrTime b) { return a.value <= b.value; }
        public static bool operator >(XrTime a, XrTime b) { return a.value > b.value; }
        public static bool operator <(XrTime a, XrTime b) { return a.value < b.value; }
        public static XrTime operator +(XrTime a, XrTime b) { return a.value + b.value; }
        public static XrTime operator -(XrTime a, XrTime b) { return a.value - b.value; }
        public static XrTime operator *(XrTime a, XrTime b) { return a.value * b.value; }
        public static XrTime operator /(XrTime a, XrTime b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }
    }
    /// <summary>
    /// The difference between two timepoints is a duration, and thus the difference between two <see cref="XrTime">XrTime</see> values is an XrDuration value.
    /// 
    /// Functions that refer to durations use XrDuration as opposed to <see cref="XrTime">XrTime</see>.
    /// </summary>
    public struct XrDuration : IEquatable<Int64>
    {
        private readonly Int64 value;

        public XrDuration(Int64 u)
        {
            value = u;
        }

        public static implicit operator Int64(XrDuration equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrDuration(Int64 u)
        {
            return new XrDuration(u);
        }

        public bool Equals(XrDuration other)
        {
            return value == other.value;
        }
        public bool Equals(Int64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrDuration && Equals((XrDuration)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrDuration a, XrDuration b) { return a.Equals(b); }
        public static bool operator !=(XrDuration a, XrDuration b) { return !a.Equals(b); }
        public static bool operator >=(XrDuration a, XrDuration b) { return a.value >= b.value; }
        public static bool operator <=(XrDuration a, XrDuration b) { return a.value <= b.value; }
        public static bool operator >(XrDuration a, XrDuration b) { return a.value > b.value; }
        public static bool operator <(XrDuration a, XrDuration b) { return a.value < b.value; }
        public static XrDuration operator +(XrDuration a, XrDuration b) { return a.value + b.value; }
        public static XrDuration operator -(XrDuration a, XrDuration b) { return a.value - b.value; }
        public static XrDuration operator *(XrDuration a, XrDuration b) { return a.value * b.value; }
        public static XrDuration operator /(XrDuration a, XrDuration b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }
    }

	public struct XrSwapchain : IEquatable<ulong>
	{
		private readonly ulong value;

		public XrSwapchain(ulong u)
		{
			value = u;
		}

		public static implicit operator ulong(XrSwapchain xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchain(ulong u)
		{
			return new XrSwapchain(u);
		}

		public bool Equals(XrSwapchain other)
		{
			return value == other.value;
		}
		public bool Equals(ulong other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchain && Equals((XrSwapchain)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchain a, XrSwapchain b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchain a, XrSwapchain b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchain a, XrSwapchain b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchain a, XrSwapchain b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchain a, XrSwapchain b) { return a.value > b.value; }
		public static bool operator <(XrSwapchain a, XrSwapchain b) { return a.value < b.value; }
		public static XrSwapchain operator +(XrSwapchain a, XrSwapchain b) { return a.value + b.value; }
		public static XrSwapchain operator -(XrSwapchain a, XrSwapchain b) { return a.value - b.value; }
		public static XrSwapchain operator *(XrSwapchain a, XrSwapchain b) { return a.value * b.value; }
		public static XrSwapchain operator /(XrSwapchain a, XrSwapchain b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}

	}

	/// <summary>
	/// There are currently no session creation flags. This is reserved for future use.
	/// </summary>
	public struct XrSessionCreateFlags : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrSessionCreateFlags(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrSessionCreateFlags equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrSessionCreateFlags(UInt64 u)
        {
            return new XrSessionCreateFlags(u);
        }

        public bool Equals(XrSessionCreateFlags other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrSessionCreateFlags && Equals((XrSessionCreateFlags)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.Equals(b); }
        public static bool operator !=(XrSessionCreateFlags a, XrSessionCreateFlags b) { return !a.Equals(b); }
        public static bool operator >=(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value >= b.value; }
        public static bool operator <=(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value <= b.value; }
        public static bool operator >(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value > b.value; }
        public static bool operator <(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value < b.value; }
        public static XrSessionCreateFlags operator +(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value + b.value; }
        public static XrSessionCreateFlags operator -(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value - b.value; }
        public static XrSessionCreateFlags operator *(XrSessionCreateFlags a, XrSessionCreateFlags b) { return a.value * b.value; }
        public static XrSessionCreateFlags operator /(XrSessionCreateFlags a, XrSessionCreateFlags b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }
    }

	public struct XrSwapchainCreateFlags : IEquatable<UInt64>
	{
		private readonly UInt64 value;

		public XrSwapchainCreateFlags(UInt64 u)
		{
			value = u;
		}

		public static implicit operator UInt64(XrSwapchainCreateFlags xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchainCreateFlags(UInt64 u)
		{
			return new XrSwapchainCreateFlags(u);
		}

		public bool Equals(XrSwapchainCreateFlags other)
		{
			return value == other.value;
		}
		public bool Equals(UInt64 other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchainCreateFlags && Equals((XrSwapchainCreateFlags)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value > b.value; }
		public static bool operator <(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value < b.value; }
		public static XrSwapchainCreateFlags operator +(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value + b.value; }
		public static XrSwapchainCreateFlags operator -(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value - b.value; }
		public static XrSwapchainCreateFlags operator *(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value * b.value; }
		public static XrSwapchainCreateFlags operator /(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}
	}

	public struct XrSwapchainUsageFlags : IEquatable<UInt64>
	{
		private readonly UInt64 value;

		public XrSwapchainUsageFlags(UInt64 u)
		{
			value = u;
		}

		public static implicit operator UInt64(XrSwapchainUsageFlags xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchainUsageFlags(UInt64 u)
		{
			return new XrSwapchainUsageFlags(u);
		}

		public bool Equals(XrSwapchainUsageFlags other)
		{
			return value == other.value;
		}
		public bool Equals(UInt64 other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchainUsageFlags && Equals((XrSwapchainUsageFlags)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value > b.value; }
		public static bool operator <(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value < b.value; }
		public static XrSwapchainUsageFlags operator +(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value + b.value; }
		public static XrSwapchainUsageFlags operator -(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value - b.value; }
		public static XrSwapchainUsageFlags operator *(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value * b.value; }
		public static XrSwapchainUsageFlags operator /(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}
	}

	/// <summary>
	/// Flag bits for XrSpaceVelocityFlags:
	/// 
	/// <see cref="OpenXRHelper.XR_SPACE_VELOCITY_LINEAR_VALID_BIT ">XR_SPACE_VELOCITY_LINEAR_VALID_BIT </see> indicates that the linearVelocity member contains valid data. Applications must not read the linearVelocity field if this flag is unset.
	/// 
	/// <see cref="OpenXRHelper.XR_SPACE_VELOCITY_ANGULAR_VALID_BIT ">XR_SPACE_VELOCITY_ANGULAR_VALID_BIT </see> indicates that the angularVelocity member contains valid data. Applications must not read the angularVelocity field if this flag is unset.
	/// </summary>
	public struct XrSpaceVelocityFlags : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrSpaceVelocityFlags(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrSpaceVelocityFlags equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrSpaceVelocityFlags(UInt64 u)
        {
            return new XrSpaceVelocityFlags(u);
        }

        public bool Equals(XrSpaceVelocityFlags other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrSpaceVelocityFlags && Equals((XrSpaceVelocityFlags)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.Equals(b); }
        public static bool operator !=(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return !a.Equals(b); }
        public static bool operator >=(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value >= b.value; }
        public static bool operator <=(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value <= b.value; }
        public static bool operator >(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value > b.value; }
        public static bool operator <(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value < b.value; }
        public static XrSpaceVelocityFlags operator +(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value + b.value; }
        public static XrSpaceVelocityFlags operator -(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value - b.value; }
        public static XrSpaceVelocityFlags operator *(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b) { return a.value * b.value; }
        public static XrSpaceVelocityFlags operator /(XrSpaceVelocityFlags a, XrSpaceVelocityFlags b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }
    }

    /// <summary>
    /// A structure indicates the space.
    /// </summary>
    public struct XrReferenceSpaceCreateInfo
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// The chosen <see cref="XrReferenceSpaceType">XrReferenceSpaceType</see>.
        /// </summary>
        public XrReferenceSpaceType referenceSpaceType;
        /// <summary>
        /// An <see cref="XrPosef">XrPosef</see> defining the position and orientation of the new space’s origin within the natural reference frame of the reference space.
        /// </summary>
        public XrPosef poseInReferenceSpace;
    }
    /// <summary>
    /// A structure indicates the system graphics properties.
    /// </summary>
    public struct XrSystemGraphicsProperties
    {
        /// <summary>
        /// The maximum swapchain image pixel height supported by this system.
        /// </summary>
        public uint maxSwapchainImageHeight;
        /// <summary>
        /// The maximum swapchain image pixel width supported by this system.
        /// </summary>
        public uint maxSwapchainImageWidth;
        /// <summary>
        /// The maximum number of composition layers supported by this system. The runtime must support at least <see cref="OpenXRHelper.XR_MIN_COMPOSITION_LAYERS_SUPPORTED">XR_MIN_COMPOSITION_LAYERS_SUPPORTED</see> layers.
        /// </summary>
        public uint maxLayerCount;
    }
    /// <summary>
    /// A structure indicates the system tracking properties.
    /// </summary>
    public struct XrSystemTrackingProperties
    {
        /// <summary>
        /// Set to XR_TRUE to indicate the system supports orientational tracking of the view pose(s), XR_FALSE otherwise.
        /// </summary>
        public uint orientationTracking;
        /// <summary>
        /// Set to XR_TRUE to indicate the system supports positional tracking of the view pose(s), XR_FALSE otherwise.
        /// </summary>
        public uint positionTracking;
    }
    /// <summary>
    /// A structure indicates the system properties.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrSystemProperties
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// The <see cref="XrSystemId">XrSystemId</see> identifies the system.
        /// </summary>
        public XrSystemId systemId;
        /// <summary>
        /// A unique identifier for the vendor of the system.
        /// </summary>
        public UInt32 vendorId;
        /// <summary>
        /// A string contains the name of the system.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] systemName; //char systemName[XR_MAX_SYSTEM_NAME_SIZE];
        /// <summary>
        /// An <see cref="XrSystemGraphicsProperties">XrSystemGraphicsProperties</see> structure specifying the system graphics properties.
        /// </summary>
        public XrSystemGraphicsProperties graphicsProperties;
        /// <summary>
        /// An <see cref="XrSystemTrackingProperties">XrSystemTrackingProperties</see> structure specifying system tracking properties.
        /// </summary>
        public XrSystemTrackingProperties trackingProperties;
    }

    /// <summary>
    /// A structure contains information about how to create the session.
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct XrSessionCreateInfo
	{
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
		public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR. Note that in most cases one graphics API extension specific struct needs to be in this next chain.
        /// </summary>
		public IntPtr next;
        /// <summary>
        /// Identifies <see cref="XrSessionCreateFlags">XrSessionCreateFlags</see> that apply to the creation.
        /// </summary>
        public XrSessionCreateFlags createFlags;
        /// <summary>
        /// The <see cref="XrSystemId">XrSystemId</see> represents the system of devices to be used by this session.
        /// </summary>
		public XrSystemId systemId;
	}

	public struct XrSwapchainCreateInfo
	{
		public XrStructureType type;
		public IntPtr next;
		public XrSwapchainCreateFlags createFlags;
		public XrSwapchainUsageFlags usageFlags;
		public UInt64 format;
		public UInt32 sampleCount;
		public UInt32 width;
		public UInt32 height;
		public UInt32 faceCount;
		public UInt32 arraySize;
		public UInt32 mipCount;
	}

    public struct XrInputSourceLocalizedNameGetInfo
    {
        public XrStructureType type;
        public IntPtr next;
        public XrPath sourcePath;
        public XrInputSourceLocalizedNameFlags whichComponents;

        public XrInputSourceLocalizedNameGetInfo(XrStructureType in_type, IntPtr in_next, XrPath in_path, XrInputSourceLocalizedNameFlags in_flags)
		{
            type = in_type;
            next = in_next;
            sourcePath = in_path;
            whichComponents = in_flags;
		}
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct XrExtensionProperties
    {
        public XrStructureType type;
        public IntPtr next;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string extensionName;
        public UInt32 extensionVersion;
    }

    public struct XrActionCreateInfo
    {
        public XrStructureType type;
        public IntPtr next;
        public char[] actionName;
        public XrActionType actionType;
        public UInt32 countSubactionPaths;
        public XrPath[] subactionPaths;
        public char[] localizedActionName;
    };

    public struct XrActionStateGetInfo
    {
        public XrStructureType type;
        public IntPtr next;
        public XrAction action;
        public XrPath subactionPath;
    }

    public struct XrActionStatePose
    {
        public XrStructureType type;
        public IntPtr next;
        public XrBool32 isActive;
    };
    /// <summary>
    /// A structure indicates the frameWaitInfo.
    /// </summary>
    public struct XrFrameWaitInfo
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// next is NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR.
        /// </summary>
        public IntPtr next;
        public XrFrameWaitInfo(IntPtr next_, XrStructureType type_)
        {
            next = next_;
            type = type_;
        }
    }
    /// <summary>
    /// A structure indicates the frameState.
    /// </summary>
    public struct XrFrameState
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// next is NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// predictedDisplayTime is the anticipated display XrTime for the next application-generated frame.
        /// </summary>
        public Int64 predictedDisplayTime;
        /// <summary>
        /// predictedDisplayPeriod is the XrDuration of the display period for the next application-generated frame, for use in predicting display times beyond the next one.
        /// </summary>
        public Int64 predictedDisplayPeriod;
        /// <summary>
        /// shouldRender is XR_TRUE if the application should render its layers as normal and submit them to xrEndFrame. When this value is XR_FALSE, the application should avoid heavy GPU work where possible, for example by skipping layer rendering and then omitting those layers when calling xrEndFrame.
        /// </summary>
        public bool shouldRender;
    }
    public static class OpenXRHelper
    {
        /// <summary>
        /// Validates if the <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@0.1/api/UnityEngine.Experimental.Input.InputActionReference.html">InputActionReference</see> is valid.
        /// </summary>
        /// <param name="actionReference">The <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@0.1/api/UnityEngine.Experimental.Input.InputActionReference.html">InputActionReference</see> input.</param>
        /// <param name="msg">The result output.</param>
        /// <returns></returns>
        public static bool VALIDATE(InputActionReference actionReference, out string msg)
        {
            msg = "Normal";

            if (actionReference == null)
            {
                msg = "Null reference.";
                return false;
            }
            else if (actionReference.action == null)
            {
                msg = "Null reference action.";
                return false;
            }
            else if (!actionReference.action.enabled)
            {
                msg = "Reference action disabled.";
                return false;
            }
            else if (actionReference.action.activeControl == null)
            {
                msg = "No active control of the reference action.";
                return false;
            }
            else if (actionReference.action.controls.Count <= 0)
            {
                msg = "Action control count is " + actionReference.action.controls.Count;
                return false;
            }

            return true;
        }
        /// <summary>
        /// Transforms an OpenXR Vector to Unity coordinates.
        /// </summary>
        /// <param name="xrVec">Vector3 in OpenXR coordinates.</param>
        /// <returns>Vector3 in Unity coordinates.</returns>
        public static Vector3 ToUnityVector(this Vector3 xrVec)
        {
            Vector3 vec = Vector3.zero;
            vec.x = xrVec.x;
            vec.y = xrVec.y;
            vec.z = -xrVec.z;
            return vec;
        }
        /// <summary>
        /// Transforms an OpenXR Vector to Unity coordinates.
        /// </summary>
        /// <param name="xrVec"><see cref="XrVector3f">XrVector3f</see> in OpenXR coordinates.</param>
        /// <returns>Vector3 in Unity coordinates.</returns>
        public static Vector3 ToUnityVector(this XrVector3f xrVec)
        {
            Vector3 vec = Vector3.zero;
            vec.x = xrVec.x;
            vec.y = xrVec.y;
            vec.z = -xrVec.z;
            return vec;
        }
        /// <summary>
        /// Transforms an OpenXR Qauternaion to Unity coordinates.
        /// </summary>
        /// <param name="xrQuat">Quaternion in OpenXR coordinates.</param>
        /// <returns>Quaternion in Unity coordinates.</returns>
        public static Quaternion ToUnityQuaternion(this Quaternion xrQuat)
        {
            Quaternion quat = Quaternion.identity;
            quat.x = xrQuat.x;
            quat.y = xrQuat.y;
            quat.z = -xrQuat.z;
            quat.w = -xrQuat.w;
            return quat;
        }
        /// <summary>
        /// Transforms an OpenXR Qauternaion to Unity coordinates.
        /// </summary>
        /// <param name="xrQuat"><see cref="XrQuaternionf">XrQuaternionf</see> in OpenXR coordinates.</param>
        /// <returns>Quaternion in Unity coordinates.</returns>
        public static Quaternion ToUnityQuaternion(this XrQuaternionf xrQuat)
        {
            Quaternion quat = Quaternion.identity;
            quat.x = xrQuat.x;
            quat.y = xrQuat.y;
            quat.z = -xrQuat.z;
            quat.w = -xrQuat.w;
            return quat;
        }
        public static XrVector3f ToOpenXRVector(this Vector3 unityVec, bool convertFromUntiyToOpenXR = true)
        {
            XrVector3f vec;
            vec.x = unityVec.x;
            vec.y = unityVec.y;
            vec.z = convertFromUntiyToOpenXR ? -unityVec.z : unityVec.z;
            return vec;
        }
        public static XrVector3f ToOpenXRVector(this XrVector3f unityVec, bool convertFromUntiyToOpenXR = true)
        {
            XrVector3f vec;
            vec.x = unityVec.x;
            vec.y = unityVec.y;
            vec.z = convertFromUntiyToOpenXR ? -unityVec.z : unityVec.z;
            return vec;
        }
        public static XrQuaternionf ToOpenXRQuaternion(this Quaternion unityQuat, bool convertFromUntiyToOpenXR = true)
        {
            XrQuaternionf quat;
            quat.x = unityQuat.x;
            quat.y = unityQuat.y;
            quat.z = convertFromUntiyToOpenXR ? -unityQuat.z : unityQuat.z;
            quat.w = convertFromUntiyToOpenXR ? -unityQuat.w : unityQuat.w;
            return quat;
        }
        public static XrQuaternionf ToOpenXRQuaternion(this XrQuaternionf unityQuat, bool convertFromUntiyToOpenXR = true)
        {
            XrQuaternionf quat;
            quat.x = unityQuat.x;
            quat.y = unityQuat.y;
            quat.z = convertFromUntiyToOpenXR ? -unityQuat.z : unityQuat.z;
            quat.w = convertFromUntiyToOpenXR ? -unityQuat.w : unityQuat.w;
            return quat;
        }

        [Obsolete("Please use XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT instead.")]
        public static XrSpaceLocationFlags XR_SPACE_LOCATION_ORIENTATION_VALID_BIT = XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT;
        [Obsolete("Please use XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT instead.")]
        public static XrSpaceLocationFlags XR_SPACE_LOCATION_POSITION_VALID_BIT = XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT;
        [Obsolete("Please use XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT instead.")]
        public static XrSpaceLocationFlags XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT = XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT;
        [Obsolete("Please use XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT instead.")]
        public static XrSpaceLocationFlags XR_SPACE_LOCATION_POSITION_TRACKED_BIT = XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT;

        // Flag bits for XrSwapchainCreateFlags
        public static XrSwapchainCreateFlags XR_SWAPCHAIN_CREATE_PROTECTED_CONTENT_BIT = 0x00000001;
        public static XrSwapchainCreateFlags XR_SWAPCHAIN_CREATE_STATIC_IMAGE_BIT = 0x00000002;

        // Flag bits for XrSwapchainUsageFlags
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_COLOR_ATTACHMENT_BIT = 0x00000001;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT = 0x00000002;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_UNORDERED_ACCESS_BIT = 0x00000004;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_TRANSFER_SRC_BIT = 0x00000008;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_TRANSFER_DST_BIT = 0x00000010;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_SAMPLED_BIT = 0x00000020;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_MUTABLE_FORMAT_BIT = 0x00000040;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_INPUT_ATTACHMENT_BIT_MND = 0x00000080;
        public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_INPUT_ATTACHMENT_BIT_KHR = 0x00000080;

        /// <summary>
        /// XrSystemId value 0 indicates an invalid system.
        /// </summary>
        public static ulong XR_NULL_SYSTEM_ID = 0;

        // XrDuration definitions
        /// <summary>
        /// For the case of timeout durations, XR_NO_DURATION may be used to indicate that the timeout is immediate.
        /// </summary>
        public static XrDuration XR_NO_DURATION = 0;
        /// <summary>
        /// A special value that may be used to indicate that the timeout never occurs.
        /// </summary>
        public static XrDuration XR_INFINITE_DURATION = 0x7fffffffffffffff;

        /// <summary>
        /// Defines the minimum number of composition layers that a conformant runtime must support. 
        /// </summary>
        public static uint XR_MIN_COMPOSITION_LAYERS_SUPPORTED = 16;

        /// <summary>
        /// Indicates the linear velocity is valid.
        /// </summary>
        public static XrSpaceVelocityFlags XR_SPACE_VELOCITY_LINEAR_VALID_BIT = 0x00000001;
        /// <summary>
        /// Indicates the angular velocity is valid.
        /// </summary>
        public static XrSpaceVelocityFlags XR_SPACE_VELOCITY_ANGULAR_VALID_BIT = 0x00000002;

        /// <summary>
        /// The only XrPath value defined to be constant across all instances is the invalid path XR_NULL_PATH. No well-formed path string is associated with XR_NULL_PATH. Unless explicitly permitted, it should not be passed to API calls or used as a structure attribute when a valid XrPath is required.
        /// </summary>
        public static UInt64 XR_NULL_PATH = 0;

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.
        /// </summary>
        /// <param name="instance">The instance from which system Id was retrieved.</param>
        /// <param name="createInfo">A pointer to an <see cref="XrSessionCreateInfo">XrSessionCreateInfo</see> structure containing information about how to create the session.</param>
        /// <param name="session">A pointer to a handle in which the created <see cref="XrSession">XrSession</see> is returned.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrCreateSessionDelegate(
            XrInstance instance,
            in XrSessionCreateInfo createInfo,
            XrSession session);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetInstanceProcAddr">xrGetInstanceProcAddr</see>.
        /// </summary>
        /// <param name="instance">The instance that the function pointer will be compatible with, or NULL for functions not dependent on any instance.</param>
        /// <param name="name">The name of the function to obtain.</param>
        /// <param name="function">The address of the function pointer to get.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrGetInstanceProcAddrDelegate(
            XrInstance instance,
            string name,
            out IntPtr function);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>.
        /// </summary>
        /// <param name="instance">The instance from which systemId was retrieved.</param>
        /// <param name="systemId">The <see cref="XrSystemId">XrSystemId</see> whose properties will be queried.</param>
        /// <param name="properties">Points to an instance of the <see cref="XrSystemProperties">XrSystemProperties</see> structure, that will be filled with returned information.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrGetSystemPropertiesDelegate(
            XrInstance instance,
            XrSystemId systemId,
            ref XrSystemProperties properties);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrEnumerateReferenceSpaces">xrEnumerateReferenceSpaces</see>.
        /// </summary>
        /// <param name="session">A handle to an <see cref="XrSession">XrSession</see> previously created with <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.</param>
        /// <param name="spaceCapacityInput">The capacity of the spaces array, or 0 to indicate a request to retrieve the required capacity.</param>
        /// <param name="spaceCountOutput">A pointer to the count of spaces written, or a pointer to the required capacity in the case that spaceCapacityInput is insufficient.</param>
        /// <param name="spaces">A pointer to an application-allocated array that will be filled with the enumerant of each supported reference space. It can be NULL if spaceCapacityInput is 0.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrEnumerateReferenceSpacesDelegate(
            XrSession session,
            UInt32 spaceCapacityInput,
            out UInt32 spaceCountOutput,
            out XrReferenceSpaceType spaces);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateReferenceSpace">xrCreateReferenceSpace</see>.
        /// </summary>
        /// <param name="session">A handle to an XrSession previously created with <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.</param>
        /// <param name="createInfo">The <see cref="XrReferenceSpaceCreateInfo">XrReferenceSpaceCreateInfo</see> indicates the space.</param>
        /// <param name="space">The returned space handle.</param>
        /// <returns></returns>
        public delegate XrResult xrCreateReferenceSpaceDelegate(
            XrSession session,
            ref XrReferenceSpaceCreateInfo createInfo,
            out XrSpace space);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroySpace">xrDestroySpace</see>.
        /// </summary>
        /// <param name="space"><b>Must</b> be a valid <see cref="XrSpace">XrSpace</see> handle</param>
        /// <returns></returns>
        public delegate XrResult xrDestroySpaceDelegate(
            XrSpace space);

        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSwapchainAndroidSurfaceKHR">xrCreateSwapchainAndroidSurfaceKHR</see>.
        /// </summary>
        /// <param name="session">A handle to an XrSession previously created with <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.</param>
        /// <param name="info">info is a pointer to an XrSwapchainCreateInfo structure.</param>
        /// <param name="swapchain">swapchain is a pointer to a handle in which the created XrSwapchain is returned.</param>
        /// <param name="surface">surface is a pointer to a jobject where the created Android Surface is returned.</param>
        /// <returns></returns>
        public delegate XrResult xrCreateSwapchainAndroidSurfaceKHRDelegate(
            XrSession session,
            in XrSwapchainCreateInfo info,
            out XrSwapchain swapchain,
            out IntPtr surface);

        public delegate XrResult xrRequestDisplayRefreshRateFBDelegate(
            XrSession session,
            float displayRefreshRate);

        public delegate XrResult xrGetDisplayRefreshRateFBDelegate(
            XrSession session,
            out float displayRefreshRate);

        public delegate XrResult xrEnumerateDisplayRefreshRatesFBDelegate(
            XrSession session,
            UInt32 displayRefreshRateCapacityInput,
            out UInt32 displayRefreshRateCountOutput,
            out float displayRefreshRates);

        public delegate XrResult xrGetInputSourceLocalizedNameDelegate(
            XrSession session,
            ref XrInputSourceLocalizedNameGetInfo getInfo,
            [In] UInt32 bufferCapacityInput,
            ref UInt32 bufferCountOutput,
            [In, Out] char[] buffer);

        public static XrResult GetInputSourceName(
            xrGetInputSourceLocalizedNameDelegate xrGetInputSourceLocalizedName,
            XrSession session,
            ref XrInputSourceLocalizedNameGetInfo nameInfo,
            out string sourceName)
        {
            string func = "GetInputSourceName() ";

            sourceName = "";
            if (xrGetInputSourceLocalizedName == null) { return XrResult.XR_ERROR_VALIDATION_FAILURE; }

            sb.Clear().Append(LOG_TAG).Append(func).Append("path: ").Append(nameInfo.sourcePath).Append(", flag: ").Append((UInt64)nameInfo.whichComponents); DEBUG(sb);

            UInt32 nameSizeIn = 0;
            UInt32 nameSizeOut = 0;
            char[] buffer = new char[0];

            XrResult result = xrGetInputSourceLocalizedName(session, ref nameInfo, nameSizeIn, ref nameSizeOut, buffer);
            sb.Clear().Append(LOG_TAG).Append(func)
                .Append("1.xrGetInputSourceLocalizedName(").Append(nameInfo.sourcePath).Append(") result: ").Append(result)
                .Append(", flag: ").Append((UInt64)nameInfo.whichComponents)
                .Append(", bufferCapacityInput: ").Append(nameSizeIn)
                .Append(", bufferCountOutput: ").Append(nameSizeOut);
            DEBUG(sb);
            if (result == XrResult.XR_SUCCESS)
            {
                if (nameSizeOut < 1)
                {
                    sb.Clear().Append(LOG_TAG).Append(func)
                        .Append("xrGetInputSourceLocalizedName(").Append(nameInfo.sourcePath).Append(")")
                        .Append(", flag: ").Append((UInt64)nameInfo.whichComponents)
                        .Append("bufferCountOutput size is invalid!");
                    ERROR(sb);
                    return XrResult.XR_ERROR_VALIDATION_FAILURE;
                }

                nameSizeIn = nameSizeOut;
                buffer = new char[nameSizeIn];

                result = xrGetInputSourceLocalizedName(session, ref nameInfo, nameSizeIn, ref nameSizeOut, buffer);
                sb.Clear().Append(LOG_TAG).Append(func)
                    .Append("2.xrGetInputSourceLocalizedName(").Append(nameInfo.sourcePath).Append(") result: ").Append(result)
                    .Append(", flag: ").Append((UInt64)nameInfo.whichComponents)
                    .Append(", bufferCapacityInput: ").Append(nameSizeIn)
                    .Append(", bufferCountOutput: ").Append(nameSizeOut);
                DEBUG(sb);
                if (result == XrResult.XR_SUCCESS) { sourceName = new string(buffer).TrimEnd('\0'); }
            }

            return result;
        }


        public delegate XrResult xrEnumerateInstanceExtensionPropertiesDelegate(
            [In] char[] layerName,
            UInt32 propertyCapacityInput,
            ref UInt32 propertyCountOutput,
            [In, Out] XrExtensionProperties[] properties);

        #region API
        const string LOG_TAG = "VIVE.OpenXR.OpenXRHelper ";
        static StringBuilder m_sb = null;
        static StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
        static void DEBUG(StringBuilder msg) { Debug.Log(msg); }
        static void WARNING(StringBuilder msg) { Debug.LogWarning(msg); }
        static void ERROR(StringBuilder msg) { Debug.LogError(msg); }

        /// <summary>
        /// Use <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrEnumerateInstanceExtensionProperties">xrEnumerateInstanceExtensionProperties</see> to check if an extension is supported by OpenXR Runtime.
        /// </summary>
        /// <param name="xrEnumerateInstanceExtensionProperties">Function pointer of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrEnumerateInstanceExtensionProperties">xrEnumerateInstanceExtensionProperties</see>.</param>
        /// <param name="extension">An OpenXR extension.</param>
        /// <returns>XR_SUCCESS for supported.</returns>
        public static XrResult IsExtensionSupported(xrEnumerateInstanceExtensionPropertiesDelegate xrEnumerateInstanceExtensionProperties, string extension)
        {
            XrResult result = XrResult.XR_ERROR_FEATURE_UNSUPPORTED;

            if (xrEnumerateInstanceExtensionProperties == null)
            {
                sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() no xrEnumerateInstanceExtensionProperties function."); ERROR(sb);
                return result;
            }

            UInt32 ext_count = 0;
            result = xrEnumerateInstanceExtensionProperties(null, 0, ref ext_count, null);
            if (result != XrResult.XR_SUCCESS || ext_count == 0)
            {
                sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() Failed to enumerate number of extension properties, result: ").Append(result); ERROR(sb);
                return result;
            }
            sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() Runtime supports ").Append(ext_count).Append(" extensions"); DEBUG(sb);

            XrExtensionProperties[] extensionProperties = new XrExtensionProperties[ext_count];
            for (int i = 0; i < ext_count; i++)
            {
                extensionProperties[i].type = XrStructureType.XR_TYPE_EXTENSION_PROPERTIES;
                extensionProperties[i].next = IntPtr.Zero;
            }

            sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() xrEnumerateInstanceExtensionProperties propertyCapacityInput: ").Append(ext_count).Append(", propertyCountOutput: ").Append(ext_count).Append(", extensionProperties size: ").Append(extensionProperties.Length); DEBUG(sb);
            result = xrEnumerateInstanceExtensionProperties(null, ext_count, ref ext_count, extensionProperties);
            if (result != XrResult.XR_SUCCESS)
            {
                sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() Failed to enumerate extension properties, result: ").Append(result); ERROR(sb);
                return result;
            }
            sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() Enumerate ").Append(ext_count).Append(" extensions"); DEBUG(sb);

            bool supported = false;
            for (UInt32 i = 0; i < ext_count; i++)
            {
                sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() Extension[").Append(i).Append("] ").Append(extensionProperties[i].type)
                    .Append(", name: ").Append(extensionProperties[i].extensionName)
                    .Append(", version: ").Append(extensionProperties[i].extensionVersion);
                DEBUG(sb);

                if (extensionProperties[i].extensionName.Equals(extension))
                {
                    supported = true;
                    break;
                }
            }
            sb.Clear().Append(LOG_TAG).Append("IsExtensionSupported() ").Append(extension).Append(" is ").Append(supported ? "supported." : "not supported."); DEBUG(sb);

            return supported ? XrResult.XR_SUCCESS : XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }
        #endregion

        public delegate XrResult xrCreateActionDelegate(
            XrActionSet actionSet,
            ref XrActionCreateInfo createInfo,
            ref XrAction action);

        public delegate XrRect2Di xrGetActionStatePoseDelegate(
            XrSession session,
            ref XrActionStateGetInfo getInfo,
            ref XrActionStatePose state);
        /// <summary>
        /// The function delegate declaration of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrWaitFrame">xrWaitFrame</see>.
        /// </summary>
        /// <param name="session">A handle to an XrSession previously created with <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see>.</param>
        /// <param name="frameWaitInfo">frameWaitInfo exists for extensibility purposes, it is NULL or a pointer to a valid XrFrameWaitInfo.</param>
        /// <param name="frameState">frameState is a pointer to a valid XrFrameState, an output parameter.</param>
        /// <returns></returns>
        public delegate int xrWaitFrameDelegate(ulong session, ref XrFrameWaitInfo frameWaitInfo, ref XrFrameState frameState);

        /// <summary>
        /// Help call xrGetInstanceProcAddr and convert the result to delegate.\
        /// For example, "OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrGetSystemProperties", out XrGetSystemProperties);"
        /// </summary>
        /// <typeparam name="Type">The function's delegate.</typeparam>
        /// <param name="XrGetInstanceProcAddr">Your xrGetInstanceProcAddr delegate instance.</param>
        /// <param name="xrInstance">Your xrInstance</param>
        /// <param name="name">The function name</param>
        /// <param name="func">The output delegate instance.</param>
        /// <returns>If return false, the outout delegate instance will be default.  Should not use it.</returns>
        public static bool GetXrFunctionDelegate<Type>(xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr, XrInstance xrInstance, string name, out Type func)
        {
            if (XrGetInstanceProcAddr(xrInstance, name, out var funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    Debug.Log("Get function pointer of " + name);
                    func = Marshal.GetDelegateForFunctionPointer<Type>(funcPtr);
                    return true;
                }
            }
            else
            {
                Debug.LogError("GetXrFunctionDelegate: fail to get address of function: " + name);
            }
            func = default;
            return false;
        }
    }

	public static class ClientInterface
    {
        /// <summary>
        /// Checks if the user is presence (near HMD p-sensor < 1cm).
        /// </summary>
        /// <returns>True for presence.</returns>
        public static bool IsUserPresence()
        {
#if UNITY_ANDROID
            if (ProximitySensor.current != null)
            {
                if (!ProximitySensor.current.IsActuated())
                    InputSystem.EnableDevice(ProximitySensor.current);

                return ProximitySensor.current.distance.ReadValue() < 1; // near p-sensor < 1cm
            }
            else
            {
                return false;
            }
#else
            return true;
#endif
        }

        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
        /// <summary>
        /// Retrieves current tracking origin.
        /// </summary>
        /// <returns>A origin mode of <see href="https://docs.unity3d.com/ScriptReference/XR.TrackingOriginModeFlags.html">TrackingOriginModeFlags</see></returns>
        public static TrackingOriginModeFlags TrackingOrigin()
        {
            SubsystemManager.GetSubsystems(s_InputSubsystems);
            if (s_InputSubsystems.Count > 0)
            {
                return s_InputSubsystems[0].GetTrackingOriginMode();
            }
            return TrackingOriginModeFlags.Unknown;
        }
    }
}
