using System;
using UnityEngine;

namespace _Project
{
    public class PlayerEnterTrigger : MonoBehaviour
    {
        public event Action PlayerEntered;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerInput>(out var playerInput))
            {
                PlayerEntered?.Invoke();
            }
        }
    }
}