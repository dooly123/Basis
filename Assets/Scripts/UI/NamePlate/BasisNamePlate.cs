using TMPro;
using UnityEngine;

public abstract class BasisNamePlate : MonoBehaviour
{
    public BasisUIComponent BasisUIComponent;
    public Transform LocalCameraDriver;
    public Vector3 directionToCamera;
    public BasisBoneControl HipTarget;
    public BasisBoneControl MouthTarget;
    public float FloatOffset = -0.5f;
    public TextMeshProUGUI Text;
    public void Initalize(BasisBoneControl hipTarget, BasisRemotePlayer BasisRemotePlayer)
    {
        HipTarget = hipTarget;
        MouthTarget = BasisRemotePlayer.MouthControl;
        LocalCameraDriver = BasisLocalCameraDriver.Instance.transform;
        Text.text = BasisRemotePlayer.DisplayName;
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
        return HipTarget.BoneTransform.position + new Vector3(0, MouthTarget.RestingLocalSpace.Position.y + FloatOffset, 0);
    }
}