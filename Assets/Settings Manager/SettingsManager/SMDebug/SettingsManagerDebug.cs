namespace BattlePhaze.SettingsManager.DebugSystem
{
    using UnityEngine;
    [AddComponentMenu("BattlePhaze/SettingsManager/Debug")]
    public static class SettingsManagerDebug
    {
        private static readonly string SettingsManagerText = "<b><size=12><color=black>[</color><color=#FF7433>Settings Manager</color><color=black>]</color>";
        public static void Log(string LogValue, UnityEngine.Object DebugItem)
        {
#if UNITY_SERVER
                Debug.Log(LogValue);
#else
            Debug.Log(SettingsManagerText + "<color=gray> | </color>" + "<color=white>" + LogValue + "</color></size></b>");
#endif
        }
        public static void Log(string LogValue)
        {
#if UNITY_SERVER
                Debug.Log(LogValue);
#else
            Debug.Log(SettingsManagerText + "<color=gray> | </color>" + "<color=white>" + LogValue + "</color></size></b>");
#endif
        }
        public static void LogError(string LogValue, UnityEngine.Object DebugItem)
        {
#if UNITY_SERVER
                Debug.LogError(LogValue);
#else
            Debug.LogError(SettingsManagerText + "<color=gray> | </color>" + "<color=white>" + LogValue + "</color></size></b>");
#endif
        }
        public static void LogError(string LogValue)
        {
#if UNITY_SERVER
                Debug.LogError(LogValue);
#else
            Debug.LogError(SettingsManagerText + "<color=gray> | </color>" + "<color=white>" + LogValue + "</color></size></b>");
#endif
        }
    }
}