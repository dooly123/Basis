// Copyright HTC Corporation All Rights Reserved.
#if UNITY_EDITOR
#define FAKE_DATA
#endif
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using VIVE.OpenXR.Feature;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.PlaneDetection
{
#if UNITY_EDITOR
	[OpenXRFeature(UiName = "VIVE XR PlaneDetection",
		Desc = "VIVE's implementaion of the XR_EXT_plane_detection.",
		Company = "HTC",
		DocumentationLink = "..\\Documentation",
		OpenxrExtensionStrings = kOpenxrExtensionString,
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
		FeatureId = featureId
	)]
#endif
	public class VivePlaneDetection : OpenXRFeature
	{
		public const string kOpenxrExtensionString = "XR_EXT_plane_detection";
		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.wave.openxr.feature.planedetection";
		private bool m_XrInstanceCreated = false;
		private XrInstance m_XrInstance = 0;
		private bool m_XrSessionCreated = false;
		private XrSession session = 0;
		private XrSystemId m_XrSystemId = 0;


		#region struct, enum, const of this extensions
		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorOrientationEXT">XrPlaneDetectorOrientationEXT</see>
		/// </summary>
		public enum XrPlaneDetectorOrientationEXT
		{
			HORIZONTAL_UPWARD_EXT = 0,
			HORIZONTAL_DOWNWARD_EXT = 1,
			VERTICAL_EXT = 2,
			ARBITRARY_EXT = 3,
			MAX_ENUM_EXT = 0x7FFFFFFF
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorSemanticTypeEXT">XrPlaneDetectorSemanticTypeEXT</see>
		/// </summary>
		public enum XrPlaneDetectorSemanticTypeEXT
		{
			UNDEFINED_EXT = 0,
			CEILING_EXT = 1,
			FLOOR_EXT = 2,
			WALL_EXT = 3,
			PLATFORM_EXT = 4,
			MAX_ENUM_EXT = 0x7FFFFFFF
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectionStateEXT">XrPlaneDetectionStateEXT</see>
		/// </summary>
		public enum XrPlaneDetectionStateEXT
		{
			NONE_EXT = 0,
			PENDING_EXT = 1,  // Try get plane detection state again
			DONE_EXT = 2,  // Ready to get result
			ERROR_EXT = 3,  // Can try begin again
			FATAL_EXT = 4,  // Should destroy the plane detector
			MAX_ENUM_EXT = 0x7FFFFFFF
		}

		//XrFlags64 XrPlaneDetectionCapabilityFlagsEXT;

		// Flag bits for XrPlaneDetectionCapabilityFlagsEXT
		public static XrFlags64 CAPABILITY_PLANE_DETECTION_BIT_EXT = 0x00000001;
		public static XrFlags64 CAPABILITY_PLANE_HOLES_BIT_EXT = 0x00000002;
		public static XrFlags64 CAPABILITY_SEMANTIC_CEILING_BIT_EXT = 0x00000004;
		public static XrFlags64 CAPABILITY_SEMANTIC_FLOOR_BIT_EXT = 0x00000008;
		public static XrFlags64 CAPABILITY_SEMANTIC_WALL_BIT_EXT = 0x00000010;
		public static XrFlags64 CAPABILITY_SEMANTIC_PLATFORM_BIT_EXT = 0x00000020;
		public static XrFlags64 CAPABILITY_ORIENTATION_BIT_EXT = 0x00000040;

		//XrFlags64 XrPlaneDetectorFlagsEXT;

		// Flag bits for XrPlaneDetectorFlagsEXT
		public static XrFlags64 XR_PLANE_DETECTOR_ENABLE_CONTOUR_BIT_EXT = 0x00000001;

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrSystemPlaneDetectionPropertiesEXT">XrSystemPlaneDetectionPropertiesEXT</see>
		/// </summary>
		public struct XrSystemPlaneDetectionPropertiesEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public XrFlags64 supportedFeatures;  // XrPlaneDetectionCapabilityFlagsEXT
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorCreateInfoEXT">XrPlaneDetectorCreateInfoEXT</see>
		/// </summary>
		public struct XrPlaneDetectorCreateInfoEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public XrFlags64 flags;  // XrPlaneDetectorFlagsEXT
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrExtent3DfEXT">XrExtent3DfEXT</see>
		/// </summary>
		public struct XrExtent3DfEXT
		{
			public float width;
			public float height;
			public float depth;
			public XrExtent3DfEXT(float width, float height, float depth)
			{
				this.width = width;
				this.height = height;
				this.depth = depth;
			}
			public XrExtent3DfEXT One => new XrExtent3DfEXT(1, 1, 1);
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorBeginInfoEXT">XrPlaneDetectorBeginInfoEXT</see>
		/// </summary>
		public struct XrPlaneDetectorBeginInfoEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public XrSpace baseSpace;
			public XrTime time;
			public uint orientationCount;
			public IntPtr orientations;  // XrPlaneDetectorOrientationEXT[] 
			public uint semanticTypeCount;
			public IntPtr semanticTypes;  // XrPlaneDetectorSemanticTypeEXT[]
			public uint maxPlanes;
			public float minArea;
			public XrPosef boundingBoxPose;
			public XrExtent3DfEXT boundingBoxExtent;
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorGetInfoEXT">XrPlaneDetectorGetInfoEXT</see>
		/// </summary>
		public struct XrPlaneDetectorGetInfoEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public XrSpace baseSpace;
			public XrTime time;
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorLocationEXT">XrPlaneDetectorLocationEXT</see>
		/// </summary>
		public struct XrPlaneDetectorLocationEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public ulong planeId;
			public XrSpaceLocationFlags locationFlags;
			public XrPosef pose;
			public XrExtent2Df extents;
			public XrPlaneDetectorOrientationEXT orientation;
			public XrPlaneDetectorSemanticTypeEXT semanticType;
			public uint polygonBufferCount;
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorLocationsEXT">XrPlaneDetectorLocationsEXT</see>
		/// </summary>
		public struct XrPlaneDetectorLocationsEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public uint planeLocationCapacityInput;
			public uint planeLocationCountOutput;
			public IntPtr planeLocations;  // XrPlaneDetectorLocationEXT[]
		}

		/// <summary>
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrPlaneDetectorPolygonBufferEXT">XrPlaneDetectorPolygonBufferEXT</see>
		/// </summary>
		public struct XrPlaneDetectorPolygonBufferEXT
		{
			public XrStructureType type;
			public IntPtr next;
			public uint vertexCapacityInput;
			public uint vertexCountOutput;
			public IntPtr vertices;  // XrVector2f[]
		}
		#endregion

		#region delegates and delegate instances

		delegate XrResult DelegateXrCreatePlaneDetectorEXT(XrSession session, ref XrPlaneDetectorCreateInfoEXT createInfo, ref IntPtr/*XrPlaneDetectorEXT*/
planeDetector);
		delegate XrResult DelegateXrDestroyPlaneDetectorEXT(IntPtr/*XrPlaneDetectorEXT*/ planeDetector);
		delegate XrResult DelegateXrBeginPlaneDetectionEXT(IntPtr/*XrPlaneDetectorEXT*/ planeDetector, ref XrPlaneDetectorBeginInfoEXT beginInfo);
		delegate XrResult DelegateXrGetPlaneDetectionStateEXT(IntPtr/*XrPlaneDetectorEXT*/planeDetector, ref XrPlaneDetectionStateEXT state);
		delegate XrResult DelegateXrGetPlaneDetectionsEXT(IntPtr/*XrPlaneDetectorEXT*/planeDetector, ref XrPlaneDetectorGetInfoEXT info, ref XrPlaneDetectorLocationsEXT locations);
		delegate XrResult DelegateXrGetPlanePolygonBufferEXT(IntPtr/*XrPlaneDetectorEXT*/planeDetector, ulong planeId, uint polygonBufferIndex, ref XrPlaneDetectorPolygonBufferEXT polygonBuffer);

		DelegateXrCreatePlaneDetectorEXT XrCreatePlaneDetectorEXT;
		DelegateXrDestroyPlaneDetectorEXT XrDestroyPlaneDetectorEXT;
		DelegateXrBeginPlaneDetectionEXT XrBeginPlaneDetectionEXT;
		DelegateXrGetPlaneDetectionStateEXT XrGetPlaneDetectionStateEXT;
		DelegateXrGetPlaneDetectionsEXT XrGetPlaneDetectionsEXT;
		DelegateXrGetPlanePolygonBufferEXT XrGetPlanePolygonBufferEXT;
		#endregion delegates and delegate instances

		#region override functions
		protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
		{
			return ViveInterceptors.Instance.HookGetInstanceProcAddr(func);
		}

		/// <inheritdoc />
		protected override bool OnInstanceCreate(ulong xrInstance)
		{
			//Debug.Log("VIVEPD OnInstanceCreate() ");
			if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
			{
				Debug.LogWarning("OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
				return false;
			}

			m_XrInstanceCreated = true;
			m_XrInstance = xrInstance;
			//Debug.Log("OnInstanceCreate() " + m_XrInstance);

			CommonWrapper.Instance.OnInstanceCreate(xrInstance, xrGetInstanceProcAddr);
			SpaceWrapper.Instance.OnInstanceCreate(xrInstance, CommonWrapper.Instance.GetInstanceProcAddr);

			return GetXrFunctionDelegates(m_XrInstance);
		}

		protected override void OnInstanceDestroy(ulong xrInstance)
		{
			CommonWrapper.Instance.OnInstanceDestroy();
			SpaceWrapper.Instance.OnInstanceDestroy();
		}

		/// <inheritdoc />
		protected override void OnSessionCreate(ulong xrSession)
		{
			Debug.Log("VIVEPD OnSessionCreate() ");

			// here's one way you can grab the session
			Debug.Log($"EXT: Got xrSession: {xrSession}");
			session = xrSession;
			m_XrSessionCreated = true;
		}

		/// <inheritdoc />
		protected override void OnSessionBegin(ulong xrSession)
		{
			Debug.Log("VIVEPD OnSessionBegin() ");

			Debug.Log($"EXT: xrBeginSession: {xrSession}");
		}

		/// <inheritdoc />
		protected override void OnSessionEnd(ulong xrSession)
		{
			Debug.Log("VIVEPD OnSessionEnd() ");

			Debug.Log($"EXT: about to xrEndSession: {xrSession}");
		}

		// XXX Every millisecond the AppSpace switched from one space to another space. I don't know what is going on.
		//private ulong appSpace;
		//protected override void OnAppSpaceChange(ulong space)
		//{
		//	Debug.Log($"VIVEPD OnAppSpaceChange({appSpace} -> {space})");
		//	appSpace = space;
		//}

		protected override void OnSystemChange(ulong xrSystem)
		{
			m_XrSystemId = xrSystem;
			Debug.Log("OnSystemChange() " + m_XrSystemId);
		}


		#endregion override functions

		private bool GetXrFunctionDelegates(XrInstance xrInstance)
		{
			Debug.Log("VIVEPD GetXrFunctionDelegates() ");

			bool ret = true;
			IntPtr funcPtr = IntPtr.Zero;
			OpenXRHelper.xrGetInstanceProcAddrDelegate GetAddr = CommonWrapper.Instance.GetInstanceProcAddr;  // shorter name
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrCreatePlaneDetectorEXT", out XrCreatePlaneDetectorEXT);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrDestroyPlaneDetectorEXT", out XrDestroyPlaneDetectorEXT);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrBeginPlaneDetectionEXT", out XrBeginPlaneDetectionEXT);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrGetPlaneDetectionStateEXT", out XrGetPlaneDetectionStateEXT);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrGetPlaneDetectionsEXT", out XrGetPlaneDetectionsEXT);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrGetPlanePolygonBufferEXT", out XrGetPlanePolygonBufferEXT);
			return ret;
		}

		#region functions of extension
		/// <summary>
		/// Helper function to get this feature' properties.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>
		/// </summary>
		public XrResult GetProperties(out XrSystemPlaneDetectionPropertiesEXT properties)
		{
			properties = new XrSystemPlaneDetectionPropertiesEXT();
			properties.type = XrStructureType.XR_TYPE_SYSTEM_PLANE_DETECTION_PROPERTIES_EXT;

#if FAKE_DATA
			if (Application.isEditor)
			{
				properties.supportedFeatures = CAPABILITY_PLANE_DETECTION_BIT_EXT;
				return XrResult.XR_SUCCESS;
			}
#endif
			if (!m_XrSessionCreated)
			{
				Debug.LogError("GetProperties() XR_ERROR_SESSION_LOST.");
				return XrResult.XR_ERROR_SESSION_LOST;
			}
			if (!m_XrInstanceCreated)
			{
				Debug.LogError("GetProperties() XR_ERROR_INSTANCE_LOST.");
				return XrResult.XR_ERROR_INSTANCE_LOST;
			}

			return CommonWrapper.Instance.GetProperties(m_XrInstance, m_XrSystemId, ref properties);
		}

		/// <summary>
		/// Create a PlaneDetector with <paramref name="createInfo"/>.  XrSession is implied.  The output handle need be destroyed.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreatePlaneDetectorEXT">xrCreatePlaneDetectorEXT</see>
		/// </summary>
		/// <param name="createInfo">Fill flags for detection engine.</param>
		/// <param name="planeDetector">The output detector's handle.</param>
		/// <seealso cref="DestroyPlaneDetector"/>
		public XrResult CreatePlaneDetector(XrPlaneDetectorCreateInfoEXT createInfo, out IntPtr/*XrPlaneDetectorEXT*/ planeDetector)
		{
			planeDetector = IntPtr.Zero;
#if FAKE_DATA
			if (Application.isEditor)
				return XrResult.XR_SUCCESS;
#endif
			return XrCreatePlaneDetectorEXT(session, ref createInfo, ref planeDetector);
		}

		/// <summary>
		/// Destroy the PlaneDetector handle.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyPlaneDetectorEXT">xrDestroyPlaneDetectorEXT</see>
		/// </summary>
		/// <param name="planeDetector">The detector's handle to be destroyed.</param>
		/// <seealso cref="CreatePlaneDetector"/>
		public XrResult DestroyPlaneDetector(IntPtr/*XrPlaneDetectorEXT*/ planeDetector)
		{
#if FAKE_DATA
			if (Application.isEditor)
				return XrResult.XR_SUCCESS;
#endif
			return XrDestroyPlaneDetectorEXT(planeDetector);
		}

		/// <summary>
		/// Let Detector start to work.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrBeginPlaneDetectionEXT">xrBeginPlaneDetectionEXT</see>
		/// </summary>
		/// <param name="planeDetector">The detector's handle to be begined.</param>
		/// <param name="beginInfo">Fill flags for detection engine.  You can use the result of MakeGetAllXrPlaneDetectorBeginInfoEXT.</param>
		/// <seealso cref="MakeGetAllXrPlaneDetectorBeginInfoEXT"/>
		public XrResult BeginPlaneDetection(IntPtr/*XrPlaneDetectorEXT*/ planeDetector, XrPlaneDetectorBeginInfoEXT beginInfo)
		{
#if FAKE_DATA
			if (Application.isEditor)
				return XrResult.XR_SUCCESS;
#endif
			return XrBeginPlaneDetectionEXT(planeDetector, ref beginInfo);
		}

		/// <summary>
		/// Check PlaneDetector's state.  If the state is DONE_EXT, you can get the result by GetPlaneDetections.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetPlaneDetectionStateEXT">xrGetPlaneDetectionStateEXT</see>
		/// </summary>
		/// <param name="planeDetector">The detector's state to be check.</param>
		/// <param name="state">Fill flags for detection engine.  You can use the result of MakeGetAllXrPlaneDetectorBeginInfoEXT.</param>
		public XrResult GetPlaneDetectionState(IntPtr/*XrPlaneDetectorEXT*/ planeDetector, ref XrPlaneDetectionStateEXT state)
		{
#if FAKE_DATA
			if (Application.isEditor)
			{
				state = XrPlaneDetectionStateEXT.DONE_EXT;
				return XrResult.XR_SUCCESS;
			}
#endif
			return XrGetPlaneDetectionStateEXT(planeDetector, ref state);
		}

		/// <summary>
		/// Get the result of PlaneDetector.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetPlaneDetectionsEXT">xrGetPlaneDetectionsEXT</see>
		/// </summary>
		/// <param name="planeDetector">The detector's state to be check.</param>
		/// <param name="info">Use info to specify the data's space.</param>
		/// <param name="locations">The output data.</param>
		public XrResult GetPlaneDetections(IntPtr/*XrPlaneDetectorEXT*/ planeDetector, ref XrPlaneDetectorGetInfoEXT info, ref XrPlaneDetectorLocationsEXT locations)
		{
#if FAKE_DATA
			if (Application.isEditor)
			{
				locations.planeLocationCountOutput = 1;
				if (locations.planeLocationCapacityInput == 0)
					return XrResult.XR_SUCCESS;
				if (locations.planeLocationCapacityInput < 1 || locations.planeLocations == IntPtr.Zero)
					return XrResult.XR_ERROR_SIZE_INSUFFICIENT;

				locations.planeLocationCountOutput = 1;
				XrPlaneDetectorLocationEXT location = new XrPlaneDetectorLocationEXT();
				location.planeId = 1;
				location.extents = new XrExtent2Df(1, 1);
				location.locationFlags = XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT | XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT;
				location.semanticType = XrPlaneDetectorSemanticTypeEXT.FLOOR_EXT;
				location.polygonBufferCount = 1;
				location.pose = new XrPosef(XrQuaternionf.Identity, new XrVector3f(0, 1, -1));  // This plane will face the Z axis
				location.orientation = XrPlaneDetectorOrientationEXT.VERTICAL_EXT;

				Marshal.StructureToPtr(location, locations.planeLocations, false);
				return XrResult.XR_SUCCESS;
			}
#endif
			return XrGetPlaneDetectionsEXT(planeDetector, ref info, ref locations);
		}

		/// <summary>
		/// Get the vertex buffer of a plane.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetPlanePolygonBufferEXT">GetPlanePolygonBuffer</see>
		/// </summary>
		/// <param name="planeDetector">The detector's state to be check.</param>
		/// <param name="planeId">The target plane's planeId. Get it from <see cref="XrPlaneDetectorLocationEXT" />.</param>
		/// <param name="polygonBufferIndex">The buffer index in the plane. Get it from <see cref="XrPlaneDetectorLocationEXT" />. Currently VIVE will only return 1 buffer for each plane.</param>
		/// <param name="polygonBuffer">The output data.</param>
		public XrResult GetPlanePolygonBuffer(IntPtr/*XrPlaneDetectorEXT*/ planeDetector, ulong planeId, uint polygonBufferIndex, ref XrPlaneDetectorPolygonBufferEXT polygonBuffer)
		{
#if FAKE_DATA
			if (Application.isEditor)
			{
				if (planeId != 1) return XrResult.XR_ERROR_NAME_INVALID;
				if (polygonBufferIndex != 0) return XrResult.XR_ERROR_INDEX_OUT_OF_RANGE;
				polygonBuffer.vertexCountOutput = 4;
				if (polygonBuffer.vertexCapacityInput == 0)
					return XrResult.XR_SUCCESS;
				if (polygonBuffer.vertexCapacityInput != 4 || polygonBuffer.vertices == IntPtr.Zero)
					return XrResult.XR_ERROR_SIZE_INSUFFICIENT;
				XrVector2f[] vertices = new XrVector2f[4];
				// Make a plane's contour
				vertices[0] = new XrVector2f(-0.5f, -0.5f);
				vertices[1] = new XrVector2f( 0.5f, -0.5f);
				vertices[2] = new XrVector2f( 0.5f,  0.5f);
				vertices[3] = new XrVector2f(-0.5f,  0.5f);

				MemoryTools.CopyToRawMemory(polygonBuffer.vertices, vertices);
				
				return XrResult.XR_SUCCESS;
			}
#endif
			return XrGetPlanePolygonBufferEXT(planeDetector, planeId, polygonBufferIndex, ref polygonBuffer);
		}
		#endregion

		#region tools for user
		/// <summary>
		/// A helper function to generate XrPlaneDetectorBeginInfoEXT.  VIVE didn't implement all possible features of this extension.
		/// Hardcode some parameters here.
		/// </summary>
		/// <seealso cref="BeginPlaneDetection"/>
		public XrPlaneDetectorBeginInfoEXT MakeGetAllXrPlaneDetectorBeginInfoEXT()
		{
			XrPlaneDetectorBeginInfoEXT beginInfo = new XrPlaneDetectorBeginInfoEXT
			{
				type = XrStructureType.XR_TYPE_PLANE_DETECTOR_BEGIN_INFO_EXT,
				baseSpace = new XrSpace(GetCurrentAppSpace()),  // Cannot depend on GetCurrentAppSpace...
				//baseSpace = GetTrackingSpace(),
				time = ViveInterceptors.Instance.GetPredictTime(),
				orientationCount = 0,  // Any orientation
				orientations = IntPtr.Zero,  // XrPlaneDetectorOrientationEXT[]
				semanticTypeCount = 0,  // Any semantic type
				semanticTypes = IntPtr.Zero,  // XrPlaneDetectorSemanticTypeEXT[]
				maxPlanes = 10000,  // Hopefully enough
				minArea = 0.1f,  // 10cm^2
				boundingBoxPose = XrPosef.Identity,
				boundingBoxExtent = new XrExtent3DfEXT(1000, 1000, 1000),
			};
			return beginInfo;
		}
		
		/// <summary>
		/// The time here is only used for info of GetPlaneDetections.  Not the real predictTime of XrWaitFrame.
		/// </summary>
		/// <returns></returns>
		public XrTime GetPredictTime()
		{
			return ViveInterceptors.Instance.GetPredictTime();
		}

		/// <summary>
		/// According to XRInputSubsystem's tracking origin mode, return the corresponding XrSpace.
		/// </summary>
		/// <returns></returns>
		public XrSpace GetTrackingSpace()
		{
			return GetCurrentAppSpace();
		}
		#endregion
	}
}