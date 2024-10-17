using System.Threading;
using UnityEngine;
public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await BasisLoadhandler.LoadBundle(BasisLoadableBundle, Report, CancellationToken);
    }
}