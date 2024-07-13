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
    public BasisLocalCharacterBinder Binder;
    public BasisLocalBoneDriver LocalBoneDriver;
    public BasisBoneControl Hips;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisFootPlacementDriver FootPlacementDriver;
    public BasisVisemeDriver VisemeDriver;
    public AudioSource SelfOutput;
    [SerializeField]
    public LayerMask GroundMask;
    public static string LoadFileName = "LastUsedAvatar.BAS";
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
        await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Character Controller.prefab", new InstantiationParameters());
        LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);
        Instance.LocalBoneDriver.ReadyToRead += SimulateHips;
        OnLocalAvatarChanged += OnCalibration;
        string LastUsedAvatar = BasisDataStore.LoadString(LoadFileName, BasisAvatarFactory.LoadingAvatar);
        await CreateAvatar(LastUsedAvatar);
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
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
                float y = BasisLockToInput.AttachedInput.LocalRawPosition.y;
                PlayerEyeHeight = y;
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
    public void Teleport(Vector3 position,Quaternion rotation)
    {
        BasisAvatarStrainJiggleDriver.PrepareTeleport();
        Debug.Log("Teleporting");
        if (Move != null)
        {
            Move.enabled = false;
            Move.transform.SetPositionAndRotation(position, Quaternion.identity);
Move.enabled = true;
        }
        if(AvatarDriver != null && AvatarDriver.AnimatorDriver != null)
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
            Quaternion rotation = Hips.BoneTransform.rotation;//we dont use Hips.BoneModelTransform.rotation as it introduces a offset that is unwanted.
            Vector3 rotatedOffset = rotation * Hips.TposeLocal.position;
            rotatedOffset = Hips.FinalisedWorldData.position - rotatedOffset;

            SimulateHips(rotatedOffset, rotation);
        }
    }
    public void SimulateHIpsRotation(Quaternion Rotation)
    {
        if (Avatar != null && Avatar.Animator != null)
        {
            Avatar.Animator.transform.rotation = Rotation;
        }
    }
    public void SimulateHips(Vector3 Position, Quaternion Rotation)
    {
        if (Avatar != null && Avatar.Animator != null)
        {
            Avatar.Animator.transform.SetPositionAndRotation(Position, Rotation);
        }
    }
#if UNITY_EDITOR
    [MenuItem("Basis/ReloadAvatar")]
    public static async void ReloadAvatar()
    {
      await  BasisLocalPlayer.Instance.CreateAvatar();
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