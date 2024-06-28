using System.Collections;
using UnityEngine;

public class BasisScene : MonoBehaviour
{
    public static BasisScene Instance;
    public Transform SpawnPoint;
    public float RespawnHeight = -100;
    public float RespawnTimer = 0.1f;
    public void Awake()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
            StartCoroutine(CheckHeightLoop());
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