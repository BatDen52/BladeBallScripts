using UnityEngine;

namespace _Project
{
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask layerMask, int layer)
        {
            // return layerMask == (layerMask | 1 << layer);
            return (1 << layer & layerMask) != 0;
        }
    }
}