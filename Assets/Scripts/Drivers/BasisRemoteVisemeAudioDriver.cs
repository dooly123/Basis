using UnityEngine;

namespace Assets.Scripts.Drivers
{
public class BasisRemoteVisemeAudioDriver : MonoBehaviour
{
    public BasisVisemeDriver BasisVisemeDriver;
    void OnAudioFilterRead(float[] data, int channels)
    {
        BasisVisemeDriver.ProcessAudioSamples(data);
    }
    public void Initalize(BasisVisemeDriver basisVisemeDriver)
    {
        if (basisVisemeDriver != null)
        {
            BasisVisemeDriver = basisVisemeDriver;
        }
        else
        {
            this.enabled = false;
        }
    }
}
}