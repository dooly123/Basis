using System.Collections.Generic;

namespace Basis.Scripts.UI
{
public partial class BasisPointRaycaster
{
    /// </summary>
    sealed class RaycastHitComparer : IComparer<RaycastHitData>
    {
        public int Compare(RaycastHitData a, RaycastHitData b)
            => b.graphic.depth.CompareTo(a.graphic.depth);
    }
}
}