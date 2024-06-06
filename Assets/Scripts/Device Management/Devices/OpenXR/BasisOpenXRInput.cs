using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;
[DefaultExecutionOrder(15101)]
public class BasisOpenXRInput : MonoBehaviour
{
    public UnityEngine.XR.InputDevice Device;
    public BasisLocalBoneDriver Driver;
    public BasisBoneTrackedRole Type;
    public BasisBoneControl Control;
    public string ID;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;
    public void OnDisable()
    {
        DisableTracking();
    }
    public void Initialize(UnityEngine.XR.InputDevice device, string iD)
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        ID = iD;
        Device = device;
        if (Device.characteristics == Characteristics.hmd)
        {
            Type = BasisBoneTrackedRole.CenterEye;
        }
        else
        {
            if (Device.characteristics == Characteristics.leftController || Device.characteristics == Characteristics.leftTrackedHand)
            {
                Type = BasisBoneTrackedRole.LeftHand;
            }
            else
            {
                if (Device.characteristics == Characteristics.rightController || Device.characteristics == Characteristics.rightTrackedHand)
                {
                    Type = BasisBoneTrackedRole.RightHand;
                }
            }
        }
        ActivateTracking();
    }
    public void ActivateTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        if (Driver.FindBone(out Control, Type))
        {

        }
        Driver.OnSimulate += SetPosRot;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasVRTracker);
    }
    public void DisableTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        Driver.OnSimulate -= SetPosRot;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasNoTracker);
    }
    public Vector2 primary2DAxis;
    public Vector2 secondary2DAxis;
    public bool gripButton;
    public bool menuButton;
    public bool primaryButton;
    public bool secondaryButton;
    public float Trigger;
    public bool secondary2DAxisClick;
    public bool primary2DAxisClick;
    public void SetPosRot()
    {
        if (Device.isValid)
        {
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
            {
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker)
                {
                    if (LocalRawPosition != Vector3.zero)
                    {
                        Control.LocalRawPosition = LocalRawPosition;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out LocalRawRotation))
            {
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker)
                {
                    if (LocalRawRotation != Quaternion.identity)
                    {
                        Control.LocalRawRotation = LocalRawRotation;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxis))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxis, out secondary2DAxis))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menuButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out Trigger))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxisClick, out secondary2DAxisClick))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out primary2DAxisClick))
            {
            }
            if (Type == BasisBoneTrackedRole.LeftHand)
            {
                BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxis;
                if (primaryButton)
                {
                    BasisLocalPlayer.Instance.Move.HandleJump();
                }
                if (secondaryButton)
                {
                    if (BasisHamburgerMenu.Instance == null)
                    {
                        if (BasisHamburgerMenu.IsLoading == false)
                        {
                            BasisHamburgerMenu.OpenMenu();
                        }
                    }
                    else
                    {
                        BasisHamburgerMenu.Instance.CloseThisMenu();
                    }
                }
            }
            else
            {
                if (Type == BasisBoneTrackedRole.RightHand)
                {
                    BasisLocalPlayer.Instance.Move.Rotation = primary2DAxis;
                }
            }
        }
        this.transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
    public void SetRealTrackers(BasisBoneControl.BasisHasTracked value)
    {
        if (Driver.FindBone(out BasisBoneControl Control, Type))
        {
            Control.HasTrackerPositionDriver = value;
            Control.HasTrackerRotationDriver = value;
        }
    }
}