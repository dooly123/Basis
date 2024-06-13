using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasisTestFakeFullBodyTracking : MonoBehaviour
{
    public BasisLocalAvatarDriver LocalDriver;
    public BasisLocalBoneDriver Driver;
    [SerializeField]
    public List<Transform> FakeTransforms = new List<Transform>();
    [SerializeField]
    private List<Vector3> calibrationOffsetPosition = new List<Vector3>();
    [SerializeField]
    private List<Quaternion> calibrationOffsetRotation = new List<Quaternion>();
    public float randomRange = 0.1f;
    public int Length;
    public void OnEnable()
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        LocalDriver = BasisLocalPlayer.Instance.AvatarDriver;
        if (BasisAvatarEyeInput.Instance != null)
        {
            BasisAvatarEyeInput.Instance.enabled = false;
        }
        Initialize();
    }
    public void OnDisable()
    {
        if (BasisAvatarEyeInput.Instance != null)
        {
            BasisAvatarEyeInput.Instance.enabled = true;
        }
        DeInitialize();
    }
    public void Initialize()
    {
        RemoveLastRunOffset();
        Length = Driver.Controls.Length;
        calibrationOffsetPosition.Clear();
        calibrationOffsetRotation.Clear();
        foreach (BasisBoneControl control in Driver.Controls)
        {
            control.HasTrackerPositionDriver = BasisHasTracked.HasTracker;
            control.HasTrackerRotationDriver = BasisHasTracked.HasTracker;
            GameObject createdFakeBone = new GameObject(control.Name);
            createdFakeBone.transform.parent = transform;
            CreateFakeOffset(out Quaternion Rotation, out Vector3 Position, control);
            createdFakeBone.transform.SetPositionAndRotation(Position, Rotation);

            FakeTransforms.Add(createdFakeBone.transform);
            Quaternion calibrationQuaternion = Quaternion.Inverse(Rotation * control.LocalRawRotation);
            calibrationOffsetRotation.Add(calibrationQuaternion);
            calibrationOffsetPosition.Add(ComputeOffset(createdFakeBone, control));
        }
        Driver.OnSimulate += Simulate;
    }
    public void RemoveLastRunOffset()
    {
        foreach (Transform Fake in FakeTransforms)
        {
            GameObject.Destroy(Fake.gameObject);
        }
        FakeTransforms.Clear();
    }
    public void DeInitialize()
    {
        Driver.OnSimulate -= Simulate;
    }
    /// <summary>
    /// first we get the offset to get the local position
    /// then we get the difference and use it as a offset
    /// </summary>
    /// <param name="createdFakeBone"></param>
    /// <param name="control"></param>
    /// <returns></returns>
    public Vector3 ComputeOffset(GameObject createdFakeBone, BasisBoneControl control)
    {
        Vector3 OffsetFromAvatar = BasisLocalBoneDriver.ConvertToAvatarSpace(LocalDriver.LocalPlayer.Avatar.Animator, createdFakeBone.transform.position, LocalDriver.LocalPlayer.Avatar.AvatarHeightOffset, out control.WorldSpaceFloor);
        return OffsetFromAvatar - control.LocalRawPosition;
    }
    public void Simulate()
    {
        for (int index = 0; index < Length; index++)
        {
            BasisBoneControl control = Driver.Controls[index];
            if (control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker)
            {
                // Calculate the position offseted
                control.LocalRawPosition = FakeTransforms[index].position - calibrationOffsetPosition[index];

            }
            if (control.HasTrackerRotationDriver != BasisHasTracked.HasNoTracker)
            {
                if (calibrationOffsetRotation[index] != Quaternion.identity)
                {
                    // Calculate the rotated offset for the rotation
                    control.LocalRawRotation = FakeTransforms[index].rotation * calibrationOffsetRotation[index];
                }
                else
                {
                    control.LocalRawRotation = FakeTransforms[index].rotation;
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        for (int index = 0; index < Length; index++)
        {
            BasisBoneControl control = Driver.Controls[index];
            if (control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && control.HasTrackerRotationDriver != BasisHasTracked.HasNoTracker)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(FakeTransforms[index].position, 0.035f);
            }
        }
    }
    public void CreateFakeOffset(out Quaternion Rotation, out Vector3 Position, BasisBoneControl control)
    {
        Rotation = Random.rotation;
        Position = ModifyVector(control.LocalRawPosition);
    }
    Vector3 ModifyVector(Vector3 original)
    {
        float randomX = Random.Range(-randomRange, randomRange);
        float randomY = Random.Range(-randomRange, randomRange);
        float randomZ = Random.Range(-randomRange, randomRange);
        return new Vector3(original.x + randomX, original.y + randomY, original.z + randomZ);
    }
}