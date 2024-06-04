// Copyright HTC Corporation All Rights Reserved.

// Remove FAKE_DATA if editor or windows is supported.
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

namespace VIVE.OpenXR.Anchor
{
#if UNITY_EDITOR
	[OpenXRFeature(UiName = "VIVE XR Anchor",
		Desc = "VIVE's implementaion of the XR_HTC_anchor.",
		Company = "HTC",
		DocumentationLink = "..\\Documentation",
		OpenxrExtensionStrings = kOpenxrExtensionString,
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
		FeatureId = featureId
	)]
#endif
	public class ViveAnchor : OpenXRFeature
	{
		public const string kOpenxrExtensionString = "XR_HTC_anchor";
		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.wave.openxr.feature.htcanchor";
		private XrInstance m_XrInstance = 0;
		private XrSession session = 0;
		private XrSystemId m_XrSystemId = 0;

		#region struct, enum, const of this extensions

		public struct XrSystemAnchorPropertiesHTC
		{
			public XrStructureType type;
			public System.IntPtr next;
			public XrBool32 supportsAnchor;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct XrSpatialAnchorNameHTC
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string name;
		}

		public struct XrSpatialAnchorCreateInfoHTC
		{
			public XrStructureType type;
			public System.IntPtr next;
			public XrSpace space;
			public XrPosef poseInSpace;
			public XrSpatialAnchorNameHTC name;
		}

		#endregion

		#region delegates and delegate instances
		delegate XrResult DelegateXrCreateSpatialAnchorHTC(XrSession session, ref XrSpatialAnchorCreateInfoHTC createInfo, ref XrSpace anchor);
		delegate XrResult DelegateXrGetSpatialAnchorNameHTC(XrSpace anchor, ref XrSpatialAnchorNameHTC name);

		DelegateXrCreateSpatialAnchorHTC XrCreateSpatialAnchorHTC;
		DelegateXrGetSpatialAnchorNameHTC XrGetSpatialAnchorNameHTC;
		#endregion delegates and delegate instances

		#region override functions
		/// <inheritdoc />
		protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
		{
			Debug.Log("ViveAnchor HookGetInstanceProcAddr() ");
			return ViveInterceptors.Instance.HookGetInstanceProcAddr(func);
		}

		/// <inheritdoc />
		protected override bool OnInstanceCreate(ulong xrInstance)
		{
			//Debug.Log("VIVEAnchor OnInstanceCreate() ");
			if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
			{
				Debug.LogWarning("ViveAnchor OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
				return false;
			}

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
			Debug.Log("ViveAnchor OnSessionCreate() ");

			// here's one way you can grab the session
			Debug.Log($"EXT: Got xrSession: {xrSession}");
			session = xrSession;
		}

		/// <inheritdoc />
		protected override void OnSessionBegin(ulong xrSession)
		{
			Debug.Log("ViveAnchor OnSessionBegin() ");
			Debug.Log($"EXT: xrBeginSession: {xrSession}");
		}

		/// <inheritdoc />
		protected override void OnSessionEnd(ulong xrSession)
		{
			Debug.Log("ViveAnchor OnSessionEnd() ");
			Debug.Log($"EXT: about to xrEndSession: {xrSession}");
		}

		// XXX Every millisecond the AppSpace switched from one space to another space. I don't know what is going on.
		//private ulong appSpace;
		//protected override void OnAppSpaceChange(ulong space)
		//{
		//	//Debug.Log($"VIVEAnchor OnAppSpaceChange({appSpace} -> {space})");
		//	appSpace = space;
		//}

		/// <inheritdoc />
		protected override void OnSystemChange(ulong xrSystem)
		{
			m_XrSystemId = xrSystem;
			Debug.Log("ViveAnchor OnSystemChange() " + m_XrSystemId);
		}


		#endregion override functions

		private bool GetXrFunctionDelegates(XrInstance xrInstance)
		{
			Debug.Log("ViveAnchor GetXrFunctionDelegates() ");

			bool ret = true;
			IntPtr funcPtr = IntPtr.Zero;
			OpenXRHelper.xrGetInstanceProcAddrDelegate GetAddr = CommonWrapper.Instance.GetInstanceProcAddr;  // shorter name
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrCreateSpatialAnchorHTC", out XrCreateSpatialAnchorHTC);
			ret &= OpenXRHelper.GetXrFunctionDelegate(GetAddr, xrInstance, "xrGetSpatialAnchorNameHTC", out XrGetSpatialAnchorNameHTC);

			return ret;
		}

		#region functions of extension
		/// <summary>
		/// Helper function to get this feature' properties.
		/// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>
		/// </summary>
		public XrResult GetProperties(out XrSystemAnchorPropertiesHTC anchorProperties)
		{
			anchorProperties = new XrSystemAnchorPropertiesHTC();
			anchorProperties.type = XrStructureType.XR_TYPE_SYSTEM_ANCHOR_PROPERTIES_HTC;

#if FAKE_DATA
			if (Application.isEditor)
			{
				anchorProperties.type = XrStructureType.XR_TYPE_SYSTEM_ANCHOR_PROPERTIES_HTC;
				anchorProperties.supportsAnchor = true;
				return XrResult.XR_SUCCESS;
			}
#endif
			return CommonWrapper.Instance.GetProperties(m_XrInstance, m_XrSystemId, ref anchorProperties);
		}

		public XrResult CreateSpatialAnchor(XrSpatialAnchorCreateInfoHTC createInfo, out XrSpace anchor)
		{
			anchor = default;
#if FAKE_DATA
			if (Application.isEditor)
				return XrResult.XR_SUCCESS;
#endif
			var ret = XrCreateSpatialAnchorHTC(session, ref createInfo, ref anchor);
			Debug.Log("ViveAnchor CreateSpatialAnchor() r=" + ret + ", a=" + anchor + ", bs=" + createInfo.space +
				", pos=(" + createInfo.poseInSpace.position.x + "," + createInfo.poseInSpace.position.y + "," + createInfo.poseInSpace.position.z +
				"), rot=(" + createInfo.poseInSpace.orientation.x + "," + createInfo.poseInSpace.orientation.y + "," + createInfo.poseInSpace.orientation.z + "," + createInfo.poseInSpace.orientation.w +
				"), n=" + createInfo.name.name);
			return ret;
		}

		public XrResult GetSpatialAnchorName(XrSpace anchor, out XrSpatialAnchorNameHTC name)
		{
			name = default;
#if FAKE_DATA
			if (Application.isEditor)
			{
				name.name = "fake anchor";
				return XrResult.XR_SUCCESS;
			}
#endif
			return XrGetSpatialAnchorNameHTC(anchor, ref name);
		}

		#endregion

		#region tools for user

		/// <summary>
		/// According to XRInputSubsystem's tracking origin mode, return the corresponding XrSpace.
		/// </summary>
		/// <returns></returns>
		public XrSpace GetTrackingSpace()
		{
			var s = GetCurrentAppSpace();
			Debug.Log("ViveAnchor GetTrackingSpace() s=" + s);
			return s;
		}
		#endregion
	}
}