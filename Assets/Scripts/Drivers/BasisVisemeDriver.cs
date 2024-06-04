using System.Linq;
using UnityEngine;

public class BasisVisemeDriver : OVRLipSyncContextBase
{
    public int laughterBlendTarget = -1;
    public float laughterThreshold = 0.5f;
    public float laughterMultiplier = 1.5f;
    public int smoothAmount = 70;
    public BasisAvatar Avatar;
    public bool audioLoopback = false;
    public bool skipAudioSource = false;
    public float gain = 1.0f;
    public float laughterScore = 0.0f;

    public void Initialize(BasisAvatar avatar)
    {
        Avatar = avatar;
        Smoothing = smoothAmount;
    }
    void Update()
    {
        // get the current viseme frame
        OVRLipSync.Frame frame = GetCurrentPhonemeFrame();
        if (frame != null)
        {
            SetVisemeToMorphTarget(frame);

            SetLaughterToMorphTarget(frame);
        }
        else
        {
            Debug.Log("missing frame");
        }
        laughterScore = this.Frame.laughterScore;
        // Update smoothing value
        if (smoothAmount != Smoothing)
        {
            Smoothing = smoothAmount;
        }
    }
    void SetVisemeToMorphTarget(OVRLipSync.Frame frame)
    {
        for (int Index = 0; Index < Avatar.FaceVisemeMovement.Length; Index++)
        {
            if (Avatar.FaceVisemeMovement[Index] != -1)
            {
                // Viseme blend weights are in range of 0->1.0, we need to make range 100
                Avatar.FaceVisemeMesh.SetBlendShapeWeight(Avatar.FaceVisemeMovement[Index], frame.Visemes[Index] * 100.0f);
            }
        }
    }
    void SetLaughterToMorphTarget(OVRLipSync.Frame frame)
    {
        if (laughterBlendTarget != -1)
        {
            // Laughter score will be raw classifier output in [0,1]
            float laughterScore = frame.laughterScore;

            // Threshold then re-map to [0,1]
            laughterScore = laughterScore < laughterThreshold ? 0.0f : laughterScore - laughterThreshold;
            laughterScore = Mathf.Min(laughterScore * laughterMultiplier, 1.0f);
            laughterScore *= 1.0f / laughterThreshold;

            Avatar.FaceVisemeMesh.SetBlendShapeWeight(laughterBlendTarget, laughterScore * 100.0f);
        }
    }
    /// <summary>
    /// Preprocess F32 PCM audio buffer
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void PreprocessAudioSamples(float[] data, int channels)
    {
        // Increase the gain of the input
        for (int Index = 0; Index < data.Length; ++Index)
        {
            data[Index] = data[Index] * gain;
        }
    }

    /// <summary>
    /// Postprocess F32 PCM audio buffer
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void PostprocessAudioSamples(float[] data, int channels)
    {
        // Turn off output (so that we don't get feedback from mics too close to speakers)
        if (!audioLoopback)
        {
            for (int Index = 0; Index < data.Length; ++Index)
            {
                data[Index] = data[Index] * 0.0f;
            }
        }
    }

    /// <summary>
    /// Pass F32 PCM audio buffer to the lip sync module
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void ProcessAudioSamplesRaw(float[] data, int channels)
    {
        // Send data into Phoneme context for processing (if context is not 0)
        lock (this)
        {
            if (Context == 0 || OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
            {
                return;
            }
            var frame = this.Frame;
            OVRLipSync.ProcessFrame(Context, data, frame, channels == 2);
        }
    }

    /// <summary>
    /// Pass S16 PCM audio buffer to the lip sync module
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void ProcessAudioSamplesRaw(short[] data, int channels)
    {
        // Send data into Phoneme context for processing (if context is not 0)
        lock (this)
        {
            if (Context == 0 || OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
            {
                return;
            }
            var frame = this.Frame;
            OVRLipSync.ProcessFrame(Context, data, frame, channels == 2);
        }
    }


    /// <summary>
    /// Process F32 audio sample and pass it to the lip sync module for computation
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void ProcessAudioSamples(float[] data, int channels)
    {
        // Do not process if we are not initialized, or if there is no
        // audio source attached to game object
        if ((OVRLipSync.IsInitialized() != OVRLipSync.Result.Success) || audioSource == null)
        {
            return;
        }
        PreprocessAudioSamples(data, channels);
        ProcessAudioSamplesRaw(data, channels);
        PostprocessAudioSamples(data, channels);
    }

    /// <summary>
    /// Raises the audio filter read event.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!skipAudioSource)
        {
            ProcessAudioSamples(data, channels);
        }
    }
}