using UnityEngine;

// Define the ScriptableObject
[CreateAssetMenu(fileName = "NewBasisAssetBundleObject", menuName = "Basis/ScriptableObjects/BasisAssetBundleObject", order = 1)]
public class BasisAssetBundleObject : ScriptableObject
{
    public string ExportDirectory = "Packages/com.basis.basisdk/TemporaryStorage";

    public void SetAssetBundleData(AssetBundleManifest Bundle)
    {
      //  Bundle.
    }
}