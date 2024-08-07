using Assets.Scripts.BasisSdk.Helpers;
using Assets.Scripts.BasisSdk.Players;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.BasisSdk
{
public class BasisScene : MonoBehaviour
{
    public static BasisScene Instance;
    public Transform SpawnPoint;
    public float RespawnHeight = -100;
    public float RespawnTimer = 0.1f;
    public UnityEngine.Audio.AudioMixerGroup Group;
    public void Awake()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
            AttachMixerToAllSceneAudioSources();
            StartCoroutine(CheckHeightLoop());
        }
    }
    public void AttachMixerToAllSceneAudioSources()
    {
        AudioSource[] Sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(AudioSource Source in Sources)
        {
            if (Source.outputAudioMixerGroup == null)
            {
                Source.outputAudioMixerGroup = Group;
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
            yield return new WaitForSeconds(RespawnTimer);
        }
    }
    void CheckHeight()
    {
        if (BasisLocalPlayer.Instance != null)
        {
            float height = BasisLocalPlayer.Instance.transform.position.y;

            // Perform the height check logic
            if (height > RespawnHeight)
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
        if (SpawnPoint == null)
        {
            this.transform.GetPositionAndRotation(out Position, out Rotation);
        }
        else
        {
            SpawnPoint.GetPositionAndRotation(out Position, out Rotation);
        }
    }
}
}