using UnityEngine;

namespace _Project
{
    public class Lobby : MonoBehaviour
    {
        [field: SerializeField] public Transform SpawnPoint { get; private set; }
        [field: SerializeField] public PlayerEnterTrigger StartRoundTrigger { get; private set; }

        [field: SerializeField] public PlayerEnterTrigger StartDuel_1_1Trigger { get; private set; }
        [field: SerializeField] public GameObject StartDuel_1_1_ActivePortal { get; private set; }
        [field: SerializeField] public GameObject StartDuel_1_1_InactivePortal { get; private set; }
    }
}