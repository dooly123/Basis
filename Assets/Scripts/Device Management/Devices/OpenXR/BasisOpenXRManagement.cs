using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

[Serializable]
public class BasisOpenXRManagement
{
    public List<InputDevice> inputDevices = new List<InputDevice>();
    public List<BasisOpenXRInput> TrackedOpenXRInputDevices = new List<BasisOpenXRInput>();
    public Dictionary<string, InputDevice> TypicalDevices = new Dictionary<string, InputDevice>();

    public void StartXRSDK()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
        UpdateDeviceList();
    }

    public void StopXR()
    {
        if (XRGeneralSettings.Instance?.Manager?.isInitializationComplete == true)
        {
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        foreach (var device in TrackedOpenXRInputDevices)
        {
            if (device != null)
            {
                GameObject.Destroy(device.gameObject);
            }
        }
        TrackedOpenXRInputDevices.Clear();

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
        basisXRInput.Initialize(device, uniqueID, unUniqueID);
        TrackedOpenXRInputDevices.Add(basisXRInput);
        BasisDeviceManagement.Instance.AllInputDevices.Add(basisXRInput);
    }

    public void DestroyPhysicalTrackedDevice(string id)
    {
        DestroyInputDevice(id);
        DestroyXRInput(id);
    }

    private void DestroyInputDevice(string id)
    {
        TypicalDevices.Remove(id);
    }

    public void DestroyXRInput(string id)
    {
        var devicesToRemove = new List<BasisOpenXRInput>();

        foreach (var device in TrackedOpenXRInputDevices)
        {
            if (device.UniqueID == id)
            {
                devicesToRemove.Add(device);
                GameObject.Destroy(device.gameObject);
            }
        }

        foreach (var device in devicesToRemove)
        {
            TrackedOpenXRInputDevices.Remove(device);
        }

        BasisDeviceManagement.Instance.AllInputDevices.RemoveAll(item => item == null);
    }
}