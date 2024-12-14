using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class BasisVirtualSpineDriver
{
    [SerializeField] public BasisBoneControl CenterEye;
    [SerializeField] public BasisBoneControl Head;
    [SerializeField] public BasisBoneControl Neck;
    [SerializeField] public BasisBoneControl Chest;
    [SerializeField] public BasisBoneControl Spine;
    [SerializeField] public BasisBoneControl Hips;

    [SerializeField] public BasisBoneControl RightShoulder;
    [SerializeField] public BasisBoneControl LeftShoulder;

    [SerializeField] public BasisBoneControl LeftLowerArm;
    [SerializeField] public BasisBoneControl RightLowerArm;

    [SerializeField] public BasisBoneControl LeftLowerLeg;
    [SerializeField] public BasisBoneControl RightLowerLeg;

    [SerializeField] public BasisBoneControl LeftHand;
    [SerializeField] public BasisBoneControl RightHand;

    [SerializeField] public BasisBoneControl LeftFoot;
    [SerializeField] public BasisBoneControl RightFoot;

    // Define influence values (from 0 to 1)
    public float NeckRotationSpeed = 12;
    public float ChestRotationSpeed = 25;
    public float SpineRotationSpeed = 30;
    public float HipsRotationSpeed = 40;
    public float MaxNeckAngle = 0; // Limit the neck's rotation range to avoid extreme twisting
    public float MaxChestAngle = 0; // Limit the chest's rotation range
    public float MaxHipsAngle = 0; // Limit the hips' rotation range
    public float MaxSpineAngle = 0;
    public void Initialize()
    {
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out CenterEye, BasisBoneTrackedRole.CenterEye))
        {
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Head, BasisBoneTrackedRole.Head))
        {
        }

        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Neck, BasisBoneTrackedRole.Neck))
        {
            Neck.HasVirtualOverride = true;
            Neck.VirtualRun += OnSimulateNeck;
            BasisLocalPlayer.Instance.LocalBoneDriver.ReadyToRead.AddAction(30, Hint);
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Chest, BasisBoneTrackedRole.Chest))
        {
            Chest.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Spine, BasisBoneTrackedRole.Spine))
        {
            Spine.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Hips, BasisBoneTrackedRole.Hips))
        {
            Hips.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out LeftLowerArm, BasisBoneTrackedRole.LeftLowerArm))
        {
         //  LeftLowerArm.HasVirtualOverride = true;
        }

        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out RightLowerArm, BasisBoneTrackedRole.RightLowerArm))
        {
         //  RightLowerArm.HasVirtualOverride = true;
        }

        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out LeftLowerLeg, BasisBoneTrackedRole.LeftLowerLeg))
        {
         //  LeftLowerLeg.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out RightLowerLeg, BasisBoneTrackedRole.RightLowerLeg))
        {
            //RightLowerLeg.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out LeftHand, BasisBoneTrackedRole.LeftHand))
        {
            // LeftHand.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out RightHand, BasisBoneTrackedRole.RightHand))
        {
            //   RightHand.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out LeftFoot, BasisBoneTrackedRole.LeftFoot))
        {
            // LeftHand.HasVirtualOverride = true;
        }
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out RightFoot, BasisBoneTrackedRole.RightFoot))
        {
            //   RightHand.HasVirtualOverride = true;
        }
    }
    public void Hint()
    {
        /*
        if (LeftLowerLeg.HasTracked != BasisHasTracked.HasTracker)
        {
        //    LeftLowerLeg.BoneTransform.position = GenerateHintPosition();
       //     Vector3 hintPosition = GenerateHintPosition(root.position, mid.position, target.position);
        }
        if (RightLowerLeg.HasTracked != BasisHasTracked.HasTracker)
        {
            RightLowerLeg.BoneTransform.position = RightFoot.BoneTransform.position;
        }
        if (LeftLowerArm.HasTracked != BasisHasTracked.HasTracker)
        {
            LeftLowerArm.BoneTransform.position = LeftHand.BoneTransform.position;
        }
        if (LeftLowerArm.HasTracked != BasisHasTracked.HasTracker)
        {
            LeftLowerArm.BoneTransform.position = LeftHand.BoneTransform.position;
        }
        if (RightLowerArm.HasTracked != BasisHasTracked.HasTracker)
        {
            RightLowerArm.BoneTransform.position = RightHand.BoneTransform.position;
        }
        */
    }
    public void DeInitialize()
    {
        Neck.VirtualRun -= OnSimulateNeck;
        Neck.HasVirtualOverride = false;
        Chest.HasVirtualOverride = false;
        Hips.HasVirtualOverride = false;
        Spine.HasVirtualOverride = false;
    }
    public void OnSimulateNeck()
    {
        float time = BasisLocalPlayer.Instance.LocalBoneDriver.DeltaTime;
        // Smooth the neck rotation and clamp it to prevent unnatural flipping
        Quaternion targetNeckRotation = Quaternion.Slerp(Neck.OutGoingData.rotation, Head.OutGoingData.rotation, time * NeckRotationSpeed);
        Vector3 EulerNeckRotation = targetNeckRotation.eulerAngles;
        float clampedHeadPitch = Mathf.Clamp(targetNeckRotation.x, -MaxNeckAngle, MaxNeckAngle);
        Neck.OutGoingData.rotation = Quaternion.Euler(clampedHeadPitch, EulerNeckRotation.y, 0);

        // Now, apply the spine curve progressively:
        // The chest should not follow the head directly, it should follow the neck but with reduced influence.
        Quaternion targetChestRotation = Quaternion.Slerp(Chest.OutGoingData.rotation,Neck.OutGoingData.rotation,time * ChestRotationSpeed);
        Vector3 EulerChestRotation = targetChestRotation.eulerAngles;
        float clampedChestPitch = Mathf.Clamp(EulerChestRotation.x, -MaxChestAngle, MaxChestAngle);
        Chest.OutGoingData.rotation = Quaternion.Euler(clampedChestPitch, EulerChestRotation.y, 0);

        // The hips should stay upright, using chest rotation as a reference
        Quaternion targetSpineRotation = Quaternion.Slerp(Spine.OutGoingData.rotation, Chest.OutGoingData.rotation, time * SpineRotationSpeed);// Lesser influence for hips to remain more upright
        Vector3 targetSpineRotationEuler = targetSpineRotation.eulerAngles;
        float clampedSpinePitch = Mathf.Clamp(targetSpineRotationEuler.x, -MaxSpineAngle, MaxSpineAngle);
        Spine.OutGoingData.rotation = Quaternion.Euler(clampedSpinePitch, targetSpineRotationEuler.y, 0);

        // The hips should stay upright, using chest rotation as a reference
        Quaternion targetHipsRotation = Quaternion.Slerp(Hips.OutGoingData.rotation, Spine.OutGoingData.rotation, time * HipsRotationSpeed);// Lesser influence for hips to remain more upright
        Vector3 targetHipsRotationEuler = targetHipsRotation.eulerAngles;
        float clampedHipsPitch = Mathf.Clamp(targetHipsRotationEuler.x, -MaxHipsAngle, MaxHipsAngle);
        Hips.OutGoingData.rotation = Quaternion.Euler(clampedHipsPitch, targetHipsRotationEuler.y, 0);

        // Handle position control for each segment if targets are set (as before)
        ApplyPositionControl(Neck);
        ApplyPositionControl(Chest);
        ApplyPositionControl(Spine);
        ApplyPositionControl(Hips);
    }

    private void ApplyPositionControl(BasisBoneControl boneControl)
    {
        if (boneControl.TargetControl.HasTarget)
        {
            float3 customDirection = math.mul(boneControl.TargetControl.Target.OutGoingData.rotation, boneControl.TargetControl.Offset);
            boneControl.OutGoingData.position = boneControl.TargetControl.Target.OutGoingData.position + customDirection;
        }
    }
}