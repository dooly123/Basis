using UnityEngine;

namespace JigglePhysics
{
    // Uses Verlet to resolve constraints easily 
    [SerializeField]
    public class JiggleBone
    {
        public int JiggleParentIndex;
        public int childIndex;
    }
}