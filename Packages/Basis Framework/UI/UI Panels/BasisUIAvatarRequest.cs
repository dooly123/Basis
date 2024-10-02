using Basis.Scripts.Common;
using System.Collections.Generic;

public static class BasisUIAvatarRequest
{
    public static string AvatarRequests = "AvatarRequests.BAS";
    public static List<string> LocallyStoredAvatarUrls = new List<string>();
    public static bool IsInitalized = false;
    public static void LoadAllAvatars()
    {
        if (IsInitalized == false)
        {
            LocallyStoredAvatarUrls = BasisDataStore.LoadUrlList(AvatarRequests, LocallyStoredAvatarUrls);
            IsInitalized = true;
        }
    }
    public static bool Save(string AvatarUrl)
    {
        if (string.IsNullOrEmpty(AvatarUrl))
        {
            return false;
        }
        if (LocallyStoredAvatarUrls.Contains(AvatarUrl) == false)
        {
            LocallyStoredAvatarUrls.Add(AvatarUrl);
            BasisDataStore.SaveUrlList(LocallyStoredAvatarUrls, AvatarRequests);
            return true;
        }
        return false;
    }
}