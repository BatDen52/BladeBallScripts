using UnityEngine;

namespace _Project
{
    public class AFKZone : MonoBehaviour
    {
        [field: SerializeField] public Transform SpawnPoint { get; private set; }
        [field: SerializeField] public PlayerEnterTrigger BackToGameTrigger { get; private set; }
    }
}