using UnityEngine;
public class BasisLocalCharacterBinder : BasisTransformBinder
{
    public BasisCharacterController Move;

    public Vector3 Position;
    public Quaternion Rotation;

    private float SkinModifiedHeight;

    private bool isMoveNull;
    private bool isRigNull;
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
    }
    /// <summary>
    /// this is not a great use a update loop but it allows for finer control & resolves many issues with up and down chain movements
    /// </summary>
    public void Update()
    {
        if (isMoveNull)
        {
         //   Debug.LogError("One or more references is null. Cannot update CharacterBinder.");
            return;
        }

        Move.transform.GetPositionAndRotation(out Position, out Rotation);

        SkinModifiedHeight = Move.characterController.skinWidth * 2;

        Position -= new Vector3(0, SkinModifiedHeight, 0);

        if (isRigNull == false)
        {
            Rig.SetPositionAndRotation(Position, Rotation);
        }
    }
}