using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using System.Collections;
using UnityEngine;

public class BasisSceneFactory : MonoBehaviour
{
    public BasisScene BasisScene;
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
        StartCoroutine(CheckHeightLoop());
    }
    public void AttachMixerToAllSceneAudioSources()
    {
        AudioSource[] Sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioSource Source in Sources)
        {
            if (Source.outputAudioMixerGroup == null)
            {
                Source.outputAudioMixerGroup = BasisScene.Group;
            }
        }
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
    IEnumerator CheckHeightLoop()
    {
        while (true)
        {
            CheckHeight();
            yield return new WaitForSeconds(BasisScene.RespawnCheckTimer);
        }
    }
    void CheckHeight()
    {
        if (BasisLocalPlayer.Instance != null)
        {
            float height = BasisLocalPlayer.Instance.transform.position.y;

            // Perform the height check logic
            if (height > BasisScene.RespawnHeight)
            {
            }
            else
            {
                SpawnPlayer(BasisLocalPlayer.Instance);
            }
        }
    }
    public void RequestSpawnPoint(out Vector3 Position, out Quaternion Rotation)
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
}