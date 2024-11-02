using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.UI_Panels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BasisProgressReport;
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
        }
        public void OnDestroy()
        {
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport -= ProgresReport;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressComplete -= OnProgressComplete;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressStart -= OnProgressStart;
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

            // Ensure that actions are executed on the main thread
            lock (actions)
            {
                while (actions.Count > 0)
                {
                    actions.Dequeue()?.Invoke();
                }
            }
        }
        public Vector3 GeneratePoint()
        {
            return HipTarget.OutgoingWorldData.position + new Vector3(0, MouthTarget.TposeLocal.position.y / YHeightMultiplier, 0);
        }
    }
}