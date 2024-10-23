#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace JigglePhysics
{
    public static class JiggleDefaults {
    private static string GetActiveFolderPath() {
        // Can't believe we need to use reflection to call this method!
        MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        string folderPath = (string)getActiveFolderPath.Invoke(null, null);
        return folderPath;
    }
}

}

#endif
