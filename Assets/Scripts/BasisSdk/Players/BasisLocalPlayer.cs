using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class BasisLocalPlayer : BasisPlayer
{
    public static float DefaultPlayerEyeHeight = 1.64f;
    public static float DefaultAvatarEyeHeight = 1.64f;
    public float PlayerEyeHeight = 1.64f;

    public float RatioPlayerToAvatarScale = 1f;
    public float RatioPlayerToEyeDefaultScale = 1f;
    public float RatioAvatarToAvatarEyeDefaultScale = 1f;
    public static BasisLocalPlayer Instance;
    public BasisCharacterController Move;
    public event Action OnLocalAvatarChanged;
    public event Action OnSpawnedEvent;
    public event Action OnPlayersHeightChanged;
    public BasisLocalBoneDriver LocalBoneDriver;
    public BasisBoneControl Hips;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisFootPlacementDriver FootPlacementDriver;
    public BasisVisemeDriver VisemeDriver;
    public AudioSource SelfOutput;
    [SerializeField]
    public LayerMask GroundMask;
    public static string LoadFileName = "LastUsedAvatar.BAS";
    public bool HasEvents = false;
    public async Task LocalInitialize()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        Instance = this;
        IsLocal = true;
        LocalBoneDriver.CreateInitialArrays(LocalBoneDriver.transform);
        await BasisLocalInputActions.CreateInputAction(this);
        await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Main Camera.prefab", new InstantiationParameters());
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
    }
    public void RecalculateMyHeight()
    {
        Debug.Log("Attempting RecalculateMyHeight");
        BasisLockToInput BasisLockToInput = BasisLocalCameraDriver.Instance.BasisLockToInput;
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
                Quaternion currentRotation = Hips.CurrentWorldData.rotation;

                // Calculate the rotated T-pose position using the current rotation
                Vector3 rotatedTposePosition = currentRotation * Hips.TposeLocal.position;
                Vector3 positionDifference = Hips.CurrentWorldData.position - rotatedTposePosition;

                // Calculate the difference between the current rotation and the T-pose rotation
                Quaternion rotationDifference = currentRotation * Quaternion.Inverse(Hips.TposeWorld.rotation);

                // Apply the calculated position and rotation to the Avatar's animator transform
                Avatar.Animator.transform.SetPositionAndRotation(positionDifference, rotationDifference);
            }
        }
    }
#if UNITY_EDITOR
    [MenuItem("Basis/ReloadAvatar")]
    public static async void ReloadAvatar()
    {
        await BasisLocalPlayer.Instance.CreateAvatar();
    }
#endif
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
        if (SelfOutput == null)
        {
            SelfOutput = BasisHelpers.GetOrAddComponent<AudioSource>(this.gameObject);
        }
        SelfOutput.loop = true;     // Set the AudioClip to loop
        SelfOutput.mute = false;
        SelfOutput.clip = AvatarDriver.MicrophoneRecorder.clip;
        SelfOutput.Play();
        VisemeDriver.audioSource = SelfOutput;
        VisemeDriver.Initialize(Avatar);
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
        if (VisemeDriver != null)
        {
            GameObject.Destroy(VisemeDriver);
        }
        if (SelfOutput != null)
        {
            GameObject.Destroy(SelfOutput);
        }
    }
}