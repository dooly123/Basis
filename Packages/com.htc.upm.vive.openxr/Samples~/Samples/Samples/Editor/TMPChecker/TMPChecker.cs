#if !TMPExist
using UnityEditor;

public class TMPChecker
{
    [InitializeOnLoadMethod]
    static void CheckTextMeshProInstallation()
    {
        EditorUtility.DisplayDialog("TextMeshPro Not Found",
            "VIVE OpenXR Sample need TextMeshPro installed.",
            "OK");
    }
}
#endif