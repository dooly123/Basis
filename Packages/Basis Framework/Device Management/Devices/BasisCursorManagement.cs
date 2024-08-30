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
        Debug.Log("Requested a forced Cursor Lock for "+ requestName);
    }
    public static CursorLockMode LockState()
    {
        return Cursor.lockState;
    }
    public static bool Visible()
    {
        return Cursor.visible;
    }
    /// <summary>
    /// Locks the cursor to the center of the screen and hides it.
    /// Adds a request to lock the cursor.
    /// </summary>
    public static void LockCursor(string requestName)
    {
        if (!cursorLockRequests.Contains(requestName))
        {
            cursorLockRequests.Add(requestName);
        }

        UpdateCursorState();
        Debug.Log("Cursor Lock Requested: " + requestName);
    }

    /// <summary>
    /// Unlocks the cursor and makes it visible.
    /// Removes a request to lock the cursor.
    /// </summary>
    public static void UnlockCursor(string requestName)
    {
        if (cursorLockRequests.Contains(requestName))
        {
            cursorLockRequests.Remove(requestName);
        }

        UpdateCursorState();
        Debug.Log("Cursor Unlock Requested: " + requestName);
    }

    /// <summary>
    /// Locks the cursor to the screen boundaries but allows movement.
    /// Adds a request to confine the cursor.
    /// </summary>
    public static void ConfineCursor(string requestName)
    {
        if (!cursorLockRequests.Contains(requestName))
        {
            cursorLockRequests.Add(requestName);
        }

        UpdateCursorState();
        Debug.Log("Cursor Confine Requested: " + requestName);
    }

    /// <summary>
    /// Updates the cursor state based on the lock requests.
    /// If there are any lock requests, the cursor is locked or confined.
    /// If there are no lock requests, the cursor is unlocked.
    /// </summary>
    private static void UpdateCursorState()
    {
        if (cursorLockRequests.Count > 0)
        {
            // Keep the cursor locked or confined based on the most recent request
            string lastRequest = cursorLockRequests[cursorLockRequests.Count - 1];

            // This is an example logic; you can modify it to handle different types of requests
            if (lastRequest.Contains("Confine"))
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                Debug.Log("Cursor Confined");
                OnCursorStateChange?.Invoke(CursorLockMode.Confined, true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("Cursor Locked");
                OnCursorStateChange?.Invoke(CursorLockMode.Locked, false);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Cursor Unlocked");
            OnCursorStateChange?.Invoke(CursorLockMode.None, true);
        }
    }
}