using UnityEngine;

public class BasisInputXRSimulate : BasisInput
{
    public override void PollData()
    {
        if (Control.HasBone)
        {
            this.transform.GetLocalPositionAndRotation(out LocalRawPosition, out LocalRawRotation);
            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
            FinalRotation = LocalRawRotation;
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && FinalPosition != Vector3.zero)
            {
                if (hasRoleAssigned)
                {
                    Control.TrackerData.position = FinalPosition - FinalRotation * pivotOffset;
                }
            }
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && FinalRotation != Quaternion.identity)
            {
                if (hasRoleAssigned)
                {
                    Control.TrackerData.rotation = FinalRotation * rotationOffset;
                }
            }
            UpdatePlayerControl();
        }
    }
}