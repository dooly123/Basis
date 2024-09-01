using TMPro;
using UnityEngine;

public class BasisFrameRateVisualization : MonoBehaviour
{
    public TextMeshProUGUI fpsText;// UI Text element to display the FPS
    private float deltaTime = 0.0f;  // Time between frames
    private float fps = 0.0f;  // Frames per second
    private float timeBetweenUpdates = 0.1f;  // Time between updates (in seconds)
    private float timeAccumulator = 0.0f;  // Time accumulator to track updates

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
            // Display FPS with two decimal places
            fpsText.text = $"FPS: {fps:F2}";

            // Reset the time accumulator
            timeAccumulator = 0.0f;
        }
    }
}
