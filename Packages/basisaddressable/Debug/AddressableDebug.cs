using UnityEngine;

namespace Basis.Scripts.Addressable_Driver.Debug
{
    public static class AddressableDebug
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void DebugError(string message, AddressableLoadResourceBase loadStruct)
        {
            loadStruct.OnLoadFailure.Invoke(message);
            UnityEngine.Debug.LogError(message);
        }
    }
}