using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;
using static UnityEngine.Camera;
using Basis.Scripts.Drivers;
using System;
using Basis.Scripts.BasisSdk.Helpers;
public class BasisSDKMirror : MonoBehaviour
{
    public Renderer Renderer;//only renders when this is visible
    public BasisMeshRendererCheck BasisMeshRendererCheck;
    public bool IsAbleToRender = false;
    public float m_ClipPlaneOffset = 0.001f;
    public Vector3 ThisPosition;
    public Action OnCamerasRenderering;
    public Matrix4x4 projectionMatrix;
    public Vector3 normal;
    public Vector4 reflectionPlane;
    public Vector3 projectionDirection = -Vector3.forward;
    public Matrix4x4 reflectionMatrix;
    public static bool InsideRendering = false;
    public Action OnCamerasFinished;
    public float nearClipLimit = 0.01f;
    public float FarClipPlane = 25f;

    public Camera LeftCamera;
    public Camera RightCamera;

    public RenderTexture PortalTextureLeft;
    public RenderTexture PortalTextureRight;

    public int XSize = 2048;
    public int YSize = 2048;
    public int Antialising = 4;
    public Material MirrorsMaterial;
    public bool IsActive;
    public int depth = 24;

    public bool allowXRRendering = true;
    public bool RenderPostProcessing = false;
    public void Awake()
    {
        IsActive = false;
        IsAbleToRender = false;
        BasisMeshRendererCheck = BasisHelpers.GetOrAddComponent<BasisMeshRendererCheck>(this.Renderer.gameObject);
        BasisMeshRendererCheck.Check += VisibilityFlag;
    }
    public void OnEnable()
    {
        if (BasisLocalCameraDriver.HasInstance)
        {
            Initalize();
        }
        BasisLocalCameraDriver.InstanceExists += Initalize;
        RenderPipeline.beginCameraRendering += UpdateCamera;
    }
    public void OnDisable()
    {
        if (PortalTextureLeft != null)
        {
            DestroyImmediate(PortalTextureLeft);
        }
        if (PortalTextureRight != null)
        {
            DestroyImmediate(PortalTextureRight);
        }
        if (LeftCamera != null)
        {
            Destroy(LeftCamera.gameObject);
        }
        if (RightCamera != null)
        {
            Destroy(RightCamera.gameObject);
        }
        BasisLocalCameraDriver.InstanceExists -= Initalize;
        RenderPipeline.beginCameraRendering -= UpdateCamera;
    }
    public void Initalize()
    {
        Camera Camera = BasisLocalCameraDriver.Instance.Camera;
        CreatePortalCamera(Camera, StereoscopicEye.Left, ref LeftCamera, ref PortalTextureLeft);
        CreatePortalCamera(Camera, StereoscopicEye.Right, ref RightCamera, ref PortalTextureRight);
        IsAbleToRender = Renderer.isVisible;
        IsActive = true;
        InsideRendering = false;
    }

    private void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        if (IsAbleToRender == false && IsActive == false)
        {
            return;
        }
        if (IsCameraAble(camera))
        {
            OnCamerasRenderering?.Invoke();
            ThisPosition = Renderer.transform.position;
            projectionMatrix = camera.projectionMatrix;
            normal = Renderer.transform.TransformDirection(projectionDirection).normalized;
            reflectionPlane = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, ThisPosition) - m_ClipPlaneOffset);
            BasisHelpers.CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);
            UpdateCameraState(SRC, camera);
            OnCamerasFinished?.Invoke();
        }
    }
    public bool IsCameraAble(Camera camera)
    {
#if UNITY_EDITOR
        bool IsCameraSceneView = camera.cameraType == CameraType.SceneView;
        if (IsCameraSceneView)
        {
            return true;
        }
#endif
        bool IsBasisMainCamera = camera.GetInstanceID() == BasisLocalCameraDriver.CameraInstanceID;
        if (IsBasisMainCamera)
        {
            return true;
        }
        return false;
    }
    private void UpdateCameraState(ScriptableRenderContext SRC, Camera camera)
    {
        // Debug.Log("UpdateCameraState");
        // Safeguard from recursive reflections.  
        if (InsideRendering)
        {
            return;
        }
        //  Debug.Log("Passed InsideRendering");
        InsideRendering = true;
        RenderCamera(camera, StereoscopicEye.Left, SRC);
        RenderCamera(camera, StereoscopicEye.Right, SRC);//for testing purposes.
        // if (XRSettings.enabled)
        // {
        //     RenderCamera(camera, StereoscopicEye.Right, SRC);
        // }

        InsideRendering = false;
    }
    private void RenderCamera(Camera camera, StereoscopicEye eye, ScriptableRenderContext SRC)
    {
        //  Debug.Log("Rendering Camera");
        Camera portalCamera;
        RenderTexture portalTexture;

        if (eye == StereoscopicEye.Left)
        {
            portalTexture = PortalTextureLeft;
            portalCamera = LeftCamera;
        }
        else
        {
            portalTexture = PortalTextureRight;
            portalCamera = RightCamera;
        }
        SetupReflection(camera, portalCamera, eye);
        GL.invertCulling = true;
#pragma warning disable CS0618
        UniversalRenderPipeline.RenderSingleCamera(SRC, portalCamera);
#pragma warning restore CS0618
        GL.invertCulling = false;
    }
    private void SetupReflection(Camera srcCamera, Camera destCamera, StereoscopicEye eye)
    {
        // Get the correct eye offset (difference between left/right eye positions)
        Vector3 eyeOffset = GetEyePosition(eye);

        // Calculate the original eye position in world space
        Vector3 oldEyePos = srcCamera.transform.position + srcCamera.transform.TransformVector(eyeOffset);

        // Reflect the old eye position using the reflection matrix
        Vector3 newEyePos = reflectionMatrix.MultiplyPoint(oldEyePos);

        // Set the new eye position for the reflection camera
        destCamera.transform.position = newEyePos;

        // Ensure the reflection camera does not inherit the head's rotation
        // Reflect the forward and up vectors, and construct the rotation manually
        Vector3 forward = srcCamera.transform.forward;
        Vector3 up = srcCamera.transform.up;

        Vector3 reflectedForward = reflectionMatrix.MultiplyVector(forward);
        Vector3 reflectedUp = reflectionMatrix.MultiplyVector(up);

        // Set the camera's rotation manually using the reflected forward and up vectors
        destCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // Calculate the correct reflection matrix for the camera's position and orientation
        Matrix4x4 reflectionWorldToCamera = srcCamera.worldToCameraMatrix * reflectionMatrix;

        // Set the worldToCameraMatrix for the reflection camera
        destCamera.worldToCameraMatrix = reflectionWorldToCamera;

        // Calculate the clip plane for the reflection camera
        Vector4 clipPlane = BasisHelpers.CameraSpacePlane(reflectionWorldToCamera, ThisPosition, normal, m_ClipPlaneOffset);

        // Modify the projection matrix for oblique near-plane clipping
        Matrix4x4 projection = srcCamera.projectionMatrix;
        BasisHelpers.CalculateObliqueMatrix(ref projection, clipPlane);

        // Apply the new projection matrix to the reflection camera
        destCamera.projectionMatrix = projection;
    }
    private Vector3 GetEyePosition(StereoscopicEye eye)
    {
        if (eye == StereoscopicEye.Left)
        {
            return BasisLocalCameraDriver.LeftEyePosition();
        }
        else
        {
            return BasisLocalCameraDriver.RightEyePosition();
        }
    }
    private void CreatePortalCamera(Camera camera, StereoscopicEye eye, ref Camera portalCamera, ref RenderTexture PortalTexture)
    {
        //  Debug.Log("creating Textures");
        PortalTexture = new RenderTexture(XSize, YSize, depth)
        {
            name = "__MirrorReflection" + eye.ToString() + GetInstanceID(),
            isPowerOfTwo = true,
            antiAliasing = Antialising
        };
        string Property = "_ReflectionTex" + eye.ToString();
        MirrorsMaterial.SetTexture(Property, PortalTexture);
        CreateNewCamera(camera, out portalCamera);
        portalCamera.targetTexture = PortalTexture;
    }
    private void CreateNewCamera(Camera currentCamera, out Camera newCamera)
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
        if (newCamera.TryGetComponent(out UniversalAdditionalCameraData CameraData))
        {
            CameraData.allowXRRendering = allowXRRendering;
            CameraData.renderPostProcessing = RenderPostProcessing;
        }
    }
    private void VisibilityFlag(bool IsVisible)
    {
        IsAbleToRender = IsVisible;
    }
}