using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI.UI_Panels;
using System;
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
            BasisRemotePlayer.ProgressReportAvatarLoad += ProgresReport;
        }
        public void OnDestroy()
        {
            BasisRemotePlayer.ProgressReportAvatarLoad -= ProgresReport;
        }
        private void ProgresReport(float progress)
        {
            if (progress != 100)
            {
                if (HasActiveLoadingbar == false)
                {
                    StartProgressBar();
                }

                UpdateProgressBar(progress);
            }
            else
            {
                StopProgressBar();
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
        private void Update()
        {
            // Get the direction to the camera
            directionToCamera = LocalCameraDriver.position - transform.position;
            transform.SetPositionAndRotation(
                GeneratePoint(),
                Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Atan2(directionToCamera.x, directionToCamera.z)
                * Mathf.Rad2Deg, transform.rotation.eulerAngles.z));
        }
        public Vector3 GeneratePoint()
        {
            return HipTarget.OutgoingWorldData.position + new Vector3(0, MouthTarget.TposeLocal.position.y / YHeightMultiplier, 0);
        }
    }
}