using System.Threading;
using UnityEngine;

public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken;
    public BasisLoadableBundle BasisLoadableBundle;
    public BasisBundleInformation BasisBundleInformation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        BasisBundleInformation BasisBundleInformation = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
    }
}