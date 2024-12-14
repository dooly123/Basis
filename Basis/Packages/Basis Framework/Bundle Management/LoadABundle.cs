using System.Threading;
using UnityEngine;
public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport Report = new BasisProgressReport();
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    public bool UseCondom = false;
    public bool useRandomizer = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void OnEnable()
    {
        // Load the GameObject asynchronously
        GameObject output = await BasisLoadHandler.LoadGameObjectBundle(BasisLoadableBundle, UseCondom, Report, CancellationToken, Vector3.zero, Quaternion.identity);
    }
}