//========= Copyright 2017-2018, HTC Corporation. All rights reserved. ===========
//
// Purpose: This file contains C#/managed code bindings for the OpenVR interfaces
//
//================================================================================

using System;
using System.Runtime.InteropServices;
using HVR;

namespace HVR
{

[StructLayout(LayoutKind.Sequential)]
public struct IHVRSystem
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _GetRecommendedRenderTargetSize(ref uint pnWidth, ref uint pnHeight);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetRecommendedRenderTargetSize GetRecommendedRenderTargetSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate HVRMatrix44_t _GetProjectionMatrix(EHVREye eEye, float fNearZ, float fFarZ, EHVRTextureType eProjType);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetProjectionMatrix GetProjectionMatrix;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _GetProjectionRaw(EHVREye eEye, ref float pfLeft, ref float pfRight, ref float pfTop, ref float pfBottom);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetProjectionRaw GetProjectionRaw;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate HVRMatrix34_t _GetEyeToHeadTransform(EHVREye eEye);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetEyeToHeadTransform GetEyeToHeadTransform;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetTimeSinceLastVsync(ref float pfSecondsSinceLastVsync, ref ulong pulFrameCounter);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetTimeSinceLastVsync GetTimeSinceLastVsync;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _GetDeviceToAbsoluteTrackingPose(EHVRTrackingUniverseOrigin eOrigin, float fPredictedSecondsToPhotonsFromNow, [In, Out] HVRTrackedDevicePose_t[] pTrackedDevicePoseArray, uint unTrackedDevicePoseArrayCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetDeviceToAbsoluteTrackingPose GetDeviceToAbsoluteTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate HVRMatrix34_t _GetRawZeroPoseToStandingAbsoluteTrackingPose();
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetRawZeroPoseToStandingAbsoluteTrackingPose GetRawZeroPoseToStandingAbsoluteTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetTrackedDeviceIndexForControllerRole(EHVRTrackedControllerRole unDeviceType);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetTrackedDeviceIndexForControllerRole GetTrackedDeviceIndexForControllerRole;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedControllerRole _GetControllerRoleForTrackedDeviceIndex(uint unDeviceIndex);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetControllerRoleForTrackedDeviceIndex GetControllerRoleForTrackedDeviceIndex;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedDeviceClass _GetTrackedDeviceClass(uint unDeviceIndex);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetTrackedDeviceClass GetTrackedDeviceClass;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _IsTrackedDeviceConnected(uint unDeviceIndex);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _IsTrackedDeviceConnected IsTrackedDeviceConnected;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetBoolTrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetBoolTrackedDeviceProperty GetBoolTrackedDeviceProperty;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate float _GetFloatTrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetFloatTrackedDeviceProperty GetFloatTrackedDeviceProperty;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate int _GetInt32TrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetInt32TrackedDeviceProperty GetInt32TrackedDeviceProperty;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate ulong _GetUint64TrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetUint64TrackedDeviceProperty GetUint64TrackedDeviceProperty;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetStringTrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, System.Text.StringBuilder pchValue, uint unBufferSize, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetStringTrackedDeviceProperty GetStringTrackedDeviceProperty;
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate HVRMatrix34_t _GetMatrix34TrackedDeviceProperty(uint unDeviceIndex, EHVRTrackedDeviceProperty prop, ref EHVRTrackedPropertyError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetMatrix34TrackedDeviceProperty GetMatrix34TrackedDeviceProperty;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _PollNextEvent(ref HVREvent_t pEvent, uint uncbVREvent);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _PollNextEvent PollNextEvent;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate HVRHiddenAreaMesh_t _GetHiddenAreaMesh(EHVREye eEye, EHVRHiddenAreaMeshType type);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetHiddenAreaMesh GetHiddenAreaMesh;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetControllerState(uint unControllerDeviceIndex, ref HVRControllerState_t pControllerState);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetControllerState GetControllerState;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetControllerStateWithPose(EHVRTrackingUniverseOrigin eOrigin, uint unControllerDeviceIndex, ref HVRControllerState_t pControllerState, uint unControllerStateSize, ref HVRTrackedDevicePose_t pTrackedDevicePose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetControllerStateWithPose GetControllerStateWithPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _TriggerHapticPulse(uint unControllerDeviceIndex, uint unAxisId, char usDurationMicroSec);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _TriggerHapticPulse TriggerHapticPulse;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _IsInputFocusCapturedByAnotherProcess();
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _IsInputFocusCapturedByAnotherProcess IsInputFocusCapturedByAnotherProcess;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate HVRChaperoneCalibrationState _GetCalibrationState();
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetCalibrationState GetCalibrationState;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetPlayAreaSize(ref float pSizeX, ref float pSizeZ);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetPlayAreaSize GetPlayAreaSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetPlayAreaRect(ref HVRQuad_t rect);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetPlayAreaRect GetPlayAreaRect;
}

[StructLayout(LayoutKind.Sequential)]
public struct IHVRCompositor
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void _SetTrackingSpace(EHVRTrackingUniverseOrigin eTrackingOrigin);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetTrackingSpace SetTrackingSpace;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRTrackingUniverseOrigin _GetTrackingSpace();
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetTrackingSpace GetTrackingSpace;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRCompositorError _WaitGetPoses([In, Out] HVRTrackedDevicePose_t[] pRenderPoseArray, uint unRenderPoseArrayCount, [In, Out] HVRTrackedDevicePose_t[] pGamePoseArray, uint unGamePoseArrayCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _WaitGetPoses WaitGetPoses;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRCompositorError _GetLastPoses([In, Out] HVRTrackedDevicePose_t[] pRenderPoseArray, uint unRenderPoseArrayCount, [In, Out] HVRTrackedDevicePose_t[] pGamePoseArray, uint unGamePoseArrayCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetLastPoses GetLastPoses;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRCompositorError _Submit(EHVREye eEye, ref HVRTexture_t pTexture, ref HVRTextureBounds_t pBounds, EHVRSubmitFlags nSubmitFlags);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _Submit Submit;
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _PostPresentHandoff();
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _PostPresentHandoff PostPresentHandoff;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _FadeToColor(float fSeconds, float fRed, float fGreen, float fBlue, float fAlpha, bool bBackground);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _FadeToColor FadeToColor;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _CanRenderScene();
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _CanRenderScene CanRenderScene;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SuspendRendering(bool bSuspend);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SuspendRendering SuspendRendering;
}

[StructLayout(LayoutKind.Sequential)]
public struct IHVROverlay
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _FindOverlay(string pchOverlayKey, ref ulong pOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _FindOverlay FindOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _CreateOverlay(string pchOverlayKey, string pchOverlayFriendlyName, ref ulong pOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _CreateOverlay CreateOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _DestroyOverlay(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _DestroyOverlay DestroyOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetHighQualityOverlay(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetHighQualityOverlay SetHighQualityOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate ulong _GetHighQualityOverlay();
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetHighQualityOverlay GetHighQualityOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetOverlayKey(ulong ulOverlayHandle, System.Text.StringBuilder pchValue, uint unBufferSize, ref EHVROverlayError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayKey GetOverlayKey;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetOverlayName(ulong ulOverlayHandle, System.Text.StringBuilder pchValue, uint unBufferSize, ref EHVROverlayError pError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayName GetOverlayName;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayImageData(ulong ulOverlayHandle, IntPtr pvBuffer, uint unBufferSize, ref uint punWidth, ref uint punHeight);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayImageData GetOverlayImageData;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate IntPtr _GetOverlayErrorNameFromEnum(EHVROverlayError error);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayErrorNameFromEnum GetOverlayErrorNameFromEnum;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayRenderingPid(ulong ulOverlayHandle, uint unPID);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayRenderingPid SetOverlayRenderingPid;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetOverlayRenderingPid(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayRenderingPid GetOverlayRenderingPid;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayFlag(ulong ulOverlayHandle, HVROverlayFlags eOverlayFlag, bool bEnabled);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayFlag SetOverlayFlag;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _GetOverlayFlag(ulong ulOverlayHandle, HVROverlayFlags eOverlayFlag, ref bool bEnabled);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetOverlayFlag GetOverlayFlag;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayColor(ulong ulOverlayHandle, float fRed, float fGreen, float fBlue);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayColor SetOverlayColor;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayColor(ulong ulOverlayHandle, ref float pfRed, ref float pfGreen, ref float pfBlue);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayColor GetOverlayColor;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayAlpha(ulong ulOverlayHandle, float fAlpha);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayAlpha SetOverlayAlpha;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayAlpha(ulong ulOverlayHandle, ref float pfAlpha);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayAlpha GetOverlayAlpha;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayTexelAspect(ulong ulOverlayHandle, float fTexelAspect);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayTexelAspect SetOverlayTexelAspect;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTexelAspect(ulong ulOverlayHandle, ref float pfTexelAspect);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTexelAspect GetOverlayTexelAspect;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlaySortOrder(ulong ulOverlayHandle, uint unSortOrder);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlaySortOrder SetOverlaySortOrder;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlaySortOrder(ulong ulOverlayHandle, ref uint punSortOrder);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlaySortOrder GetOverlaySortOrder;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayWidthInMeters(ulong ulOverlayHandle, float fWidthInMeters);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayWidthInMeters SetOverlayWidthInMeters;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayWidthInMeters(ulong ulOverlayHandle, ref float pfWidthInMeters);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayWidthInMeters GetOverlayWidthInMeters;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayAutoCurveDistanceRangeInMeters(ulong ulOverlayHandle, float fMinDistanceInMeters, float fMaxDistanceInMeters);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayAutoCurveDistanceRangeInMeters SetOverlayAutoCurveDistanceRangeInMeters;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _GetOverlayAutoCurveDistanceRangeInMeters(ulong ulOverlayHandle, ref float fMinDistanceInMeters, ref float fMaxDistanceInMeters);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetOverlayAutoCurveDistanceRangeInMeters GetOverlayAutoCurveDistanceRangeInMeters;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayTextureBounds SetOverlayTextureBounds;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _GetOverlayTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetOverlayTextureBounds GetOverlayTextureBounds;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTransformType(ulong ulOverlayHandle, ref HVROverlayTransformType peTransformType);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTransformType GetOverlayTransformType;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayTransformAbsolute(ulong ulOverlayHandle, EHVRTrackingUniverseOrigin eTrackingOrigin, ref HVRMatrix34_t pmatTrackingOriginToOverlayTransform);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayTransformAbsolute SetOverlayTransformAbsolute;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTransformAbsolute(ulong ulOverlayHandle, ref EHVRTrackingUniverseOrigin peTrackingOrigin, ref HVRMatrix34_t pmatTrackingOriginToOverlayTransform);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTransformAbsolute GetOverlayTransformAbsolute;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayTransformTrackedDeviceRelative(ulong ulOverlayHandle, uint unTrackedDevice, ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayTransformTrackedDeviceRelative SetOverlayTransformTrackedDeviceRelative;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTransformTrackedDeviceRelative(ulong ulOverlayHandle, ref uint punTrackedDevice, ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTransformTrackedDeviceRelative GetOverlayTransformTrackedDeviceRelative;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayTransformTrackedDeviceComponent(ulong ulOverlayHandle, uint unDeviceIndex, string pchComponentName);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayTransformTrackedDeviceComponent SetOverlayTransformTrackedDeviceComponent;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTransformTrackedDeviceComponent(ulong ulOverlayHandle, ref uint punDeviceIndex, string pchComponentName, uint unComponentNameSize);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTransformTrackedDeviceComponent GetOverlayTransformTrackedDeviceComponent;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _ShowOverlay(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ShowOverlay ShowOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _HideOverlay(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _HideOverlay HideOverlay;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _IsOverlayVisible(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _IsOverlayVisible IsOverlayVisible;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayTexture(ulong ulOverlayHandle, ref HVRTexture_t pTexture);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayTexture SetOverlayTexture;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _ClearOverlayTexture(ulong ulOverlayHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ClearOverlayTexture ClearOverlayTexture;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayRaw(ulong ulOverlayHandle, IntPtr pvBuffer, uint unWidth, uint unHeight, uint unDepth);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayRaw SetOverlayRaw;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _SetOverlayFromFile(ulong ulOverlayHandle, string pchFilePath);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetOverlayFromFile SetOverlayFromFile;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTexture(ulong ulOverlayHandle, ref IntPtr pNativeTextureHandle, IntPtr pNativeTextureRef, ref uint pWidth, ref uint pHeight, ref uint pNativeFormat, ref EHVRTextureType pAPI, ref EHVRColorSpace pColorSpace);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTexture GetOverlayTexture;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVROverlayError _GetOverlayTextureSize(ulong ulOverlayHandle, ref uint pWidth, ref uint pHeight);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetOverlayTextureSize GetOverlayTextureSize;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayStereoTexture(ulong ulOverlayHandle, ref HVRTexture_t pTexture, EHVREye eye);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayStereoTexture SetOverlayStereoTexture;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayStereoMode(ulong ulOverlayHandle, bool bEnabled);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayStereoMode SetOverlayStereoMode;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayStereoTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds, EHVREye eye);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayStereoTextureBounds SetOverlayStereoTextureBounds;
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetOverlayTextureUpsideDownFlag(ulong ulOverlayHandle, bool flag);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetOverlayTextureUpsideDownFlag SetOverlayTextureUpsideDownFlag;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _CreateDesktopCaptureOverlay(uint targetHandle);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _CreateDesktopCaptureOverlay CreateDesktopCaptureOverlay;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _DestroyDesktopCaptureOverlay(uint targetHandle);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _DestroyDesktopCaptureOverlay DestroyDesktopCaptureOverlay;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetDesktopCaptureOverlayWidth(uint targetHandle, float width);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetDesktopCaptureOverlayWidth SetDesktopCaptureOverlayWidth;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetDesktopCaptureOverlayAbsolutePose(uint targetHandle, ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetDesktopCaptureOverlayAbsolutePose SetDesktopCaptureOverlayAbsolutePose;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVROverlayError _SetDesktopCaptureOverlayCurveDistance(uint targetHandle, float minDist, float maxDist);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _SetDesktopCaptureOverlayCurveDistance SetDesktopCaptureOverlayCurveDistance;
}

[StructLayout(LayoutKind.Sequential)]
public struct IHVRRenderModels
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRRenderModelError _LoadRenderModel_Async(string pchRenderModelName, ref IntPtr ppRenderModel);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _LoadRenderModel_Async LoadRenderModel_Async;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void _FreeRenderModel(IntPtr pRenderModel);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _FreeRenderModel FreeRenderModel;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRRenderModelError _LoadTexture_Async(int textureId, ref IntPtr ppTexture);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _LoadTexture_Async LoadTexture_Async;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void _FreeTexture(IntPtr pTexture);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _FreeTexture FreeTexture;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRRenderModelError _LoadTextureD3D11_Async(int textureId, IntPtr pD3D11Device, ref IntPtr ppD3D11Texture2D);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _LoadTextureD3D11_Async LoadTextureD3D11_Async;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRRenderModelError _LoadIntoTextureD3D11_Async(int textureId, IntPtr pDstTexture);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _LoadIntoTextureD3D11_Async LoadIntoTextureD3D11_Async;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void _FreeTextureD3D11(IntPtr pD3D11Texture2D);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _FreeTextureD3D11 FreeTextureD3D11;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetRenderModelName(uint unRenderModelIndex, System.Text.StringBuilder pchRenderModelName, uint unRenderModelNameLen);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetRenderModelName GetRenderModelName;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetRenderModelCount();
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetRenderModelCount GetRenderModelCount;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetComponentCount(string pchRenderModelName);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentCount GetComponentCount;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetComponentName(string pchRenderModelName, uint unComponentIndex, System.Text.StringBuilder pchComponentName, uint unComponentNameLen);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentName GetComponentName;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate ulong _GetComponentButtonMask(string pchRenderModelName, string pchComponentName);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentButtonMask GetComponentButtonMask;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetComponentRenderModelName(string pchRenderModelName, string pchComponentName, System.Text.StringBuilder pchComponentRenderModelName, uint unComponentRenderModelNameLen);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentRenderModelName GetComponentRenderModelName;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _GetComponentStateForDevicePath(string pchRenderModelName, string pchComponentName, ulong devicePath, ref RenderModel_ControllerMode_State_t pState, ref RenderModel_ComponentState_t pComponentState);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentStateForDevicePath GetComponentStateForDevicePath;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _GetComponentState(string pchRenderModelName, string pchComponentName, ref HVRControllerState_t pControllerState, ref RenderModel_ControllerMode_State_t pState, ref RenderModel_ComponentState_t pComponentState);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetComponentState GetComponentState;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _RenderModelHasComponent(string pchRenderModelName, string pchComponentName);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _RenderModelHasComponent RenderModelHasComponent;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetRenderModelThumbnailURL(string pchRenderModelName, System.Text.StringBuilder pchThumbnailURL, uint unThumbnailURLLen, ref EHVRRenderModelError peError);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetRenderModelThumbnailURL GetRenderModelThumbnailURL;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint _GetRenderModelOriginalPath(string pchRenderModelName, System.Text.StringBuilder pchOriginalPath, uint unOriginalPathLen, ref EHVRRenderModelError peError);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetRenderModelOriginalPath GetRenderModelOriginalPath;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate IntPtr _GetRenderModelErrorNameFromEnum(EHVRRenderModelError error);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetRenderModelErrorNameFromEnum GetRenderModelErrorNameFromEnum;

}

    [StructLayout(LayoutKind.Sequential)]
public struct IHVRTrackedCamera
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate IntPtr _GetCameraErrorNameFromEnum(EHVRTrackedCameraError eCameraError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetCameraErrorNameFromEnum GetCameraErrorNameFromEnum;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _HasCamera(uint nDeviceIndex, ref bool pHasCamera);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _HasCamera HasCamera;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetCameraFrameSize(uint nDeviceIndex, EHVRTrackedCameraFrameType eFrameType, ref uint pnWidth, ref uint pnHeight, ref uint pnFrameBufferSize);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetCameraFrameSize GetCameraFrameSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetCameraIntrinsics(uint nDeviceIndex, uint nCameraIndex, EHVRTrackedCameraFrameType eFrameType, ref HVRVector2_t pFocalLength, ref HVRVector2_t pCenter);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetCameraIntrinsics GetCameraIntrinsics;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetCameraProjection(uint nDeviceIndex, uint nCameraIndex, EHVRTrackedCameraFrameType eFrameType, float flZNear, float flZFar, ref HVRMatrix44_t pProjection);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetCameraProjection GetCameraProjection;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _AcquireVideoStreamingService(uint nDeviceIndex, ref ulong pHandle);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _AcquireVideoStreamingService AcquireVideoStreamingService;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _ReleaseVideoStreamingService(ulong hTrackedCamera);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ReleaseVideoStreamingService ReleaseVideoStreamingService;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetVideoStreamFrameBuffer(ulong hTrackedCamera, EHVRTrackedCameraFrameType eFrameType, IntPtr pFrameBuffer, uint nFrameBufferSize, ref HVRCameraVideoStreamFrameHeader_t pFrameHeader, uint nFrameHeaderSize);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetVideoStreamFrameBuffer GetVideoStreamFrameBuffer;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetVideoStreamTextureSize(uint nDeviceIndex, EHVRTrackedCameraFrameType eFrameType, ref HVRTextureBounds_t pTextureBounds, ref uint pnWidth, ref uint pnHeight);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetVideoStreamTextureSize GetVideoStreamTextureSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetVideoStreamTextureD3D11(ulong hTrackedCamera, EHVRTrackedCameraFrameType eFrameType, IntPtr pD3D11DeviceOrResource, ref IntPtr ppD3D11ShaderResourceView, ref HVRCameraVideoStreamFrameHeader_t pFrameHeader, uint nFrameHeaderSize);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetVideoStreamTextureD3D11 GetVideoStreamTextureD3D11;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate EHVRTrackedCameraError _GetVideoStreamSeperateTextureD3D11(ulong hTrackedCamera, EHVRTrackedCameraFrameType eFrameType, IntPtr pD3D11DeviceOrResource, ref IntPtr ppD3D11ShaderResourceViewLeft, ref IntPtr ppD3D11ShaderResourceViewRight, ref HVRCameraVideoStreamFrameHeader_t pFrameHeader, uint nFrameHeaderSize);
    [MarshalAs(UnmanagedType.FunctionPtr)]
    internal _GetVideoStreamSeperateTextureD3D11 GetVideoStreamSeperateTextureD3D11;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _GetVideoStreamTextureGL(ulong hTrackedCamera, EHVRTrackedCameraFrameType eFrameType, ref uint pglTextureId, ref HVRCameraVideoStreamFrameHeader_t pFrameHeader, uint nFrameHeaderSize);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetVideoStreamTextureGL GetVideoStreamTextureGL;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate EHVRTrackedCameraError _ReleaseVideoStreamTextureGL(ulong hTrackedCamera, uint glTextureId);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ReleaseVideoStreamTextureGL ReleaseVideoStreamTextureGL;
}

[StructLayout(LayoutKind.Sequential)]
public struct IHVRSettings
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate IntPtr _GetSettingsErrorNameFromEnum(EHVRSettingsError eError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetSettingsErrorNameFromEnum GetSettingsErrorNameFromEnum;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _Sync(bool bForce, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _Sync Sync;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetBool(string pchSection, string pchSettingsKey, bool bValue, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetBool SetBool;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetInt32(string pchSection, string pchSettingsKey, int nValue, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetInt32 SetInt32;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetFloat(string pchSection, string pchSettingsKey, float flValue, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetFloat SetFloat;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetString(string pchSection, string pchSettingsKey, string pchValue, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetString SetString;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetBool(string pchSection, string pchSettingsKey, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetBool GetBool;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate int _GetInt32(string pchSection, string pchSettingsKey, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetInt32 GetInt32;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate float _GetFloat(string pchSection, string pchSettingsKey, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetFloat GetFloat;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _GetString(string pchSection, string pchSettingsKey, System.Text.StringBuilder pchValue, uint unValueLen, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetString GetString;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _RemoveSection(string pchSection, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _RemoveSection RemoveSection;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _RemoveKeyInSection(string pchSection, string pchSettingsKey, ref EHVRSettingsError peError);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _RemoveKeyInSection RemoveKeyInSection;

}

[StructLayout(LayoutKind.Sequential)]
public struct IHVRChaperoneSetup
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _CommitWorkingCopy(EHVRChaperoneConfigFile configFile);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _CommitWorkingCopy CommitWorkingCopy;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _RevertWorkingCopy();
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _RevertWorkingCopy RevertWorkingCopy;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetWorkingPlayAreaSize(ref float pSizeX, ref float pSizeZ);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetWorkingPlayAreaSize GetWorkingPlayAreaSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetWorkingPlayAreaRect(ref HVRQuad_t rect);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetWorkingPlayAreaRect GetWorkingPlayAreaRect;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetWorkingCollisionBoundsInfo([In, Out] HVRQuad_t[] pQuadsBuffer, ref uint punQuadsCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetWorkingCollisionBoundsInfo GetWorkingCollisionBoundsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetLiveCollisionBoundsInfo([In, Out] HVRQuad_t[] pQuadsBuffer, ref uint punQuadsCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetLiveCollisionBoundsInfo GetLiveCollisionBoundsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetWorkingSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatSeatedZeroPoseToRawTrackingPose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetWorkingSeatedZeroPoseToRawTrackingPose GetWorkingSeatedZeroPoseToRawTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetWorkingStandingZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatStandingZeroPoseToRawTrackingPose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetWorkingStandingZeroPoseToRawTrackingPose GetWorkingStandingZeroPoseToRawTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetWorkingPlayAreaSize(float sizeX, float sizeZ);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingPlayAreaSize SetWorkingPlayAreaSize;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetWorkingCollisionBoundsInfo([In, Out] HVRQuad_t[] pQuadsBuffer, uint unQuadsCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingCollisionBoundsInfo SetWorkingCollisionBoundsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetWorkingSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pMatSeatedZeroPoseToRawTrackingPose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingSeatedZeroPoseToRawTrackingPose SetWorkingSeatedZeroPoseToRawTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetWorkingStandingZeroPoseToRawTrackingPose(ref HVRMatrix34_t pMatStandingZeroPoseToRawTrackingPose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingStandingZeroPoseToRawTrackingPose SetWorkingStandingZeroPoseToRawTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _ReloadFromDisk(EHVRChaperoneConfigFile configFile);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ReloadFromDisk ReloadFromDisk;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetLiveSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatSeatedZeroPoseToRawTrackingPose);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetLiveSeatedZeroPoseToRawTrackingPose GetLiveSeatedZeroPoseToRawTrackingPose;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void _SetWorkingCollisionBoundsTagsInfo([In, Out] byte[] pTagsBuffer, uint unTagCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingCollisionBoundsTagsInfo SetWorkingCollisionBoundsTagsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetLiveCollisionBoundsTagsInfo([In, Out] byte[] pTagsBuffer, ref uint punTagCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetLiveCollisionBoundsTagsInfo GetLiveCollisionBoundsTagsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _SetWorkingPhysicalBoundsInfo([In, Out] HVRQuad_t[] pQuadsBuffer, uint unQuadsCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _SetWorkingPhysicalBoundsInfo SetWorkingPhysicalBoundsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _GetLivePhysicalBoundsInfo([In, Out] HVRQuad_t[] pQuadsBuffer, ref uint punQuadsCount);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetLivePhysicalBoundsInfo GetLivePhysicalBoundsInfo;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _ExportLiveToBuffer(System.Text.StringBuilder pBuffer, ref uint pnBufferLength);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ExportLiveToBuffer ExportLiveToBuffer;

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate bool _ImportFromBufferToWorking(string pBuffer, uint nImportFlags);
	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _ImportFromBufferToWorking ImportFromBufferToWorking;
}

public class CHVRSystem
{
	IHVRSystem FnTable;
	internal CHVRSystem(IntPtr pInterface)
	{
		FnTable = (IHVRSystem)Marshal.PtrToStructure(pInterface, typeof(IHVRSystem));
	}
	public void GetRecommendedRenderTargetSize(ref uint pnWidth,ref uint pnHeight)
	{
		pnWidth = 0;
		pnHeight = 0;
		FnTable.GetRecommendedRenderTargetSize(ref pnWidth,ref pnHeight);
	}
	public HVRMatrix44_t GetProjectionMatrix(EHVREye eEye,float fNearZ,float fFarZ, EHVRTextureType eProjType)
	{
		HVRMatrix44_t result = FnTable.GetProjectionMatrix(eEye,fNearZ,fFarZ,eProjType);
		return result;
	}
	public void GetProjectionRaw(EHVREye eEye,ref float pfLeft,ref float pfRight,ref float pfTop,ref float pfBottom)
	{
		pfLeft = 0;
		pfRight = 0;
		pfTop = 0;
		pfBottom = 0;
		FnTable.GetProjectionRaw(eEye,ref pfLeft,ref pfRight,ref pfTop,ref pfBottom);
	}

	public HVRMatrix34_t GetEyeToHeadTransform(EHVREye eEye)
	{
		HVRMatrix34_t result = FnTable.GetEyeToHeadTransform(eEye);
		return result;
	}
	public bool GetTimeSinceLastVsync(ref float pfSecondsSinceLastVsync,ref ulong pulFrameCounter)
	{
		pfSecondsSinceLastVsync = 0;
		pulFrameCounter = 0;
		bool result = FnTable.GetTimeSinceLastVsync(ref pfSecondsSinceLastVsync,ref pulFrameCounter);
		return result;
	}
	public void GetDeviceToAbsoluteTrackingPose(EHVRTrackingUniverseOrigin eOrigin,float fPredictedSecondsToPhotonsFromNow,HVRTrackedDevicePose_t [] pTrackedDevicePoseArray)
	{
		FnTable.GetDeviceToAbsoluteTrackingPose(eOrigin,fPredictedSecondsToPhotonsFromNow,pTrackedDevicePoseArray,(uint) pTrackedDevicePoseArray.Length);
	}
	public HVRMatrix34_t GetRawZeroPoseToStandingAbsoluteTrackingPose()
	{
		HVRMatrix34_t result = FnTable.GetRawZeroPoseToStandingAbsoluteTrackingPose();
		return result;
	}
	public uint GetTrackedDeviceIndexForControllerRole(EHVRTrackedControllerRole unDeviceType)
	{
		uint result = FnTable.GetTrackedDeviceIndexForControllerRole(unDeviceType);
		return result;
	}
	public EHVRTrackedControllerRole GetControllerRoleForTrackedDeviceIndex(uint unDeviceIndex)
	{
		EHVRTrackedControllerRole result = FnTable.GetControllerRoleForTrackedDeviceIndex(unDeviceIndex);
		return result;
	}
	public EHVRTrackedDeviceClass GetTrackedDeviceClass(uint unDeviceIndex)
	{
		EHVRTrackedDeviceClass result = FnTable.GetTrackedDeviceClass(unDeviceIndex);
		return result;
	}
	public bool IsTrackedDeviceConnected(uint unDeviceIndex)
	{
		bool result = FnTable.IsTrackedDeviceConnected(unDeviceIndex);
		return result;
	}
	public bool GetBoolTrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,ref EHVRTrackedPropertyError pError)
	{
		bool result = FnTable.GetBoolTrackedDeviceProperty(unDeviceIndex,prop,ref pError);
		return result;
	}
	public float GetFloatTrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,ref EHVRTrackedPropertyError pError)
	{
		float result = FnTable.GetFloatTrackedDeviceProperty(unDeviceIndex,prop,ref pError);
		return result;
	}
	public int GetInt32TrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,ref EHVRTrackedPropertyError pError)
	{
		int result = FnTable.GetInt32TrackedDeviceProperty(unDeviceIndex,prop,ref pError);
		return result;
	}
	public ulong GetUint64TrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,ref EHVRTrackedPropertyError pError)
	{
		ulong result = FnTable.GetUint64TrackedDeviceProperty(unDeviceIndex,prop,ref pError);
		return result;
	}
	public uint GetStringTrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,System.Text.StringBuilder pchValue,uint unBufferSize,ref EHVRTrackedPropertyError pError)
	{
		uint result = FnTable.GetStringTrackedDeviceProperty(unDeviceIndex,prop,pchValue,unBufferSize,ref pError);
		return result;
	}
	public HVRMatrix34_t GetMatrix34TrackedDeviceProperty(uint unDeviceIndex,EHVRTrackedDeviceProperty prop,ref EHVRTrackedPropertyError pError)
	{
		HVRMatrix34_t result = FnTable.GetMatrix34TrackedDeviceProperty(unDeviceIndex,prop,ref pError);
		return result;
	}
	public bool PollNextEvent(ref HVREvent_t pEvent,uint uncbVREvent)
	{
		bool result = FnTable.PollNextEvent(ref pEvent,uncbVREvent);
		return result;
	}
	public HVRHiddenAreaMesh_t GetHiddenAreaMesh(EHVREye eEye, EHVRHiddenAreaMeshType type)
	{
		HVRHiddenAreaMesh_t result = FnTable.GetHiddenAreaMesh(eEye, type);
		return result;
	}
	public bool GetControllerState(uint unControllerDeviceIndex,ref HVRControllerState_t pControllerState, uint unControllerStateSize)
	{
		bool result = FnTable.GetControllerState(unControllerDeviceIndex,ref pControllerState);
		return result;
	}
	public bool GetControllerStateWithPose(EHVRTrackingUniverseOrigin eOrigin,uint unControllerDeviceIndex,ref HVRControllerState_t pControllerState, uint unControllerStateSize, ref HVRTrackedDevicePose_t pTrackedDevicePose)
	{
		bool result = FnTable.GetControllerStateWithPose(eOrigin,unControllerDeviceIndex,ref pControllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(HVRControllerState_t)),ref pTrackedDevicePose);
		return result;
	}
	public void TriggerHapticPulse(uint unControllerDeviceIndex,uint unAxisId,char usDurationMicroSec)
	{
		FnTable.TriggerHapticPulse(unControllerDeviceIndex,unAxisId,usDurationMicroSec);
	}
    public bool IsInputFocusCapturedByAnotherProcess()
    {
        return FnTable.IsInputFocusCapturedByAnotherProcess();
    }
    public bool GetPlayAreaSize(ref float pSizeX,ref float pSizeZ)
	{
		pSizeX = 0;
		pSizeZ = 0;
		bool result = FnTable.GetPlayAreaSize(ref pSizeX,ref pSizeZ);
		return result;
	}
	public bool GetPlayAreaRect(ref HVRQuad_t rect)
	{
		bool result = FnTable.GetPlayAreaRect(ref rect);
		return result;
	}
    public HVRChaperoneCalibrationState GetCalibrationState()
	{
		HVRChaperoneCalibrationState result = FnTable.GetCalibrationState();
		return result;
	}
}

public class CHVRCompositor
{
	IHVRCompositor FnTable;
	internal CHVRCompositor(IntPtr pInterface)
	{
		FnTable = (IHVRCompositor)Marshal.PtrToStructure(pInterface, typeof(IHVRCompositor));
	}
    public void SetTrackingSpace(EHVRTrackingUniverseOrigin eTrackingOrigin)
    {
        FnTable.SetTrackingSpace(eTrackingOrigin);
    }
    public EHVRTrackingUniverseOrigin GetTrackingSpace()
    {
        return FnTable.GetTrackingSpace();
    }
    public EHVRCompositorError WaitGetPoses(HVRTrackedDevicePose_t [] pRenderPoseArray,HVRTrackedDevicePose_t [] pGamePoseArray)
	{
		EHVRCompositorError result = FnTable.WaitGetPoses(pRenderPoseArray,(uint) pRenderPoseArray.Length,pGamePoseArray,(uint) pGamePoseArray.Length);
		return result;
	}
	public EHVRCompositorError GetLastPoses(HVRTrackedDevicePose_t [] pRenderPoseArray,HVRTrackedDevicePose_t [] pGamePoseArray)
	{
		EHVRCompositorError result = FnTable.GetLastPoses(pRenderPoseArray,(uint) pRenderPoseArray.Length,pGamePoseArray,(uint) pGamePoseArray.Length);
		return result;
	}
	public EHVRCompositorError Submit(EHVREye eEye,ref HVRTexture_t pTexture,ref HVRTextureBounds_t pBounds,EHVRSubmitFlags nSubmitFlags)
	{
		EHVRCompositorError result = FnTable.Submit(eEye,ref pTexture,ref pBounds,nSubmitFlags);
		return result;
	}
	public void PostPresentHandoff()
	{
		FnTable.PostPresentHandoff();
	}
	public void FadeToColor(float fSeconds,float fRed,float fGreen,float fBlue,float fAlpha,bool bBackground)
	{
		FnTable.FadeToColor(fSeconds,fRed,fGreen,fBlue,fAlpha,bBackground);
	}
	public void SuspendRendering(bool bSuspend)
	{
		FnTable.SuspendRendering(bSuspend);
	}

    public bool CanRenderScene()
	{
		bool ret = FnTable.CanRenderScene();
        return ret;
	}
}

public class CHVROverlay
{
	IHVROverlay FnTable;
	internal CHVROverlay(IntPtr pInterface)
	{
		FnTable = (IHVROverlay)Marshal.PtrToStructure(pInterface, typeof(IHVROverlay));
	}
	public EHVROverlayError FindOverlay(string pchOverlayKey,ref ulong pOverlayHandle)
	{
		pOverlayHandle = 0;
		EHVROverlayError result = FnTable.FindOverlay(pchOverlayKey,ref pOverlayHandle);
		return result;
	}
	public EHVROverlayError CreateOverlay(string pchOverlayKey,string pchOverlayFriendlyName,ref ulong pOverlayHandle)
	{
		pOverlayHandle = 0;
		EHVROverlayError result = FnTable.CreateOverlay(pchOverlayKey,pchOverlayFriendlyName,ref pOverlayHandle);
		return result;
	}
	public EHVROverlayError DestroyOverlay(ulong ulOverlayHandle)
	{
		EHVROverlayError result = FnTable.DestroyOverlay(ulOverlayHandle);
		return result;
	}
	public EHVROverlayError SetHighQualityOverlay(ulong ulOverlayHandle)
	{
		EHVROverlayError result = FnTable.SetHighQualityOverlay(ulOverlayHandle);
		return result;
	}
	public ulong GetHighQualityOverlay()
	{
		ulong result = FnTable.GetHighQualityOverlay();
		return result;
	}
	public uint GetOverlayKey(ulong ulOverlayHandle,System.Text.StringBuilder pchValue,uint unBufferSize,ref EHVROverlayError pError)
	{
		uint result = FnTable.GetOverlayKey(ulOverlayHandle,pchValue,unBufferSize,ref pError);
		return result;
	}
	public uint GetOverlayName(ulong ulOverlayHandle,System.Text.StringBuilder pchValue,uint unBufferSize,ref EHVROverlayError pError)
	{
		uint result = FnTable.GetOverlayName(ulOverlayHandle,pchValue,unBufferSize,ref pError);
		return result;
	}
	public EHVROverlayError GetOverlayImageData(ulong ulOverlayHandle,IntPtr pvBuffer,uint unBufferSize,ref uint punWidth,ref uint punHeight)
	{
		punWidth = 0;
		punHeight = 0;
		EHVROverlayError result = FnTable.GetOverlayImageData(ulOverlayHandle,pvBuffer,unBufferSize,ref punWidth,ref punHeight);
		return result;
	}
	public string GetOverlayErrorNameFromEnum(EHVROverlayError error)
	{
		IntPtr result = FnTable.GetOverlayErrorNameFromEnum(error);
		return Marshal.PtrToStringAnsi(result);
	}
	public EHVROverlayError SetOverlayRenderingPid(ulong ulOverlayHandle,uint unPID)
	{
		EHVROverlayError result = FnTable.SetOverlayRenderingPid(ulOverlayHandle,unPID);
		return result;
	}
	public uint GetOverlayRenderingPid(ulong ulOverlayHandle)
	{
		uint result = FnTable.GetOverlayRenderingPid(ulOverlayHandle);
		return result;
	}
    public EHVROverlayError SetOverlayFlag(ulong ulOverlayHandle, HVROverlayFlags eOverlayFlag, bool bEnabled)
    {
        EHVROverlayError result = FnTable.SetOverlayFlag(ulOverlayHandle, eOverlayFlag, bEnabled);
        return result;
    }
    public EHVROverlayError GetOverlayFlag(ulong ulOverlayHandle, HVROverlayFlags eOverlayFlag, ref bool bEnabled)
    {
        EHVROverlayError result = FnTable.GetOverlayFlag(ulOverlayHandle, eOverlayFlag, ref bEnabled);
        return result;
    }
    public EHVROverlayError SetOverlayColor(ulong ulOverlayHandle,float fRed,float fGreen,float fBlue)
	{
		EHVROverlayError result = FnTable.SetOverlayColor(ulOverlayHandle,fRed,fGreen,fBlue);
		return result;
	}
	public EHVROverlayError GetOverlayColor(ulong ulOverlayHandle,ref float pfRed,ref float pfGreen,ref float pfBlue)
	{
		pfRed = 0;
		pfGreen = 0;
		pfBlue = 0;
		EHVROverlayError result = FnTable.GetOverlayColor(ulOverlayHandle,ref pfRed,ref pfGreen,ref pfBlue);
		return result;
	}
	public EHVROverlayError SetOverlayAlpha(ulong ulOverlayHandle,float fAlpha)
	{
		EHVROverlayError result = FnTable.SetOverlayAlpha(ulOverlayHandle,fAlpha);
		return result;
	}
	public EHVROverlayError GetOverlayAlpha(ulong ulOverlayHandle,ref float pfAlpha)
	{
		pfAlpha = 0;
		EHVROverlayError result = FnTable.GetOverlayAlpha(ulOverlayHandle,ref pfAlpha);
		return result;
	}
	public EHVROverlayError SetOverlayTexelAspect(ulong ulOverlayHandle,float fTexelAspect)
	{
		EHVROverlayError result = FnTable.SetOverlayTexelAspect(ulOverlayHandle,fTexelAspect);
		return result;
	}
	public EHVROverlayError GetOverlayTexelAspect(ulong ulOverlayHandle,ref float pfTexelAspect)
	{
		pfTexelAspect = 0;
		EHVROverlayError result = FnTable.GetOverlayTexelAspect(ulOverlayHandle,ref pfTexelAspect);
		return result;
	}
	public EHVROverlayError SetOverlaySortOrder(ulong ulOverlayHandle,uint unSortOrder)
	{
		EHVROverlayError result = FnTable.SetOverlaySortOrder(ulOverlayHandle,unSortOrder);
		return result;
	}
	public EHVROverlayError GetOverlaySortOrder(ulong ulOverlayHandle,ref uint punSortOrder)
	{
		punSortOrder = 0;
		EHVROverlayError result = FnTable.GetOverlaySortOrder(ulOverlayHandle,ref punSortOrder);
		return result;
	}
	public EHVROverlayError SetOverlayWidthInMeters(ulong ulOverlayHandle,float fWidthInMeters)
	{
		EHVROverlayError result = FnTable.SetOverlayWidthInMeters(ulOverlayHandle,fWidthInMeters);
		return result;
	}
	public EHVROverlayError GetOverlayWidthInMeters(ulong ulOverlayHandle,ref float pfWidthInMeters)
	{
		pfWidthInMeters = 0;
		EHVROverlayError result = FnTable.GetOverlayWidthInMeters(ulOverlayHandle,ref pfWidthInMeters);
		return result;
	}
    public EHVROverlayError SetOverlayAutoCurveDistanceRangeInMeters(ulong ulOverlayHandle, float fMinDistanceInMeters, float fMaxDistanceInMeters)
    {
        EHVROverlayError result = FnTable.SetOverlayAutoCurveDistanceRangeInMeters(ulOverlayHandle, fMinDistanceInMeters, fMaxDistanceInMeters);
        return result;
    }
    public EHVROverlayError GetOverlayAutoCurveDistanceRangeInMeters(ulong ulOverlayHandle, ref float fMinDistanceInMeters, ref float fMaxDistanceInMeters)
    {
        EHVROverlayError result = FnTable.GetOverlayAutoCurveDistanceRangeInMeters(ulOverlayHandle, ref fMinDistanceInMeters, ref fMaxDistanceInMeters);
        return result;
    }
    public EHVROverlayError SetOverlayTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds)
    {
        EHVROverlayError result = FnTable.SetOverlayTextureBounds(ulOverlayHandle, ref pOverlayTextureBounds);
        return result;
    }
    public EHVROverlayError GetOverlayTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds)
    {
        EHVROverlayError result = FnTable.GetOverlayTextureBounds(ulOverlayHandle, ref pOverlayTextureBounds);
        return result;
    }
    public EHVROverlayError GetOverlayTransformType(ulong ulOverlayHandle,ref HVROverlayTransformType peTransformType)
	{
		EHVROverlayError result = FnTable.GetOverlayTransformType(ulOverlayHandle,ref peTransformType);
		return result;
	}
	public EHVROverlayError SetOverlayTransformAbsolute(ulong ulOverlayHandle,EHVRTrackingUniverseOrigin eTrackingOrigin,ref HVRMatrix34_t pmatTrackingOriginToOverlayTransform)
	{
		EHVROverlayError result = FnTable.SetOverlayTransformAbsolute(ulOverlayHandle,eTrackingOrigin,ref pmatTrackingOriginToOverlayTransform);
		return result;
	}
	public EHVROverlayError GetOverlayTransformAbsolute(ulong ulOverlayHandle,ref EHVRTrackingUniverseOrigin peTrackingOrigin,ref HVRMatrix34_t pmatTrackingOriginToOverlayTransform)
	{
		EHVROverlayError result = FnTable.GetOverlayTransformAbsolute(ulOverlayHandle,ref peTrackingOrigin,ref pmatTrackingOriginToOverlayTransform);
		return result;
	}
	public EHVROverlayError SetOverlayTransformTrackedDeviceRelative(ulong ulOverlayHandle,uint unTrackedDevice,ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform)
	{
		EHVROverlayError result = FnTable.SetOverlayTransformTrackedDeviceRelative(ulOverlayHandle,unTrackedDevice,ref pmatTrackedDeviceToOverlayTransform);
		return result;
	}
	public EHVROverlayError GetOverlayTransformTrackedDeviceRelative(ulong ulOverlayHandle,ref uint punTrackedDevice,ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform)
	{
		punTrackedDevice = 0;
		EHVROverlayError result = FnTable.GetOverlayTransformTrackedDeviceRelative(ulOverlayHandle,ref punTrackedDevice,ref pmatTrackedDeviceToOverlayTransform);
		return result;
	}
	public EHVROverlayError SetOverlayTransformTrackedDeviceComponent(ulong ulOverlayHandle,uint unDeviceIndex,string pchComponentName)
	{
		EHVROverlayError result = FnTable.SetOverlayTransformTrackedDeviceComponent(ulOverlayHandle,unDeviceIndex,pchComponentName);
		return result;
	}
	public EHVROverlayError GetOverlayTransformTrackedDeviceComponent(ulong ulOverlayHandle,ref uint punDeviceIndex,string pchComponentName,uint unComponentNameSize)
	{
		punDeviceIndex = 0;
		EHVROverlayError result = FnTable.GetOverlayTransformTrackedDeviceComponent(ulOverlayHandle,ref punDeviceIndex,pchComponentName,unComponentNameSize);
		return result;
	}
	public EHVROverlayError ShowOverlay(ulong ulOverlayHandle)
	{
		EHVROverlayError result = FnTable.ShowOverlay(ulOverlayHandle);
		return result;
	}
	public EHVROverlayError HideOverlay(ulong ulOverlayHandle)
	{
		EHVROverlayError result = FnTable.HideOverlay(ulOverlayHandle);
		return result;
	}
	public bool IsOverlayVisible(ulong ulOverlayHandle)
	{
		bool result = FnTable.IsOverlayVisible(ulOverlayHandle);
		return result;
	}
	public EHVROverlayError SetOverlayTexture(ulong ulOverlayHandle,ref HVRTexture_t pTexture)
	{
		EHVROverlayError result = FnTable.SetOverlayTexture(ulOverlayHandle,ref pTexture);
		return result;
	}
	public EHVROverlayError ClearOverlayTexture(ulong ulOverlayHandle)
	{
		EHVROverlayError result = FnTable.ClearOverlayTexture(ulOverlayHandle);
		return result;
	}
	public EHVROverlayError SetOverlayRaw(ulong ulOverlayHandle,IntPtr pvBuffer,uint unWidth,uint unHeight,uint unDepth)
	{
		EHVROverlayError result = FnTable.SetOverlayRaw(ulOverlayHandle,pvBuffer,unWidth,unHeight,unDepth);
		return result;
	}
	public EHVROverlayError SetOverlayFromFile(ulong ulOverlayHandle,string pchFilePath)
	{
		EHVROverlayError result = FnTable.SetOverlayFromFile(ulOverlayHandle,pchFilePath);
		return result;
	}
	public EHVROverlayError GetOverlayTexture(ulong ulOverlayHandle,ref IntPtr pNativeTextureHandle,IntPtr pNativeTextureRef,ref uint pWidth,ref uint pHeight,ref uint pNativeFormat,ref EHVRTextureType pAPI,ref EHVRColorSpace pColorSpace)
	{
		pWidth = 0;
		pHeight = 0;
		pNativeFormat = 0;
		EHVROverlayError result = FnTable.GetOverlayTexture(ulOverlayHandle,ref pNativeTextureHandle,pNativeTextureRef,ref pWidth,ref pHeight,ref pNativeFormat,ref pAPI,ref pColorSpace);
		return result;
	}
	public EHVROverlayError GetOverlayTextureSize(ulong ulOverlayHandle,ref uint pWidth,ref uint pHeight)
	{
		pWidth = 0;
		pHeight = 0;
		EHVROverlayError result = FnTable.GetOverlayTextureSize(ulOverlayHandle,ref pWidth,ref pHeight);
		return result;
	}
    public EHVROverlayError SetOverlayStereoTexture(ulong ulOverlayHandle, ref HVRTexture_t pTexture, EHVREye eye)
    {
        EHVROverlayError result = FnTable.SetOverlayStereoTexture(ulOverlayHandle, ref pTexture, eye);
        return result;
    }
    public EHVROverlayError SetOverlayStereoMode(ulong ulOverlayHandle, bool bEnabled)
    {
        EHVROverlayError result = FnTable.SetOverlayStereoMode(ulOverlayHandle, bEnabled);
        return result;
    }
    public EHVROverlayError SetOverlayStereoTextureBounds(ulong ulOverlayHandle, ref HVRTextureBounds_t pOverlayTextureBounds, EHVREye eye)
    {
        EHVROverlayError result = FnTable.SetOverlayStereoTextureBounds(ulOverlayHandle, ref pOverlayTextureBounds, eye);
        return result;
    }
	public EHVROverlayError SetOverlayTextureUpsideDownFlag(ulong ulOverlayHandle, bool flag)
    {
        EHVROverlayError result = FnTable.SetOverlayTextureUpsideDownFlag(ulOverlayHandle, flag);
        return result;
    }

    public EHVROverlayError CreateDesktopCaptureOverlay(uint targetHandle)
    {
        EHVROverlayError result = FnTable.CreateDesktopCaptureOverlay(targetHandle);
        return result;
    }

    public EHVROverlayError DestroyDesktopCaptureOverlay(uint targetHandle)
    {
        EHVROverlayError result = FnTable.DestroyDesktopCaptureOverlay(targetHandle);
        return result;
    }

    public EHVROverlayError SetDesktopCaptureOverlayWidth(uint targetHandle, float width)
    {
        EHVROverlayError result = FnTable.SetDesktopCaptureOverlayWidth(targetHandle, width);
        return result;
    }

    public EHVROverlayError SetDesktopCaptureOverlayAbsolutePose(
        uint targetHandle, ref HVRMatrix34_t pmatTrackedDeviceToOverlayTransform)
    {
        EHVROverlayError result = FnTable.SetDesktopCaptureOverlayAbsolutePose(targetHandle, ref pmatTrackedDeviceToOverlayTransform);
        return result;
    }

    public EHVROverlayError SetDesktopCaptureOverlayCurveDistance(uint targetHandle, float minDist, float maxDist)
    {
        EHVROverlayError result = FnTable.SetDesktopCaptureOverlayCurveDistance(targetHandle, minDist, maxDist);
        return result;
    }
}

public class CHVRRenderModels
{
    IHVRRenderModels FnTable;
    internal CHVRRenderModels(IntPtr pInterface)
    {
        FnTable = (IHVRRenderModels)Marshal.PtrToStructure(pInterface, typeof(IHVRRenderModels));
    }
    public EHVRRenderModelError LoadRenderModel_Async(string pchRenderModelName, ref IntPtr ppRenderModel)
    {
            EHVRRenderModelError result = FnTable.LoadRenderModel_Async(pchRenderModelName, ref ppRenderModel);
        return result;
    }
    public void FreeRenderModel(IntPtr pRenderModel)
    {
        FnTable.FreeRenderModel(pRenderModel);
    }
    public EHVRRenderModelError LoadTexture_Async(int textureId, ref IntPtr ppTexture)
    {
        EHVRRenderModelError result = FnTable.LoadTexture_Async(textureId, ref ppTexture);
        return result;
    }
    public void FreeTexture(IntPtr pTexture)
    {
        FnTable.FreeTexture(pTexture);
    }
    public EHVRRenderModelError LoadTextureD3D11_Async(int textureId, IntPtr pD3D11Device, ref IntPtr ppD3D11Texture2D)
    {
            EHVRRenderModelError result = FnTable.LoadTextureD3D11_Async(textureId, pD3D11Device, ref ppD3D11Texture2D);
        return result;
    }
    public EHVRRenderModelError LoadIntoTextureD3D11_Async(int textureId, IntPtr pDstTexture)
    {
            EHVRRenderModelError result = FnTable.LoadIntoTextureD3D11_Async(textureId, pDstTexture);
        return result;
    }
    public void FreeTextureD3D11(IntPtr pD3D11Texture2D)
    {
        FnTable.FreeTextureD3D11(pD3D11Texture2D);
    }
    public uint GetRenderModelName(uint unRenderModelIndex, System.Text.StringBuilder pchRenderModelName, uint unRenderModelNameLen)
    {
        uint result = FnTable.GetRenderModelName(unRenderModelIndex, pchRenderModelName, unRenderModelNameLen);
        return result;
    }
    public uint GetRenderModelCount()
    {
        uint result = FnTable.GetRenderModelCount();
        return result;
    }
    public uint GetComponentCount(string pchRenderModelName)
    {
        uint result = FnTable.GetComponentCount(pchRenderModelName);
        return result;
    }
    public uint GetComponentName(string pchRenderModelName, uint unComponentIndex, System.Text.StringBuilder pchComponentName, uint unComponentNameLen)
    {
        uint result = FnTable.GetComponentName(pchRenderModelName, unComponentIndex, pchComponentName, unComponentNameLen);
        return result;
    }
    public ulong GetComponentButtonMask(string pchRenderModelName, string pchComponentName)
    {
        ulong result = FnTable.GetComponentButtonMask(pchRenderModelName, pchComponentName);
        return result;
    }
    public uint GetComponentRenderModelName(string pchRenderModelName, string pchComponentName, System.Text.StringBuilder pchComponentRenderModelName, uint unComponentRenderModelNameLen)
    {
        uint result = FnTable.GetComponentRenderModelName(pchRenderModelName, pchComponentName, pchComponentRenderModelName, unComponentRenderModelNameLen);
        return result;
    }
    public bool GetComponentStateForDevicePath(string pchRenderModelName, string pchComponentName, ulong devicePath, ref RenderModel_ControllerMode_State_t pState, ref RenderModel_ComponentState_t pComponentState)
    {
        bool result = FnTable.GetComponentStateForDevicePath(pchRenderModelName, pchComponentName, devicePath, ref pState, ref pComponentState);
        return result;
    }
    // This is a terrible hack to workaround the fact that VRControllerState_t and VREvent_t were
    // originally mis-compiled with the wrong packing for Linux and OSX.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate bool _GetComponentStatePacked(string pchRenderModelName, string pchComponentName, ref VRControllerState_t_Packed pControllerState, ref RenderModel_ControllerMode_State_t pState, ref RenderModel_ComponentState_t pComponentState);
    [StructLayout(LayoutKind.Explicit)]
    struct GetComponentStateUnion
    {
        [FieldOffset(0)]
        public IHVRRenderModels._GetComponentState pGetComponentState;
        [FieldOffset(0)]
        public _GetComponentStatePacked pGetComponentStatePacked;
    }
    public bool GetComponentState(string pchRenderModelName, string pchComponentName, ref HVRControllerState_t pControllerState, ref RenderModel_ControllerMode_State_t pState, ref RenderModel_ComponentState_t pComponentState)
    {
#if !UNITY_METRO
        if ((System.Environment.OSVersion.Platform == System.PlatformID.MacOSX) ||
                (System.Environment.OSVersion.Platform == System.PlatformID.Unix))
        {
            GetComponentStateUnion u;
            VRControllerState_t_Packed state_packed = new VRControllerState_t_Packed(pControllerState);
            u.pGetComponentStatePacked = null;
            u.pGetComponentState = FnTable.GetComponentState;
            bool packed_result = u.pGetComponentStatePacked(pchRenderModelName, pchComponentName, ref state_packed, ref pState, ref pComponentState);

            state_packed.Unpack(ref pControllerState);
            return packed_result;
        }
#endif
        bool result = FnTable.GetComponentState(pchRenderModelName, pchComponentName, ref pControllerState, ref pState, ref pComponentState);
        return result;
    }
    public bool RenderModelHasComponent(string pchRenderModelName, string pchComponentName)
    {
        bool result = FnTable.RenderModelHasComponent(pchRenderModelName, pchComponentName);
        return result;
    }
    public uint GetRenderModelThumbnailURL(string pchRenderModelName, System.Text.StringBuilder pchThumbnailURL, uint unThumbnailURLLen, ref EHVRRenderModelError peError)
    {
        uint result = FnTable.GetRenderModelThumbnailURL(pchRenderModelName, pchThumbnailURL, unThumbnailURLLen, ref peError);
        return result;
    }
    public uint GetRenderModelOriginalPath(string pchRenderModelName, System.Text.StringBuilder pchOriginalPath, uint unOriginalPathLen, ref EHVRRenderModelError peError)
    {
        uint result = FnTable.GetRenderModelOriginalPath(pchRenderModelName, pchOriginalPath, unOriginalPathLen, ref peError);
        return result;
    }
    public string GetRenderModelErrorNameFromEnum(EHVRRenderModelError error)
    {
        IntPtr result = FnTable.GetRenderModelErrorNameFromEnum(error);
        return Marshal.PtrToStringAnsi(result);
    }
}

    public class CHVRTrackedCamera
{
	IHVRTrackedCamera FnTable;
	internal CHVRTrackedCamera(IntPtr pInterface)
	{
		FnTable = (IHVRTrackedCamera)Marshal.PtrToStructure(pInterface, typeof(IHVRTrackedCamera));
	}
	public string GetCameraErrorNameFromEnum(EHVRTrackedCameraError eCameraError)
	{
		IntPtr result = FnTable.GetCameraErrorNameFromEnum(eCameraError);
		return Marshal.PtrToStringAnsi(result);
	}
	public EHVRTrackedCameraError HasCamera(uint nDeviceIndex,ref bool pHasCamera)
	{
		pHasCamera = false;
		EHVRTrackedCameraError result = FnTable.HasCamera(nDeviceIndex,ref pHasCamera);
		return result;
	}
	public EHVRTrackedCameraError GetCameraFrameSize(uint nDeviceIndex,EHVRTrackedCameraFrameType eFrameType,ref uint pnWidth,ref uint pnHeight,ref uint pnFrameBufferSize)
	{
		pnWidth = 0;
		pnHeight = 0;
		pnFrameBufferSize = 0;
		EHVRTrackedCameraError result = FnTable.GetCameraFrameSize(nDeviceIndex,eFrameType,ref pnWidth,ref pnHeight,ref pnFrameBufferSize);
		return result;
	}
	public EHVRTrackedCameraError GetCameraIntrinsics(uint nDeviceIndex, uint nCameraIndex, EHVRTrackedCameraFrameType eFrameType,ref HVRVector2_t pFocalLength,ref HVRVector2_t pCenter)
	{
		EHVRTrackedCameraError result = FnTable.GetCameraIntrinsics(nDeviceIndex, nCameraIndex, eFrameType, ref pFocalLength,ref pCenter);
		return result;
	}
	public EHVRTrackedCameraError GetCameraProjection(uint nDeviceIndex,uint nCameraIndex, EHVRTrackedCameraFrameType eFrameType,float flZNear,float flZFar,ref HVRMatrix44_t pProjection)
	{
		EHVRTrackedCameraError result = FnTable.GetCameraProjection(nDeviceIndex,nCameraIndex, eFrameType,flZNear,flZFar,ref pProjection);
		return result;
	}
	public EHVRTrackedCameraError AcquireVideoStreamingService(uint nDeviceIndex,ref ulong pHandle)
	{
		pHandle = 0;
		EHVRTrackedCameraError result = FnTable.AcquireVideoStreamingService(nDeviceIndex,ref pHandle);
		return result;
	}
	public EHVRTrackedCameraError ReleaseVideoStreamingService(ulong hTrackedCamera)
	{
		EHVRTrackedCameraError result = FnTable.ReleaseVideoStreamingService(hTrackedCamera);
		return result;
	}
	public EHVRTrackedCameraError GetVideoStreamFrameBuffer(ulong hTrackedCamera,EHVRTrackedCameraFrameType eFrameType,IntPtr pFrameBuffer,uint nFrameBufferSize,ref HVRCameraVideoStreamFrameHeader_t pFrameHeader,uint nFrameHeaderSize)
	{
		EHVRTrackedCameraError result = FnTable.GetVideoStreamFrameBuffer(hTrackedCamera,eFrameType,pFrameBuffer,nFrameBufferSize,ref pFrameHeader,nFrameHeaderSize);
		return result;
	}
	public EHVRTrackedCameraError GetVideoStreamTextureSize(uint nDeviceIndex,EHVRTrackedCameraFrameType eFrameType,ref HVRTextureBounds_t pTextureBounds,ref uint pnWidth,ref uint pnHeight)
	{
		pnWidth = 0;
		pnHeight = 0;
		EHVRTrackedCameraError result = FnTable.GetVideoStreamTextureSize(nDeviceIndex,eFrameType,ref pTextureBounds,ref pnWidth,ref pnHeight);
		return result;
	}
	public EHVRTrackedCameraError GetVideoStreamTextureD3D11(ulong hTrackedCamera,EHVRTrackedCameraFrameType eFrameType,IntPtr pD3D11DeviceOrResource,ref IntPtr ppD3D11ShaderResourceView,ref HVRCameraVideoStreamFrameHeader_t pFrameHeader,uint nFrameHeaderSize)
	{
		EHVRTrackedCameraError result = FnTable.GetVideoStreamTextureD3D11(hTrackedCamera,eFrameType,pD3D11DeviceOrResource,ref ppD3D11ShaderResourceView,ref pFrameHeader,nFrameHeaderSize);
		return result;
	}
	public EHVRTrackedCameraError GetVideoStreamTextureGL(ulong hTrackedCamera,EHVRTrackedCameraFrameType eFrameType,ref uint pglTextureId,ref HVRCameraVideoStreamFrameHeader_t pFrameHeader,uint nFrameHeaderSize)
	{
		pglTextureId = 0;
		EHVRTrackedCameraError result = FnTable.GetVideoStreamTextureGL(hTrackedCamera,eFrameType,ref pglTextureId,ref pFrameHeader,nFrameHeaderSize);
		return result;
	}
	public EHVRTrackedCameraError ReleaseVideoStreamTextureGL(ulong hTrackedCamera,uint glTextureId)
	{
		EHVRTrackedCameraError result = FnTable.ReleaseVideoStreamTextureGL(hTrackedCamera,glTextureId);
		return result;
	}
}

public class CHVRSettings
{
	IHVRSettings FnTable;
	internal CHVRSettings(IntPtr pInterface)
	{
		FnTable = (IHVRSettings)Marshal.PtrToStructure(pInterface, typeof(IHVRSettings));
	}
	public string GetSettingsErrorNameFromEnum(EHVRSettingsError eError)
	{
		IntPtr result = FnTable.GetSettingsErrorNameFromEnum(eError);
		return Marshal.PtrToStringAnsi(result);
	}
	public bool Sync(bool bForce,ref EHVRSettingsError peError)
	{
		bool result = FnTable.Sync(bForce,ref peError);
		return result;
	}
	public void SetBool(string pchSection,string pchSettingsKey,bool bValue,ref EHVRSettingsError peError)
	{
		FnTable.SetBool(pchSection,pchSettingsKey,bValue,ref peError);
	}
	public void SetInt32(string pchSection,string pchSettingsKey,int nValue,ref EHVRSettingsError peError)
	{
		FnTable.SetInt32(pchSection,pchSettingsKey,nValue,ref peError);
	}
	public void SetFloat(string pchSection,string pchSettingsKey,float flValue,ref EHVRSettingsError peError)
	{
		FnTable.SetFloat(pchSection,pchSettingsKey,flValue,ref peError);
	}
	public void SetString(string pchSection,string pchSettingsKey,string pchValue,ref EHVRSettingsError peError)
	{
		FnTable.SetString(pchSection,pchSettingsKey,pchValue,ref peError);
	}
	public bool GetBool(string pchSection,string pchSettingsKey,ref EHVRSettingsError peError)
	{
		bool result = FnTable.GetBool(pchSection,pchSettingsKey,ref peError);
		return result;
	}
	public int GetInt32(string pchSection,string pchSettingsKey,ref EHVRSettingsError peError)
	{
		int result = FnTable.GetInt32(pchSection,pchSettingsKey,ref peError);
		return result;
	}
	public float GetFloat(string pchSection,string pchSettingsKey,ref EHVRSettingsError peError)
	{
		float result = FnTable.GetFloat(pchSection,pchSettingsKey,ref peError);
		return result;
	}
	public void GetString(string pchSection,string pchSettingsKey,System.Text.StringBuilder pchValue,uint unValueLen,ref EHVRSettingsError peError)
	{
		FnTable.GetString(pchSection,pchSettingsKey,pchValue,unValueLen,ref peError);
	}
	public void RemoveSection(string pchSection,ref EHVRSettingsError peError)
	{
		FnTable.RemoveSection(pchSection,ref peError);
	}
	public void RemoveKeyInSection(string pchSection,string pchSettingsKey,ref EHVRSettingsError peError)
	{
		FnTable.RemoveKeyInSection(pchSection,pchSettingsKey,ref peError);
	}
}

public class CHVRChaperoneSetup
{
	IHVRChaperoneSetup FnTable;
	internal CHVRChaperoneSetup(IntPtr pInterface)
	{
		FnTable = (IHVRChaperoneSetup)Marshal.PtrToStructure(pInterface, typeof(IHVRChaperoneSetup));
	}
	public bool CommitWorkingCopy(EHVRChaperoneConfigFile configFile)
	{
		bool result = FnTable.CommitWorkingCopy(configFile);
		return result;
	}
	public void RevertWorkingCopy()
	{
		FnTable.RevertWorkingCopy();
	}
	public bool GetWorkingPlayAreaSize(ref float pSizeX,ref float pSizeZ)
	{
		pSizeX = 0;
		pSizeZ = 0;
		bool result = FnTable.GetWorkingPlayAreaSize(ref pSizeX,ref pSizeZ);
		return result;
	}
	public bool GetWorkingPlayAreaRect(ref HVRQuad_t rect)
	{
		bool result = FnTable.GetWorkingPlayAreaRect(ref rect);
		return result;
	}
	public bool GetWorkingCollisionBoundsInfo(out HVRQuad_t [] pQuadsBuffer)
	{
		uint punQuadsCount = 0;
		bool result = FnTable.GetWorkingCollisionBoundsInfo(null,ref punQuadsCount);
		pQuadsBuffer= new HVRQuad_t[punQuadsCount];
		result = FnTable.GetWorkingCollisionBoundsInfo(pQuadsBuffer,ref punQuadsCount);
		return result;
	}
	public bool GetLiveCollisionBoundsInfo(out HVRQuad_t [] pQuadsBuffer)
	{
		uint punQuadsCount = 0;
		bool result = FnTable.GetLiveCollisionBoundsInfo(null,ref punQuadsCount);
		pQuadsBuffer= new HVRQuad_t[punQuadsCount];
		result = FnTable.GetLiveCollisionBoundsInfo(pQuadsBuffer,ref punQuadsCount);
		return result;
	}
	public bool GetWorkingSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatSeatedZeroPoseToRawTrackingPose)
	{
		bool result = FnTable.GetWorkingSeatedZeroPoseToRawTrackingPose(ref pmatSeatedZeroPoseToRawTrackingPose);
		return result;
	}
	public bool GetWorkingStandingZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatStandingZeroPoseToRawTrackingPose)
	{
		bool result = FnTable.GetWorkingStandingZeroPoseToRawTrackingPose(ref pmatStandingZeroPoseToRawTrackingPose);
		return result;
	}
	public void SetWorkingPlayAreaSize(float sizeX,float sizeZ)
	{
		FnTable.SetWorkingPlayAreaSize(sizeX,sizeZ);
	}
	public void SetWorkingCollisionBoundsInfo(HVRQuad_t [] pQuadsBuffer)
	{
		FnTable.SetWorkingCollisionBoundsInfo(pQuadsBuffer,(uint) pQuadsBuffer.Length);
	}
	public void SetWorkingSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pMatSeatedZeroPoseToRawTrackingPose)
	{
		FnTable.SetWorkingSeatedZeroPoseToRawTrackingPose(ref pMatSeatedZeroPoseToRawTrackingPose);
	}
	public void SetWorkingStandingZeroPoseToRawTrackingPose(ref HVRMatrix34_t pMatStandingZeroPoseToRawTrackingPose)
	{
		FnTable.SetWorkingStandingZeroPoseToRawTrackingPose(ref pMatStandingZeroPoseToRawTrackingPose);
	}
	public void ReloadFromDisk(EHVRChaperoneConfigFile configFile)
	{
		FnTable.ReloadFromDisk(configFile);
	}
	public bool GetLiveSeatedZeroPoseToRawTrackingPose(ref HVRMatrix34_t pmatSeatedZeroPoseToRawTrackingPose)
	{
		bool result = FnTable.GetLiveSeatedZeroPoseToRawTrackingPose(ref pmatSeatedZeroPoseToRawTrackingPose);
		return result;
	}
	public void SetWorkingCollisionBoundsTagsInfo(byte [] pTagsBuffer)
	{
		FnTable.SetWorkingCollisionBoundsTagsInfo(pTagsBuffer,(uint) pTagsBuffer.Length);
	}
	public bool GetLiveCollisionBoundsTagsInfo(out byte [] pTagsBuffer)
	{
		uint punTagCount = 0;
		bool result = FnTable.GetLiveCollisionBoundsTagsInfo(null,ref punTagCount);
		pTagsBuffer= new byte[punTagCount];
		result = FnTable.GetLiveCollisionBoundsTagsInfo(pTagsBuffer,ref punTagCount);
		return result;
	}
	public bool SetWorkingPhysicalBoundsInfo(HVRQuad_t [] pQuadsBuffer)
	{
		bool result = FnTable.SetWorkingPhysicalBoundsInfo(pQuadsBuffer,(uint) pQuadsBuffer.Length);
		return result;
	}
	public bool GetLivePhysicalBoundsInfo(out HVRQuad_t [] pQuadsBuffer)
	{
		uint punQuadsCount = 0;
		bool result = FnTable.GetLivePhysicalBoundsInfo(null,ref punQuadsCount);
		pQuadsBuffer= new HVRQuad_t[punQuadsCount];
		result = FnTable.GetLivePhysicalBoundsInfo(pQuadsBuffer,ref punQuadsCount);
		return result;
	}
	public bool ExportLiveToBuffer(System.Text.StringBuilder pBuffer,ref uint pnBufferLength)
	{
		pnBufferLength = 0;
		bool result = FnTable.ExportLiveToBuffer(pBuffer,ref pnBufferLength);
		return result;
	}
	public bool ImportFromBufferToWorking(string pBuffer,uint nImportFlags)
	{
		bool result = FnTable.ImportFromBufferToWorking(pBuffer,nImportFlags);
		return result;
	}
}

public class HVRInterop
{
	[DllImportAttribute("ViveVR_api", EntryPoint = "HVR_Init")]
	internal static extern EHVRErrorCode HVR_InitInternal(EHVRApplicationType eApplicationType);
	[DllImportAttribute("ViveVR_api", EntryPoint = "HVR_ShutDown")]
	internal static extern EHVRErrorCode HVR_ShutdownInternal();
	[DllImportAttribute("ViveVR_api", EntryPoint = "HVR_GetGenericInterface")]
	internal static extern IntPtr HVR_GetGenericInterface([In, MarshalAs(UnmanagedType.LPStr)] string pchInterfaceVersion, ref EHVRErrorCode peError);
}

public enum EHVREye
{
	EHVREye_Left = 0,
	EHVREye_Right = 1,
}

public enum EHVRTextureType
{
	EHVRTextureType_DirectX = 0,
	EHVRTextureType_OpenGL = 1,
    EHVRTextureType_Vulkan = 2,
    EHVRTextureType_IOSurface = 3,
    EHVRTextureType_DirectX12 = 4,
}

public enum EHVRColorSpace
{
	EHVRColorSpace_Auto = 0,
	EHVRColorSpace_Gamma = 1,
	EHVRColorSpace_Linear = 2,
}

public enum EHVRSubmitFlags
{
	EHVRSubmitFlags_Default = 0,
	EHVRSubmitFlags_LensDistortionAlreadyApplied = 1,
	EHVRSubmitFlags_GlRenderBuffer = 2,
	EHVRSubmitFlags_Reserved = 4,
	EHVRSubmitFlags_TextureWithPose = 8,
}

public enum EHVRTrackingUniverseOrigin
{
	EHVRTrackingUniverseOrigin_Seated = 0,
	EHVRTrackingUniverseOrigin_Standing = 1,
	EHVRTrackingUniverseOrigin_RawAndUncalibrated = 2,
}

public enum EHVRTrackingResult
{
	EHVRTrackingResult_Uninitialized = 1,
	EHVRTrackingResult_Calibrating_InProgress = 100,
	EHVRTrackingResult_Calibrating_OutOfRange = 101,
	EHVRTrackingResult_Running_OK = 200,
	EHVRTrackingResult_Running_OutOfRange = 201,
}

public enum EHVRTrackedControllerRole
{
	EHVRTrackedControllerRole_Invalid = 0,
	EHVRTrackedControllerRole_LeftHand = 1,
	EHVRTrackedControllerRole_RightHand = 2,
}

public enum EHVRTrackedDeviceClass
{
	EHVRTrackedDeviceClass_Invalid = 0,
	EHVRTrackedDeviceClass_HMD = 1,
	EHVRTrackedDeviceClass_Controller = 2,
	EHVRTrackedDeviceClass_GenericTracker = 3,
	EHVRTrackedDeviceClass_TrackingReference = 4,
}

public enum EHVRApplicationType
{
	EHVRApplication_Other = 0,
	EHVRApplication_Scene = 1,
	EHVRApplication_Overlay = 2,
	EHVRApplication_Background = 3,
	EHVRApplication_Utility = 4,
	EHVRApplication_VRMonitor = 5,
	EHVRApplication_Max = 6,
}

public enum EHVRTrackedDeviceProperty
{
	EHVRTrackedDeviceProperty_Invalid = 0,
	EHVRTrackedDeviceProperty_TrackingSystemName_String = 1000,
	EHVRTrackedDeviceProperty_ModelNumber_String = 1001,
	EHVRTrackedDeviceProperty_SerialNumber_String = 1002,
	EHVRTrackedDeviceProperty_RenderModelName_String = 1003,
	EHVRTrackedDeviceProperty_WillDriftInYaw_Bool = 1004,
	EHVRTrackedDeviceProperty_ManufacturerName_String = 1005,
	EHVRTrackedDeviceProperty_TrackingFirmwareVersion_String = 1006,
	EHVRTrackedDeviceProperty_HardwareRevision_String = 1007,
	EHVRTrackedDeviceProperty_AllWirelessDongleDescriptions_String = 1008,
	EHVRTrackedDeviceProperty_ConnectedWirelessDongle_String = 1009,
	EHVRTrackedDeviceProperty_DeviceIsWireless_Bool = 1010,
	EHVRTrackedDeviceProperty_DeviceIsCharging_Bool = 1011,
	EHVRTrackedDeviceProperty_DeviceBatteryPercentage_Float = 1012,
	EHVRTrackedDeviceProperty_StatusDisplayTransform_Matrix34 = 1013,
	EHVRTrackedDeviceProperty_Firmware_UpdateAvailable_Bool = 1014,
	EHVRTrackedDeviceProperty_Firmware_ManualUpdate_Bool = 1015,
	EHVRTrackedDeviceProperty_Firmware_ManualUpdateURL_String = 1016,
	EHVRTrackedDeviceProperty_HardwareRevision_Uint64 = 1017,
	EHVRTrackedDeviceProperty_FirmwareVersion_Uint64 = 1018,
	EHVRTrackedDeviceProperty_FPGAVersion_Uint64 = 1019,
	EHVRTrackedDeviceProperty_VRCVersion_Uint64 = 1020,
	EHVRTrackedDeviceProperty_RadioVersion_Uint64 = 1021,
	EHVRTrackedDeviceProperty_DongleVersion_Uint64 = 1022,
	EHVRTrackedDeviceProperty_BlockServerShutdown_Bool = 1023,
	EHVRTrackedDeviceProperty_CanUnifyCoordinateSystemWithHmd_Bool = 1024,
	EHVRTrackedDeviceProperty_ContainsProximitySensor_Bool = 1025,
	EHVRTrackedDeviceProperty_DeviceProvidesBatteryStatus_Bool = 1026,
	EHVRTrackedDeviceProperty_DeviceCanPowerOff_Bool = 1027,
	EHVRTrackedDeviceProperty_Firmware_ProgrammingTarget_String = 1028,
	EHVRTrackedDeviceProperty_DeviceClass_Int32 = 1029,
	EHVRTrackedDeviceProperty_HasCamera_Bool = 1030,
	EHVRTrackedDeviceProperty_DriverVersion_String = 1031,
	EHVRTrackedDeviceProperty_Firmware_ForceUpdateRequired_Bool = 1032,
	EHVRTrackedDeviceProperty_ViveSystemButtonFixRequired_Bool = 1033,
	EHVRTrackedDeviceProperty_ParentDriver_Uint64 = 1034,
	EHVRTrackedDeviceProperty_ResourceRoot_String = 1035,
	EHVRTrackedDeviceProperty_ReportsTimeSinceVSync_Bool = 2000,
	EHVRTrackedDeviceProperty_SecondsFromVsyncToPhotons_Float = 2001,
	EHVRTrackedDeviceProperty_DisplayFrequency_Float = 2002,
	EHVRTrackedDeviceProperty_UserIpdMeters_Float = 2003,
	EHVRTrackedDeviceProperty_CurrentUniverseId_Uint64 = 2004,
	EHVRTrackedDeviceProperty_PreviousUniverseId_Uint64 = 2005,
	EHVRTrackedDeviceProperty_DisplayFirmwareVersion_Uint64 = 2006,
	EHVRTrackedDeviceProperty_IsOnDesktop_Bool = 2007,
	EHVRTrackedDeviceProperty_DisplayMCType_Int32 = 2008,
	EHVRTrackedDeviceProperty_DisplayMCOffset_Float = 2009,
	EHVRTrackedDeviceProperty_DisplayMCScale_Float = 2010,
	EHVRTrackedDeviceProperty_EdidVendorID_Int32 = 2011,
	EHVRTrackedDeviceProperty_DisplayMCImageLeft_String = 2012,
	EHVRTrackedDeviceProperty_DisplayMCImageRight_String = 2013,
	EHVRTrackedDeviceProperty_DisplayGCBlackClamp_Float = 2014,
	EHVRTrackedDeviceProperty_EdidProductID_Int32 = 2015,
	EHVRTrackedDeviceProperty_CameraToHeadTransform_Matrix34 = 2016,
	EHVRTrackedDeviceProperty_DisplayGCType_Int32 = 2017,
	EHVRTrackedDeviceProperty_DisplayGCOffset_Float = 2018,
	EHVRTrackedDeviceProperty_DisplayGCScale_Float = 2019,
	EHVRTrackedDeviceProperty_DisplayGCPrescale_Float = 2020,
	EHVRTrackedDeviceProperty_DisplayGCImage_String = 2021,
	EHVRTrackedDeviceProperty_LensCenterLeftU_Float = 2022,
	EHVRTrackedDeviceProperty_LensCenterLeftV_Float = 2023,
	EHVRTrackedDeviceProperty_LensCenterRightU_Float = 2024,
	EHVRTrackedDeviceProperty_LensCenterRightV_Float = 2025,
	EHVRTrackedDeviceProperty_UserHeadToEyeDepthMeters_Float = 2026,
	EHVRTrackedDeviceProperty_CameraFirmwareVersion_Uint64 = 2027,
	EHVRTrackedDeviceProperty_CameraFirmwareDescription_String = 2028,
	EHVRTrackedDeviceProperty_DisplayFPGAVersion_Uint64 = 2029,
	EHVRTrackedDeviceProperty_DisplayBootloaderVersion_Uint64 = 2030,
	EHVRTrackedDeviceProperty_DisplayHardwareVersion_Uint64 = 2031,
	EHVRTrackedDeviceProperty_AudioFirmwareVersion_Uint64 = 2032,
	EHVRTrackedDeviceProperty_CameraCompatibilityMode_Int32 = 2033,
	EHVRTrackedDeviceProperty_ScreenshotHorizontalFieldOfViewDegrees_Float = 2034,
	EHVRTrackedDeviceProperty_ScreenshotVerticalFieldOfViewDegrees_Float = 2035,
	EHVRTrackedDeviceProperty_DisplaySuppressed_Bool = 2036,
	EHVRTrackedDeviceProperty_DisplayAllowNightMode_Bool = 2037,
	EHVRTrackedDeviceProperty_DisplayMCImageWidth_Int32 = 2038,
	EHVRTrackedDeviceProperty_DisplayMCImageHeight_Int32 = 2039,
	EHVRTrackedDeviceProperty_DisplayMCImageNumChannels_Int32 = 2040,
	EHVRTrackedDeviceProperty_DisplayMCImageData_Binary = 2041,
	EHVRTrackedDeviceProperty_SecondsFromPhotonsToVblank_Float = 2042,
	EHVRTrackedDeviceProperty_DriverDirectModeSendsVsyncEvents_Bool = 2043,
	EHVRTrackedDeviceProperty_DisplayDebugMode_Bool = 2044,
	EHVRTrackedDeviceProperty_GraphicsAdapterLuid_Uint64 = 2045,
	EHVRTrackedDeviceProperty_AttachedDeviceId_String = 3000,
	EHVRTrackedDeviceProperty_SupportedButtons_Uint64 = 3001,
	EHVRTrackedDeviceProperty_Axis0Type_Int32 = 3002,
	EHVRTrackedDeviceProperty_Axis1Type_Int32 = 3003,
	EHVRTrackedDeviceProperty_Axis2Type_Int32 = 3004,
	EHVRTrackedDeviceProperty_Axis3Type_Int32 = 3005,
	EHVRTrackedDeviceProperty_Axis4Type_Int32 = 3006,
	EHVRTrackedDeviceProperty_ControllerRoleHint_Int32 = 3007,
	EHVRTrackedDeviceProperty_FieldOfViewLeftDegrees_Float = 4000,
	EHVRTrackedDeviceProperty_FieldOfViewRightDegrees_Float = 4001,
	EHVRTrackedDeviceProperty_FieldOfViewTopDegrees_Float = 4002,
	EHVRTrackedDeviceProperty_FieldOfViewBottomDegrees_Float = 4003,
	EHVRTrackedDeviceProperty_TrackingRangeMinimumMeters_Float = 4004,
	EHVRTrackedDeviceProperty_TrackingRangeMaximumMeters_Float = 4005,
	EHVRTrackedDeviceProperty_ModeLabel_String = 4006,
	EHVRTrackedDeviceProperty_IconPathName_String = 5000,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceOff_String = 5001,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceSearching_String = 5002,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceSearchingAlert_String = 5003,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceReady_String = 5004,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceReadyAlert_String = 5005,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceNotReady_String = 5006,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceStandby_String = 5007,
	EHVRTrackedDeviceProperty_NamedIconPathDeviceAlertLow_String = 5008,
	EHVRTrackedDeviceProperty_DisplayHiddenArea_Binary_Start = 5100,
	EHVRTrackedDeviceProperty_DisplayHiddenArea_Binary_End = 5150,
	EHVRTrackedDeviceProperty_UserConfigPath_String = 6000,
	EHVRTrackedDeviceProperty_InstallPath_String = 6001,
	EHVRTrackedDeviceProperty_HasDisplayComponent_Bool = 6002,
	EHVRTrackedDeviceProperty_HasControllerComponent_Bool = 6003,
	EHVRTrackedDeviceProperty_HasCameraComponent_Bool = 6004,
	EHVRTrackedDeviceProperty_HasDriverDirectModeComponent_Bool = 6005,
	EHVRTrackedDeviceProperty_HasVirtualDisplayComponent_Bool = 6006,
	EHVRTrackedDeviceProperty_VendorSpecific_Reserved_Start = 10000,
	EHVRTrackedDeviceProperty_VendorSpecific_Reserved_End = 10999,
}

public enum EHVRTrackedPropertyError
{
	EHVRTrackedPropertyError_Success = 0,
	EHVRTrackedPropertyError_WrongDataType = 1,
	EHVRTrackedPropertyError_WrongDeviceClass = 2,
	EHVRTrackedPropertyError_BufferTooSmall = 3,
	EHVRTrackedPropertyError_UnknownProperty = 4,
	EHVRTrackedPropertyError_InvalidDevice = 5,
	EHVRTrackedPropertyError_CouldNotContactServer = 6,
	EHVRTrackedPropertyError_ValueNotProvidedByDevice = 7,
	EHVRTrackedPropertyError_StringExceedsMaximumLength = 8,
	EHVRTrackedPropertyError_NotYetAvailable = 9,
	EHVRTrackedPropertyError_PermissionDenied = 10,
	EHVRTrackedPropertyError_InvalidOperation = 11,
}

public enum EHVRHiddenAreaMeshType
{
	EHVRHiddenAreaMeshType_Standard = 0,
	EHVRHiddenAreaMeshType_Inverse = 1,
	EHVRHiddenAreaMeshType_LineLoop = 2,
	EHVRHiddenAreaMeshType_Max = 3,
}

public enum EHVRButtonId
{
	EHVRButtonId_System = 0,
	EHVRButtonId_ApplicationMenu = 1,
	EHVRButtonId_Grip = 2,
	EHVRButtonId_DPad_Left = 3,
	EHVRButtonId_DPad_Up = 4,
	EHVRButtonId_DPad_Right = 5,
	EHVRButtonId_DPad_Down = 6,
	EHVRButtonId_A = 7,
	EHVRButtonId_ProximitySensor = 31,
	EHVRButtonId_Axis0 = 32,
	EHVRButtonId_Axis1 = 33,
	EHVRButtonId_Axis2 = 34,
	EHVRButtonId_Axis3 = 35,
	EHVRButtonId_Axis4 = 36,
    EVRButton_Touchpad = 32,
    EVRButton_Trigger = 33,
    EVRButton_Dashboard_Back = 2,
    EHVRButtonId_Max = 64,
}

public enum EHVRControllerAxisType
{
	EHVRControllerAxisType_None = 0,
	EHVRControllerAxisType_TrackPad = 1,
	EHVRControllerAxisType_Joystick = 2,
	EHVRControllerAxisType_Trigger = 3,
}


    public enum HVROverlayTransformType
{
	HVROverlayTransformType_Absolute = 0,
	HVROverlayTransformType_TrackedDeviceRelative = 1,
	HVROverlayTransformType_SystemOverlay = 2,
	HVROverlayTransformType_TrackedComponent = 3,
}

public enum EHVREventType
{
	EHVREventType_None = 0,
	EHVREventType_TrackedDeviceActivated = 100,
	EHVREventType_TrackedDeviceDeactivated = 101,
	EHVREventType_TrackedDeviceUpdated = 102,
	EHVREventType_TrackedDeviceUserInteractionStarted = 103,
	EHVREventType_TrackedDeviceUserInteractionEnded = 104,
	EHVREventType_IpdChanged = 105,
	EHVREventType_EnterStandbyMode = 106,
	EHVREventType_LeaveStandbyMode = 107,
	EHVREventType_TrackedDeviceRoleChanged = 108,
	EHVREventType_WatchdogWakeUpRequested = 109,
	EHVREventType_LensDistortionChanged = 110,
	EHVREventType_PropertyChanged = 111,
	EHVREventType_ButtonPress = 200,
	EHVREventType_ButtonUnpress = 201,
	EHVREventType_ButtonTouch = 202,
	EHVREventType_ButtonUntouch = 203,
	EHVREventType_MouseMove = 300,
	EHVREventType_MouseButtonDown = 301,
	EHVREventType_MouseButtonUp = 302,
	EHVREventType_FocusEnter = 303,
	EHVREventType_FocusLeave = 304,
	EHVREventType_Scroll = 305,
	EHVREventType_TouchPadMove = 306,
	EHVREventType_OverlayFocusChanged = 307,
	EHVREventType_InputFocusCaptured = 400,
	EHVREventType_InputFocusReleased = 401,
	EHVREventType_SceneFocusLost = 402,
	EHVREventType_SceneFocusGained = 403,
	EHVREventType_SceneApplicationChanged = 404,
	EHVREventType_SceneFocusChanged = 405,
	EHVREventType_InputFocusChanged = 406,
	EHVREventType_SceneApplicationSecondaryRenderingStarted = 407,
	EHVREventType_HideRenderModels = 410,
	EHVREventType_ShowRenderModels = 411,
	EHVREventType_OverlayShown = 500,
	EHVREventType_OverlayHidden = 501,
	EHVREventType_DashboardActivated = 502,
	EHVREventType_DashboardDeactivated = 503,
	EHVREventType_DashboardThumbSelected = 504,
	EHVREventType_DashboardRequested = 505,
	EHVREventType_ResetDashboard = 506,
	EHVREventType_RenderToast = 507,
	EHVREventType_ImageLoaded = 508,
	EHVREventType_ShowKeyboard = 509,
	EHVREventType_HideKeyboard = 510,
	EHVREventType_OverlayGamepadFocusGained = 511,
	EHVREventType_OverlayGamepadFocusLost = 512,
	EHVREventType_OverlaySharedTextureChanged = 513,
	EHVREventType_DashboardGuideButtonDown = 514,
	EHVREventType_DashboardGuideButtonUp = 515,
	EHVREventType_ScreenshotTriggered = 516,
	EHVREventType_ImageFailed = 517,
	EHVREventType_DashboardOverlayCreated = 518,
	EHVREventType_RequestScreenshot = 520,
	EHVREventType_ScreenshotTaken = 521,
	EHVREventType_ScreenshotFailed = 522,
	EHVREventType_SubmitScreenshotToDashboard = 523,
	EHVREventType_ScreenshotProgressToDashboard = 524,
	EHVREventType_PrimaryDashboardDeviceChanged = 525,
	EHVREventType_Notification_Shown = 600,
	EHVREventType_Notification_Hidden = 601,
	EHVREventType_Notification_BeginInteraction = 602,
	EHVREventType_Notification_Destroyed = 603,
	EHVREventType_Quit = 700,
	EHVREventType_ProcessQuit = 701,
	EHVREventType_QuitAborted_UserPrompt = 702,
	EHVREventType_QuitAcknowledged = 703,
	EHVREventType_DriverRequestedQuit = 704,
	EHVREventType_ChaperoneDataHasChanged = 800,
	EHVREventType_ChaperoneUniverseHasChanged = 801,
	EHVREventType_ChaperoneTempDataHasChanged = 802,
	EHVREventType_ChaperoneSettingsHaveChanged = 803,
	EHVREventType_SeatedZeroPoseReset = 804,
	EHVREventType_AudioSettingsHaveChanged = 820,
	EHVREventType_BackgroundSettingHasChanged = 850,
	EHVREventType_CameraSettingsHaveChanged = 851,
	EHVREventType_ReprojectionSettingHasChanged = 852,
	EHVREventType_ModelSkinSettingsHaveChanged = 853,
	EHVREventType_EnvironmentSettingsHaveChanged = 854,
	EHVREventType_PowerSettingsHaveChanged = 855,
	EHVREventType_EnableHomeAppSettingsHaveChanged = 856,
	EHVREventType_StatusUpdate = 900,
	EHVREventType_MCImageUpdated = 1000,
	EHVREventType_FirmwareUpdateStarted = 1100,
	EHVREventType_FirmwareUpdateFinished = 1101,
	EHVREventType_KeyboardClosed = 1200,
	EHVREventType_KeyboardCharInput = 1201,
	EHVREventType_KeyboardDone = 1202,
	EHVREventType_ApplicationTransitionStarted = 1300,
	EHVREventType_ApplicationTransitionAborted = 1301,
	EHVREventType_ApplicationTransitionNewAppStarted = 1302,
	EHVREventType_ApplicationListUpdated = 1303,
	EHVREventType_ApplicationMimeTypeLoad = 1304,
	EHVREventType_ApplicationTransitionNewAppLaunchComplete = 1305,
	EHVREventType_ProcessConnected = 1306,
	EHVREventType_ProcessDisconnected = 1307,
	EHVREventType_Compositor_MirrorWindowShown = 1400,
	EHVREventType_Compositor_MirrorWindowHidden = 1401,
	EHVREventType_Compositor_ChaperoneBoundsShown = 1410,
	EHVREventType_Compositor_ChaperoneBoundsHidden = 1411,
	EHVREventType_TrackedCamera_StartVideoStream = 1500,
	EHVREventType_TrackedCamera_StopVideoStream = 1501,
	EHVREventType_TrackedCamera_PauseVideoStream = 1502,
	EHVREventType_TrackedCamera_ResumeVideoStream = 1503,
	EHVREventType_TrackedCamera_EditingSurface = 1550,
	EHVREventType_PerformanceTest_EnableCapture = 1600,
	EHVREventType_PerformanceTest_DisableCapture = 1601,
	EHVREventType_PerformanceTest_FidelityLevel = 1602,
	EHVREventType_MessageOverlay_Closed = 1650,
	EHVREventType_VendorSpecific_Reserved_Start = 10000,
	EHVREventType_VendorSpecific_Reserved_End = 19999,
}

public enum EHVRCompositorError
{
	EHVRCompositorError_None = 0,
	EHVRCompositorError_RequestFailed = 1,
	EHVRCompositorError_IncompatibleVersion = 100,
	EHVRCompositorError_DoNotHaveFocus = 101,
	EHVRCompositorError_InvalidTexture = 102,
	EHVRCompositorError_IsNotSceneApplication = 103,
	EHVRCompositorError_TextureIsOnWrongDevice = 104,
	EHVRCompositorError_TextureUsesUnsupportedFormat = 105,
	EHVRCompositorError_SharedTexturesNotSupported = 106,
	EHVRCompositorError_IndexOutOfRange = 107,
	EHVRCompositorError_AlreadySubmitted = 108,
	EHVRCompositorError_InvalidBounds = 109,
}

public enum EHVROverlayError
{
	EHVROverlayError_None = 0,
	EHVROverlayError_UnknownOverlay = 10,
	EHVROverlayError_InvalidHandle = 11,
	EHVROverlayError_PermissionDenied = 12,
	EHVROverlayError_OverlayLimitExceeded = 13,
	EHVROverlayError_WrongVisibilityType = 14,
	EHVROverlayError_KeyTooLong = 15,
	EHVROverlayError_NameTooLong = 16,
	EHVROverlayError_KeyInUse = 17,
	EHVROverlayError_WrongTransformType = 18,
	EHVROverlayError_InvalidTrackedDevice = 19,
	EHVROverlayError_InvalidParameter = 20,
	EHVROverlayError_ThumbnailCantBeDestroyed = 21,
	EHVROverlayError_ArrayTooSmall = 22,
	EHVROverlayError_RequestFailed = 23,
	EHVROverlayError_InvalidTexture = 24,
	EHVROverlayError_UnableToLoadFile = 25,
	EHVROverlayError_KeyboardAlreadyInUse = 26,
	EHVROverlayError_NoNeighbor = 27,
	EHVROverlayError_TooManyMaskPrimitives = 29,
	EHVROverlayError_BadMaskPrimitive = 30,
}

public enum EHVRTrackedCameraError
{
	EHVRTrackedCameraError_None = 0,
	EHVRTrackedCameraError_OperationFailed = 100,
	EHVRTrackedCameraError_InvalidHandle = 101,
	EHVRTrackedCameraError_InvalidFrameHeaderVersion = 102,
	EHVRTrackedCameraError_OutOfHandles = 103,
	EHVRTrackedCameraError_IPCFailure = 104,
	EHVRTrackedCameraError_NotSupportedForThisDevice = 105,
	EHVRTrackedCameraError_SharedMemoryFailure = 106,
	EHVRTrackedCameraError_FrameBufferingFailure = 107,
	EHVRTrackedCameraError_StreamSetupFailure = 108,
	EHVRTrackedCameraError_InvalidGLTextureId = 109,
	EHVRTrackedCameraError_InvalidSharedTextureHandle = 110,
	EHVRTrackedCameraError_FailedToGetGLTextureId = 111,
	EHVRTrackedCameraError_SharedTextureFailure = 112,
	EHVRTrackedCameraError_NoFrameAvailable = 113,
	EHVRTrackedCameraError_InvalidArgument = 114,
	EHVRTrackedCameraError_InvalidFrameBufferSize = 115,
}

public enum EHVRTrackedCameraFrameType
{
	EHVRTrackedCameraFrameType_Distorted = 0,
	EHVRTrackedCameraFrameType_Undistorted = 1,
	EHVRTrackedCameraFrameType_MaximumUndistorted = 2,
	EHVRTrackedCameraFrameType_MAX_CAMERA_FRAME_TYPES = 3,
}

public enum EHVRRenderModelError
{
    None = 0,
    Loading = 100,
    NotSupported = 200,
    InvalidArg = 300,
    InvalidModel = 301,
    NoShapes = 302,
    MultipleShapes = 303,
    TooManyVertices = 304,
    MultipleTextures = 305,
    BufferTooSmall = 306,
    NotEnoughNormals = 307,
    NotEnoughTexCoords = 308,
    InvalidTexture = 400,
}

public enum EHVRComponentProperty
{
    HVRComponentProperty_IsStatic = (1 << 0),
    HVRComponentProperty_IsVisible = (1 << 1),
    HVRComponentProperty_IsTouched = (1 << 2),
    HVRComponentProperty_IsPressed = (1 << 3),
    HVRComponentProperty_IsScrolled = (1 << 4),
}

public enum EHVRSettingsError
{
	EVRSettingsError_None = 0,
	EVRSettingsError_IPCFailed = 1,
	EVRSettingsError_WriteFailed = 2,
	EVRSettingsError_ReadFailed = 3,
	EVRSettingsError_JsonParseFailed = 4,
	EVRSettingsError_UnsetSettingHasNoDefault = 5,
}

public enum EHVRChaperoneConfigFile
{
	EHVRChaperoneConfigFile_Live = 1,
	EHVRChaperoneConfigFile_Temp = 2,
}

public enum EHVRErrorCode
{
	HVR_ERR_None = 0,
	HVR_ERR_Unknown = 1,
	HVR_ERR_Init_ServerInstallationNotFound = 100,
	HVR_ERR_Init_CompositorInstallationNotFound = 101,
	HVR_ERR_Init_MonitorInstallationNotFound = 102,
	HVR_ERR_Init_ServerProcessCannotBeLaunched = 103,
	HVR_ERR_Init_CompositorProcessCannotBeLaunched = 104,
	HVR_ERR_Init_MonitorProcessCannotBeLaunched = 105,
	HVR_ERR_Init_DriverDLLNotFound = 106,
	HVR_ERR_Init_FactoryNotFound = 107,
	HVR_ERR_Init_InterfaceNotFound = 108,
	HVR_ERR_Init_UserConfigDirectoryInvalid = 109,
	HVR_ERR_Init_HmdNotFound = 110,
	HVR_ERR_Init_NotInitialized = 111,
	HVR_ERR_Init_NoConfigPath = 112,
	HVR_ERR_Init_NoLogPath = 113,
	HVR_ERR_Init_ServerRegistryNotFound = 114,
	HVR_ERR_Init_CompositorRegistryNotFound = 115,
	HVR_ERR_Init_MonitorRegistryNotFound = 116,
	HVR_ERR_Init_SettingsInitFailed = 117,
	HVR_ERR_Init_ShuttingDown = 118,
	HVR_ERR_Init_VRMonitorNotFound = 119,
	HVR_ERR_Init_VRMonitorInitFailed = 120,
	HVR_ERR_Init_InvalidApplicationType = 121,
	HVR_ERR_Init_ServerInstanceInitFailed = 122,
	HVR_ERR_Init_CompositorInstanceInitFailed = 123,
	HVR_ERR_Init_MonitorInstanceInitFailed = 124,
	HVR_ERR_Init_RenderModelInstanceInitFailed = 125,
	HVR_ERR_Driver_Failed = 200,
	HVR_ERR_Driver_LoadFailed = 201,
	HVR_ERR_Driver_InitFailed = 202,
	HVR_ERR_Driver_InterfaceNotFound = 203,
	HVR_ERR_Driver_HmdUnknown = 204,
	HVR_ERR_Driver_RuntimeOutOfDate = 205,
	HVR_ERR_IPC_ServerDisconnect = 300,
	HVR_ERR_IPC_ServerInitFailed = 301,
	HVR_ERR_IPC_ServerConnectFailed = 302,
	HVR_ERR_IPC_CompositorInitFailed = 303,
	HVR_ERR_IPC_CompositorConnectFailed = 304,
	HVR_ERR_IPC_ConnectFailedAFterManyAttemps = 305,
	HVR_ERR_Compositor_Failed = 400,
	HVR_ERR_Compositor_GLFWInitFailed = 401,
	HVR_ERR_Compositor_GLEWInitFailed = 402,
	HVR_ERR_Compositor_DXInitFailed = 403,
	HVR_ERR_Compositor_InitFailed = 404,
	HVR_ERR_Compositor_D3D11HardwareRequired = 405,
	HVR_ERR_Compositor_DirectModeFailed = 406,
	HVR_ERR_Compositor_NVAPIFailed = 407,
	HVR_ERR_Compositor_AMDAPIFailed = 408,
	HVR_ERR_Interface_LoadCoreFailed = 501,
	HVR_ERR_SDK_RegistryNotFound = 901,
	HVR_ERR_SDK_LibsNotFound = 902,
	HVR_ERR_SDK_LibsError = 903
}

public enum HVRChaperoneCalibrationState
{
	HVRChaperoneCalibrationState_OK = 1,
	HVRChaperoneCalibrationState_Warning = 100,
	HVRChaperoneCalibrationState_Warning_BaseStationMayHaveMoved = 101,
	HVRChaperoneCalibrationState_Warning_BaseStationRemoved = 102,
	HVRChaperoneCalibrationState_Warning_SeatedBoundsInvalid = 103,
	HVRChaperoneCalibrationState_Error = 200,
	HVRChaperoneCalibrationState_Error_BaseStationUninitialized = 201,
	HVRChaperoneCalibrationState_Error_BaseStationConflict = 202,
	HVRChaperoneCalibrationState_Error_PlayAreaInvalid = 203,
	HVRChaperoneCalibrationState_Error_CollisionBoundsInvalid = 204,
}

public enum HVROverlayFlags
{
	HVROverlayFlags_None = 0,
	HVROverlayFlags_Curved = 1,
	HVROverlayFlags_RGSS4X = 2,
	HVROverlayFlags_NoDashboardTab = 3,
	HVROverlayFlags_AcceptsGamepadEvents = 4,
	HVROverlayFlags_ShowGamepadFocus = 5,
	HVROverlayFlags_SendVRScrollEvents = 6,
	HVROverlayFlags_SendVRTouchpadEvents = 7,
	HVROverlayFlags_ShowTouchPadScrollWheel = 8,
	HVROverlayFlags_TransferOwnershipToInternalProcess = 9,
	HVROverlayFlags_SideBySide_Parallel = 10,
	HVROverlayFlags_SideBySide_Crossed = 11,
	HVROverlayFlags_Panorama = 12,
	HVROverlayFlags_StereoPanorama = 13,
	HVROverlayFlags_SortWithNonSceneOverlays = 14,
	HVROverlayFlags_VisibleInDashboard = 15,
}

[StructLayout(LayoutKind.Explicit)] public struct HVREvent_Data_t
{
	[FieldOffset(0)] public HVREvent_Reserved_t reserved;
	[FieldOffset(0)] public HVREvent_Controller_t controller;
	[FieldOffset(0)] public HVREvent_Process_t process;
	[FieldOffset(0)] public HVREvent_Overlay_t overlay;
	[FieldOffset(0)] public HVREvent_Status_t status;
	[FieldOffset(0)] public HVREvent_Ipd_t ipd;
	[FieldOffset(0)] public HVREvent_Chaperone_t chaperone;
	[FieldOffset(0)] public HVREvent_TouchPadMove_t touchPadMove;
}

[StructLayout(LayoutKind.Sequential)] public struct HVRMatrix34_t
{
	public float m0; //float[3][4]
	public float m1;
	public float m2;
	public float m3;
	public float m4;
	public float m5;
	public float m6;
	public float m7;
	public float m8;
	public float m9;
	public float m10;
	public float m11;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRMatrix44_t
{
	public float m0; //float[4][4]
	public float m1;
	public float m2;
	public float m3;
	public float m4;
	public float m5;
	public float m6;
	public float m7;
	public float m8;
	public float m9;
	public float m10;
	public float m11;
	public float m12;
	public float m13;
	public float m14;
	public float m15;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRVector3_t
{
	public float v0; //float[3]
	public float v1;
	public float v2;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRVector4_t
{
	public float v0; //float[4]
	public float v1;
	public float v2;
	public float v3;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRVector3d_t
{
	public double v0; //double[3]
	public double v1;
	public double v2;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRVector2_t
{
	public float v0; //float[2]
	public float v1;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRQuaternion_t
{
	public double w;
	public double x;
	public double y;
	public double z;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRColor_t
{
	public float r;
	public float g;
	public float b;
	public float a;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRQuad_t
{
	public HVRVector3_t vCorners0;
	public HVRVector3_t vCorners1;
	public HVRVector3_t vCorners2;
	public HVRVector3_t vCorners3;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRRect2_t
{
	public HVRVector2_t vTopLeft;
	public HVRVector2_t vBottomRight;
}

[StructLayout(LayoutKind.Sequential)] public struct HVRTexture_t
{
	public ulong handle; // uint64
	public EHVRTextureType eType;
	public EHVRColorSpace eColorSpace;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRTrackedDevicePose_t
{
	public HVRMatrix34_t mDeviceToAbsoluteTracking;
	public HVRVector3_t vVelocity;
	public HVRVector3_t vAngularVelocity;
	public EHVRTrackingResult eTrackingResult;
	[MarshalAs(UnmanagedType.I1)]
	public bool bPoseIsValid;
	[MarshalAs(UnmanagedType.I1)]
	public bool bDeviceIsConnected;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRTextureBounds_t
{
	public float uMin;
	public float vMin;
	public float uMax;
	public float vMax;
}
[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Controller_t
{
	public uint button;
}

[StructLayout(LayoutKind.Sequential)] public struct HVREvent_TouchPadMove_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool bFingerDown;
	public float flSecondsFingerDown;
	public float fValueXFirst;
	public float fValueYFirst;
	public float fValueXRaw;
	public float fValueYRaw;
}

[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Process_t
{
	public uint pid;
	public uint oldPid;
	[MarshalAs(UnmanagedType.I1)]
	public bool bForced;
}
[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Overlay_t
{
	public ulong overlayHandle;
}
[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Status_t
{
	public uint statusState;
}

[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Ipd_t
{
	public float ipdMeters;
}
[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Chaperone_t
{
	public ulong m_nPreviousUniverse;
	public ulong m_nCurrentUniverse;
}
[StructLayout(LayoutKind.Sequential)] public struct HVREvent_Reserved_t
{
	public ulong reserved0;
	public ulong reserved1;
}

[StructLayout(LayoutKind.Sequential)] public struct HVREvent_t
{
	public uint eventType;
	public uint trackedDeviceIndex;
	public float eventAgeSeconds;
	public HVREvent_Data_t data;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRHiddenAreaMesh_t
{
	public IntPtr pVertexData;
	public uint unTriangleCount;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRControllerAxis_t
{
	public float x;
	public float y;
}
[StructLayout(LayoutKind.Sequential)] public struct HVRControllerState_t
{
	public uint unPacketNum;
	public ulong ulButtonPressed;
	public ulong ulButtonTouched;
	public HVRControllerAxis_t rAxis0;
	public HVRControllerAxis_t rAxis1;
	public HVRControllerAxis_t rAxis2;
	public HVRControllerAxis_t rAxis3;
	public HVRControllerAxis_t rAxis4;
}
[StructLayout(LayoutKind.Sequential)]
public struct HVROverlayIntersectionParams_t
{
    public HVRVector3_t vSource;
    public HVRVector3_t vDirection;
    public EHVRTrackingUniverseOrigin eOrigin;
}
[StructLayout(LayoutKind.Sequential)]
public struct VROverlayIntersectionResults_t
{
    public HVRVector3_t vPoint;
    public HVRVector3_t vNormal;
    public HVRVector2_t vUVs;
    public float fDistance;
}

[StructLayout(LayoutKind.Sequential)]
public struct HVRCameraVideoStreamFrameHeader_t
{
	public EHVRTrackedCameraFrameType eFrameType;
	public uint nWidth;
	public uint nHeight;
	public uint nBytesPerPixel;
	public uint nFrameSequence;
	public HVRTrackedDevicePose_t standingTrackedDevicePose;
}

[StructLayout(LayoutKind.Sequential)]
public struct RenderModel_ControllerMode_State_t
{
    [MarshalAs(UnmanagedType.I1)]
    public bool bScrollWheelVisible;
}
[StructLayout(LayoutKind.Sequential)]
public struct RenderModel_ComponentState_t
{
    public HVRMatrix34_t mTrackingToComponentRenderModel;
    public HVRMatrix34_t mTrackingToComponentLocal;
    public uint uProperties;
}

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderModel_TextureMap_t
    {
        public ushort unWidth;
        public ushort unHeight;
        public IntPtr rubTextureMapData; // const uint8_t*
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderModel_Vertex_t
    {
        public HVRVector3_t vPosition;
        public HVRVector3_t vNormal;
        public float rfTextureCoord0;
        public float rfTextureCoord1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderModel_t
    {
        public IntPtr rVertexData; // const HVRRenderModel_Vertex_t*
        public uint unVertexCount;
        public IntPtr rIndexData; // const uint16_t*
        public uint unTriangleCount;
        public uint diffuseTextureId;
    }

// This structure is for backwards binary compatibility on Linux and OSX only
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct VRControllerState_t_Packed
{
    public uint unPacketNum;
    public ulong ulButtonPressed;
    public ulong ulButtonTouched;
    public HVRControllerAxis_t rAxis0; //VRControllerAxis_t[5]
    public HVRControllerAxis_t rAxis1;
    public HVRControllerAxis_t rAxis2;
    public HVRControllerAxis_t rAxis3;
    public HVRControllerAxis_t rAxis4;
    public VRControllerState_t_Packed(HVRControllerState_t unpacked)
    {
        this.unPacketNum = unpacked.unPacketNum;
        this.ulButtonPressed = unpacked.ulButtonPressed;
        this.ulButtonTouched = unpacked.ulButtonTouched;
        this.rAxis0 = unpacked.rAxis0;
        this.rAxis1 = unpacked.rAxis1;
        this.rAxis2 = unpacked.rAxis2;
        this.rAxis3 = unpacked.rAxis3;
        this.rAxis4 = unpacked.rAxis4;
    }
    public void Unpack(ref HVRControllerState_t unpacked)
    {
        unpacked.unPacketNum = this.unPacketNum;
        unpacked.ulButtonPressed = this.ulButtonPressed;
        unpacked.ulButtonTouched = this.ulButtonTouched;
        unpacked.rAxis0 = this.rAxis0;
        unpacked.rAxis1 = this.rAxis1;
        unpacked.rAxis2 = this.rAxis2;
        unpacked.rAxis3 = this.rAxis3;
        unpacked.rAxis4 = this.rAxis4;
    }
}
public class HVRClass
{
	public static EHVRErrorCode InitInternal(EHVRApplicationType eApplicationType)
	{
		return HVRInterop.HVR_InitInternal(eApplicationType);
	}

	public static void ShutdownInternal()
	{
		HVRInterop.HVR_ShutdownInternal();
	}

	public static IntPtr GetGenericInterface(string pchInterfaceVersion, ref EHVRErrorCode peError)
	{
		return HVRInterop.HVR_GetGenericInterface(pchInterfaceVersion, ref peError);
	}

	public const string IHVRSystem_Version = "IHVRSYSTEM_001";
	public const string IHVRCompositor_Version = "IHVRCOMPOSITOR_001";
	public const string IHVROverlay_Version = "IHVROVERLAY_001";
	public const string IHVRTrackedCamera_Version = "IHVRTRACKEDCAMERA_001";
	public const string IHVRSettings_Version = "IHVRSETTINGS_001";
	public const string IHVRChaperoneSetup_Version = "IHVRCHAPERONESETUP_001";
    public const string IHVRRenderModel_Version = "IHVRENDERMODEL_001";
    
	public const uint k_unHVRTrackedDeviceIndex_Hmd = 0;
	public const uint k_unHVRMaxTrackedDeviceCount = 16;
    public const uint k_unHVRTrackedDeviceIndexInvalid = 4294967295;
    public const ulong k_ulHVROverlayHandleInvalid = 0;

    class CHVRContext
	{
		public CHVRContext() { Clear(); }

		public void Clear()
		{
			m_pHVRSystem = null;
			m_pHVRCompositor = null;
			m_pHVROverlay = null;
			m_pHVRTrackedCamera = null;
			m_pHVRSettings = null;
			m_pHVRChaperoneSetup = null;
            m_pHVRRendeModel = null;
		}

		public CHVRSystem HVRSystem()
		{
			if (m_pHVRSystem == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRSystem_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVRSystem = new CHVRSystem(pInterface);
			}
			return m_pHVRSystem;
		}

		public CHVRCompositor HVRCompositor()
		{
			if (m_pHVRCompositor == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRCompositor_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVRCompositor = new CHVRCompositor(pInterface);
			}
			return m_pHVRCompositor;
		}

		public CHVROverlay HVROverlay()
		{
			if (m_pHVROverlay == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVROverlay_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVROverlay = new CHVROverlay(pInterface);
			}
			return m_pHVROverlay;
		}
		
		public CHVRTrackedCamera HVRTrackedCamera()
		{
			if (m_pHVRTrackedCamera == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRTrackedCamera_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVRTrackedCamera = new CHVRTrackedCamera(pInterface);
			}
			return m_pHVRTrackedCamera;
		}
		
		public CHVRSettings HVRSettings()
		{
			if (m_pHVRSettings == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRSettings_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVRSettings = new CHVRSettings(pInterface);
			}
			return m_pHVRSettings;
		}

		public CHVRChaperoneSetup HVRChaperoneSetup()
		{
			if (m_pHVRChaperoneSetup == null)
			{
				var eError = EHVRErrorCode.HVR_ERR_None;
				var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRChaperoneSetup_Version, ref eError);
				if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
					m_pHVRChaperoneSetup = new CHVRChaperoneSetup(pInterface);
			}
			return m_pHVRChaperoneSetup;
		}

            public CHVRRenderModels HVRRenderModelSetup()
            {
                if (m_pHVRRendeModel == null)
                {
                    var eError = EHVRErrorCode.HVR_ERR_None;
                    var pInterface = HVRInterop.HVR_GetGenericInterface(IHVRRenderModel_Version, ref eError);
                    if (pInterface != IntPtr.Zero && eError == EHVRErrorCode.HVR_ERR_None)
                        m_pHVRRendeModel = new CHVRRenderModels(pInterface);
                }
                return m_pHVRRendeModel;
            }

		private CHVRSystem m_pHVRSystem;
		private CHVRCompositor m_pHVRCompositor;
		private CHVROverlay m_pHVROverlay;
		private CHVRTrackedCamera m_pHVRTrackedCamera;
		private CHVRSettings m_pHVRSettings;
		private CHVRChaperoneSetup m_pHVRChaperoneSetup;
		private CHVRRenderModels m_pHVRRendeModel;
	};

	private static CHVRContext _HVRInternal_ModuleContext = null;
	static CHVRContext HVRInternal_ModuleContext
	{
		get
		{
			if (_HVRInternal_ModuleContext == null)
				_HVRInternal_ModuleContext = new CHVRContext();
			return _HVRInternal_ModuleContext;
		}
	}

	public static CHVRSystem System { get { return HVRInternal_ModuleContext.HVRSystem(); } }
	public static CHVRCompositor Compositor { get { return HVRInternal_ModuleContext.HVRCompositor(); } }
	public static CHVROverlay Overlay { get { return HVRInternal_ModuleContext.HVROverlay(); } }
	public static CHVRTrackedCamera TrackedCamera { get { return HVRInternal_ModuleContext.HVRTrackedCamera(); } }
	public static CHVRSettings Settings { get { return HVRInternal_ModuleContext.HVRSettings(); } }
	public static CHVRChaperoneSetup ChaperoneSetup { get { return HVRInternal_ModuleContext.HVRChaperoneSetup(); } }
	public static CHVRRenderModels RenderModels { get { return HVRInternal_ModuleContext.HVRRenderModelSetup(); } }
	
	/** Finds the active installation of vrclient.dll and initializes it */
	public static CHVRSystem Init(ref EHVRErrorCode peError, EHVRApplicationType eApplicationType = EHVRApplicationType.EHVRApplication_Scene)
	{
		peError = InitInternal(eApplicationType);
		HVRInternal_ModuleContext.Clear();

		if (peError !=  EHVRErrorCode.HVR_ERR_None)
			return null;

		return HVRClass.System;
	}

	/** unloads vrclient.dll. Any interface pointers from the interface are
	* invalid after this point */
	public static void Shutdown()
	{
		ShutdownInternal();
	}

}

}
