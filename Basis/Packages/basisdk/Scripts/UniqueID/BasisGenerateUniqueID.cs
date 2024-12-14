using System;
using UnityEngine;

public static class BasisGenerateUniqueID
{
    /// <summary>
    /// This will generate a new unique ID for the prefab, scene, or asset bundle.
    /// </summary>
    /// <returns>A unique identifier combining a GUID and UTC ticks.</returns>
    public static string GenerateUniqueID()
    {
        Guid newGuid = Guid.NewGuid();  // Generate a new GUID
        long utcTicks = DateTime.UtcNow.Ticks;  // Get the current UTC ticks

        // Format the GUID and ticks into a proper string
        return $"{newGuid}_{utcTicks}";
    }
}
