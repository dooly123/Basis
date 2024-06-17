using UnityEngine;

public class BasisScene : MonoBehaviour
{
    public static BasisScene Instance;
    public Transform SpawnPoint;
    public void Awake()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
    }
    public void SpawnPlayer(BasisLocalPlayer Basis)
    {
        Debug.Log("Spawning Player");
        RequestSpawnPoint(out Vector3 position, out Quaternion rotation);
        if (Basis != null)
        {

            if (Basis.Move != null)
            {
                Debug.Log("Teleporting");
                Basis.Move.enabled = false;
                Basis.transform.SetPositionAndRotation(position, rotation);
                Basis.Move.transform.SetPositionAndRotation(position, rotation);
                Basis.Move.enabled = true;
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