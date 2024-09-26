using Basis.Scripts.Common;
using System.Collections.Generic;

public static class BasisUIAvatarRequest
{
    public static string AvatarRequests = "/AvatarRequests/AvatarRequests.BAS";
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
    public static void AddAvatarURL(string URL)
    {
        Save(URL);
    }
    private static bool Save(string AvatarUrl)
    {
        if (LocallyStoredAvatarUrls.Contains(AvatarUrl) == false)
        {
            BasisDataStore.SaveUrlList(LocallyStoredAvatarUrls, AvatarRequests);
            return true;
        }
        return false;
    }
}