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
        public BasisCharacterController.BasisCharacterController Move;
        public event Action OnLocalAvatarChanged;
        public event Action OnSpawnedEvent;
        /// <summary>
        /// the bool when true is the final size
        /// the bool when false is not the final size
        /// use the bool to 
        /// </summary>
        public event Action<bool> OnPlayersHeightChanged;
        public BasisLocalBoneDriver LocalBoneDriver;
        public BasisBoneControl Hips;
        public BasisLocalAvatarDriver AvatarDriver;
    //    public BasisFootPlacementDriver FootPlacementDriver;
        public BasisVisemeDriver VisemeDriver;
        [SerializeField]
        public LayerMask GroundMask;
        public static string LoadFileNameAndExtension = "LastUsedAvatar.BAS";
        public bool HasEvents = false;
        public MicrophoneRecorder MicrophoneRecorder;
        public static string MainCamera = "Assets/Prefabs/Loadins/Main Camera.prefab";
        public bool SpawnPlayerOnSceneLoad = true;
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
            if (HasEvents == false)
            {
                LocalBoneDriver.ReadyToRead += SimulateHips;
                OnLocalAvatarChanged += OnCalibration;
                SceneManager.sceneLoaded += OnSceneLoadedCallback;
                HasEvents = true;
            }
            (string,byte) LastUsedAvatar = BasisDataStore.LoadAvatar(LoadFileNameAndExtension,BasisAvatarFactory.LoadingAvatar,BasisPlayer.LoadModeLocal);
            await CreateAvatar(LastUsedAvatar.Item1, LastUsedAvatar.Item2, string.Empty);
            if (MicrophoneRecorder == null)
            {
                MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(this.gameObject);
            }
            MicrophoneRecorder.TryInitialize();
            OnLocalPlayerCreatedAndReady?.Invoke();
        }
        /// <summary>
        /// instead of reading the data of the component i would use OnPlayersHeightChanged
        ///  we wait until the next 2 frame so we can let all devices and systems reset to there native size first. 
        ///  (total of 4 frames)
        /// </summary>
        public async Task SetPlayersEyeHeight()
        {
            Debug.Log("recalculating local Height!");
            RatioPlayerToAvatarScale = 1f;

            RatioPlayerToEyeDefaultScale = 1f;
            RatioAvatarToAvatarEyeDefaultScale = 1f;

            OnPlayersHeightChanged?.Invoke(false);


            // This will wait for 3 frames allowing the devices to provide good final positions
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            TransformBinders.BasisLockToInput BasisLockToInput = BasisLocalCameraDriver.Instance.BasisLockToInput;
            if (BasisLockToInput != null)
            {
                if (BasisLockToInput.AttachedInput != null)
                {
                    PlayerEyeHeight = BasisLockToInput.AttachedInput.LocalRawPosition.y;
                    Debug.Log("recalculating local Height!");
                    Debug.Log("Local Eye Height is " + PlayerEyeHeight);
                }
            }

        float avatarHeight = AvatarDriver.ActiveEyeHeight();
            Debug.Log("Reading Player Eye Height "+ PlayerEyeHeight);
            if (PlayerEyeHeight <= 0 || avatarHeight <= 0)
            {
                RatioPlayerToAvatarScale = 1;
                if (PlayerEyeHeight <= 0)
                {
                    PlayerEyeHeight = 1.64f;
                }
                Debug.LogError("Scale was below zero");
            }
            else
            {
                RatioPlayerToAvatarScale = avatarHeight / PlayerEyeHeight;
            }
            //lets get the some height / the default for that height
            RatioAvatarToAvatarEyeDefaultScale = avatarHeight / DefaultAvatarEyeHeight;
            RatioPlayerToEyeDefaultScale = PlayerEyeHeight / DefaultPlayerEyeHeight;
            // This will wait for 3 frames allowing the devices to provide good final positions
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            Debug.Log("Player Height Set" + PlayerEyeHeight);
            OnPlayersHeightChanged?.Invoke(true);
        }
        public void Teleport(Vector3 position, Quaternion rotation)
        {
         //   BasisAvatarStrainJiggleDriver.PrepareTeleport();
            Debug.Log("Teleporting");
            Move.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            Move.enabled = true;
            if (AvatarDriver != null && AvatarDriver.AnimatorDriver != null)
            {
                AvatarDriver.AnimatorDriver.HandleTeleport();
            }
           // BasisAvatarStrainJiggleDriver.FinishTeleport();
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
            //  if (Hips.HasBone && AvatarDriver.InTPose == false)
            //{
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
            //}
        }
        public async Task CreateAvatar(string AddressableID,byte mode,string hash = "")
        {
            await BasisAvatarFactory.LoadAvatar(this, AddressableID, mode, hash);
            BasisDataStore.SaveAvatar(AddressableID, mode, LoadFileNameAndExtension);
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
                    LocalBoneDriver.ReadyToRead -= SimulateHips;
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