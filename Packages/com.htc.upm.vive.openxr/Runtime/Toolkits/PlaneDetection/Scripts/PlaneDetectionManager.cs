// Copyright HTC Corporation All Rights Reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.PlaneDetection;
using static VIVE.OpenXR.PlaneDetection.VivePlaneDetection;

namespace VIVE.OpenXR.Toolkits.PlaneDetection
{
	/// <summary>
	/// The detected plane's data.
	/// See <see cref="VivePlaneDetection.XrPlaneDetectorLocationEXT"/>
	/// </summary>
	public class PlaneDetectorLocation
	{
		public ulong planeId;
		public XrSpaceLocationFlags locationFlags;
		public Pose pose;
		public Vector3 size;  // Only width(X) and height(Y) are valid, Z is always 0.
		public XrPlaneDetectorOrientationEXT orientation;
		public XrPlaneDetectorSemanticTypeEXT semanticType;
		public uint polygonBufferCount;
		public XrPlaneDetectorLocationEXT locationRaw;
	}

	/// <summary>
	/// The information for creating Mesh.
	/// Plane's normal is facing +Z in Unity's coordination.
	/// </summary>
	public class Plane
	{
		public Vector3 scale;  // Only width(X) and height(Y) are valid, Z is always 1.
        public Vector3 center;  // Should always be Vector3.Zero.
		public Vector3[] verticesRaw;  // The original vertices from <see cref="XrPlaneDetectorPolygonBufferEXT"/>
		public Vector3[] verticesGenerated;  // generated vertices for creating Mesh.
		public Vector2[] uvsGenerated;
		public int[] indicesGenerated;

		/// <summary>
		/// According to the input vertices, calculate the rectangle, and create the plane.
		/// </summary>
		/// <param name="vertices">The vertices from <see cref="XrPlaneDetectorPolygonBufferEXT"/></param>
		public static Plane CreateFromVertices(Vector2[] vertices)
		{
			// Assume the polygon is a rectangle.
			if (vertices.Length != 4)
				return null;
			// Check the size from vertices.
			Vector2 min, max;
			min = max = new Vector2(vertices[0].x, vertices[0].y);
			for (int i = 1; i < vertices.Length; i++)
			{
				min.x = Mathf.Min(min.x, vertices[i].x);
				min.y = Mathf.Min(min.y, vertices[i].y);
				max.x = Mathf.Max(max.x, vertices[i].x);
				max.y = Mathf.Max(max.y, vertices[i].y);
			}
			var verticesRaw = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				verticesRaw[i] = new Vector3(vertices[i].x, 0, vertices[i].y);

			var verticesGenerated = new Vector3[4];
			verticesGenerated[0] = new Vector3(min.x, min.y, 0);
			verticesGenerated[1] = new Vector3(max.x, min.y, 0);
			verticesGenerated[2] = new Vector3(min.x, max.y, 0);
			verticesGenerated[3] = new Vector3(max.x, max.y, 0);

			var indicesGenerated = new int[] { 0, 3, 2, 0, 1, 3 };

			var uvsGenerated = new Vector2[4];
			uvsGenerated[0] = new Vector2(0, 0);
			uvsGenerated[1] = new Vector2(1, 0);
			uvsGenerated[2] = new Vector2(0, 1);
			uvsGenerated[3] = new Vector2(1, 1);

			return new Plane()
			{
				scale = max - min,
				center = (max + min) / 2,
				verticesRaw = verticesRaw,
				verticesGenerated = verticesGenerated,
				indicesGenerated = indicesGenerated,
				uvsGenerated = uvsGenerated
			};
		}
	}

	/// <summary>
	/// The PlaneDetector is created by <see cref="PlaneDetectionManager.CreatePlaneDetector" />.
	/// </summary>
	public class PlaneDetector
	{
		IntPtr planeDetector = IntPtr.Zero;
		VivePlaneDetection feature = null;

		public PlaneDetector(IntPtr pd, VivePlaneDetection f)
		{
			feature = f;
			planeDetector = pd;
		}

		/// <summary>
		/// Get the raw plane detector handle.  See <see cref="VivePlaneDetection.CreatePlaneDetector"/>
		/// </summary>
		/// <returns>The raw plane detector handle</returns>
		public IntPtr GetDetectorRaw()
		{
			return planeDetector;
		}

		/// <summary>
		/// Begin detect plane.  In VIVE's implementation, planes are predefined in Room Setup. 
		/// However you have to call this function to get the plane information.
		/// See <see cref="VivePlaneDetection.BeginPlaneDetection"/>
		/// </summary>
		public XrResult BeginPlaneDetection()
		{
			if (feature == null)
				return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
			Debug.Log("BeginPlaneDetection()");
			var beginInfo = feature.MakeGetAllXrPlaneDetectorBeginInfoEXT();
			return feature.BeginPlaneDetection(planeDetector, beginInfo);
		}

		/// <summary>
		/// Get the state of plane detection.
		/// See <see cref="VivePlaneDetection.GetPlaneDetectionState"/>
		/// </summary>
		/// <returns></returns>
		public XrPlaneDetectionStateEXT GetPlaneDetectionState()
		{
			if (feature == null)
				return XrPlaneDetectionStateEXT.NONE_EXT;
			Debug.Log("GetPlaneDetectionState()");
			XrPlaneDetectionStateEXT state = XrPlaneDetectionStateEXT.NONE_EXT;
			feature.GetPlaneDetectionState(planeDetector, ref state);
			return state;
		}

		/// <summary>
		/// Get result of plane detection.
		/// See <see cref="VivePlaneDetection.GetPlaneDetections"/>
		/// </summary>
		/// <param name="locations">The detected planes.</param>
		/// <returns></returns>
		public XrResult GetPlaneDetections(out List<PlaneDetectorLocation> locations)
		{
			locations = null;
			if (feature == null)
				return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
			Debug.Log("GetPlaneDetections()");
			XrPlaneDetectorGetInfoEXT info = new XrPlaneDetectorGetInfoEXT
			{
				type = XrStructureType.XR_TYPE_PLANE_DETECTOR_GET_INFO_EXT,
				baseSpace = feature.GetTrackingSpace(),
				time = feature.GetPredictTime(),
			};
			XrPlaneDetectorLocationsEXT locationsRaw = new XrPlaneDetectorLocationsEXT
			{
				type = XrStructureType.XR_TYPE_PLANE_DETECTOR_LOCATIONS_EXT,
				planeLocationCapacityInput = 0,
				planeLocationCountOutput = 0,
				planeLocations = IntPtr.Zero,
			};
			var ret = feature.GetPlaneDetections(planeDetector, ref info, ref locationsRaw);
			if (ret != XrResult.XR_SUCCESS || locationsRaw.planeLocationCountOutput == 0)
				return ret;
			
			Debug.Log("GetPlaneDetections() locations.planeLocationCountOutput: " + locationsRaw.planeLocationCountOutput);
			var locationsArray = new XrPlaneDetectorLocationEXT[locationsRaw.planeLocationCountOutput];
			var locationsPtr = MemoryTools.MakeRawMemory(locationsArray);
			locationsRaw.planeLocationCapacityInput = locationsRaw.planeLocationCountOutput;
			locationsRaw.planeLocationCountOutput = 0;
			locationsRaw.planeLocations = locationsPtr;

			ret = feature.GetPlaneDetections(planeDetector, ref info, ref locationsRaw);
			if (ret != XrResult.XR_SUCCESS)
			{
				MemoryTools.ReleaseRawMemory(locationsPtr);
				return ret;
			}
			MemoryTools.CopyFromRawMemory(locationsArray, locationsPtr);

			locations = new List<PlaneDetectorLocation>();

            // The plane's neutral pose is horizontal, and not like the plane pose in unity which is vertical.
            // Therefore, we wil perform a rotation to convert from the OpenXR's forward to unity's forward.
            // In Unity, the rotation applied order is in ZXY order.
            Quaternion forward = Quaternion.Euler(-90, 180, 0);
			for (int i = 0; i < locationsRaw.planeLocationCountOutput; i++)
			{
				XrPlaneDetectorLocationEXT location = locationsArray[i];
				PlaneDetectorLocation pdl = new PlaneDetectorLocation
				{
					planeId = location.planeId,
					locationFlags = location.locationFlags,
					pose = new Pose(OpenXRHelper.ToUnityVector(location.pose.position), OpenXRHelper.ToUnityQuaternion(location.pose.orientation) * forward),
					// Because the pose is converted, we will apply extent to X and Y.
					size = new Vector3(location.extents.width, location.extents.height, 0),
					orientation = location.orientation,
					semanticType = location.semanticType,
					polygonBufferCount = location.polygonBufferCount,
					locationRaw = location,
				};
				locations.Add(pdl);
			}

			MemoryTools.ReleaseRawMemory(locationsPtr);

			for (int i = 0; i < locationsRaw.planeLocationCountOutput; i++)
			{
				var location = locations[i];
				Debug.Log("GetPlaneDetections() location.planeId: " + location.planeId);
				Debug.Log("GetPlaneDetections() location.locationFlags: " + location.locationFlags);
				Debug.Log("GetPlaneDetections() location.pose.position: " + location.pose.position);
				Debug.Log("GetPlaneDetections() location.pose.rotation: " + location.pose.rotation);
				Debug.Log("GetPlaneDetections() location.pose.rotation.eulerAngles: " + location.pose.rotation.eulerAngles);
				var rot = location.locationRaw.pose.orientation;
				Debug.Log($"GetPlaneDetections() locationRaw.pose.rotation: {rot.x}, {rot.y}, {rot.z}, {rot.w}");
			}

			return ret;
		}

		/// <summary>
		/// Get the vertices of the plane from extension.  Because there is no triangle 
		/// information from extension, it is hard to generate a mesh from only these vertices.
		/// However VIVE only have rectangle plane.  In this function, it will return the
		/// <see cref="Plane"/> class which contains generated information for creating Mesh.
		/// </summary>
		/// <param name="planeId">The planeId from <see cref="PlaneDetectorLocation"/></param>
		/// <returns>The information for creating Mesh.</returns>
		public Plane GetPlane(ulong planeId)
		{
			if (feature == null)
				return null;

			XrPlaneDetectorPolygonBufferEXT polygonBuffer = new XrPlaneDetectorPolygonBufferEXT
			{
				type = XrStructureType.XR_TYPE_PLANE_DETECTOR_POLYGON_BUFFER_EXT,
				vertexCapacityInput = 0,
				vertexCountOutput = 0,
				vertices = IntPtr.Zero,
			};

			var ret = feature.GetPlanePolygonBuffer(planeDetector, planeId, 0, ref polygonBuffer);
			Debug.Log("GetPlane() polygonBuffer.vertexCountOutput: " + polygonBuffer.vertexCountOutput);
			if (ret != XrResult.XR_SUCCESS || polygonBuffer.vertexCountOutput == 0)
				return null;
			var verticesArray = new Vector2[polygonBuffer.vertexCountOutput];
			var verticesPtr = MemoryTools.MakeRawMemory(verticesArray);
			polygonBuffer.vertexCapacityInput = polygonBuffer.vertexCountOutput;
			polygonBuffer.vertexCountOutput = 0;
			polygonBuffer.vertices = verticesPtr;
			if (feature.GetPlanePolygonBuffer(planeDetector, planeId, 0, ref polygonBuffer) != XrResult.XR_SUCCESS)
			{
				MemoryTools.ReleaseRawMemory(verticesPtr);
				return null;
			}
			MemoryTools.CopyFromRawMemory(verticesArray, verticesPtr);
			MemoryTools.ReleaseRawMemory(verticesPtr);

			for (int j = 0; j < verticesArray.Length; j++)
			{
				var v = verticesArray[j];
				Debug.Log($"GetPlane() verticesArray[{j}]: ({v.x}, {v.y})");
			}
			return Plane.CreateFromVertices(verticesArray);
		}
	}

	public static class PlaneDetectionManager
	{
		static VivePlaneDetection feature = null;
        static bool isSupported = false;

        static void CheckFeature()
		{
			if (feature != null) return;
			feature = OpenXRSettings.Instance.GetFeature<VivePlaneDetection>();
			if (feature == null)
				throw new NotSupportedException("PlaneDetection feature is not enabled");
		}

		/// <summary>
		/// Helper to get the extention feature instance.
		/// </summary>
		/// <returns></returns>
		public static VivePlaneDetection GetFeature()
		{
            try
            {
                CheckFeature();
            }
            catch (NotSupportedException)
            {
                Debug.LogWarning("PlaneDetection feature is not enabled");
                return null;
            }
            return feature;
		}

		/// <summary>
		/// Check if the extension is supported.
		/// </summary>
		/// <returns></returns>
		public static bool IsSupported()
		{
			if (GetFeature() == null) return false;
            if (isSupported) return true;
			if (feature == null) return false;

			bool ret = false;

			if (feature.GetProperties(out var properties) == XrResult.XR_SUCCESS)
			{
				Debug.Log("PlaneDetection: IsSupported() properties.supportedFeatures: " + properties.supportedFeatures);
				ret = (properties.supportedFeatures & CAPABILITY_PLANE_DETECTION_BIT_EXT) > 0;
                isSupported = ret;
            }
            else
			{
				Debug.Log("PlaneDetection: IsSupported() GetSystemProperties failed.");
			}
			return ret;
		}


		/// <summary>
		/// This is a helper function.  Currently only one createInfo is available.  Developepr should create their own
		/// </summary>
		/// <returns></returns>
		public static XrPlaneDetectorCreateInfoEXT MakeXrPlaneDetectorCreateInfoEXT()
		{
			return new XrPlaneDetectorCreateInfoEXT
			{
				type = XrStructureType.XR_TYPE_PLANE_DETECTOR_CREATE_INFO_EXT,
				flags = XR_PLANE_DETECTOR_ENABLE_CONTOUR_BIT_EXT,
			};
		}

		/// <summary>
		/// Plane detector is a session of detect plane.  You don't need to create multiple plane detector in VIVE's implemention.  You need destroy it.
		/// </summary>
		/// <returns>PlaneDetector's handle</returns>
		public static PlaneDetector CreatePlaneDetector()
		{
			CheckFeature();
			if (feature == null)
				return null;

			var createInfo = MakeXrPlaneDetectorCreateInfoEXT();
			var ret = feature.CreatePlaneDetector(createInfo, out var planeDetector);
			if (ret != XrResult.XR_SUCCESS)
				return null;
			return new PlaneDetector(planeDetector, feature);
		}

		/// <summary>
		/// Destroy the plane detector to release resource.
		/// </summary>
		public static void DestroyPlaneDetector(PlaneDetector pd)
		{
			if (pd == null)
				return;
			CheckFeature();
			if (feature == null)
				return;
			feature.DestroyPlaneDetector(pd.GetDetectorRaw());
		}
	}
}