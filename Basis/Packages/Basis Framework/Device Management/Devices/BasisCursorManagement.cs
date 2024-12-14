using UnityEngine;
using System.Collections.Generic;
using System;

public static class BasisCursorManagement
{
    // A list to keep track of cursor lock requests
    private static List<string> cursorLockRequests = new List<string>();
    // Event that gets triggered whenever the cursor state changes
    public static event Action<CursorLockMode, bool> OnCursorStateChange;
    public static void OverrideableLock(string requestName)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Requested a forced Cursor Lock for " + requestName);
    }
    public static CursorLockMode ActiveLockState()
    {
        return Cursor.lockState;
    }
    public static bool IsVisible()
    {
        return Cursor.visible;
    }
    /// <summary>
    /// Locks the cursor to the center of the screen and hides it.
    /// Adds a request to lock the cursor.
    /// </summary>
    public static void LockCursor(string requestName)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Cursor Locked");
        OnCursorStateChange?.Invoke(CursorLockMode.Locked, false);
    }

    /// <summary>
    /// Unlocks the cursor and makes it visible.
    /// Removes a request to lock the cursor.
    /// </summary>
    public static void UnlockCursor(string requestName)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor Unlocked");
        OnCursorStateChange?.Invoke(CursorLockMode.None, true);
    }

    /// <summary>
    /// Locks the cursor to the screen boundaries but allows movement.
    /// Adds a request to confine the cursor.
    /// </summary>
    public static void ConfineCursor(string requestName)
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Debug.Log("Cursor Confined");
        OnCursorStateChange?.Invoke(CursorLockMode.Confined, true);
    }
}