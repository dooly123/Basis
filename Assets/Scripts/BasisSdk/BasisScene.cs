using UnityEngine;
using UnityEngine.Events;

namespace Basis.Scripts.BasisSdk
{
    public class BasisScene : MonoBehaviour
    {
        public Transform SpawnPoint;
        public float RespawnHeight = -100;
        public float RespawnCheckTimer = 0.1f;
        public UnityEngine.Audio.AudioMixerGroup Group;
        public static UnityEvent<BasisScene> Ready = new UnityEvent<BasisScene>();
        public void Awake()
        {
            Ready.Invoke(this);
        }
    }
}