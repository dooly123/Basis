using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Drivers;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisHeightDriver
{
    public static string FileNameAndExtension = "SavedHeight.BAS";
    /// <summary>
    /// Adjusts the player's eye height after allowing all devices and systems to reset to their native size. 
    /// This method waits for 4 frames (including asynchronous frames) to ensure the final positions are updated.
    /// </summary>
    public static void SetPlayersEyeHeight(BasisLocalPlayer basisPlayer)
    {
        if (basisPlayer == null)
        {
            BasisDebug.LogError("BasisPlayer is null. Cannot set player's eye height.");
            return;
        }
        // Retrieve the player's eye height from the input device
        CapturePlayerHeight();
        // Retrieve the active avatar's eye height
        basisPlayer.AvatarEyeHeight = basisPlayer.AvatarDriver?.ActiveAvatarEyeHeight() ?? 0;
        BasisDebug.Log($"Avatar eye height: {basisPlayer.AvatarEyeHeight}, Player eye height: {basisPlayer.PlayerEyeHeight}", BasisDebug.LogTag.Avatar);

        // Handle potential issues with height data
        if (basisPlayer.PlayerEyeHeight <= 0 || basisPlayer.AvatarEyeHeight <= 0)
        {
            basisPlayer.RatioPlayerToAvatarScale = 1;
            if (basisPlayer.PlayerEyeHeight <= 0)
            {
                basisPlayer.PlayerEyeHeight = BasisLocalPlayer.DefaultPlayerEyeHeight; // Set a default eye height if invalid
                Debug.LogWarning("Player eye height was invalid. Set to default: 1.64f.");
            }

            BasisDebug.LogError("Invalid height data. Scaling ratios set to defaults.");
        }
        else
        {
            // Calculate scaling ratios
            basisPlayer.RatioPlayerToAvatarScale = basisPlayer.AvatarEyeHeight / basisPlayer.PlayerEyeHeight;
        }

        // Calculate other scaling ratios
        basisPlayer.EyeRatioAvatarToAvatarDefaultScale = basisPlayer.AvatarEyeHeight / BasisLocalPlayer.DefaultAvatarEyeHeight;
        basisPlayer.EyeRatioPlayerToDefaultScale = basisPlayer.PlayerEyeHeight / BasisLocalPlayer.DefaultPlayerEyeHeight;

        // Notify listeners that height recalculation is complete
        BasisDebug.Log($"Final Player Eye Height: {basisPlayer.PlayerEyeHeight}", BasisDebug.LogTag.Avatar);
        basisPlayer.OnPlayersHeightChanged?.Invoke();
    }
    public static void CapturePlayerHeight()
    {
        Basis.Scripts.TransformBinders.BasisLockToInput basisLockToInput = BasisLocalCameraDriver.Instance?.BasisLockToInput;
        if (basisLockToInput?.AttachedInput != null)
        {
            BasisLocalPlayer.Instance.PlayerEyeHeight = basisLockToInput.AttachedInput.LocalRawPosition.y;
            BasisDebug.Log($"Player's raw eye height recalculated: {BasisLocalPlayer.Instance.PlayerEyeHeight}", BasisDebug.LogTag.Avatar);
        }
        else
        {
            BasisDebug.LogWarning("No attached input found for BasisLockToInput. Using default player eye height.", BasisDebug.LogTag.Avatar);
            BasisLocalPlayer.Instance.PlayerEyeHeight = BasisLocalPlayer.DefaultPlayerEyeHeight; // Set a reasonable default
        }
    }

    public static float GetDefaultOrLoadPlayerHeight()
    {
        float DefaultHeight = BasisLocalPlayer.DefaultPlayerEyeHeight;
        if (BasisDataStore.LoadFloat(FileNameAndExtension, DefaultHeight, out float FoundHeight))
        {
            return FoundHeight;
        }
        else
        {
            SaveHeight(FoundHeight);
            return FoundHeight;
        }
    }
    public static void SaveHeight()
    {
        float DefaultHeight = BasisLocalPlayer.DefaultPlayerEyeHeight;
        SaveHeight(DefaultHeight);
    }
    public static void SaveHeight(float EyeHeight)
    {
        BasisDataStore.SaveFloat(EyeHeight, FileNameAndExtension);
    }
}
