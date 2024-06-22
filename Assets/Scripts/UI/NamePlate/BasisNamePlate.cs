using UnityEngine;

public abstract class BasisNamePlate : MonoBehaviour
{
    public BasisUIComponent BasisUIComponent;
    public Transform LocalCameraDriver;
    public Vector3 directionToCamera;
    public BasisBoneControl HipTarget;
    public BasisBoneControl MouthTarget;
    public Vector3 Offset = new Vector3(0, -0.5f, 0f);
    public void Initalize(BasisBoneControl hipTarget, BasisBoneControl mouthTarget)
    {
        HipTarget = hipTarget;
        MouthTarget = mouthTarget;
        LocalCameraDriver = BasisLocalCameraDriver.Instance.transform;
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
      return HipTarget.BoneTransform.position + MouthTarget.RestingLocalSpace.Position + Offset;
    }
}