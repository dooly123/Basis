using UnityEngine;
namespace JigglePhysics
{
    [System.Serializable]
    public abstract class JiggleRigLOD
    {
        public abstract bool CheckActive(Vector3 position);
        public abstract void UpdateCameraPosition(Camera Camera);
        public abstract void UpdateDistance(Vector3 position);
    }
}