using Assets.Scripts.BasisSdk;
using Assets.Scripts.LipSync.Scripts;
using UnityEngine;

namespace Assets.Scripts.Drivers
{
public class BasisVisemeDriver : OVRLipSyncContextBase
{
    public int laughterBlendTarget = -1;
    public float laughterThreshold = 0.5f;
    public float laughterMultiplier = 1.5f;
    public int smoothAmount = 70;
    public BasisAvatar Avatar;
    public float laughterScore = 0.0f;
    public void Initialize(BasisAvatar avatar)
    {
        // Debug.Log("Initalizing " + nameof(BasisVisemeDriver));  
        Avatar = avatar;
        Smoothing = smoothAmount;
    }
    public void EventLateUpdate()
    {
        if (Avatar != null)
        {
            // get the current viseme frame
            OVRLipSync.Frame frame = GetCurrentPhonemeFrame();
            if (frame != null)
            {
                for (int Index = 0; Index < Avatar.FaceVisemeMovement.Length; Index++)
                {
                    if (Avatar.FaceVisemeMovement[Index] != -1)
                    {
                        // Viseme blend weights are in range of 0->1.0, we need to make range 100
                        Avatar.FaceVisemeMesh.SetBlendShapeWeight(Avatar.FaceVisemeMovement[Index], frame.Visemes[Index] * 100.0f);
                    }
                }
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
    }
    /// <summary>
    /// Process F32 audio sample and pass it to the lip sync module for computation
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void ProcessAudioSamples(float[] data)
    {
        // Send data into Phoneme context for processing (if context is not 0)
        lock (this)
        {
            if (Context == 0 || OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
            {
                return;
            }
            OVRLipSync.Frame frame = this.Frame;
            OVRLipSync.ProcessFrame(Context, data, frame, false);
        }
    }
}
}