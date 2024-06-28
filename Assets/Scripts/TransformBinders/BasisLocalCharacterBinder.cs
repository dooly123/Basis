using UnityEngine;
public class BasisLocalCharacterBinder : BasisTransformBinder
{
    public BasisCharacterController Move;
    private bool isMoveNull;
    private bool isRigNull;
    public bool AssignedReadyToRead = false;
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
        if (isMoveNull == false)
        {
            if (AssignedReadyToRead == false)
            {
                newMove.ReadyToRead += Simulate;
                AssignedReadyToRead = true;
            }
        }
    }
    public void OnDestroy()
    {
        if (AssignedReadyToRead)
        {
            if (Move != null)
            {
                Move.ReadyToRead -= Simulate;
            }
        }
    }
    /// <summary>
    /// this is not a great use a update loop but it allows for finer control & resolves many issues with up and down chain movements
    /// </summary>
    public void Simulate()
    {
        if (isRigNull == false)
        {
            Rig.SetPositionAndRotation(Move.OutPosition, Move.OutRotation);
        }
    }
}