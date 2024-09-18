using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project
{
    public class CharacterAnimator : MonoBehaviour
    {
        private static readonly int StateHash = Animator.StringToHash("State");
        private static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");
        private static readonly int BlockHash = Animator.StringToHash("Block");
        
        [field: SerializeField] public Animator Animator { get; set; }
        [SerializeField] private Character _character;
        
        private static readonly Dictionary<Type, int> StatesToInt = new Dictionary<Type, int>
        {
            { typeof(IdleCharacterState), 0 },
            { typeof(WalkCharacterState), 1 },
            { typeof(FallCharacterState), 2 },
            { typeof(DashCharacterState), 3 },
            { typeof(ThunderDashCharacterState), 4 },
        };

        public void Block()
        {
            Animator.SetTrigger(BlockHash);
        }
        
        private void LateUpdate()
        {
            var activeState = _character.StateMachine.ActiveState;
            Animator.SetInteger(StateHash, StatesToInt[activeState.GetType()]);
            Animator.SetBool(IsBlockingHash, _character.IsBlockingByInput);
        }
    }
}