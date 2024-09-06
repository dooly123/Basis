using UnityEngine;
namespace JigglePhysics
{
    [System.Serializable]
    public abstract class JiggleRigLOD
    {
        public abstract bool CheckActive(Vector3 position);
        public abstract void UpdateCameraPosition();
        public abstract JiggleSettingsData AdjustJiggleSettingsData(Vector3 position, JiggleSettingsData data);
    }
}