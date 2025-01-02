using System.IO;
using System.Xml;
[System.Serializable]
public class Configuration
{
    public int PeerLimit = 1024;
    public ushort SetPort = 4296;
    public int QueueEvents = 10;
    public bool UseNativeSockets = false;
    public bool NatPunchEnabled = true;
    public int PingInterval = 1500;
    public int DisconnectTimeout = 30000;
    public bool SimulatePacketLoss = false;
    public bool SimulateLatency = false;
    public int SimulationPacketLossChance = 10;
    public int SimulationMinLatency = 50;
    public int SimulationMaxLatency = 150;
    public bool UnsyncedEvents = false;
    public bool UnsyncedReceiveEvent = false;
    public bool UnsyncedDeliveryEvent = false;
    public int ReconnectDelay = 2000;
    public int MaxConnectAttempts = 5;
    public bool ReuseAddress = false;
    public bool DontRoute = false;
    public bool EnableStatistics = true;
    public bool IPv6Enabled = false;
    public int MtuOverride = 1500;
    public bool MtuDiscovery = true;
    public bool DisconnectOnUnreachable = true;
    public bool AllowPeerAddressChange = true;
    public bool UsingLoggingFile = false;
    public string HealthCheckHost = "localhost";
    public ushort HealthCheckPort = 10666;
    public string HealthPath = "/health";
    public int BSRSMillisecondDefaultInterval = 50;
    public int BSRBaseMultiplier = 1;
    public float BSRSIncreaseRate = 0.005f;
    public bool OverrideAutoDiscoveryOfIpv = false;
    public string IPv4Address = "0.0.0.0";
    public string IPv6Address = "::1";
    public int PromethusPort = 1234;
    public string PromethusUrl = "/metrics";
    public string Password = "default_password";
    public static Configuration LoadFromXml(string filePath)
    {
        if (!File.Exists(filePath))
        {
            CreateDefaultXml(filePath);
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        return new Configuration
        {
            PeerLimit = int.Parse(doc.SelectSingleNode("/Configuration/PeerLimit")?.InnerText ?? "1024"),
            SetPort = ushort.Parse(doc.SelectSingleNode("/Configuration/SetPort")?.InnerText ?? "4296"),
            QueueEvents = int.Parse(doc.SelectSingleNode("/Configuration/QueueEvents")?.InnerText ?? "10"),
            UseNativeSockets = bool.Parse(doc.SelectSingleNode("/Configuration/UseNativeSockets")?.InnerText ?? "false"),
            NatPunchEnabled = bool.Parse(doc.SelectSingleNode("/Configuration/NatPunchEnabled")?.InnerText ?? "true"),
            PingInterval = int.Parse(doc.SelectSingleNode("/Configuration/PingInterval")?.InnerText ?? "1500"),
            DisconnectTimeout = int.Parse(doc.SelectSingleNode("/Configuration/DisconnectTimeout")?.InnerText ?? "30000"),
            SimulatePacketLoss = bool.Parse(doc.SelectSingleNode("/Configuration/SimulatePacketLoss")?.InnerText ?? "false"),
            SimulateLatency = bool.Parse(doc.SelectSingleNode("/Configuration/SimulateLatency")?.InnerText ?? "false"),
            SimulationPacketLossChance = int.Parse(doc.SelectSingleNode("/Configuration/SimulationPacketLossChance")?.InnerText ?? "10"),
            SimulationMinLatency = int.Parse(doc.SelectSingleNode("/Configuration/SimulationMinLatency")?.InnerText ?? "50"),
            SimulationMaxLatency = int.Parse(doc.SelectSingleNode("/Configuration/SimulationMaxLatency")?.InnerText ?? "150"),
            UnsyncedEvents = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedEvents")?.InnerText ?? "false"),
            UnsyncedReceiveEvent = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedReceiveEvent")?.InnerText ?? "false"),
            UnsyncedDeliveryEvent = bool.Parse(doc.SelectSingleNode("/Configuration/UnsyncedDeliveryEvent")?.InnerText ?? "false"),
            ReconnectDelay = int.Parse(doc.SelectSingleNode("/Configuration/ReconnectDelay")?.InnerText ?? "2000"),
            MaxConnectAttempts = int.Parse(doc.SelectSingleNode("/Configuration/MaxConnectAttempts")?.InnerText ?? "5"),
            ReuseAddress = bool.Parse(doc.SelectSingleNode("/Configuration/ReuseAddress")?.InnerText ?? "false"),
            DontRoute = bool.Parse(doc.SelectSingleNode("/Configuration/DontRoute")?.InnerText ?? "false"),
            EnableStatistics = bool.Parse(doc.SelectSingleNode("/Configuration/EnableStatistics")?.InnerText ?? "true"),
            IPv6Enabled = bool.Parse(doc.SelectSingleNode("/Configuration/IPv6Enabled")?.InnerText ?? "false"),
            MtuOverride = int.Parse(doc.SelectSingleNode("/Configuration/MtuOverride")?.InnerText ?? "1500"),
            MtuDiscovery = bool.Parse(doc.SelectSingleNode("/Configuration/MtuDiscovery")?.InnerText ?? "true"),
            DisconnectOnUnreachable = bool.Parse(doc.SelectSingleNode("/Configuration/DisconnectOnUnreachable")?.InnerText ?? "true"),
            AllowPeerAddressChange = bool.Parse(doc.SelectSingleNode("/Configuration/AllowPeerAddressChange")?.InnerText ?? "true"),
            UsingLoggingFile = bool.Parse(doc.SelectSingleNode("/Configuration/UsingLoggingFile")?.InnerText ?? "true"),
            HealthCheckHost = doc.SelectSingleNode("/Configuration/HealthCheckHost")?.InnerText ?? "localhost",
            HealthCheckPort = ushort.Parse(doc.SelectSingleNode("/Configuration/HealthCheckPort")?.InnerText ?? "10666"),
            HealthPath = doc.SelectSingleNode("/Configuration/HealthPath")?.InnerText ?? "/health",
            BSRSMillisecondDefaultInterval = int.Parse(doc.SelectSingleNode("/Configuration/BSRSMillisecondDefaultInterval")?.InnerText ?? "50"),
            BSRBaseMultiplier = int.Parse(doc.SelectSingleNode("/Configuration/BSRBaseMultiplier")?.InnerText ?? "1"),
            BSRSIncreaseRate = float.Parse(doc.SelectSingleNode("/Configuration/BSRSIncreaseRate")?.InnerText ?? "0.005"),
            OverrideAutoDiscoveryOfIpv = bool.Parse(doc.SelectSingleNode("/Configuration/OverrideAutoDiscoveryOfIpv")?.InnerText ?? "false"),
            IPv4Address = doc.SelectSingleNode("/Configuration/IPv4Address")?.InnerText ?? "0.0.0.0",
            IPv6Address = doc.SelectSingleNode("/Configuration/IPv6Address")?.InnerText ?? "::1",
            PromethusPort = int.Parse(doc.SelectSingleNode("/Configuration/PromethusPort")?.InnerText ?? "1234"),
            PromethusUrl = doc.SelectSingleNode("/Configuration/PromethusUrl")?.InnerText ?? "/metrics",
            Password = doc.SelectSingleNode("/Configuration/Password")?.InnerText ?? "default_password", // Parse password
        };
    }

    private static void CreateDefaultXml(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("Configuration");

        root.AppendChild(CreateElement(doc, "PeerLimit", "1024"));
        root.AppendChild(CreateElement(doc, "SetPort", "4296"));
        root.AppendChild(CreateElement(doc, "QueueEvents", "10"));
        root.AppendChild(CreateElement(doc, "UseNativeSockets", "true"));
        root.AppendChild(CreateElement(doc, "NatPunchEnabled", "true"));
        root.AppendChild(CreateElement(doc, "PingInterval", "1500"));
        root.AppendChild(CreateElement(doc, "DisconnectTimeout", "5000"));
        root.AppendChild(CreateElement(doc, "SimulatePacketLoss", "false"));
        root.AppendChild(CreateElement(doc, "SimulateLatency", "false"));
        root.AppendChild(CreateElement(doc, "SimulationPacketLossChance", "10"));
        root.AppendChild(CreateElement(doc, "SimulationMinLatency", "50"));
        root.AppendChild(CreateElement(doc, "SimulationMaxLatency", "150"));
        root.AppendChild(CreateElement(doc, "UnsyncedEvents", "false"));
        root.AppendChild(CreateElement(doc, "UnsyncedReceiveEvent", "false"));
        root.AppendChild(CreateElement(doc, "UnsyncedDeliveryEvent", "false"));
        root.AppendChild(CreateElement(doc, "ReconnectDelay", "500"));
        root.AppendChild(CreateElement(doc, "MaxConnectAttempts", "10"));
        root.AppendChild(CreateElement(doc, "ReuseAddress", "true"));
        root.AppendChild(CreateElement(doc, "DontRoute", "false"));
        root.AppendChild(CreateElement(doc, "EnableStatistics", "true"));
        root.AppendChild(CreateElement(doc, "IPv6Enabled", "true"));
        root.AppendChild(CreateElement(doc, "MtuOverride", "1500"));
        root.AppendChild(CreateElement(doc, "MtuDiscovery", "true"));
        root.AppendChild(CreateElement(doc, "DisconnectOnUnreachable", "true"));
        root.AppendChild(CreateElement(doc, "AllowPeerAddressChange", "true"));
        root.AppendChild(CreateElement(doc, "UsingLoggingFile", "true"));
        root.AppendChild(CreateElement(doc, "HealthCheckHost", "localhost"));
        root.AppendChild(CreateElement(doc, "HealthCheckPort", "10666"));
        root.AppendChild(CreateElement(doc, "HealthPath", "/health"));
        root.AppendChild(CreateElement(doc, "BSRSMillisecondDefaultInterval", "50"));
        root.AppendChild(CreateElement(doc, "BSRBaseMultiplier", "1"));
        root.AppendChild(CreateElement(doc, "BSRSIncreaseRate", "0.005"));
        root.AppendChild(CreateElement(doc, "OverrideAutoDiscoveryOfIpv", "false"));
        root.AppendChild(CreateElement(doc, "IPv4Address", "0.0.0.0"));
        root.AppendChild(CreateElement(doc, "IPv6Address", "::1"));
        root.AppendChild(CreateElement(doc, "PromethusPort", "1234"));
        root.AppendChild(CreateElement(doc, "PromethusUrl", "/metrics"));
        root.AppendChild(CreateElement(doc, "Password", "default_password")); // Default password

        doc.AppendChild(root);
        doc.Save(filePath);
    }

    private static XmlElement CreateElement(XmlDocument doc, string name, string value)
    {
        XmlElement element = doc.CreateElement(name);
        element.InnerText = value;
        return element;
    }
}
