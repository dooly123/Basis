using UnityEngine;

public class BasisPickupSocket : MonoBehaviour
{
    public BasisPickup BasisPickup;
    public void AddPickup(BasisPickup BasisPickup)
    {

    }
    public void RemovePickup()
    {

    }
    void LateUpdate()
    {
        if(BasisPickup != null)
        {
            BasisPickup.LateUpdateSocket();
        }
    }
}
