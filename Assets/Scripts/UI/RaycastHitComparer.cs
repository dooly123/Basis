using System.Collections.Generic;

namespace Assets.Scripts.UI
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