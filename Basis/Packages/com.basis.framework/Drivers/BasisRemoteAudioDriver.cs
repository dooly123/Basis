using Basis.Scripts.Networking.Recievers;
using UnityEngine;

namespace Basis.Scripts.Drivers
{
    public class BasisRemoteAudioDriver : MonoBehaviour
    {
        public BasisAudioAndVisemeDriver BasisAudioAndVisemeDriver;
        public BasisAudioReceiver BasisAudioReceiver;
        public void OnAudioFilterRead(float[] data, int channels)
        {
            //2048  BasisDebug.Log("data" + data.Length);
            int Value = BasisAudioReceiver.OnAudioFilterRead(data, channels);
            BasisAudioAndVisemeDriver.ProcessAudioSamples(data, channels, Value);
        }
        public void Initalize(BasisAudioAndVisemeDriver basisVisemeDriver)
        {
            if (basisVisemeDriver != null)
            {
                BasisAudioAndVisemeDriver = basisVisemeDriver;
            }
            else
            {
                this.enabled = false;
            }
        }
    }
}
