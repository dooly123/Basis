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
    public BasisLocalAvatarDriver LocalAvatarDriver;
    public BasisFootPlacementDriver FootPlacementDriver;
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
        CreateAvatar();
        BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead += Simulate;
    }
    public void Simulate()
    {
        if (Hips.HasBone && Avatar != null)
        {
            Avatar.Animator.transform.SetPositionAndRotation((Hips.BoneTransform.position - Hips.RestingLocalSpace.BeginningPosition), Hips.BoneTransform.rotation);
        }
    }
    public async void CreateAvatar()
    {
        await BasisAvatarFactory.LoadAvatar(this, FallBackAvatar);
      // await BasisAvatarFactory.LoadAvatar(this, "Assets/unity-chan!/unitychan.prefab");
        await CreateInputAction();
        OnLocalAvatarChanged?.Invoke();
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
}