using UnityEngine;
[DefaultExecutionOrder(15002)]
public partial class BasisNetworkTransmitter : BasisNetworkSendBase
{
    public float timer = 0f;
    public float interval = 0.05f;
    [SerializeField]
    public BasisAudioTransmission Module = new BasisAudioTransmission();
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
            Ready = true;
            NetworkedPlayer = networkedPlayer;
            Module.OnEnable(networkedPlayer, this.gameObject);
            OnAvatarCalibration();
            BasisLocalPlayer LocalPlayer = (BasisLocalPlayer)NetworkedPlayer.Player;
            LocalPlayer.LocalAvatarDriver.CalibrationComplete.AddListener(Oncalibration);
        }
        else
        {
            Debug.Log("Already Ready");
        }
    }
    public void Oncalibration()
    {
        Module.OnCalibration(NetworkedPlayer, this.gameObject);
    }
    public override void DeInitialize()
    {
        if (Ready)
        {
        }
        Module.OnDisable();
    }
}