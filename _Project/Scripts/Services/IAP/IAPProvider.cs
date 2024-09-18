using UnityEngine;

namespace _Project.IAP
{
    public class IAPProvider : MonoBehaviour
    {
        [field: SerializeField] public IAPProduct ProductId { get; private set; }
    }
}
