using System;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class PlayerInput : CharacterInput
    {
        private const float JumpBuffer = 0.15f;
        
        private float? _lastJumpTime;
        
        public IInput Input { get; private set; }
        
        public override Vector2 Move
        {
            get => Input.Move;
            set {}
        }
        
        public override Vector3 MoveDirection
        {
            get => Input.MoveDirection;
            set {}
        }

        public override Vector3 LookDirection { 
            get => Input.LookDirection;
            set {}
        } 

        public override event Action Block;
        public override event Action UseSkill;
        public override event Action<bool> RotationLock;

        [Inject]
        private void Construct(IInput input)
        {
            Input = input;
            Input.Block += OnBlock;
            Input.UseSkill += OnUseSkill;
            Input.RotationLock += OnRotationLock;
        }

        private void OnRotationLock(bool value)
        {
            RotationLock?.Invoke(value);
        }

        private void OnUseSkill()
        {
            UseSkill?.Invoke();
        }

        protected virtual void Update()
        {
            if (Input.Jump.WasPressedThisFrame())
            {
                _lastJumpTime = Time.time;
            }
        }
        
        private void OnDestroy()
        {
            Input.Block -= OnBlock;
        }

        private void OnBlock()
        {
            Block?.Invoke();
        }
        
        public override bool GetJumpDown()
        {
            if (_lastJumpTime != null &&
                Time.time - _lastJumpTime < JumpBuffer)
            {
                _lastJumpTime = null;
                return true;
            }

            return false;
        }

        public override bool GetJumpUp() => Input.Jump.WasReleasedThisFrame();

    }
}