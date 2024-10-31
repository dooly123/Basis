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
using Basis.Scripts.Avatar;
using Basis.Scripts.Common;
using System.Collections.Generic;
using Basis.Scripts.UI.UI_Panels;
namespace Basis.Scripts.BasisSdk.Players
{
    public class BasisLocalPlayer : BasisPlayer
    {
        public static BasisLocalPlayer Instance;
        public static Action OnLocalPlayerCreatedAndReady;
        public static Action OnLocalPlayerCreated;
        public BasisCharacterController.BasisCharacterController Move;
        public event Action OnLocalAvatarChanged;
        public event Action OnSpawnedEvent;

        public static float DefaultPlayerEyeHeight = 1.64f;
        public static float DefaultAvatarEyeHeight = 1.64f;
        public float PlayerEyeHeight = 1.64f;
        public float RatioPlayerToAvatarScale = 1f;
        public float EyeRatioPlayerToDefaultScale = 1f;
        public float EyeRatioAvatarToAvatarDefaultScale = 1f;//should be used for the player

        /// <summary>
        /// the bool when true is the final size
        /// the bool when false is not the final size
        /// use the bool to 
        /// </summary>
        public Action OnPlayersHeightChanged;
        public BasisLocalBoneDriver LocalBoneDriver;
        public BasisBoneControl Hips;
        public BasisBoneControl CenterEye;
        public BasisLocalAvatarDriver AvatarDriver;
    //   public BasisFootPlacementDriver FootPlacementDriver;
        public BasisVisemeDriver VisemeDriver;
        [SerializeField]
        public LayerMask GroundMask;
        public static string LoadFileNameAndExtension = "LastUsedAvatar.BAS";
        public bool HasEvents = false;
        public MicrophoneRecorder MicrophoneRecorder;
        public static string MainCamera = "Assets/Prefabs/Loadins/Main Camera.prefab";
        public bool SpawnPlayerOnSceneLoad = true;
        public const string DefaultAvatar = "LoadingAvatar";
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
            await BasisDeviceManagement.LoadGameobject(MainCamera, new InstantiationParameters());
            //  FootPlacementDriver = BasisHelpers.GetOrAddComponent<BasisFootPlacementDriver>(this.gameObject);
            //  FootPlacementDriver.Initialize();
            Move.Initialize();
            LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);
            LocalBoneDriver.FindBone(out CenterEye, BasisBoneTrackedRole.Neck);
            if (HasEvents == false)
            {
                LocalBoneDriver.ReadyToRead.AddAction(97, SimulateHips);
                OnLocalAvatarChanged += OnCalibration;
                SceneManager.sceneLoaded += OnSceneLoadedCallback;
                HasEvents = true;
            }
         bool LoadedState = BasisDataStore.LoadAvatar(LoadFileNameAndExtension, DefaultAvatar, BasisPlayer.LoadModeLocal,out BasisDataStore.BasisSavedAvatar LastUsedAvatar);
            if(LoadedState)
            {
                await LoadInitalAvatar(LastUsedAvatar);
            }
            else
            {
                await CreateAvatar(BasisPlayer.LoadModeLocal, BasisAvatarFactory.LoadingAvatar);
            }
            if (MicrophoneRecorder == null)
            {
                MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(this.gameObject);
            }
            MicrophoneRecorder.TryInitialize();
            OnLocalPlayerCreatedAndReady?.Invoke();
        }
        public async Task LoadInitalAvatar(BasisDataStore.BasisSavedAvatar LastUsedAvatar)
        {
            if (BasisLoadHandler.IsMetaDataOnDisc(LastUsedAvatar.UniqueID, out OnDiscInformation info))
            {
                await BasisDataStoreAvatarKeys.LoadKeys();
                List<BasisDataStoreAvatarKeys.AvatarKey> activeKeys = BasisDataStoreAvatarKeys.DisplayKeys();
                foreach (BasisDataStoreAvatarKeys.AvatarKey Key in activeKeys)
                {
                    if (Key.Url == LastUsedAvatar.UniqueID)
                    {
                        BasisLoadableBundle bundle = new BasisLoadableBundle
                        {
                            BasisRemoteBundleEncrypted = info.StoredRemote,
                            BasisBundleInformation = new BasisBundleInformation
                            {
                                BasisBundleDescription = new BasisBundleDescription(),
                                BasisBundleGenerated = new BasisBundleGenerated()
                            },
                            BasisLocalEncryptedBundle = info.StoredLocal,
                            UnlockPassword = Key.Pass
                        };
                        Debug.Log("loading previously loaded avatar");
                        await CreateAvatar(LastUsedAvatar.loadmode, bundle);
                        return;
                    }
                }
                Debug.Log("failed to load last used : no key found to load but was found on disc");
                await CreateAvatar(BasisPlayer.LoadModeLocal, BasisAvatarFactory.LoadingAvatar);
            }
            else
            {
                Debug.Log("failed to load last used : url was not found on disc");
                await CreateAvatar(BasisPlayer.LoadModeLocal, BasisAvatarFactory.LoadingAvatar);
            }
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
            if (BasisSceneFactory.Instance != null && SpawnPlayerOnSceneLoad)
            {
                //swap over to on scene load
                BasisSceneFactory.Instance.SpawnPlayer(this);
            }
        }
        public void SimulateHips()
        {
            if (Hips.HasBone)
            {
                if (Avatar != null)
                {
                    // Get the current rotation of the hips bone
                    Quaternion currentRotation = Hips.OutgoingWorldData.rotation;

                    // Calculate the rotated T-pose position using the current rotation
                    Vector3 rotatedTposePosition = currentRotation * Hips.TposeLocal.position;
                    Vector3 positionDifference = Hips.OutgoingWorldData.position - rotatedTposePosition;

                    //    Avatar.Animator.transform.localRotation = rotationDifference;
                    Avatar.transform.position = positionDifference;
                    // Apply the calculated position and rotation to the Avatar's animator transform
                    Avatar.transform.localRotation = currentRotation;
                }
            }
        }
        public async Task CreateAvatar(byte mode, BasisLoadableBundle BasisLoadableBundle)
        {
            await BasisAvatarFactory.LoadAvatarLocal(this, mode, BasisLoadableBundle);
            BasisDataStore.SaveAvatar(BasisLoadableBundle.BasisRemoteBundleEncrypted.MetaURL, mode, LoadFileNameAndExtension);
            OnLocalAvatarChanged?.Invoke();
        }
        public void OnCalibration()
        {
            if (VisemeDriver == null)
            {
                VisemeDriver = BasisHelpers.GetOrAddComponent<BasisVisemeDriver>(this.gameObject);
            }
            VisemeDriver.TryInitialize(this);
            if (HasCalibrationEvents == false)
            {
                MicrophoneRecorderBase.OnHasAudio += DriveAudioToViseme;
                MicrophoneRecorderBase.OnHasSilence += DriveAudioToViseme;
                HasCalibrationEvents = true;
            }
        }
        public bool HasCalibrationEvents = false;
        public void OnDestroy()
        {
            if (HasEvents)
            {
                if (LocalBoneDriver != null)
                {
                    LocalBoneDriver.ReadyToRead.RemoveAction(97,SimulateHips);
                }
                OnLocalAvatarChanged -= OnCalibration;
                SceneManager.sceneLoaded -= OnSceneLoadedCallback;
                HasEvents = false;
            }
            if (HasCalibrationEvents)
            {
                MicrophoneRecorderBase.OnHasAudio -= DriveAudioToViseme;
                MicrophoneRecorderBase.OnHasSilence -= DriveAudioToViseme;
                HasCalibrationEvents = false;
            }
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