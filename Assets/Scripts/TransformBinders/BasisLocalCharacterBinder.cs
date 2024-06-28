using UnityEngine;
public class BasisLocalCharacterBinder : BasisTransformBinder
{
    public BasisCharacterController Move;

    public Vector3 Position;
    public Quaternion Rotation;

    private float SkinModifiedHeight;

    private bool isMoveNull;
    private bool isRigNull;
    public BasisLocalBoneDriver Driver;
    public void OnEnable()
    {
        SetRigTransform(this.transform);
        SetCharacterControllerMove(Move);
    }
    public void SetRigTransform(Transform newTransform)
    {
        Rig = newTransform;
        isRigNull = Rig == null;
    }
    public void SetCharacterControllerMove(BasisCharacterController newMove)
    {
        Move = newMove;
        isMoveNull = Move == null;
        if (BasisLocalPlayer.Instance != null && isMoveNull == false)
        {
            Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
            if (AssignedReadyToRead)
            {
                Driver.ReadyToRead -= Simulate;
            }
            Driver.ReadyToRead += Simulate;
            AssignedReadyToRead = true;
        }
    }
    public void OnDestroy()
    {
        if (AssignedReadyToRead)
        {
            if (Driver != null)
            {
                Driver.ReadyToRead -= Simulate;
            }
        }
    }
    public bool AssignedReadyToRead = false;
    /// <summary>
    /// this is not a great use a update loop but it allows for finer control & resolves many issues with up and down chain movements
    /// </summary>
    public void Simulate()
    {
        Move.transform.GetPositionAndRotation(out Position, out Rotation);

        SkinModifiedHeight = Move.characterController.skinWidth * 2;

        Position -= new Vector3(0, SkinModifiedHeight, 0);

        if (isRigNull == false)
        {
            Rig.SetPositionAndRotation(Position, Rotation);
        }
    }
}