using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

public class BasisSceneFactory : MonoBehaviour
{
    public BasisScene BasisScene;
    public AudioMixerGroup WorldDefaultMixer;
    public static BasisSceneFactory Instance;
    public void Awake()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        BasisScene.Ready.AddListener(Initalize);
    }
    public void Initalize(BasisScene scene)
    {
        BasisScene = scene;
        AttachMixerToAllSceneAudioSources();
        RespawnCheckTimer = BasisScene.RespawnCheckTimer;
        RespawnHeight = BasisScene.RespawnHeight;
        if (scene.MainCamera != null)
        {
            LoadCameraPropertys(scene.MainCamera);
            GameObject.DestroyImmediate(scene.MainCamera.gameObject);
            Debug.Log("Destroying Main Camera Attached To Scene");
        }
        else
        {
            Debug.Log("No attached camera to scene script Found");
        }
        List<GameObject> MainCameras = new List<GameObject>();
        GameObject.FindGameObjectsWithTag("MainCamera", MainCameras);
        foreach (GameObject PotentialCamera in MainCameras)
        {
            if (PotentialCamera.TryGetComponent(out Camera camera))
            {
                if (camera != BasisLocalCameraDriver.Instance.Camera)
                {
                    LoadCameraPropertys(camera);
                    GameObject.DestroyImmediate(camera.gameObject);
                }
                else
                {
                    Debug.Log("No New main Camera Found");
                }
            }
        }
        BasisLocalPlayer = BasisLocalPlayer.Instance;
    }
    public BasisLocalPlayer BasisLocalPlayer;
    public void LoadCameraPropertys(Camera Camera)
    {
        Camera RealCamera = BasisLocalCameraDriver.Instance.Camera;
        RealCamera.useOcclusionCulling = Camera.useOcclusionCulling;
        RealCamera.backgroundColor = Camera.backgroundColor;
        RealCamera.barrelClipping = Camera.barrelClipping;
        RealCamera.usePhysicalProperties = Camera.usePhysicalProperties;
        RealCamera.farClipPlane = Camera.farClipPlane;
        RealCamera.nearClipPlane = Camera.nearClipPlane;

        if (Camera.TryGetComponent(out UniversalAdditionalCameraData AdditionalCameraData))
        {
            UniversalAdditionalCameraData Data = BasisLocalCameraDriver.Instance.CameraData;

           Data.stopNaN = AdditionalCameraData.stopNaN;
            Data.dithering = AdditionalCameraData.dithering;

           Data.volumeTrigger = AdditionalCameraData.volumeTrigger;
        }
    }
    public void AttachMixerToAllSceneAudioSources()
    {
        // Check if mixerGroup is assigned
        if (BasisScene.Group == null)
        {
            BasisScene.Group = WorldDefaultMixer;
        }


        // Get all active and inactive AudioSources in the scene
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int AudioSourceCount = sources.Length;
        // Loop through each AudioSource and assign the mixer group if not already assigned
        for (int Index = 0; Index < AudioSourceCount; Index++)
        {
            AudioSource source = sources[Index];
            if (source != null && source.outputAudioMixerGroup == null)
            {
                source.outputAudioMixerGroup = BasisScene.Group;
            }
        }

        Debug.Log("Mixer group assigned to all scene AudioSources.");
    }
    public void SpawnPlayer(BasisLocalPlayer Basis)
    {
        Debug.Log("Spawning Player");
        RequestSpawnPoint(out Vector3 position, out Quaternion rotation);
        if (Basis != null)
        {
            Basis.Teleport(position, rotation);
        }
    }
    private float timeSinceLastCheck = 0f;
    public float RespawnCheckTimer = 5f;
    public float RespawnHeight = -100f;
    public void FixedUpdate()
    {
        timeSinceLastCheck += Time.deltaTime;
        // Check only if enough time has passed
        if (timeSinceLastCheck > RespawnCheckTimer)
        {
            timeSinceLastCheck = 0f; // Reset timer
            if (BasisLocalPlayer != null && BasisLocalPlayer.transform.position.y < RespawnHeight)
            {
                SpawnPlayer(BasisLocalPlayer);
            }
        }
    }
    public void RequestSpawnPoint(out Vector3 Position, out Quaternion Rotation)
    {
        if (BasisScene != null)
        {
            if (BasisScene.SpawnPoint == null)
            {
                this.transform.GetPositionAndRotation(out Position, out Rotation);
            }
            else
            {
                BasisScene.SpawnPoint.GetPositionAndRotation(out Position, out Rotation);
            }
        }
        else
        {
            Debug.LogError("Missing BasisScene!");
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }
    }
}