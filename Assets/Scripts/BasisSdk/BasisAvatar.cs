
using System.Collections.Generic;
using UnityEngine;
public class BasisAvatar : MonoBehaviour
{
    public Animator Animator;
    public SkinnedMeshRenderer FaceVisemeMesh;
    public SkinnedMeshRenderer FaceBlinkMesh;
    public Vector2 AvatarEyePosition;
    public Vector2 AvatarMouthPosition;
    public int[] FaceVisemeMovement = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    public int[] BlinkViseme = new int[] { -1 };
    public int laughterBlendTarget = -1;
    [SerializeField]
    public List<Renderer> Renders = new List<Renderer>();
    [SerializeField]
    public List<BasisJiggleStrain> JiggleStrains;
}