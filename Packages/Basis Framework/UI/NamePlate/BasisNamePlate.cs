using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.UI_Panels;
using System;
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
        public float YHeightMultiplier = 1.25f;
        public bool HasActiveLoadingbar = false;
        public BasisRemotePlayer BasisRemotePlayer;
        public void Initalize(BasisBoneControl hipTarget, BasisRemotePlayer basisRemotePlayer)
        {
            BasisRemotePlayer = basisRemotePlayer;
            HipTarget = hipTarget;
            MouthTarget = BasisRemotePlayer.MouthControl;
            LocalCameraDriver = BasisLocalCameraDriver.Instance.transform;
            Text.text = BasisRemotePlayer.DisplayName;
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport += ProgresReport;
        }
        public void OnDestroy()
        {
            BasisRemotePlayer.ProgressReportAvatarLoad.OnProgressReport -= ProgresReport;
        }
        public void ProgresReport(float progress)
        {
            // Add the action to the queue to be executed on the main thread
            EnqueueOnMainThread(() =>
            {
                if (progress != 100)
                {
                    if (!HasActiveLoadingbar)
                    {
                        StartProgressBar();
                    }

                    UpdateProgressBar(progress);
                }
                else
                {
                    StopProgressBar();
                }
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

        public void StartProgressBar()
        {
            Loadingbar.gameObject.SetActive(true);
            HasActiveLoadingbar = true;
        }
        public void UpdateProgressBar(float progress)
        {
            Loadingbar.rectTransform.localScale = new Vector3(progress/100, 1f, 1f);
        }
        public void StopProgressBar()
        {
            Loadingbar.gameObject.SetActive(false);
            HasActiveLoadingbar = false;
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