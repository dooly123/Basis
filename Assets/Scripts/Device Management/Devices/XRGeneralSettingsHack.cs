using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Management;

public static class XRGeneralSettingsHack
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void UpdateRegisteredLoadersList()
    {
        var xrSettingsManager = XRGeneralSettings.Instance.Manager;
        var bindings = BindingFlags.Instance | BindingFlags.NonPublic;

        var registeredLoadersField = xrSettingsManager.GetType()
             .GetField("m_RegisteredLoaders", bindings);

        var registeredLoaders = registeredLoadersField
               .GetValue(xrSettingsManager) as HashSet<XRLoader>;

        if (registeredLoaders.Count == 0)
        {
            foreach (var activeLoader in xrSettingsManager.activeLoaders)
            {
                registeredLoaders.Add(activeLoader);
            }
        }
    }
}