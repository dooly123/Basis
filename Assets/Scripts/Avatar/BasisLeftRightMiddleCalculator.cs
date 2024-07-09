using UnityEngine;
using static BasisAvatarIKStageCalibration;
using static BasisAvatarIKStageCalibration.BasisTrackerMapping;

public static class BasisLeftRightMiddleCalculator
{
    /*
    public static void SetSpot(CalibrationConnector calibrationConnector)
    {
        if (BasisAvatarDriver.IsApartOfSpineVertical(calibrationConnector.BasisBoneControlRole))
        {
          //  calibrationConnector.GeneralLocation = GeneralLocation.Middle;
        }
        else
        {
            if (IsRight(calibrationConnector.BasisBoneControl.BoneModelTransform, BasisLocalCameraDriver.Instance.Camera.transform))
            {
               // calibrationConnector.GeneralLocation = GeneralLocation.Right;
            }
            else
            {
               //calibrationConnector.GeneralLocation = GeneralLocation.Left;
            }
        }
    }
    */
    public static bool IsRight(Transform TransformRightCheck, Transform avatarMiddle)
    {
        Vector3 directionToOther = TransformRightCheck.position - avatarMiddle.position;
        // Vector3 avatarForward = avatarMiddle.forward;
        Vector3 avatarRight = avatarMiddle.right;
        // Check if the other transform is to the left or right
        float dotProduct = Vector3.Dot(avatarRight, directionToOther);

        if (dotProduct > 0)
        {
            //  Debug.Log(TransformRightCheck.name + " is on the right.");
            return true;
        }
        else if (dotProduct < 0)
        {
            // Debug.Log(TransformRightCheck.name + " is on the left.");
            return false;
        }
        else
        {
            //  Debug.Log(TransformRightCheck.name + " is directly in front or behind.");
        }
        return false;
    }
    public static bool CheckForNextPriority6Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.Chest || Role == BasisBoneTrackedRole.LeftUpperLeg || Role == BasisBoneTrackedRole.RightUpperLeg);
    }
    public static bool CheckForNextPriority9Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.UpperChest || Role == BasisBoneTrackedRole.LeftUpperArm || Role == BasisBoneTrackedRole.RightUpperArm);
    }
    public static bool CheckForNextPriority11Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.LeftToes || Role == BasisBoneTrackedRole.RightToes || Role == BasisBoneTrackedRole.Neck);
    }
    public static bool CheckForNextPriority13Point(BasisBoneTrackedRole Role)
    {
        return true;
    }
    public static bool ReplaceMeOnceyouhaveTime(BasisBoneTrackedRole Role)
    {
        if (Role == BasisBoneTrackedRole.Head || Role == BasisBoneTrackedRole.Neck || Role == BasisBoneTrackedRole.CenterEye || Role == BasisBoneTrackedRole.Mouth)
        {
            return false;
        }
        return true;
    }
    public static bool DisableAsIhaveNotImplemented(BasisBoneTrackedRole Role)
    {
        if (Role == BasisBoneTrackedRole.Chest || Role == BasisBoneTrackedRole.UpperChest || Role == BasisBoneTrackedRole.Spine)
        {
            return false;
        }
        return true;
    }
    /*        List<CalibrationConnector> Left = new List<CalibrationConnector>();
        List<CalibrationConnector> Right = new List<CalibrationConnector>();
        List<CalibrationConnector> Middle = new List<CalibrationConnector>();

        foreach (CalibrationConnector Connector in availableBoneControl)
        {
            if (Connector.GeneralLocation == GeneralLocation.Left)
            {
                Left.Add(Connector);
            }
            else
            {
                if (Connector.GeneralLocation == GeneralLocation.Right)
                {
                    Right.Add(Connector);
                }
                else
                {
                    if (Connector.GeneralLocation == GeneralLocation.Middle)
                    {
                        Middle.Add(Connector);
                    }
                }
            }
        }
     */
}
