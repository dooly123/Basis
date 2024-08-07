using Assets.Scripts.Addressable_Driver.Resource;
using Assets.Scripts.BasisSdk.Players;
using Assets.Scripts.TransformBinders.BoneControl;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.NamePlate
{
public class BasisRemoteNamePlate : BasisNamePlate
{
    public static async Task LoadRemoteNamePlate(BasisRemotePlayer Player, string RemoteNamePlate = "Assets/UI/Prefabs/NamePlate.prefab")
    {
        var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(RemoteNamePlate, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
        List<GameObject> Gameobjects = data.Item1;
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                if (gameObject.TryGetComponent(out BasisRemoteNamePlate BasisRemoteNamePlate))
                {
                    if (Player == null)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    BasisRemoteNamePlate.transform.SetParent(Player.transform, false);
                    if (Player.RemoteBoneDriver.FindBone(out BasisBoneControl Hips, BasisBoneTrackedRole.Hips))
                    {
                        BasisRemoteNamePlate.Initalize(Hips, Player);
                    }
                }
            }
        }
    }
}
}