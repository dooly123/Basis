using UnityEngine;

public class BasisScene : MonoBehaviour
{
    public static BasisScene Instance;
    public Transform SpawnPoint;
    public void OnEnable()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        Instance = this;
    }
    public void SpawnPlayer(BasisPlayer Basis)
    {
        RequestSpawnPoint(out Vector3 position, out Quaternion rotation);
        if (Basis != null)
        {
            Basis.transform.SetPositionAndRotation(position, rotation);
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