using System;
using System.Threading;
using UnityEngine;
public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    public bool UseCondom = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void OnEnable()
    {
        // Load the GameObject asynchronously
        GameObject output = await BasisLoadHandler.LoadGameObjectBundle(BasisLoadableBundle, UseCondom, Report, CancellationToken);

        // Set a random position with x, y, z between -50 and 50
        float randomX = UnityEngine.Random.Range(-50f, 50f);
        float randomY = UnityEngine.Random.Range(-50f, 50f);
        float randomZ = UnityEngine.Random.Range(-50f, 50f);
        output.transform.position = new Vector3(randomX, randomY, randomZ);

        // Set a random rotation in degrees for x, y, and z
        float randomRotX = UnityEngine.Random.Range(0f, 360f);
        float randomRotY = UnityEngine.Random.Range(0f, 360f);
        float randomRotZ = UnityEngine.Random.Range(0f, 360f);
        output.transform.rotation = Quaternion.Euler(randomRotX, randomRotY, randomRotZ);
    }
}