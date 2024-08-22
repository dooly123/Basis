using System.Threading.Tasks;
using UnityEngine;

public abstract class BasisBaseTypeManagement : MonoBehaviour
{
    public abstract void StopSDK();
    public abstract Task BeginLoadSDK();
    public abstract Task StartSDK();
    public abstract string Type();
}
