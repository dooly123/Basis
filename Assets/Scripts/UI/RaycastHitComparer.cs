using System.Collections.Generic;
public partial class BasisPointRaycaster
{
    /// </summary>
    sealed class RaycastHitComparer : IComparer<RaycastHitData>
    {
        public int Compare(RaycastHitData a, RaycastHitData b)
            => b.graphic.depth.CompareTo(a.graphic.depth);
    }
}