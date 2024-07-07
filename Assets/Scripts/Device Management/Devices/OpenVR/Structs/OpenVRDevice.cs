using System;
using Valve.VR;
[System.Serializable]
public struct OpenVRDevice
{
    public ETrackedDeviceClass deviceClass;
    public uint deviceIndex;
    public string deviceName;
}