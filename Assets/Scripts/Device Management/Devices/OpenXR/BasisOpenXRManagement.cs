using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

[Serializable]
public class BasisOpenXRManagement
{
    public List<InputDevice> inputDevices = new List<InputDevice>();
    public Dictionary<string, InputDevice> TypicalDevices = new Dictionary<string, InputDevice>();

    public void StartXRSDK()
    {
        Debug.Log("Starting BasisOpenXRManagement");
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
        UpdateDeviceList();
    }

    public void StopXRSDK()
    {
        Debug.Log("Stopping BasisOpenXRManagement");
        List<string> Devices = TypicalDevices.Keys.ToList();
        foreach (string device in Devices)
        {
            DestroyPhysicalTrackedDevice(device);
        }
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        UpdateDeviceList();
    }

    private void OnDeviceDisconnected(InputDevice device)
    {
        UpdateDeviceList();
    }

    private void UpdateDeviceList()
    {
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.TrackingReference))
                continue;

            if (device != null)
            {
                string id = GenerateID(device);
                if (!TypicalDevices.ContainsKey(id))
                {
                    CreatePhysicalTrackedDevice(device, id, device.name);
                    TypicalDevices[id] = device;
                }
            }
        }

        var keysToRemove = new List<string>();
        foreach (var kvp in TypicalDevices)
        {
            if (!inputDevices.Contains(kvp.Value))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            DestroyPhysicalTrackedDevice(key);
            TypicalDevices.Remove(key);
        }
    }

    private string GenerateID(InputDevice device)
    {
        return $"{device.name}|{device.serialNumber}|{device.manufacturer}|{(int)device.characteristics}";
    }

    private void CreatePhysicalTrackedDevice(InputDevice device, string uniqueID, string unUniqueID)
    {
        var gameObject = new GameObject(uniqueID)
        {
            transform =
            {
                parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform
            }
        };
        var basisXRInput = gameObject.AddComponent<BasisOpenXRInput>();
        bool state = GetControllerOrHMD(device, out BasisBoneTrackedRole BasisBoneTrackedRole);
        basisXRInput.Initialize(device, uniqueID, unUniqueID, "BasisOpenXRManagement", state, BasisBoneTrackedRole);
        BasisDeviceManagement.Instance.TryAdd(basisXRInput);
    }
    private bool GetControllerOrHMD(InputDevice device,out BasisBoneTrackedRole BasisBoneTrackedRole)
    {
        BasisBoneTrackedRole = BasisBoneTrackedRole.CenterEye;
        if (device.characteristics == Characteristics.hmd)
        {
            BasisBoneTrackedRole = BasisBoneTrackedRole.CenterEye;
            return true;
        }
        else if (device.characteristics == Characteristics.leftController || device.characteristics == Characteristics.leftTrackedHand)
        {
            BasisBoneTrackedRole = BasisBoneTrackedRole.LeftHand;
            return true;
        }
        else if (device.characteristics == Characteristics.rightController || device.characteristics == Characteristics.rightTrackedHand)
        {
            BasisBoneTrackedRole = BasisBoneTrackedRole.RightHand;
            return true;
        }
        return false;
    }
    public void DestroyPhysicalTrackedDevice(string id)
    {
        TypicalDevices.Remove(id);
        BasisDeviceManagement.Instance.RemoveDevicesFrom("BasisOpenXRManagement", id);
    }
    public static class Characteristics
    {
        /// <summary>
        /// HMD characteristics.
        /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
        /// </summary>
        public static InputDeviceCharacteristics hmd => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;

        /// <summary>
        /// Eye gaze characteristics.
        /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.EyeTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
        /// </summary>
        public static InputDeviceCharacteristics eyeGaze => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice;

        /// <summary>
        /// Left controller characteristics.
        /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
        /// </summary>
        public static InputDeviceCharacteristics leftController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        /// <summary>
        /// Right controller characteristics.
        /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
        /// </summary>
        public static InputDeviceCharacteristics rightController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

        /// <summary>
        /// Left tracked hand characteristics.
        /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
        /// </summary>
        public static InputDeviceCharacteristics leftTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;

        /// <summary>
        /// Right tracked hand characteristics.
        /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
        /// </summary>
        public static InputDeviceCharacteristics rightTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;
    }
}