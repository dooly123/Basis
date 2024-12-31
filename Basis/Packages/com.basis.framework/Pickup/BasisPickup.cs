using UnityEngine;

public class BasisPickup : MonoBehaviour
{
    public PickedupMode PickupMode;
    public enum PickedupMode
    {
        None,
        Local,
        Remote
    }
    public void Start()
    {
        LocalOwnedLogic.Initalize();
        RemoteOwnedLogic.Initalize();
    }
    public void OwnershipTransfer(PickedupMode PickedupMode)
    {
        this.PickupMode = PickedupMode;
    }
    public void UpdateData(Vector3 Position, Quaternion Rotation)
    {

    }
    public void LateUpdateSocket()
    {
        switch (PickupMode)
        {
            case PickedupMode.Local:
                LocalOwnedLogic.LateUpdate();
                break;
            case PickedupMode.Remote:
                RemoteOwnedLogic.LateUpdate();
                break;
            case PickedupMode.None:

                break;
        }
    }
    public LocallyOwnedLogic LocalOwnedLogic = new LocallyOwnedLogic();
    public RemotellyOwnedLogic RemoteOwnedLogic = new RemotellyOwnedLogic();
    public class LocallyOwnedLogic
    {
        public void Initalize()
        {

        }
        public void LateUpdate()
        {

        }
    }
    public class RemotellyOwnedLogic
    {
        public void Initalize()
        {

        }
        public void LateUpdate()
        {

        }
    }
}