using System;
using System.Net;
using UnityEngine;

public static class BasisNetworkIPResolve
{
    public static string[] ResolveLocalhostToIP(string hostname)
    {
        try
        {
            IPAddress[] ips = Dns.GetHostAddresses(hostname);
            if (ips != null && ips.Length > 0)
            {
                string[] addresses = new string[ips.Length];
                for (int Index = 0; Index < ips.Length; Index++)
                {
                    addresses[Index] = ips[Index].ToString();
                }
                return addresses;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to resolve localhost to IP address: " + ex.Message);
        }
        return null;
    }
    public static string LocalHost = "localhost";
    public static IPAddress IpOutput(string IpString)
    {
        if (IpString.ToLower() == LocalHost)
        {
            string[] IpStrings = BasisNetworkIPResolve.ResolveLocahost(IpString);
            IpString = IpStrings[0];
        }
        return IPAddress.Parse(IpString);
    }
    public static string[] ResolveLocahost(string localhost)
    {
        string[] addresses = ResolveLocalhostToIP(localhost);
        if (addresses != null)
        {
        }
        else
        {
            Debug.LogError("Failed to resolve localhost to IP address.");
        }
        return addresses;
    }
}
