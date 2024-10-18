using System.Threading;
using UnityEngine;
public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    public bool UseCondom = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await BasisLoadhandler.LoadGameobjectBundle(BasisLoadableBundle, UseCondom, Report, CancellationToken);
    }
}