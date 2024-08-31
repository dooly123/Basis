using Basis.Scripts.BasisSdk;
using Basis.Scripts.LipSync.Scripts;
using System;
using UnityEngine;
using static Basis.Scripts.LipSync.Scripts.OVRLipSync;

namespace Basis.Scripts.Drivers
{
    public class BasisVisemeDriver : OVRLipSyncContextBase
    {
        public int laughterBlendTarget = -1;
        public float laughterThreshold = 0.5f;
        public float laughterMultiplier = 1.5f;
        public int smoothAmount = 70;
        public BasisAvatar Avatar;
        public float laughterScore = 0.0f;
        public float LastlaughterScore = 0.0f;
        public float[] FinalBlendShapes;
        public bool[] HasViseme;
        public bool HasVisemeLaughter;
        public int BlendShapeCount;
        public void Initialize(BasisAvatar avatar)
        {
            // Debug.Log("Initalizing " + nameof(BasisVisemeDriver));  
            Avatar = avatar;
            Smoothing = smoothAmount;
            BlendShapeCount = Avatar.FaceVisemeMovement.Length;
            FinalBlendShapes = new float[Enum.GetNames(typeof(Viseme)).Length];
            Array.Fill(FinalBlendShapes, -1);
            HasViseme = new bool[BlendShapeCount];
            for (int Index = 0; Index < BlendShapeCount; Index++)
            {
                if (Avatar.FaceVisemeMovement[Index] != -1)
                {
                    HasViseme[Index] = true;
                }
                else
                {
                    HasViseme[Index] = false;
                }
            }
            LastlaughterScore = -1;
            HasVisemeLaughter = laughterBlendTarget != -1;
        }
        public void EventLateUpdate()
        {
            if (Avatar != null)
            {
                if (Avatar.FaceVisemeMesh.isVisible)
                {
                    // get the current viseme frame
                    OVRLipSync.Frame frame = GetCurrentPhonemeFrame();
                    if (frame != null)
                    {
                        for (int Index = 0; Index < BlendShapeCount; Index++)
                        {
                            if (HasViseme[Index])
                            {
                                // Viseme blend weights are in range of 0->1.0, we need to make range 100
                                if (FinalBlendShapes[Index] != frame.Visemes[Index])
                                {
                                    float VisemeModified = frame.Visemes[Index] * 100.0f;
                                    Avatar.FaceVisemeMesh.SetBlendShapeWeight(Avatar.FaceVisemeMovement[Index], VisemeModified);
                                    FinalBlendShapes[Index] = frame.Visemes[Index];
                                }
                            }
                        }
                        if (HasVisemeLaughter)
                        {
                            // Laughter score will be raw classifier output in [0,1]
                            float laughterScore = frame.laughterScore;
                            if (LastlaughterScore != laughterScore)
                            {
                                // Threshold then re-map to [0,1]
                                laughterScore = laughterScore < laughterThreshold ? 0.0f : laughterScore - laughterThreshold;
                                laughterScore = Mathf.Min(laughterScore * laughterMultiplier, 1.0f);
                                laughterScore *= 1.0f / laughterThreshold;
                                float LaughterScoreModified = laughterScore * 100.0f;
                                Avatar.FaceVisemeMesh.SetBlendShapeWeight(laughterBlendTarget, LaughterScoreModified);
                                LastlaughterScore = laughterScore;
                            }
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
        }
        /// <summary>
        /// data is coming from the audio thread
        /// place that data into a seperate thread. false, no longer doing this.
        /// this way we dont bog down the audio thread. false, no longer doing this.
        /// it comes in every 20ms
        /// </summary>
        /// <param name="data"></param>
        public void ProcessAudioSamples(float[] data)
        {
            if (Avatar != null)
            {
                if (Avatar.FaceVisemeMesh.isVisible)
                {
                    if (Context == 0)
                    {
                        return;
                    }
                    OVRLipSync.ProcessFrame(Context, data, this.Frame, false);
                }
            }
        }
    }
}