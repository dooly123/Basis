using UnityEngine;
public class FFUnityPreInitalization
{
    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        Debug.Log("warming up Basis VideoPlayer");
        DynamicallyLinkedBindings.Initialize();
        Debug.Log("warmed up Basis VideoPlayer");
    }
}