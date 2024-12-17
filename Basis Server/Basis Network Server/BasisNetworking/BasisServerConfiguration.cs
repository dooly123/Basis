using System;
using System.Xml;

public class Configuration
{
    public int PeerLimit { get; set; }
    public ushort SetPort { get; set; }
    public int QueueEvents { get; set; }
    public bool UseNativeSockets { get; set; }
    public bool UnconnectedMessagesEnabled { get; set; }
    public bool NatPunchEnabled { get; set; }
    public int UpdateTime { get; set; }
    public int PingInterval { get; set; }
    public int DisconnectTimeout { get; set; }
    public bool SimulatePacketLoss { get; set; }
    public bool SimulateLatency { get; set; }
    public int SimulationPacketLossChance { get; set; }
    public int SimulationMinLatency { get; set; }
    public int SimulationMaxLatency { get; set; }
    public bool UnsyncedEvents { get; set; }
    public bool UnsyncedReceiveEvent { get; set; }
    public bool UnsyncedDeliveryEvent { get; set; }
    public bool BroadcastReceiveEnabled { get; set; }
    public int ReconnectDelay { get; set; }
    public int MaxConnectAttempts { get; set; }
    public bool ReuseAddress { get; set; }
    public bool DontRoute { get; set; }
    public bool EnableStatistics { get; set; }
    public bool IPv6Enabled { get; set; }
    public int MtuOverride { get; set; }
    public bool MtuDiscovery { get; set; }
    public bool DisconnectOnUnreachable { get; set; }
    public bool AllowPeerAddressChange { get; set; }

    public static Configuration LoadFromXml(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        return new Configuration
        {
            PeerLimit = int.Parse(doc.SelectSingleNode("/Configuration/PeerLimit")?.InnerText),
            SetPort = ushort.Parse(doc.SelectSingleNode("/Configuration/SetPort")?.InnerText),
            QueueEvents = int.Parse(doc.SelectSingleNode("/Configuration/QueueEvents")?.InnerText),
            UseNativeSockets = bool.Parse(doc.SelectSingleNode("/Configuration/UseNativeSockets")?.InnerText),
            UnconnectedMessagesEnabled = bool.Parse(doc.SelectSingleNode("/Configuration/UnconnectedMessagesEnabled")?.InnerText),
            NatPunchEnabled = bool.Parse(doc.SelectSingleNode("/Configuration/NatPunchEnabled")?.InnerText),
            UpdateTime = int.Parse(doc.SelectSingleNode("/Configuration/UpdateTime")?.InnerText),
            PingInterval = int.Parse(doc.SelectSingleNode("/Configuration/PingInterval")?.InnerText),
            DisconnectTimeout = int.Parse(doc.SelectSingleNode("/Configuration/DisconnectTimeout")?.InnerText),
            SimulatePacketLoss = bool.Parse(doc.SelectSingleNode("/Configuration/SimulatePacketLoss")?.InnerText),
            SimulateLatency = bool.Parse(doc.SelectSingleNode("/Configuration/SimulateLatency")?.InnerText),
            SimulationPacketLossChance = int.Parse(doc.SelectSingleNode("/Configuration/SimulationPacketLossChance")?.InnerText),
            SimulationMinLatency = int.Parse(doc.SelectSingleNode("/Configuration/SimulationMinLatency")?.InnerText),
            SimulationMaxLatency = int.Parse(doc.SelectSingleNode("/Configuration/SimulationMaxLatency")?.InnerText),
            UnsyncedEvents = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedEvents")?.InnerText),
            UnsyncedReceiveEvent = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedReceiveEvent")?.InnerText),
            UnsyncedDeliveryEvent = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedDeliveryEvent")?.InnerText),
            BroadcastReceiveEnabled = bool.Parse(doc.SelectSingleNode("/Configuration/BroadcastReceiveEnabled")?.InnerText),
            ReconnectDelay = int.Parse(doc.SelectSingleNode("/Configuration/ReconnectDelay")?.InnerText),
            MaxConnectAttempts = int.Parse(doc.SelectSingleNode("/Configuration/MaxConnectAttempts")?.InnerText),
            ReuseAddress = bool.Parse(doc.SelectSingleNode("/Configuration/ReuseAddress")?.InnerText),
            DontRoute = bool.Parse(doc.SelectSingleNode("/Configuration/DontRoute")?.InnerText),
            EnableStatistics = bool.Parse(doc.SelectSingleNode("/Configuration/EnableStatistics")?.InnerText),
            IPv6Enabled = bool.Parse(doc.SelectSingleNode("/Configuration/IPv6Enabled")?.InnerText),
            MtuOverride = int.Parse(doc.SelectSingleNode("/Configuration/MtuOverride")?.InnerText),
            MtuDiscovery = bool.Parse(doc.SelectSingleNode("/Configuration/MtuDiscovery")?.InnerText),
            DisconnectOnUnreachable = bool.Parse(doc.SelectSingleNode("/Configuration/DisconnectOnUnreachable")?.InnerText),
            AllowPeerAddressChange = bool.Parse(doc.SelectSingleNode("/Configuration/AllowPeerAddressChange")?.InnerText)
        };
    }
}