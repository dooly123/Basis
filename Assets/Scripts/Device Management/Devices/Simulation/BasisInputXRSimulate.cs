#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class BasisInputXRSimulate : BasisInput
{
    public override void PollData()
    {
        FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
        FinalRotation = LocalRawRotation;
        if (hasRoleAssigned)
        {
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && FinalPosition != Vector3.zero)
            {
                Control.TrackerData.position = FinalPosition - FinalRotation * pivotOffset;
            }
            if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && FinalRotation != Quaternion.identity)
            {
                Control.TrackerData.rotation = FinalRotation * Quaternion.Euler(BasisDeviceMatchableNames.RotationOffset);//normally its rotation offset but this is to debug
            }
        }
        UpdatePlayerControl();
        transform.SetLocalPositionAndRotation(FinalPosition, FinalRotation);
    }
}
#if UNITY_EDITOR
public class BasisInputXRSimulateEditor : Editor
{
    private void OnSceneGUI()
    {
        BasisInputXRSimulate simulate = (BasisInputXRSimulate)target;

        EditorGUI.BeginChangeCheck();

        Vector3 newLocalRawPosition = Handles.PositionHandle(simulate.LocalRawPosition + BasisLocalPlayer.Instance.LocalBoneDriver.transform.position, Quaternion.identity);
        Quaternion newLocalRawRotation = Handles.RotationHandle(simulate.LocalRawRotation, simulate.LocalRawPosition + BasisLocalPlayer.Instance.LocalBoneDriver.transform.position);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(simulate, "Move and Rotate Basis Input");
            simulate.LocalRawPosition = newLocalRawPosition - BasisLocalPlayer.Instance.LocalBoneDriver.transform.position;
            simulate.LocalRawRotation = newLocalRawRotation;
            simulate.PollData();
        }
    }
}
#endif