using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.SceneUnderstanding
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Scene Understanding",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA},
        Company = "HTC",
        Desc = "Get function pointers related to openxr scene understanding",
        DocumentationLink = "https://developer.vive.com/resources/openxr/openxr-pcvr/tutorials/unity/interact-real-world-openxr-scene-understanding/",
        OpenxrExtensionStrings = "XR_MSFT_scene_understanding",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class SceneUnderstanding_OpenXR_API : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.htc.openxr.sceneunderstanding.feature";

        #region OpenXR callbacks
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            UnityEngine.Debug.Log($"OnInstanceCreate({xrInstance})");
            m_XrInstance = xrInstance;

            return GetXrFunctionDelegates(xrInstance);
        }
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            UnityEngine.Debug.Log($"OnInstanceDestroy({xrInstance})");
        }
        protected override void OnSessionCreate(ulong xrSession)
        {
            UnityEngine.Debug.Log($"OnSessionCreate({xrSession})");
            m_XrSession = xrSession;
            systemProperties.type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            int res = xrGetSystemProperties(ref systemProperties);
            if (res != (int)XrResult.XR_SUCCESS)
            {
                UnityEngine.Debug.Log("Failed to get systemproperties with error code : " + res);

            }
        }
        protected override void OnSystemChange(ulong xrSystem)
        {
            UnityEngine.Debug.Log($"OnSystemChange({xrSystem})");
            m_systemid = xrSystem;
        }
        protected override void OnSessionDestroy(ulong xrSession)
        {
            UnityEngine.Debug.Log($"OnSessionDestroy({xrSession})");
        }

        bool CheckResult(XrResult result) => result == XrResult.XR_SUCCESS;
        public bool GetXrFunctionDelegates(ulong xrInstance)
        {
            Debug.Log("GetXrFunctionDelegates() begin");
            if (xrGetInstanceProcAddr == null || xrGetInstanceProcAddr == IntPtr.Zero)
                UnityEngine.Debug.LogError("xrGetInstanceProcAddr is null");
            // Get delegate of xrGetInstanceProcAddr.
            m_XrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<xrGetInstanceProcDelegate>(xrGetInstanceProcAddr);
            // Get delegate of other OpenXR functions using xrGetInstanceProcAddr.
            bool successful = true;
            IntPtr funcPtr = IntPtr.Zero;

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrGetSystemProperties", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrGetSystemProperties function failed"); return false; }
            m_xrGetSystemProperties = Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(xrGetSystemPropertiesDelegate)) as xrGetSystemPropertiesDelegate;
            if (m_xrGetSystemProperties == null) { Debug.Log("m_xrGetSystemProperties == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrEnumerateReferenceSpaces", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrEnumerateReferenceSpaces function failed"); return false; }
            m_XrEnumerateReferenceSpaces = Marshal.GetDelegateForFunctionPointer<XrEnumerateReferenceSpacesDelegate>(funcPtr);
            if (m_XrEnumerateReferenceSpaces == null) { Debug.Log("m_XrEnumerateReferenceSpaces == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrCreateReferenceSpace", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrCreateReferenceSpace function failed"); return false; }
            m_XrCreateReferenceSpace = Marshal.GetDelegateForFunctionPointer<XrCreateReferenceSpaceDelegate>(funcPtr);
            if (m_XrCreateReferenceSpace == null) { Debug.Log("m_XrCreateReferenceSpace == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrDestroySpace", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrDestroySpace function failed"); return false; }
            m_XrDestroySpace = Marshal.GetDelegateForFunctionPointer<XrDestroySpaceDelegate>(funcPtr);
            if (m_XrDestroySpace == null) { Debug.Log("m_XrDestroySpace == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrEnumerateSceneComputeFeaturesMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrEnumerateSceneComputeFeaturesMSFT function failed"); return false; }
            m_XrEnumerateSceneComputeFeaturesMSFT = Marshal.GetDelegateForFunctionPointer<XrEnumerateSceneComputeFeaturesMSFTDelegate>(funcPtr);
            if (m_XrEnumerateSceneComputeFeaturesMSFT == null) { Debug.Log("m_XrEnumerateSceneComputeFeaturesMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrCreateSceneObserverMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrCreateSceneObserverMSFT function failed"); return false; }
            m_XrCreateSceneObserverMSFT = Marshal.GetDelegateForFunctionPointer<XrCreateSceneObserverMSFTDelegate>(funcPtr);
            if (m_XrCreateSceneObserverMSFT == null) { Debug.Log("m_XrCreateSceneObserverMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrDestroySceneObserverMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrDestroySceneObserverMSFT function failed"); return false; }
            m_XrDestroySceneObserverMSFT = Marshal.GetDelegateForFunctionPointer<XrDestroySceneObserverMSFTDelegate>(funcPtr);
            if (m_XrDestroySceneObserverMSFT == null) { Debug.Log("m_XrDestroySceneObserverMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrCreateSceneMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrCreateSceneMSFT function failed"); return false; }
            m_XrCreateSceneMSFT = Marshal.GetDelegateForFunctionPointer<XrCreateSceneMSFTDelegate>(funcPtr);
            if (m_XrCreateSceneMSFT == null) { Debug.Log("m_XrCreateSceneMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrDestroySceneMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrDestroySceneMSFT function failed"); return false; }
            m_XrDestroySceneMSFT = Marshal.GetDelegateForFunctionPointer<XrDestroySceneMSFTDelegate>(funcPtr);
            if (m_XrDestroySceneMSFT == null) { Debug.Log("m_XrDestroySceneMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrComputeNewSceneMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrComputeNewSceneMSFT function failed"); return false; }
            m_XrComputeNewSceneMSFT = Marshal.GetDelegateForFunctionPointer<XrComputeNewSceneMSFTDelegate>(funcPtr);
            if (m_XrComputeNewSceneMSFT == null) { Debug.Log("m_XrComputeNewSceneMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrGetSceneComputeStateMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrGetSceneComputeStateMSFT function failed"); return false; }
            m_XrGetSceneComputeStateMSFT = Marshal.GetDelegateForFunctionPointer<XrGetSceneComputeStateMSFTDelegate>(funcPtr);
            if (m_XrGetSceneComputeStateMSFT == null) { Debug.Log("m_XrGetSceneComputeStateMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrGetSceneComponentsMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrGetSceneComponentsMSFT function failed"); return false; }
            m_XrGetSceneComponentsMSFT = Marshal.GetDelegateForFunctionPointer<XrGetSceneComponentsMSFTDelegate>(funcPtr);
            if (m_XrGetSceneComponentsMSFT == null) { Debug.Log("m_XrGetSceneComponentsMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrLocateSceneComponentsMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrLocateSceneComponentsMSFT function failed"); return false; }
            m_XrLocateSceneComponentsMSFT = Marshal.GetDelegateForFunctionPointer<XrLocateSceneComponentsMSFTDelegate>(funcPtr);
            if (m_XrLocateSceneComponentsMSFT == null) { Debug.Log("m_XrLocateSceneComponentsMSFT == null"); }

            successful &= CheckResult((XrResult)m_XrGetInstanceProcAddr(xrInstance, "xrGetSceneMeshBuffersMSFT", out funcPtr));
            if (funcPtr == IntPtr.Zero) { UnityEngine.Debug.Log("Get xrGetSceneMeshBuffersMSFT function failed"); return false; }
            m_XrGetSceneMeshBuffersMSFT = Marshal.GetDelegateForFunctionPointer<XrGetSceneMeshBuffersMSFTDelegate>(funcPtr);
            if (m_XrGetSceneMeshBuffersMSFT == null) { Debug.Log("m_XrGetSceneMeshBuffersMSFT == null"); }

            Debug.Log("GetXrFunctionDelegates() end");
            return successful;
        }
        #endregion
        public ulong m_XrInstance;
        public ulong m_XrSession;
        public ulong m_systemid;
        public XrSystemProperties systemProperties;
        xrGetInstanceProcDelegate m_XrGetInstanceProcAddr;

        public delegate int xrGetSystemPropertiesDelegate(ulong instance, ulong systemId, ref XrSystemProperties properties);
        public xrGetSystemPropertiesDelegate m_xrGetSystemProperties;
        public int xrGetSystemProperties(ref XrSystemProperties properties) =>
            m_xrGetSystemProperties(m_XrInstance, m_systemid, ref properties);

        public delegate XrResult XrEnumerateReferenceSpacesDelegate(
            ulong session,
            uint spaceCapacityInput,
            out int spaceCountOutput,
            IntPtr spaces);
        public XrEnumerateReferenceSpacesDelegate m_XrEnumerateReferenceSpaces;
        public XrResult XrEnumerateReferenceSpaces(
            uint spaceCapacityInput,
            out int spaceCountOutput,
            IntPtr spaces
        ) => m_XrEnumerateReferenceSpaces(
            m_XrSession,
            spaceCapacityInput,
            out spaceCountOutput,
            spaces
        );
        public delegate XrResult XrCreateReferenceSpaceDelegate(
            ulong session,
            ref XrReferenceSpaceCreateInfo createInfo,
            out ulong space);
        public XrCreateReferenceSpaceDelegate m_XrCreateReferenceSpace;
        public XrResult XrCreateReferenceSpace(
            ref XrReferenceSpaceCreateInfo createInfo,
            out ulong space
        ) => m_XrCreateReferenceSpace(
            m_XrSession,
            ref createInfo,
            out space
        );
        public delegate XrResult XrDestroySpaceDelegate(
            ulong space);
        public XrDestroySpaceDelegate m_XrDestroySpace;
        public XrResult XrDestroySpace(
            ulong space
        ) => m_XrDestroySpace(
            space
        );

        public delegate XrResult XrEnumerateSceneComputeFeaturesMSFTDelegate(
            ulong instance,
            ulong systemId,
            uint featureCapacityInput,
            out uint featureCountOutput,
            IntPtr features
        );
        public XrEnumerateSceneComputeFeaturesMSFTDelegate m_XrEnumerateSceneComputeFeaturesMSFT;
        public XrResult XrEnumerateSceneComputeFeaturesMSFT(
            ulong systemId,
            uint featureCapacityInput,
            out uint featureCountOutput,
            IntPtr features
        ) => m_XrEnumerateSceneComputeFeaturesMSFT(
            m_XrInstance,
            systemId,
            featureCapacityInput,
            out featureCountOutput,
            features
        );
        public delegate XrResult XrCreateSceneObserverMSFTDelegate(
            ulong session,
            ref XrSceneObserverCreateInfoMSFT createInfo,
            out ulong sceneObserver
        );
        public XrCreateSceneObserverMSFTDelegate m_XrCreateSceneObserverMSFT;
        public XrResult XrCreateSceneObserverMSFT(
            ref XrSceneObserverCreateInfoMSFT createInfo,
            out ulong sceneObserver
        ) => m_XrCreateSceneObserverMSFT(
            m_XrSession,
            ref createInfo,
            out sceneObserver
        );

        public delegate XrResult XrDestroySceneObserverMSFTDelegate(
            ulong sceneObserver
        );
        public XrDestroySceneObserverMSFTDelegate m_XrDestroySceneObserverMSFT;
        public XrResult XrDestroySceneObserverMSFT(
            ulong sceneObserver
        ) => m_XrDestroySceneObserverMSFT(
            sceneObserver
        );

        public delegate XrResult XrCreateSceneMSFTDelegate(
            ulong sceneObserver,
            ref XrSceneCreateInfoMSFT createInfo,
            out ulong scene
        );
        public XrCreateSceneMSFTDelegate m_XrCreateSceneMSFT;
        public XrResult XrCreateSceneMSFT(
            ulong sceneObserver,
            ref XrSceneCreateInfoMSFT createInfo,
            out ulong scene
        ) => m_XrCreateSceneMSFT(
            sceneObserver,
            ref createInfo,
            out scene
        );

        public delegate XrResult XrDestroySceneMSFTDelegate(
            ulong scene
        );
        public XrDestroySceneMSFTDelegate m_XrDestroySceneMSFT;
        public XrResult XrDestroySceneMSFT(
            ulong scene
        ) => m_XrDestroySceneMSFT(
            scene
        );

        public delegate XrResult XrComputeNewSceneMSFTDelegate(
            ulong sceneObserver,
            ref XrNewSceneComputeInfoMSFT computeInfo
        );
        public XrComputeNewSceneMSFTDelegate m_XrComputeNewSceneMSFT;
        public XrResult XrComputeNewSceneMSFT(
            ulong sceneObserver,
            ref XrNewSceneComputeInfoMSFT computeInfo
        ) => m_XrComputeNewSceneMSFT(
            sceneObserver,
            ref computeInfo
        );

        public delegate XrResult XrGetSceneComputeStateMSFTDelegate(
            ulong sceneObserver,
            out XrSceneComputeStateMSFT state
        );
        public XrGetSceneComputeStateMSFTDelegate m_XrGetSceneComputeStateMSFT;
        public XrResult XrGetSceneComputeStateMSFT(
            ulong sceneObserver,
            out XrSceneComputeStateMSFT state
        ) => m_XrGetSceneComputeStateMSFT(
            sceneObserver,
            out state
        );

        public delegate XrResult XrGetSceneComponentsMSFTDelegate(
            ulong scene,
            ref XrSceneComponentsGetInfoMSFT getInfo,
            ref XrSceneComponentsMSFT components
        );
        public XrGetSceneComponentsMSFTDelegate m_XrGetSceneComponentsMSFT;
        public XrResult XrGetSceneComponentsMSFT(
            ulong scene,
            ref XrSceneComponentsGetInfoMSFT getInfo,
            ref XrSceneComponentsMSFT components
        ) => m_XrGetSceneComponentsMSFT(
            scene,
            ref getInfo,
            ref components
        );

        public delegate XrResult XrLocateSceneComponentsMSFTDelegate(
            ulong scene,
            ref XrSceneComponentsLocateInfoMSFT locateInfo,
            ref XrSceneComponentLocationsMSFT locations
        );
        public XrLocateSceneComponentsMSFTDelegate m_XrLocateSceneComponentsMSFT;
        public XrResult XrLocateSceneComponentsMSFT(
            ulong scene,
            ref XrSceneComponentsLocateInfoMSFT locateInfo,
            ref XrSceneComponentLocationsMSFT locations
        ) => m_XrLocateSceneComponentsMSFT(
            scene,
            ref locateInfo,
            ref locations
        );

        public delegate XrResult XrGetSceneMeshBuffersMSFTDelegate(
            ulong scene,
            ref XrSceneMeshBuffersGetInfoMSFT getInfo,
            ref XrSceneMeshBuffersMSFT buffers
        );
        public XrGetSceneMeshBuffersMSFTDelegate m_XrGetSceneMeshBuffersMSFT;
        public XrResult XrGetSceneMeshBuffersMSFT(
            ulong scene,
            ref XrSceneMeshBuffersGetInfoMSFT getInfo,
            ref XrSceneMeshBuffersMSFT buffers
        ) => m_XrGetSceneMeshBuffersMSFT(
            scene,
            ref getInfo,
            ref buffers
        );

    }
}
