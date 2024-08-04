using UnityEngine;

namespace Basis
{
    public abstract class BasisInputEye : MonoBehaviour
    {
        public static Vector3 LeftPosition;
        public static Vector3 RightPosition;
        public abstract void Initalize();
        public abstract void Simulate();
    }
}
