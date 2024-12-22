using UnityEngine;
using System.Collections.Generic;
using System;
using Basis.Scripts.Device_Management;

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
        if (ShouldIgnoreCursorRequests()) return;

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
        if (ShouldIgnoreCursorRequests()) return;

        InternalUnlockCursor();
    }

    /// <summary>
    /// Unlocks the cursor and makes it visible. Bypasses checks that would have prevented it from being unlocked.
    /// </summary>
    public static void UnlockCursorBypassChecks()
    {
        InternalUnlockCursor();
    }

    private static void InternalUnlockCursor()
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
        if (ShouldIgnoreCursorRequests()) return;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Debug.Log("Cursor Confined");
        OnCursorStateChange?.Invoke(CursorLockMode.Confined, true);
    }

    private static bool ShouldIgnoreCursorRequests()
    {
        var isUserInVR = !BasisDeviceManagement.IsUserInDesktop();

        // When in VR mode, all cursor lock requests are must be ignored,
        // so that cursor control is not taken away from other external desktop overlay applications.
        return isUserInVR;
    }
}
