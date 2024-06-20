using UnityEngine;

public class BasisInputXRSimulate : BasisInput
{
    public override void PollData()
    {
        if (Control.HasBone)
        {
            this.transform.GetLocalPositionAndRotation(out LocalRawPosition, out LocalRawRotation);
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
            {
                if (hasRoleAssigned)
                {
                    Control.TrackerData.Position = LocalRawPosition;
                }
            }
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
            {
                if (hasRoleAssigned)
                {
                    Control.TrackerData.Rotation = LocalRawRotation;
                }
            }
            UpdatePlayerControl();
        }
    }
}