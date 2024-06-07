using UnityEngine;

public abstract class BasisInput : MonoBehaviour
{
    public BasisLocalBoneDriver Driver;
    private BasisBoneTrackedRole TrackedRole;
    public bool hasRoleAssigned = false;
    public BasisBoneControl Control;
    public string ID;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;

    public Vector2 primary2DAxis;
    public Vector2 secondary2DAxis;
    public bool gripButton;
    public bool menuButton;
    public bool primaryButton;
    public bool secondaryButton;
    public float Trigger;
    public bool secondary2DAxisClick;
    public bool primary2DAxisClick;

    public BasisBoneTrackedRole Type
    {
        get => TrackedRole;
        set
        {
            TrackedRole = value;
            hasRoleAssigned = true;
        }
    }

    public void OnDisable()
    {
        DisableTracking();
    }
    public void OnDestroy()
    {
        DisableTracking();
    }

    public virtual void Initialize(string iD)
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        ID = iD;
        ActivateTracking();
    }

    public void ActivateTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        if (hasRoleAssigned && Driver.FindBone(out Control, Type))
        {
            Control.HasRigLayerPositionDriver = BasisBoneControl.BasisHasRigLayer.HasRigLayer;
            Control.HasRigLayerRotationDriver = BasisBoneControl.BasisHasRigLayer.HasRigLayer;
            // Do nothing if bone is found successfully
        }
        Driver.OnSimulate += PollData;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasTracker, BasisBoneControl.BasisHasRigLayer.HasRigLayer);
    }

    public void DisableTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        Driver.OnSimulate -= PollData;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasNoTracker, BasisBoneControl.BasisHasRigLayer.HasNoRigLayer);
    }

    public abstract void PollData();

    public void SetRealTrackers(BasisBoneControl.BasisHasTracked hasTracked, BasisBoneControl.BasisHasRigLayer HasLayer)
    {
        if (Driver.FindBone(out Control, Type))
        {
            Control.HasTrackerPositionDriver = hasTracked;
            Control.HasTrackerRotationDriver = hasTracked;
            Control.HasRigLayerPositionDriver = HasLayer;
            Control.HasRigLayerRotationDriver = HasLayer;
        }
    }
}