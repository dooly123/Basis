using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
using static SerializableDarkRift;
[DefaultExecutionOrder(15002)]
public partial class BasisNetworkTransmitter : BasisNetworkSendBase
{
    public float timer = 0f;
    public float interval = 0.05f;
    [SerializeField]
    public BasisAudioTransmission AudioTransmission = new BasisAudioTransmission();
    public override void Compute()
    {
        if (Ready)
        {
            if (NetworkedPlayer.Player.Avatar != null)
            {
                BasisNetworkAvatarCompressor.Compress(this, NetworkedPlayer.Player.Avatar.Animator);
            }
        }
    }
    void LateUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            Compute();
            timer = 0f;
        }
    }
    public void OnDestroy()
    {
        DeInitialize();
    }
    public override void Initialize(BasisNetworkedPlayer networkedPlayer)
    {
        if (Ready == false)
        {
            InitalizeDataJobs();
            InitalizeAvatarStoredData(ref Target);
            InitalizeAvatarStoredData(ref Output);
            Ready = true;
            NetworkedPlayer = networkedPlayer;
            AudioTransmission.OnEnable(networkedPlayer);
            OnAvatarCalibration();
            networkedPlayer.Player.OnAvatarSwitchedFallBack += OnAvatarCalibration;
            networkedPlayer.Player.OnAvatarSwitched += OnAvatarCalibration;
            networkedPlayer.Player.OnAvatarSwitched += SendOutLatestAvatar;
        }
        else
        {
            Debug.Log("Already Ready");
        }
    }
    public override void DeInitialize()
    {
        if (Ready)
        {
            AudioTransmission.OnDisable();
        }
    }
    public void SendOutLatestAvatar()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            ClientAvatarChangeMessage ClientAvatarChangeMessage = new ClientAvatarChangeMessage();
            ClientAvatarChangeMessage.avatarID = NetworkedPlayer.Player.AvatarUrl;
            writer.Write(ClientAvatarChangeMessage);
            using (var msg = Message.Create(BasisTags.AvatarChangeMessage, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}