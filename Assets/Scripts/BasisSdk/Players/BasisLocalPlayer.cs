using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System;
using Basis.Scripts.Drivers;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.Device_Management.Devices.Desktop;
using Basis.Scripts.Device_Management;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.Common;
using Basis.Scripts.Avatar;
namespace Basis.Scripts.BasisSdk.Players
{
public class BasisLocalPlayer : BasisPlayer
{
    public static float DefaultPlayerEyeHeight = 1.64f;
    public static float DefaultAvatarEyeHeight = 1.64f;
    public float PlayerEyeHeight = 1.64f;

    public float RatioPlayerToAvatarScale = 1f;
    public float RatioPlayerToEyeDefaultScale = 1f;
    public float RatioAvatarToAvatarEyeDefaultScale = 1f;
    public static BasisLocalPlayer Instance;
    public static Action OnLocalPlayerCreatedAndReady;
    public static Action OnLocalPlayerCreated;
        public Basis.Scripts.BasisCharacterController.BasisCharacterController Move;
    public event Action OnLocalAvatarChanged;
    public event Action OnSpawnedEvent;
    public event Action OnPlayersHeightChanged;
    public BasisLocalBoneDriver LocalBoneDriver;
    public BasisBoneControl Hips;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisFootPlacementDriver FootPlacementDriver;
    public BasisVisemeDriver VisemeDriver;
    [SerializeField]
    public LayerMask GroundMask;
    public static string LoadFileName = "LastUsedAvatar.BAS";
    public bool HasEvents = false;
    public MicrophoneRecorder MicrophoneRecorder;
    public async Task LocalInitialize()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        Instance = this;
        OnLocalPlayerCreated?.Invoke();
        IsLocal = true;
        LocalBoneDriver.CreateInitialArrays(LocalBoneDriver.transform);
        await BasisLocalInputActions.CreateInputAction(this);
        await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Main Camera.prefab", new InstantiationParameters());
          FootPlacementDriver = BasisHelpers.GetOrAddComponent<BasisFootPlacementDriver>(this.gameObject);
          FootPlacementDriver.Initialize();
        Move.Initialize();
        LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);
        if (HasEvents == false)
        {
            LocalBoneDriver.ReadyToRead += SimulateHips;
            OnLocalAvatarChanged += OnCalibration;
            SceneManager.sceneLoaded += OnSceneLoadedCallback;
            HasEvents = true;
        }
        string LastUsedAvatar = BasisDataStore.LoadString(LoadFileName, BasisAvatarFactory.LoadingAvatar);
        await CreateAvatar(LastUsedAvatar);
        if (MicrophoneRecorder == null)
        {
            MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(this.gameObject);
        }
        MicrophoneRecorder.TryInitialize();
        OnLocalPlayerCreatedAndReady?.Invoke();
    }
    public void RecalculateMyHeight()
    {
        Debug.Log("Attempting RecalculateMyHeight");
            Basis.Scripts.TransformBinders.BasisLockToInput BasisLockToInput = BasisLocalCameraDriver.Instance.BasisLockToInput;
        if (BasisLockToInput != null)
        {
            if (BasisLockToInput.AttachedInput != null)
            {
                Debug.Log("recalculating local Height!");
                PlayerEyeHeight = BasisLockToInput.AttachedInput.LocalRawPosition.y;
                Debug.Log("Local Eye Height is " + PlayerEyeHeight);
                SetPlayersEyeHeight(PlayerEyeHeight, AvatarDriver.ActiveEyeHeight);
            }
        }
    }
    public void SetPlayersEyeHeight(float realEyeHeight, float avatarHeight)
    {
        if (BasisDeviceManagement.Instance.CurrentMode == BasisBootedMode.Desktop)
        {
            RatioPlayerToAvatarScale = 1;
        }
        else
        {
            if (realEyeHeight <= 0 || avatarHeight <= 0)
            {
                RatioPlayerToAvatarScale = 1;
                Debug.LogError("Scale was below zero");
            }
            else
            {
                RatioPlayerToAvatarScale = avatarHeight / realEyeHeight;
            }
        }
        RatioAvatarToAvatarEyeDefaultScale = avatarHeight / DefaultAvatarEyeHeight;
        RatioPlayerToEyeDefaultScale = realEyeHeight / DefaultPlayerEyeHeight;
        OnPlayersHeightChanged?.Invoke();
    }
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        BasisAvatarStrainJiggleDriver.PrepareTeleport();
        Debug.Log("Teleporting");
        Move.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        Move.enabled = true;
        if (AvatarDriver != null && AvatarDriver.AnimatorDriver != null)
        {
            AvatarDriver.AnimatorDriver.HandleTeleport();
        }
        BasisAvatarStrainJiggleDriver.FinishTeleport();
        OnSpawnedEvent?.Invoke();
    }
    public void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (BasisScene.Instance != null)
        {
            //swap over to on scene load
            BasisScene.Instance.SpawnPlayer(this);
        }
    }
    public void SimulateHips()
    {
        if (Hips.HasBone)
        {
            if (Avatar != null && Avatar.Animator != null)
            {
                // Get the current rotation of the hips bone
                Quaternion currentRotation = Hips.OutgoingWorldData.rotation;

                // Calculate the rotated T-pose position using the current rotation
                Vector3 rotatedTposePosition = currentRotation * Hips.TposeLocal.position;
                Vector3 positionDifference = Hips.OutgoingWorldData.position - rotatedTposePosition;

                // Calculate the difference between the current rotation and the T-pose rotation
                Quaternion rotationDifference = currentRotation * Quaternion.Inverse(Hips.TposeWorld.rotation);

                // Apply the calculated position and rotation to the Avatar's animator transform
                Avatar.Animator.transform.SetPositionAndRotation(positionDifference, rotationDifference);
            }
        }
    }
    public async Task CreateAvatar(string AddressableID = BasisAvatarFactory.LoadingAvatar)
    {
        await BasisAvatarFactory.LoadAvatar(this, AddressableID);
        BasisDataStore.SaveString(AddressableID, LoadFileName);
        OnLocalAvatarChanged?.Invoke();
    }
    public void OnCalibration()
    {
        if (VisemeDriver == null)
        {
            VisemeDriver = BasisHelpers.GetOrAddComponent<BasisVisemeDriver>(this.gameObject);
        }
        VisemeDriver.Initialize(Avatar);
        BasisLocalInputActions.LateUpdateEvent += VisemeDriver.EventLateUpdate;
        MicrophoneRecorderBase.OnHasAudio += DriveAudioToViseme;
        MicrophoneRecorderBase.OnHasSilence += DriveAudioToViseme;
    }
    public void OnDestroy()
    {
        if (HasEvents)
        {
            if (LocalBoneDriver != null)
            {
                LocalBoneDriver.ReadyToRead -= SimulateHips;
            }
            OnLocalAvatarChanged -= OnCalibration;
            SceneManager.sceneLoaded -= OnSceneLoadedCallback;
            HasEvents = false;
        }
        BasisLocalInputActions.LateUpdateEvent -= VisemeDriver.EventLateUpdate;
        MicrophoneRecorderBase.OnHasAudio -= DriveAudioToViseme;
        MicrophoneRecorderBase.OnHasSilence -= DriveAudioToViseme;
        if (VisemeDriver != null)
        {
            GameObject.Destroy(VisemeDriver);
        }
    }
    public void DriveAudioToViseme()
    {
        VisemeDriver.ProcessAudioSamples(MicrophoneRecorder.processBufferArray);
    }
}
}