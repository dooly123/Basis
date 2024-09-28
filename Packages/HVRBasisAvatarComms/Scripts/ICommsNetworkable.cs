using System;
using UnityEngine.Scripting;

namespace HVR.Basis.Comms
{
    [Preserve]
    public interface ICommsNetworkable
    {
        void OnGuidAssigned(int guidIndex, Guid guid);
    }
}