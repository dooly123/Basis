using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;
using static UnityEngine.Camera;
using Basis.Scripts.Drivers;
using System;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.Device_Management;
using System.Collections;
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

    public IEnumerator Start()
    {
        yield return new WaitUntil(() => BasisDeviceManagement.Instance);
        BasisDeviceManagement.Instance.OnBootModeChanged += BootModeChanged;
    }

    public void OnDestroy()
    {
        BasisDeviceManagement.Instance.OnBootModeChanged -= BootModeChanged;
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

    private void BootModeChanged(string obj)
    {
        StartCoroutine(BootModeChangedCoroutine(obj));
    }

    private IEnumerator BootModeChangedCoroutine(string obj)
    {
        yield return null;
        OnDisable();
        OnEnable();
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
            BasisLocalCameraDriver.Instance.ScaleHeadToNormal();
            ThisPosition = Renderer.transform.position;
            projectionMatrix = camera.projectionMatrix;
            normal = Renderer.transform.TransformDirection(projectionDirection);
            UpdateCameraState(SRC, camera);
            OnCamerasFinished?.Invoke();
            BasisLocalCameraDriver.Instance.ScaleheadToZero();
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
        if (camera.stereoEnabled)
        {
            RenderCamera(camera, MonoOrStereoscopicEye.Left, SRC);
            RenderCamera(camera, MonoOrStereoscopicEye.Right, SRC);
        }
        else
        {
            RenderCamera(camera, MonoOrStereoscopicEye.Mono, SRC);
        }

        InsideRendering = false;
    }

    private void RenderCamera(Camera camera, MonoOrStereoscopicEye eye, ScriptableRenderContext SRC)
    {
        //  Debug.Log("Rendering Camera");
        Camera portalCamera;
        RenderTexture portalTexture;

        if (eye != MonoOrStereoscopicEye.Right)
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
#pragma warning disable CS0618
        UniversalRenderPipeline.RenderSingleCamera(SRC, portalCamera);
#pragma warning restore CS0618
    }
    private void SetupReflection(Camera srcCamera, Camera destCamera, MonoOrStereoscopicEye eye)
    {
        Vector3 eyeOffset = eye == MonoOrStereoscopicEye.Mono ? srcCamera.transform.position : srcCamera.GetStereoViewMatrix((StereoscopicEye)eye).inverse.MultiplyPoint(Vector3.zero);

        Vector3 Position = Vector3.Reflect(transform.InverseTransformPoint(eyeOffset), Vector3.forward);
        Quaternion Rotation = Quaternion.LookRotation(Vector3.Reflect(transform.InverseTransformDirection(srcCamera.transform.rotation * Vector3.forward), Vector3.forward), Vector3.Reflect(transform.InverseTransformDirection(srcCamera.transform.rotation * Vector3.up), Vector3.forward));
        destCamera.transform.SetLocalPositionAndRotation(Position, Rotation);
        // Calculate the clip plane for the reflection camera
        Vector4 clipPlane = BasisHelpers.CameraSpacePlane(destCamera.worldToCameraMatrix, ThisPosition, normal, m_ClipPlaneOffset);
        clipPlane.x *= -1; // Applied to projection matrix with flipped x

        // Modify the projection matrix for oblique near-plane clipping
        Matrix4x4 projectionMatrix = eye == MonoOrStereoscopicEye.Mono ? srcCamera.projectionMatrix : srcCamera.GetStereoProjectionMatrix((StereoscopicEye)eye);
        BasisHelpers.CalculateObliqueMatrix(ref projectionMatrix, clipPlane);
        // Chirality hack: Flip X on the projection matrix, since the shader inverts the uv.x
        projectionMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
        destCamera.projectionMatrix = projectionMatrix;
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
        Renderer.material = MirrorsMaterial;
        Renderer.sharedMaterial.SetTexture(Property, PortalTexture);
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