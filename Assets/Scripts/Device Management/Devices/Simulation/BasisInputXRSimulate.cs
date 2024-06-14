using UnityEngine;

public class BasisInputXRSimulate : BasisInput
{
    public override void PollData()
    {
        this.transform.GetLocalPositionAndRotation(out LocalRawPosition, out LocalRawRotation);
        if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
        {
            if (hasRoleAssigned)
            {
                Control.LocalRawPosition = LocalRawPosition;
            }
        }
        if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
        {
            if (hasRoleAssigned)
            {
                Control.LocalRawRotation = LocalRawRotation;
            }
        }
        UpdatePlayerControl();
    }
}