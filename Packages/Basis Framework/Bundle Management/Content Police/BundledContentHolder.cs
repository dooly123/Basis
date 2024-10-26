using UnityEngine;

public class BundledContentHolder : MonoBehaviour
{
    public ContentPoliceSelector Selector;
    public BasisLoadableBundle DefaultScene;
    public BasisLoadableBundle DefaultAvatar;
    public static BundledContentHolder Instance;
    public bool UseAddressables;
    public void Awake()
    {
        Instance = this;
    }
}