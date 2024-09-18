using System;
using UnityEngine;

namespace _Project
{
    public class BotInput : CharacterInput
    {
        public override Vector3 LookDirection { get; set; } = Vector3.forward;
        public override event Action Block;
        public override event Action UseSkill;
        public override event Action<bool> RotationLock;
        public override Vector2 Move { get; set; }
        public override Vector3 MoveDirection { get; set; }
        public bool IsJumping;
        public override bool GetJumpDown() => IsJumping;
        public override bool GetJumpUp() => false;

        public void ActivateBlock()
        {
            Block?.Invoke();
        }
        
        public void ActivateUseSkill()
        {
            UseSkill?.Invoke();
        }

        public void SetRotationLock(bool value)
        {
            RotationLock?.Invoke(value);
        }
    }
}