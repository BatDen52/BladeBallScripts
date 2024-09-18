using UnityEngine;

namespace _Project
{
    public static class ColliderExtensions
    {
        public static bool IsInLayer(this Collider collider, LayerMask layerMask)
        {
            return (1 << collider.gameObject.layer & layerMask) != 0;
        }
    }
}