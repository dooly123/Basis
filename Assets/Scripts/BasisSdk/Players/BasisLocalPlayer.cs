using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BasisLocalPlayer : BasisPlayer
{
    public static BasisLocalPlayer Instance;
    public BasisCharacterController Move;
    public static string InputActions = "InputActions";
    public static event System.Action OnLocalAvatarChanged;
    public BasisLocalCharacterBinder Binder;
    public BasisLocalBoneDriver LocalBoneDriver;
    public BasisBoneControl Hips;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisFootPlacementDriver FootPlacementDriver;
    public BasisVisemeDriver VisemeDriver;
    public AudioSource SelfOutput;
    [SerializeField]
    public LayerMask GroundMask;
    public async Task LocalInitialize()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        Instance = this;
        IsLocal = true;
        LocalBoneDriver.CreateInitialArrays(LocalBoneDriver.transform);
        await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Main Camera.prefab", new InstantiationParameters());
        await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Character Controller.prefab", new InstantiationParameters());
        //  FootPlacementDriver = Helpers.GetOrAddComponent<FootPlacementDriver>(this.gameObject);
        //  FootPlacementDriver.Initialize();
        LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips);
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead += Simulate;
        OnLocalAvatarChanged += OnCalibration;
        await CreateAvatar();
    }
    public void Simulate()
    {
        if (Hips.HasBone && Avatar != null)
        {
            Avatar.Animator.transform.SetPositionAndRotation((Hips.BoneTransform.position - Hips.RestingLocalSpace.BeginningPosition), Hips.BoneTransform.rotation);
        }
    }
    public async Task CreateAvatar()
    {
        await BasisAvatarFactory.LoadAvatar(this, FallBackAvatar);
      // await BasisAvatarFactory.LoadAvatar(this, "Assets/unity-chan!/unitychan.prefab");
        await CreateInputAction();
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
    public async Task CreateInputAction()
    {
        List<GameObject> Gameobjects = await AddressableResourceProcess.LoadAsGameObjectsAsync(InputActions, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                if (gameObject.TryGetComponent(out BasisLocalInputActions CharacterInputActions))
                {
                    CharacterInputActions.Initialize(this);
                }
            }
        }
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