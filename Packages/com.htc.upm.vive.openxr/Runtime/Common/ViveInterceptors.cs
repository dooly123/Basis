// Copyright HTC Corporation All Rights Reserved.
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using AOT;

namespace VIVE.OpenXR
{
	/// <summary>
	/// This class is made for all features that need to intercept OpenXR API calls.
	/// Some APIs will be called by Unity internally, and we need to intercept them in c# to get some information.
	/// Append more interceptable functions for this class by adding a new partial class.
	/// The partial class can help the delegate name be nice to read and search.
	/// Please create per function in one partial class.
	/// 
	/// For all features want to use this class, please call <see cref="HookGetInstanceProcAddr" /> in your feature class.
	/// For example:
	///     protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
	///     {
	///         return HtcInterceptors.Instance.HookGetInstanceProcAddr(func);
	///     }
	/// </summary>
	partial class ViveInterceptors
	{
		public const string TAG = "Interceptors";

		public static ViveInterceptors instance = null;
		public static ViveInterceptors Instance
		{
			get
			{
				if (instance == null)
					instance = new ViveInterceptors();
				return instance;
			}
		}

		public ViveInterceptors()
		{
			Debug.Log("HtcInterceptors");
		}

		bool isInited = false;

		public delegate XrResult DelegateXrGetInstanceProcAddr(XrInstance instance, string name, out IntPtr function);
		private static readonly DelegateXrGetInstanceProcAddr hookXrGetInstanceProcAddrHandle = new DelegateXrGetInstanceProcAddr(XrGetInstanceProcAddrInterceptor);
		private static readonly IntPtr hookGetInstanceProcAddrHandlePtr = Marshal.GetFunctionPointerForDelegate(hookXrGetInstanceProcAddrHandle);
		static DelegateXrGetInstanceProcAddr XrGetInstanceProcAddrOriginal = null;

		[MonoPInvokeCallback(typeof(DelegateXrGetInstanceProcAddr))]
		private static XrResult XrGetInstanceProcAddrInterceptor(XrInstance instance, string name, out IntPtr function)
		{
			// Custom interceptors
			if (name == "xrWaitFrame")
			{
				Debug.Log($"{TAG}: XrGetInstanceProcAddrInterceptor() {name} is intercepted.");
				var ret = XrGetInstanceProcAddrOriginal(instance, name, out function);
				if (ret == XrResult.XR_SUCCESS)
				{
					XrWaitFrameOriginal = Marshal.GetDelegateForFunctionPointer<DelegateXrWaitFrame>(function);
					function = xrWaitFrameInterceptorPtr;
				}
				return ret;
			}
			return XrGetInstanceProcAddrOriginal(instance, name, out function);
		}

		public IntPtr HookGetInstanceProcAddr(IntPtr func)
		{
			Debug.Log($"{TAG}: registering our own xrGetInstanceProcAddr");
			if (XrGetInstanceProcAddrOriginal == null)
			{
				XrGetInstanceProcAddrOriginal = Marshal.GetDelegateForFunctionPointer<DelegateXrGetInstanceProcAddr>(func);
				isInited = true;
				return hookGetInstanceProcAddrHandlePtr;
			}
			else
			{
				return func;
			}
		}
	}
}