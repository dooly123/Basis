// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VIVE.OpenXR.Feature
{
    /// <summary>
    /// To use this wrapper, you need to call CommonWrapper.Instance.OnInstanceCreate() in your feature's OnInstanceCreate(), 
    /// and call CommonWrapper.Instance.OnInstanceDestroy() in your feature's OnInstanceDestroy().
    /// </summary>
    public class CommonWrapper
    {
        static CommonWrapper instance = null;
        public static CommonWrapper Instance
        {
            get
            {
                if (instance == null)
                    instance = new CommonWrapper();
                return instance;
            }
        }

        bool isInited = false;

        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;
        OpenXRHelper.xrGetSystemPropertiesDelegate XrGetSystemProperties;

        /// <summary>
        /// In feature's OnInstanceCreate(), call CommonWrapper.Instance.OnInstanceCreate() for init common APIs.
        /// </summary>
        /// <param name="xrInstance">Passed in feature's OnInstanceCreate.</param>
        /// <param name="xrGetInstanceProcAddr">Pass OpenXRFeature.xrGetInstanceProcAddr in.</param>
        /// <returns></returns>
        /// <exception cref="Exception">If input data not valid.</exception>
        public bool OnInstanceCreate(XrInstance xrInstance, IntPtr xrGetInstanceProcAddr)
        {
            if (isInited) return true;

            if (xrInstance == 0)
                throw new Exception("CommonWrapper: xrInstance is null");

            Debug.Log("CommonWrapper: OnInstanceCreate()");
            /// OpenXRFeature.xrGetInstanceProcAddr
            if (xrGetInstanceProcAddr == null || xrGetInstanceProcAddr == IntPtr.Zero)
                throw new Exception("CommonWrapper: xrGetInstanceProcAddr is null");

            Debug.Log("CommonWrapper: Get function pointer of xrGetInstanceProcAddr.");
            XrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer(
                xrGetInstanceProcAddr,
                typeof(OpenXRHelper.xrGetInstanceProcAddrDelegate)) as OpenXRHelper.xrGetInstanceProcAddrDelegate;

            bool ret = true;
            IntPtr funcPtr = IntPtr.Zero;

            ret &= OpenXRHelper.GetXrFunctionDelegate(XrGetInstanceProcAddr, xrInstance, "xrGetSystemProperties", out XrGetSystemProperties);

            if (!ret)
                throw new Exception("CommonWrapper: Get function pointers failed.");

            isInited = ret;
            return ret;
        }

        /// <summary>
        /// In feature's OnInstanceDestroy(), call CommonWrapper.Instance.OnInstanceDestroy() for disable common APIs.
        /// </summary>
        /// <returns></returns>
        public void OnInstanceDestroy()
        {
            isInited = false;
            XrGetInstanceProcAddr = null;
            XrGetSystemProperties = null;
            Debug.Log("CommonWrapper: OnInstanceDestroy()");
        }

        public XrResult GetInstanceProcAddr(XrInstance instance, string name, out IntPtr function)
        {
            if (isInited == false || XrGetInstanceProcAddr == null)
            {
                function = IntPtr.Zero;
                return XrResult.XR_ERROR_HANDLE_INVALID;
            }

            return XrGetInstanceProcAddr(instance, name, out function);
        }

        /// <summary>
        /// Helper function to get system properties.  Need input your features' xrInstance and xrSystemId.  Fill the system properites in next for you feature.
        /// See <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="systemId"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public XrResult GetSystemProperties(XrInstance instance, XrSystemId systemId, ref XrSystemProperties properties)
        {
            if (isInited == false || XrGetSystemProperties == null)
            {
                return XrResult.XR_ERROR_HANDLE_INVALID;
            }

            return XrGetSystemProperties(instance, systemId, ref properties);
        }


        public XrResult GetProperties<T>(XrInstance instance, XrSystemId systemId, ref T featureProperty)
        {
            XrSystemProperties systemProperties = new XrSystemProperties();
            systemProperties.type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            systemProperties.next = Marshal.AllocHGlobal(Marshal.SizeOf(featureProperty));

            long offset = 0;
            if (IntPtr.Size == 4)
                offset = systemProperties.next.ToInt32();
            else
                offset = systemProperties.next.ToInt64();

            IntPtr pdPropertiesPtr = new IntPtr(offset);
            Marshal.StructureToPtr(featureProperty, pdPropertiesPtr, false);

            var ret = GetSystemProperties(instance, systemId, ref systemProperties);
            if (ret == XrResult.XR_SUCCESS)
            {
                if (IntPtr.Size == 4)
                    offset = systemProperties.next.ToInt32();
                else
                    offset = systemProperties.next.ToInt64();

                pdPropertiesPtr = new IntPtr(offset);
                featureProperty = Marshal.PtrToStructure<T>(pdPropertiesPtr);
            }

            Marshal.FreeHGlobal(systemProperties.next);
            return ret;
        }
    }
}
