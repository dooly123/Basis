using System.IO;
public static class BasisAddressableRenameExtensions
{
    public static string[] Renameable = new string[] { ".json", ".hash" };
    public static string[] IgnoredExtensions = new string[] { ".cs", ".dll" };
    public static string[] ContainsPath = new string[] { "/Editor/" };
    public static bool IsApproved(string Validation)
    {
        string PathExtension = Path.GetExtension(Validation);
        foreach (string Extension in IgnoredExtensions)
        {
            if (PathExtension == Extension)
            {
                return false;
            }
        }
        foreach (string PathPart in ContainsPath)
        {
            if (Validation.Contains(PathPart))
            {
                return false;
            }
        }
        return true;
    }
}