using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.Drivers
{
    public class BasisFacialBlinkDriver : MonoBehaviour
    {
        public SkinnedMeshRenderer meshRenderer;
        public float minBlinkInterval = 5f;
        public float maxBlinkInterval = 25f;
        public float blinkDuration = 0.2f;
        public float visemeTransitionDuration = 0.05f;
        public List<int> blendShapeIndex = new List<int>();
        public int blendShapeCount = 0;

        private bool isBlinking = false;
        private float nextBlinkTime;
        private float blinkStartTime;
        private bool isVisemeClosing = false;
        private float visemeStartTime;

        public bool HasRendererCheckWiredUp = false;
        public BasisPlayer LinkedPlayer;
        public void Initialize(BasisPlayer Player, BasisAvatar Avatar)
        {
            LinkedPlayer = Player;
            blendShapeIndex.Clear();
            meshRenderer = Avatar.FaceBlinkMesh;
            foreach (int Blink in Avatar.BlinkViseme)
            {
                if (Blink != -1)
                {
                    blendShapeIndex.Add(Blink);
                }
            }
            blendShapeCount = blendShapeIndex.Count;
            // Start blinking
            SetNextBlinkTime();
            if (HasRendererCheckWiredUp == false)
            {
                if (LinkedPlayer != null && LinkedPlayer.FaceRenderer != null)
                {
                    BasisDebug.Log("Wired up Renderer Check For Blinking", BasisDebug.LogTag.Avatar);
                    LinkedPlayer.FaceRenderer.Check += UpdateFaceVisibility;
                    UpdateFaceVisibility(LinkedPlayer.FaceisVisible);
                    HasRendererCheckWiredUp = true;
                }
            }
        }
        public void OnDestroy()
        {
            if (HasRendererCheckWiredUp  && LinkedPlayer != null && LinkedPlayer.FaceRenderer != null)
            {
                LinkedPlayer.FaceRenderer.Check -= UpdateFaceVisibility;
            }
        }
        private void UpdateFaceVisibility(bool State)
        {
            enabled = State;
        }
        public static bool MeetsRequirements(BasisAvatar Avatar)
        {
            if (Avatar != null)
            {
                if (Avatar.FaceBlinkMesh != null)
                {
                    if (Avatar.BlinkViseme != null && Avatar.BlinkViseme.Length >= 1)
                    {
                        for (int Index = 0; Index < Avatar.BlinkViseme.Length; Index++)
                        {
                            int Blink = Avatar.BlinkViseme[Index];
                            if (Blink != -1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        void Update()
        {
            if (!isBlinking && Time.time >= nextBlinkTime)
            {
                StartBlink();
            }
            else if (isBlinking)
            {
                float Time = (UnityEngine.Time.time - blinkStartTime) / blinkDuration;
                float blendWeight = math.lerp(0, 100, Time);
                for (int Index = 0; Index < blendShapeCount; Index++)
                {
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex[Index], blendWeight);
                }
                if (Time >= 1f)
                {
                    FinishBlink();
                }
            }
            else if (isVisemeClosing)
            {
                float Time = (UnityEngine.Time.time - visemeStartTime) / visemeTransitionDuration;
                float blendWeight = Mathf.Lerp(100, 0, Time);
                for (int Index = 0; Index < blendShapeCount; Index++)
                {
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex[Index], blendWeight);
                }
                if (Time >= 1f)
                {
                    isVisemeClosing = false;
                }
            }
        }

        void SetNextBlinkTime()
        {
            nextBlinkTime = Time.time + UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
        }

        void StartBlink()
        {
            isBlinking = true;
            blinkStartTime = Time.time;
            // Trigger viseme animation for closing
            for (int Index = 0; Index < blendShapeCount; Index++)
            {
                meshRenderer.SetBlendShapeWeight(blendShapeIndex[Index], 0);
            }
            isVisemeClosing = true;
            visemeStartTime = Time.time;
        }

        void FinishBlink()
        {
            isBlinking = false;
            SetNextBlinkTime(); // Set next blink time after eyes open
        }
    }
}
