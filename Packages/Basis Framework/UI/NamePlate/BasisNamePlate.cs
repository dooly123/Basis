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
        [SerializeField] private float transitionDuration = 0.3f;
        // Delay before returning to NormalColor
        [SerializeField] private float returnDelay = 0.4f;
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
        } 
        // Store the current running coroutine to cancel it when needed
        private Coroutine colorTransitionCoroutine;
        private Coroutine returnToNormalCoroutine;
        public void OnAudioReceived(bool hasRealAudio)
        {
            // Determine the target color based on whether real audio is being received
            Color targetColor = hasRealAudio ? IsTalkingColor : NormalColor;

            // Stop any ongoing color transition
            if (colorTransitionCoroutine != null)
            {
                StopCoroutine(colorTransitionCoroutine);
            }

            // Start a new color transition coroutine
            colorTransitionCoroutine = StartCoroutine(TransitionColor(targetColor));
        }

        private IEnumerator TransitionColor(Color targetColor)
        {
            // Record the initial color at the start of the transition
            Color initialColor = namePlateImage.color;
            float elapsedTime = 0f;

            // Perform the transition over the specified duration
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                // Lerp from the initial color to the target color
                namePlateImage.color = Color.Lerp(initialColor, targetColor, t);

                yield return null; // Wait until the next frame
            }

            // Set the final color to the target color
            namePlateImage.color = targetColor;

            // Clear the coroutine reference as the transition is complete
            colorTransitionCoroutine = null;

            // If we reached IsTalkingColor and should transition back, start the delayed return coroutine
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
            // Wait for the specified delay
            yield return new WaitForSeconds(returnDelay);

            // Smoothly transition back to NormalColor over transitionDuration
            Color initialColor = namePlateImage.color;
            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                // Lerp from the initial color back to NormalColor
                namePlateImage.color = Color.Lerp(initialColor, NormalColor, t);

                yield return null; // Wait until the next frame
            }

            // Ensure the final color is set to NormalColor
            namePlateImage.color = NormalColor;

            // Clear the coroutine reference as the pullback is complete
            returnToNormalCoroutine = null;
        }
        public void OnDestroy()
        {
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport -= ProgresReport;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressComplete -= OnProgressComplete;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressStart -= OnProgressStart;
            BasisRemotePlayer.AudioReceived -= OnAudioReceived;
        }

        private void OnProgressStart()
        {            // Add the action to the queue to be executed on the main thread
            EnqueueOnMainThread(() =>
            {
                Debug.Log("progress bar starting");
                Loadingbar.gameObject.SetActive(true);
                Loadingtext.gameObject.SetActive(true);
            });
        }

        private void OnProgressComplete()
        {            // Add the action to the queue to be executed on the main thread
            EnqueueOnMainThread(() =>
            {
                Debug.Log("progress bar ending");
                Loadingtext.gameObject.SetActive(false);
                Loadingbar.gameObject.SetActive(false);
            });
        }

        public void ProgresReport(float progress,string info)
        {
            // Add the action to the queue to be executed on the main thread
            EnqueueOnMainThread(() =>
            {
                Loadingtext.text = info;
               // Debug.Log("updating progress bar " + progress + " | " + info);
                UpdateProgressBar(progress);
            });
        }
        // This method will queue the action to be executed on the main thread
        private static void EnqueueOnMainThread(Action action)
        {
            lock (actions)
            {
                actions.Enqueue(action);
            }
        }
        public void UpdateProgressBar(float progress)
        {
            Loadingbar.rectTransform.localScale = new Vector3(progress/100, 1f, 1f);
        }
        private static readonly Queue<Action> actions = new Queue<Action>();
        private void Update()
        {
            // Get the direction to the camera
            directionToCamera = LocalCameraDriver.position - transform.position;
            transform.SetPositionAndRotation(
                GeneratePoint(),
                Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Atan2(directionToCamera.x, directionToCamera.z)
                * Mathf.Rad2Deg, transform.rotation.eulerAngles.z));

            // Only lock and execute actions if there are actions to process
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
            return HipTarget.OutgoingWorldData.position + new Vector3(0, MouthTarget.TposeLocal.position.y / YHeightMultiplier, 0);
        }
    }
}