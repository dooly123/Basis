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
          //2048  Debug.Log("data" + data.Length);
            BasisAudioReceiver.OnAudioFilterRead(data, channels); 
            BasisAudioAndVisemeDriver.ProcessAudioSamples(data, channels);
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