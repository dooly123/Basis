using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif
using VIVE.OpenXR.SceneUnderstanding;
using VIVE.OpenXR;
namespace UnityEngine.XR.OpenXR.Samples.MeshingFeature
{
    /// <summary>
    /// Example extension showing how to supply a mesh from native code with OpenXR SceneUnderstanding functions.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Meshing Subsystem",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Company = "HTC",
        Desc = "Example extension showing how to supply a mesh from native code with OpenXR SceneUnderstanding functions.",
        DocumentationLink = "https://developer.vive.com/resources/openxr/openxr-pcvr/tutorials/unity/interact-real-world-openxr-scene-understanding/",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class MeshingTeapotFeature : SceneUnderstanding_OpenXR_API
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        new public const string featureId = "com.unity.openxr.feature.example.meshing";
        private static List<XRMeshSubsystemDescriptor> s_MeshDescriptors =
            new List<XRMeshSubsystemDescriptor>();
#region Subsystem callbacks
        /// <inheritdoc />
        protected override void OnSubsystemCreate ()
        {
            CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshDescriptors, "Sample Meshing");
        }

        /// <inheritdoc />
        protected override void OnSubsystemStart ()
        {
            StartSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc />
        protected override void OnSubsystemStop ()
        {
            StopSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc />
        protected override void OnSubsystemDestroy ()
        {
            DestroySubsystem<XRMeshSubsystem>();
        }
        #endregion

        #region OpenXR callbacks
        protected override void OnSessionCreate(ulong xrSession)
        {
            UnityEngine.Debug.Log($"OnSessionCreate({xrSession})");
            m_XrSession = xrSession;

            NativeApi.SetOpenXRVariables(m_XrInstance, m_XrSession,
                Marshal.GetFunctionPointerForDelegate(m_XrEnumerateReferenceSpaces),
                Marshal.GetFunctionPointerForDelegate(m_XrCreateReferenceSpace),
                Marshal.GetFunctionPointerForDelegate(m_XrDestroySpace),
                Marshal.GetFunctionPointerForDelegate(m_XrEnumerateSceneComputeFeaturesMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrCreateSceneObserverMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrDestroySceneObserverMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrCreateSceneMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrDestroySceneMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrComputeNewSceneMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrGetSceneComputeStateMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrGetSceneComponentsMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrLocateSceneComponentsMSFT),
                Marshal.GetFunctionPointerForDelegate(m_XrGetSceneMeshBuffersMSFT));
            systemProperties.type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            XrSystemPassThroughPropertiesHTC SystemPassThroughPropertiesHTC;
            int res = xrGetSystemProperties(ref systemProperties);
            if (res != (int)XrResult.XR_SUCCESS)
            {
                UnityEngine.Debug.Log("Failed to get systemproperties with error code : " + res);

            }
        }
        protected override void OnSessionDestroy(ulong xrSession)
        {
            UnityEngine.Debug.Log($"OnSessionDestroy({xrSession})");
        }
        #endregion

        #region OpenXR scene compute consistency
        public void SetSceneComputeConsistency(XrSceneComputeConsistencyMSFT consistency)
        {
            NativeApi.SetSceneComputeConsistency(consistency);
        }
        #endregion

#region OpenXR scene compute bounds
        public void SetSceneComputeSphereBound(Vector3 center, float radius)
        {
            // Convert to right-handed center.
            center.z *= -1;
            // Debug.Log("box, pos: " + center + ", radi: " + radius);
            NativeApi.SetSceneComputeSphereBound(center, radius);
        }

        public void SetSceneComputeOrientedBoxBound(Transform transform, Vector3 extent)
        {
            // Convert to right-handed transform.
            Vector4 rotation;
            Vector3 position;
            ConvertTransform(transform, out rotation, out position);
            NativeApi.SetSceneComputeOrientedBoxBound(rotation, position, extent);
        }

        /// <summary>
        /// Set scene computation frustum bound.
        /// </summary>
        /// <param name = "angleUp">The fov up angle in radian.</param>
        /// <param name = "angleDown">The fov down angle in radian.</param>
        /// <param name = "angleRight">The fov right angle in radian.</param>
        /// <param name = "angleLeft">The fov left angle in radian.</param>
        public void SetSceneComputeFrustumBound(Transform transform, float angleUp,
            float angleDown, float angleRight, float angleLeft, float farDistance)
        {
            // Convert to right-handed transform.
            Vector4 rotation;
            Vector3 position;
            ConvertTransform(transform, out rotation, out position);
            NativeApi.SetSceneComputeFrustumBound(rotation, position, angleUp, angleDown,
                angleRight, angleLeft, farDistance);
        }

        /// <summery>
        /// Convert a Unity left-handed tranform to right-handed world-space
        /// position and rotation.
        /// <param name="rotation">Output right-handed quaternion rotation</param>
        /// <param name="position">Output right-handed position</param>
        /// </summary>
        private void ConvertTransform(Transform transform, out Vector4 rotation, out Vector3 position)
        {
            // Get left-handed values.
            position = transform.position;
            float angle;
            Vector3 axis;
            transform.rotation.ToAngleAxis(out angle, out axis);
            // Convert left-handed values to right-handed.
            position.z *= -1;
            angle *= -1;
            axis.z *= -1;
            var rotationQuaternion = Quaternion.AngleAxis(angle, axis);
            rotation = Vector4.zero;
            for (var i = 0; i < 4; ++i) rotation[i] = rotationQuaternion[i];
        }

        public void ClearSceneComputeBounds(XrSceneBoundType type)
        {
            NativeApi.ClearSceneComputeBounds(type);
        }
#endregion

#region OpenXR mesh compute lod
        public void SetMeshComputeLod(XrMeshComputeLodMSFT lod)
        {
            NativeApi.SetMeshComputeLod(lod);
        }
#endregion

        class NativeApi
        {
            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetOpenXRVariables(ulong instance, ulong session,
                IntPtr PFN_XrEnumerateReferenceSpaces,
                IntPtr PFN_XrCreateReferenceSpace,
                IntPtr PFN_XrDestroySpace,
                IntPtr PFN_XrEnumerateSceneComputeFeaturesMSFT,
                IntPtr PFN_XrCreateSceneObserverMSFT,
                IntPtr PFN_XrDestroySceneObserverMSFT,
                IntPtr PFN_XrCreateSceneMSFT,
                IntPtr PFN_XrDestroySceneMSFT,
                IntPtr PFN_XrComputeNewSceneMSFT,
                IntPtr PFN_XrGetSceneComputeStateMSFT,
                IntPtr PFN_XrGetSceneComponentsMSFT,
                IntPtr PFN_XrLocateSceneComponentsMSFT,
                IntPtr PFN_XrGetSceneMeshBuffersMSFT);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetSceneComputeConsistency(XrSceneComputeConsistencyMSFT consistency);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetSceneComputeSphereBound(Vector3 center, float radius);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetSceneComputeOrientedBoxBound(Vector4 rotation, Vector3 position, Vector3 extent);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetSceneComputeFrustumBound(Vector4 rotation, Vector3 position,
                float angleUp, float angleDown, float angleRight, float angleLeft, float farDistance);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void ClearSceneComputeBounds(XrSceneBoundType type);

            [DllImport("MeshingFeaturePlugin")]
            public static extern void SetMeshComputeLod(XrMeshComputeLodMSFT lod);
        }
    }
}
