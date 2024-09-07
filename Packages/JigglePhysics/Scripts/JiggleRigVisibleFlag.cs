using System;
using UnityEngine;

public class JiggleRigVisibleFlag : MonoBehaviour
{
    public int VisibleFlagIndex;
    public Action<bool, int> VisibilityChange;
    public void OnBecameInvisible()
    {
        VisibilityChange?.Invoke(false, VisibleFlagIndex);
    }
    public void OnBecameVisible()
    {
        VisibilityChange?.Invoke(true, VisibleFlagIndex);
    }
}