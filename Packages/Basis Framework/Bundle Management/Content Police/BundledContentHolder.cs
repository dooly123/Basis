using UnityEngine;

public class BundledContentHolder : MonoBehaviour
{
    public ContentPoliceSelector Selector;
    public BasisLoadableBundle DefaultScene;
    public BasisLoadableBundle DefaultAvatar;
    public static BundledContentHolder Instance;
    public void Awake()
    {
        Instance = this;
    }
}