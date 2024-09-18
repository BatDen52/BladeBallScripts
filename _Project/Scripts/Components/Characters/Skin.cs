using UnityEngine;

namespace _Project
{
    public class Skin : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator;
        [field: SerializeField] public Transform WeaponSlot;
        [field: SerializeField] public Transform IdleWeaponSlot;
        [field: SerializeField] public SkinnedMeshRenderer[] Renderers;
        

        private void OnValidate()
        {
            Animator = GetComponentInChildren<Animator>();
            Renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            
        }
    }
}