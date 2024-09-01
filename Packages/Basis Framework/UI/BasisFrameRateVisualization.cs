using TMPro;
using UnityEngine;
using Basis.Scripts.Networking;
public class BasisFrameRateVisualization : MonoBehaviour
{
    public TextMeshProUGUI fpsText;// UI Text element to display the FPS
    private float deltaTime = 0.0f;  // Time between frames
    private float fps = 0.0f;  // Frames per second
    private float timeBetweenUpdates = 0.1f;  // Time between updates (in seconds)
    private float timeAccumulator = 0.0f;  // Time accumulator to track updates
    public string FinalText;
    void Update()
    {
        // Calculate the time it took to render the last frame
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate FPS based on deltaTime
        fps = 1.0f / deltaTime;

        // Accumulate time
        timeAccumulator += Time.unscaledDeltaTime;

        // If the accumulated time exceeds the update interval, update the FPS display
        if (timeAccumulator >= timeBetweenUpdates)
        {
            FinalText = $"FPS: {fps:F2}";
            /*
            if (BasisNetworkManagement.Instance != null && BasisNetworkManagement.Instance.Client != null && BasisNetworkManagement.Instance.Client.Client != null)
            {
                DarkRift.RoundTripTimeHelper RoundTripTimeHelper = BasisNetworkManagement.Instance.Client.Client.RoundTripTime;
                FinalText += "| " + RoundTripTimeHelper.SmoothedRtt + "| " + RoundTripTimeHelper.LatestRtt + "| " + RoundTripTimeHelper.RttSampleCount;
            }
            */
            // Display FPS with two decimal places
            fpsText.text = FinalText;

            // Reset the time accumulator
            timeAccumulator = 0.0f;
        }
    }
}