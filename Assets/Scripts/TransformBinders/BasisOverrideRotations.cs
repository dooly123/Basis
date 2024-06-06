using UnityEngine;

public class BasisOverrideRotations : MonoBehaviour
{
    public BasisBoneControl Head;
    public BasisBoneControl Hips;
    public BasisBoneControl UpperChest;
    public BasisBoneControl Chest;
    public BasisBoneControl Spine;

    public float DelayedHips = 6f;
    public float DelayedUpperChest = 6f;
    public float DelayedChest = 6f;
    public float DelayedSpine = 6f;
    public float RangeOfMotionBeforeTurn = 13;
    public float roty;
    public BasisLocalBoneDriver Driver;
    /// <summary>
    /// this is used to simulate the rest of the body if no tracked is issued to said bonecontrol
    /// </summary>
    public void Simulate()
    {
        float DeltaTime = Time.deltaTime;

        roty = Head.LocalRawRotation.eulerAngles.y;
        Quaternion coreRotation = Quaternion.Euler(0, roty, 0);

        if (AngleCheck(coreRotation, Hips.LocalRawRotation, RangeOfMotionBeforeTurn))
        {
            // Slerp rotation for hips and upper body
            if (Hips.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Hips.LocalRawRotation = SlerpYRotation(Hips.LocalRawRotation, coreRotation, DelayedHips * DeltaTime);
            }
            if (UpperChest.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                UpperChest.LocalRawRotation = SlerpYRotation(UpperChest.LocalRawRotation, coreRotation, DelayedUpperChest * DeltaTime);
            }
            if (Chest.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Chest.LocalRawRotation = SlerpYRotation(Chest.LocalRawRotation, coreRotation, DelayedChest * DeltaTime);
            }
            if (Spine.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Spine.LocalRawRotation = SlerpYRotation(Spine.LocalRawRotation, coreRotation, DelayedSpine * DeltaTime);
            }
        }
    }

    private Quaternion SlerpYRotation(Quaternion from, Quaternion to, float t)
    {
        Vector3 fromEuler = from.eulerAngles;
        Vector3 toEuler = to.eulerAngles;
        Vector3 resultEuler = new Vector3(fromEuler.x, Mathf.LerpAngle(fromEuler.y, toEuler.y, t), fromEuler.z);
        return Quaternion.Euler(resultEuler);
    }
    public bool AngleCheck(Quaternion AngleA, Quaternion AngleB, float MaximumTolerance = 0.005f)
    {
        float Angle = Quaternion.Angle(AngleA, AngleB);
        bool AngleLargeEnough = Angle > MaximumTolerance;
        return AngleLargeEnough;
    }
    public void Initialize()
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver.FindBone(out Head, BasisBoneTrackedRole.Head))
        {
        }
        if (Driver.FindBone(out Hips, BasisBoneTrackedRole.Hips))
        {
            if (Hips.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Hips.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }

        if (Driver.FindBone(out UpperChest, BasisBoneTrackedRole.UpperChest))
        {
            if (UpperChest.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                UpperChest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }
        if (Driver.FindBone(out Chest, BasisBoneTrackedRole.Chest))
        {
            if (Chest.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Chest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }

        if (Driver.FindBone(out Spine, BasisBoneTrackedRole.Spine))
        {
            if (Spine.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Spine.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }
    }

    public void Update()
    {
        Simulate();
    }
}