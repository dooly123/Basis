using UnityEngine;

public class BasisInputXRSimulate : BasisInput
{
    public Transform FollowMovement;
    public override void PollData()
    {
        FollowMovement.GetLocalPositionAndRotation(out LocalRawPosition, out LocalRawRotation);
        LocalRawPosition = LocalRawPosition / BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;

        FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
        FinalRotation = LocalRawRotation;
        if (hasRoleAssigned)
        {
            if (Control.HasTracked != BasisHasTracked.HasNoTracker)
            {
                AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;//normally we dont do this but im doing it so we can see direct colliation
                Control.TrackerData.position = FinalPosition - FinalRotation * AvatarPositionOffset;
            }
            if (Control.HasTracked != BasisHasTracked.HasNoTracker)
            {
                AvatarRotationOffset = Quaternion.Euler(BasisDeviceMatchableNames.AvatarRotationOffset);//normally we dont do this but im doing it so we can see direct colliation
                Control.TrackerData.rotation = FinalRotation * AvatarRotationOffset;
            }


        }
        UpdatePlayerControl();
        transform.SetLocalPositionAndRotation(FinalPosition, FinalRotation);
    }
}