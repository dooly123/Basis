using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.UI_Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Basis.Scripts.UI.NamePlate
{
    public abstract class BasisNamePlate : MonoBehaviour
    {
        public BasisUIComponent BasisUIComponent;
        public Transform LocalCameraDriver;
        public Vector3 directionToCamera;
        public BasisBoneControl HipTarget;
        public BasisBoneControl MouthTarget;
        public TextMeshProUGUI Text;
        public Image Loadingbar;
        public TextMeshProUGUI Loadingtext;
        public float YHeightMultiplier = 1.25f;
        public BasisRemotePlayer BasisRemotePlayer;
        public Button NamePlateButton;
        public Image namePlateImage;
        public Color NormalColor;
        public Color IsTalkingColor;
        [SerializeField] 
        private float transitionDuration = 0.3f;
        [SerializeField] 
        private float returnDelay = 0.4f;

        private Coroutine colorTransitionCoroutine;
        private Coroutine returnToNormalCoroutine;
        private static readonly Queue<Action> actions = new Queue<Action>();
        private static Vector3 cachedDirection;
        private static Quaternion cachedRotation;
        public bool HasRendererCheckWiredUp = false;
        public bool IsVisible = true;
        public void Initalize(BasisBoneControl hipTarget, BasisRemotePlayer basisRemotePlayer)
        {
            BasisRemotePlayer = basisRemotePlayer;
            HipTarget = hipTarget;
            MouthTarget = BasisRemotePlayer.MouthControl;
            LocalCameraDriver = BasisLocalCameraDriver.Instance.transform;
            Text.text = BasisRemotePlayer.DisplayName;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport += ProgresReport;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressComplete += OnProgressComplete;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressStart += OnProgressStart;
            BasisRemotePlayer.AudioReceived += OnAudioReceived;
            BasisRemotePlayer.OnAvatarSwitched += RebuildRenderCheck;
            BasisRemotePlayer.OnAvatarSwitchedFallBack += RebuildRenderCheck;
        }
        public void RebuildRenderCheck()
        {
            if (HasRendererCheckWiredUp)
            {
                DeInitalizeCallToRender();
            }
            HasRendererCheckWiredUp = false;
            if (BasisRemotePlayer != null && BasisRemotePlayer.FaceRenderer != null)
            {
                Debug.Log("Wired up Renderer Check For Blinking");
                BasisRemotePlayer.FaceRenderer.Check += UpdateFaceVisibility;
                UpdateFaceVisibility(BasisRemotePlayer.FaceisVisible);
                HasRendererCheckWiredUp = true;
            }
        }
        private void UpdateFaceVisibility(bool State)
        {
            IsVisible = State;
            gameObject.SetActive(State);
            if (IsVisible == false)
            {
                if (returnToNormalCoroutine != null)
                {
                    StopCoroutine(returnToNormalCoroutine);
                }
                if (colorTransitionCoroutine != null)
                {
                    StopCoroutine(colorTransitionCoroutine);
                }
            }
        }
                    
        public void OnAudioReceived(bool hasRealAudio)
        {
            if (IsVisible)
            {
                Color targetColor = hasRealAudio ? IsTalkingColor : NormalColor;

                if (colorTransitionCoroutine != null)
                {
                    StopCoroutine(colorTransitionCoroutine);
                }

                colorTransitionCoroutine = StartCoroutine(TransitionColor(targetColor));
            }
        }

        private IEnumerator TransitionColor(Color targetColor)
        {
            Color initialColor = namePlateImage.color;
            float elapsedTime = 0f;
            float lerpProgress = 0f;

            // Only write to namePlateImage.color when necessary to avoid redundant writes
            while (lerpProgress < 1f)
            {
                elapsedTime += Time.deltaTime;
                lerpProgress = Mathf.Clamp01(elapsedTime / transitionDuration);

                // Only update the color if there is a meaningful change
                Color newColor = Color.Lerp(initialColor, targetColor, lerpProgress);
                namePlateImage.color = newColor;
                yield return null;
            }

            // Ensure the final color is exactly the target color
            namePlateImage.color = targetColor;
            colorTransitionCoroutine = null;

            // If we need to transition back, start the delayed return coroutine
            if (targetColor == IsTalkingColor)
            {
                if (returnToNormalCoroutine != null)
                {
                    StopCoroutine(returnToNormalCoroutine);
                }
                returnToNormalCoroutine = StartCoroutine(DelayedReturnToNormal());
            }
        }

        private IEnumerator DelayedReturnToNormal()
        {
            yield return new WaitForSeconds(returnDelay);
            yield return StartCoroutine(TransitionColor(NormalColor));
            returnToNormalCoroutine = null;
        }

        public void OnDestroy()
        {
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport -= ProgresReport;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressComplete -= OnProgressComplete;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressStart -= OnProgressStart;
            BasisRemotePlayer.AudioReceived -= OnAudioReceived;
            DeInitalizeCallToRender();
        }
        public void DeInitalizeCallToRender()
        {
            if (HasRendererCheckWiredUp && BasisRemotePlayer != null && BasisRemotePlayer.FaceRenderer != null)
            {
                BasisRemotePlayer.FaceRenderer.Check -= UpdateFaceVisibility;
            }
        }
        private void OnProgressStart()
        {
            EnqueueOnMainThread(() =>
            {
                Loadingbar.gameObject.SetActive(true);
                Loadingtext.gameObject.SetActive(true);
            });
        }

        private void OnProgressComplete()
        {
            EnqueueOnMainThread(() =>
            {
                Loadingtext.gameObject.SetActive(false);
                Loadingbar.gameObject.SetActive(false);
            });
        }

        public void ProgresReport(float progress, string info)
        {
            EnqueueOnMainThread(() =>
            {
                Loadingtext.text = info;
                UpdateProgressBar(progress);
            });
        }

        private static void EnqueueOnMainThread(Action action)
        {
            lock (actions)
            {
                actions.Enqueue(action);
            }
        }

        public void UpdateProgressBar(float progress)
        {
            Vector3 scale = Loadingbar.rectTransform.localScale;
            scale.x = progress / 100;
            Loadingbar.rectTransform.localScale = scale;
        }

        private void LateUpdate()
        {
            directionToCamera = LocalCameraDriver.position - transform.position;

            cachedRotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                Mathf.Atan2(directionToCamera.x, directionToCamera.z) * Mathf.Rad2Deg,
                transform.rotation.eulerAngles.z
            );

            transform.SetPositionAndRotation(GeneratePoint(), cachedRotation);

            if (actions.Count > 0)
            {
                lock (actions)
                {
                    while (actions.Count > 0)
                    {
                        actions.Dequeue()?.Invoke();
                    }
                }
            }
        }

        public Vector3 GeneratePoint()
        {
            cachedDirection = HipTarget.OutgoingWorldData.position;
            cachedDirection.y += MouthTarget.TposeLocal.position.y / YHeightMultiplier;
            return cachedDirection;
        }
    }
}