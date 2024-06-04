using UnityEngine;
public static class AddressableDebug
{
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    public static void DebugError(string message, AddressableLoadResourceBase loadStruct)
    {
        loadStruct.OnLoadFailure.Invoke(message);
        Debug.LogError(message);
    }
}