// Copyright HTC Corporation All Rights Reserved.
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using AOT;

namespace VIVE.OpenXR
{
	partial class ViveInterceptors
	{
		#region XRWaitFrame
		public struct XrFrameWaitInfo
		{
			public XrStructureType type;
			public IntPtr next;
		}

		public struct XrFrameState
		{
			public XrStructureType type;
			public IntPtr next;
			public XrTime predictedDisplayTime;
			public XrDuration predictedDisplayPeriod;
			public XrBool32 shouldRender;
		}

		public delegate XrResult DelegateXrWaitFrame(XrSession session, ref XrFrameWaitInfo frameWaitInfo, ref XrFrameState frameState);
		private static readonly DelegateXrWaitFrame xrWaitFrameInterceptorHandle = new DelegateXrWaitFrame(XrWaitFrameInterceptor);
		private static readonly IntPtr xrWaitFrameInterceptorPtr = Marshal.GetFunctionPointerForDelegate(xrWaitFrameInterceptorHandle);
		static DelegateXrWaitFrame XrWaitFrameOriginal = null;

		[MonoPInvokeCallback(typeof(DelegateXrWaitFrame))]
		private static XrResult XrWaitFrameInterceptor(XrSession session, ref XrFrameWaitInfo frameWaitInfo, ref XrFrameState frameState)
		{
			var ret = XrWaitFrameOriginal(session, ref frameWaitInfo, ref frameState);
			currentFrameState = frameState;
			return ret;
		}

		static XrFrameState currentFrameState = new XrFrameState() { predictedDisplayTime = 0 };

		public XrFrameState GetCurrentFrameState()
		{
			if (!isInited) throw new Exception("ViveInterceptors is not inited");

			return currentFrameState;
		}

		public XrTime GetPredictTime()
		{
			if (!isInited) throw new Exception("ViveInterceptors is not inited");

			Debug.Log($"{TAG}: XrWaitFrameInterceptor(predictedDisplayTime={currentFrameState.predictedDisplayTime}");
			if (currentFrameState.predictedDisplayTime == 0)
				return new XrTime((long)(1000000L * (Time.unscaledTimeAsDouble + 0.011f)));
			else
				return currentFrameState.predictedDisplayTime;
		}
		#endregion XRWaitFrame
	}
}