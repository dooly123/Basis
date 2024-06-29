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
        basisXRInput.SubSystem = nameof(BasisOpenXRManagement);
        basisXRInput.Initialize(device, uniqueID, unUniqueID);
        if (BasisDeviceManagement.Instance.AllInputDevices.Contains(basisXRInput) == false)
        {
            BasisDeviceManagement.Instance.AllInputDevices.Add(basisXRInput);
        }
        else
        {
            Debug.LogError("already added a Input Device thats identical!");
        }
    }

    public void DestroyPhysicalTrackedDevice(string id)
    {
        TypicalDevices.Remove(id);
        BasisDeviceManagement.Instance.RemoveDevicesFrom(nameof(BasisOpenXRManagement), id);
    }
}