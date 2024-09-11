using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;
using static UnityEngine.Camera;
using Basis.Scripts.Drivers;
using System;
[ExecuteInEditMode]
public class BasisSDKMirror : MonoBehaviour
{
    [Header("Main Settings")]
    public Vector3 projectionDirection = -Vector3.forward;
    public int m_TextureSize = 2048;

    [Header("Advanced Settings")]
    public float m_ClipPlaneOffset = 0.001f;
    public float nearClipLimit = 0.01f;
    public float FarClipPlane = 25f;
    public bool DisablePixelLights = true;
    public static bool InsideRendering = false;
    public RenderTexture PortalTextureLeft = null;
    public RenderTexture PortalTextureRight = null;
    public int Antialising = 4;
    public Matrix4x4 reflectionMatrix;
    public Material MirrorsMaterial;

    public Camera LeftCamera;
    public Camera RightCamera;
    public Action OnCamerasRenderering;
    public Action OnCamerasFinished;
    public Vector3 ThisPosition;

    public Matrix4x4 projectionMatrix;
    public Vector3 normal;
    public Vector4 reflectionPlane;
    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += UpdateCamera;
        BasisLocalCameraDriver.InstanceExists += ReInitalizeEvents;

        ReInitalizeEvents();
    }
    public void ReInitalizeEvents()
    {
        if (BasisLocalCameraDriver.Instance != null)
        {
            OnCamerasRenderering += BasisLocalCameraDriver.Instance.ScaleHeadToNormal;
            OnCamerasFinished += BasisLocalCameraDriver.Instance.ScaleheadToZero;
        }
    }
    private void OnDisable()
    {
        Cleanup();
    }
    public void OnDestroy()
    {
        Cleanup();
    }
    public void Cleanup()
    {
        RenderPipeline.beginCameraRendering -= UpdateCamera;
        if (PortalTextureLeft != null)
        {
            DestroyImmediate(PortalTextureLeft);
            PortalTextureLeft = null;
        }

        if (PortalTextureRight != null)
        {
            DestroyImmediate(PortalTextureRight);
            PortalTextureRight = null;
        }
        if (BasisLocalCameraDriver.Instance != null)
        {
            OnCamerasRenderering -= BasisLocalCameraDriver.Instance.ScaleHeadToNormal;
            OnCamerasFinished -= BasisLocalCameraDriver.Instance.ScaleheadToZero;
        }
    }
    private void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        bool IsBasisMainCamera = camera.GetInstanceID() == BasisLocalCameraDriver.CameraInstanceID;
        if (IsBasisMainCamera)
        {
            OnCamerasRenderering?.Invoke();

            ThisPosition = transform.position;
            projectionMatrix = camera.projectionMatrix;
            normal = transform.TransformDirection(projectionDirection).normalized;
            CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);
            reflectionPlane = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, ThisPosition) - m_ClipPlaneOffset);
        }

        if (camera.cameraType == CameraType.Game)
        {
            UpdateCameraState(SRC, camera);
        }
#if UNITY_EDITOR
        else if (camera.cameraType == CameraType.SceneView)
        {
            UpdateCameraState(SRC, camera);
        }
#endif
        if (IsBasisMainCamera)
        {
            OnCamerasFinished?.Invoke();
        }
    }

    private void UpdateCameraState(ScriptableRenderContext SRC, Camera camera)
    {
        // Safeguard from recursive reflections.  
        if (InsideRendering)
        {
            return;
        }
        InsideRendering = true;
        RenderCamera(camera, StereoscopicEye.Left, SRC);

        if (XRSettings.enabled)
        {
            RenderCamera(camera, StereoscopicEye.Right, SRC);
        }

        InsideRendering = false;
    }
    public void CreateOrGetCamera(Camera currentCamera, out Camera portalCamera, bool isLeft)
    {
        if (isLeft)
        {
            if(LeftCamera == null)
            {
                CreateNewCamera(currentCamera, out LeftCamera);
            }
            portalCamera = LeftCamera;
        }
        else
        {
            if (RightCamera == null)
            {
                CreateNewCamera(currentCamera, out RightCamera);
            }
            portalCamera = RightCamera;
        }
    }
    private void RenderCamera(Camera camera, StereoscopicEye eye, ScriptableRenderContext SRC)
    {
        Camera portalCamera;
        RenderTexture portalTexture;

        if (eye == StereoscopicEye.Left)
        {
            CreatePortalCamera(camera, eye, out portalCamera, ref PortalTextureLeft);
            portalTexture = PortalTextureLeft;
        }
        else
        {
            CreatePortalCamera(camera, eye, out portalCamera, ref PortalTextureRight);
            portalTexture = PortalTextureRight;
        }
        SetupReflection(camera, portalCamera, eye);

        portalCamera.targetTexture = portalTexture;
        GL.invertCulling = true;
#pragma warning disable CS0618
        UniversalRenderPipeline.RenderSingleCamera(SRC, portalCamera);
#pragma warning restore CS0618
        GL.invertCulling = false;
    }
    private void SetupReflection(Camera srcCamera, Camera destCamera, StereoscopicEye eye)
    {
        Vector3 eyeOffset = XRSettings.enabled ? GetEyePosition(eye) : Vector3.zero;
        eyeOffset.z = 0.0f;
        Vector3 oldEyePos = srcCamera.transform.position + srcCamera.transform.TransformVector(eyeOffset);
        Vector3 newEyePos = reflectionMatrix.MultiplyPoint(oldEyePos);

        destCamera.transform.position = newEyePos;
        destCamera.worldToCameraMatrix = srcCamera.worldToCameraMatrix * reflectionMatrix;
        Vector4 clipPlane = CameraSpacePlane(destCamera.worldToCameraMatrix, ThisPosition, normal);
        MakeProjectionMatrixOblique(ref projectionMatrix, clipPlane);
        destCamera.projectionMatrix = projectionMatrix;
    }
    private Vector3 GetEyePosition(StereoscopicEye eye)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(eye == StereoscopicEye.Left ? XRNode.LeftEye : XRNode.RightEye);
        if (device.isValid && device.TryGetFeatureValue(eye == StereoscopicEye.Left ? CommonUsages.leftEyePosition : CommonUsages.rightEyePosition, out Vector3 position))
        {
            return position;
        }
        return BasisLocalCameraDriver.Position();
    }
    private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign = 1.0f)
    {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
        Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
    // Extended sign: returns -1, 0 or 1 based on sign of a
    private static float Sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }
    // taken from http://www.terathon.com/code/oblique.html
    private static void MakeProjectionMatrixOblique(ref Matrix4x4 matrix, Vector4 clipPlane)
    {
        Vector4 q;
        // Calculate the clip-space corner point opposite the clipping plane
        // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
        // transform it into camera space by multiplying it
        // by the inverse of the projection matrix
        q.x = (Sgn(clipPlane.x) + matrix[8]) / matrix[0];
        q.y = (Sgn(clipPlane.y) + matrix[9]) / matrix[5];
        q.z = -1.0F;
        q.w = (1.0F + matrix[10]) / matrix[14];
        // Calculate the scaled plane vector
        Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));
        // Replace the third row of the projection matrix
        matrix[2] = c.x;
        matrix[6] = c.y;
        matrix[10] = c.z + 1.0F;
        matrix[14] = c.w;
    }
    private void CreatePortalCamera(Camera currentCamera, StereoscopicEye eye, out Camera portalCamera, ref RenderTexture portalTexture)
    {
        if (portalTexture == null)
        {
            Debug.Log("creating Textures");
            if (portalTexture != null)
            {
                DestroyImmediate(portalTexture);
            }
            portalTexture = new RenderTexture(m_TextureSize, m_TextureSize, 24)
            {
                name = "__MirrorReflection" + eye.ToString() + GetInstanceID(),
                isPowerOfTwo = true,
                antiAliasing = Antialising
            };
            string Property = "_ReflectionTex" + eye.ToString();
            if (MirrorsMaterial != null)
            {
                MirrorsMaterial.SetTexture(Property, portalTexture);
            }
        }
        CreateOrGetCamera(currentCamera, out portalCamera, eye == StereoscopicEye.Left);
    }
    private Camera CreateNewCamera(Camera currentCamera, out Camera newCamera)
    {
        GameObject go = new GameObject("Mirror Reflection Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera));
        go.transform.SetParent(transform);

        go.TryGetComponent(out newCamera);
        newCamera.enabled = false;
        newCamera.clearFlags = currentCamera.clearFlags;
        newCamera.backgroundColor = currentCamera.backgroundColor;
        newCamera.farClipPlane = FarClipPlane;
        newCamera.nearClipPlane = currentCamera.nearClipPlane;
        newCamera.orthographic = currentCamera.orthographic;
        newCamera.fieldOfView = currentCamera.fieldOfView;
        newCamera.aspect = currentCamera.aspect;
        newCamera.orthographicSize = currentCamera.orthographicSize;
        newCamera.depth = 2;

        return newCamera;
    }
}