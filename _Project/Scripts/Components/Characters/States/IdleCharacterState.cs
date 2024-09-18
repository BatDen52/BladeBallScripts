using UnityEngine;

namespace _Project
{
    public class IdleCharacterState : CharacterState
    {
        public override void Enter(Character character)
        {
        }

        public override void Exit(Character character)
        {
        }

        public override void Tick(Character character)
        {
            character.Gravity();
            character.Jump();
            
            // if (character.IsRotationLocked)
            // {
            //     character.FaceDirection(character.Input.LookDirection);
            // }
            
            Vector2 inputDirection = character.Input.MoveDirection;

            if (inputDirection.sqrMagnitude > 0 || character.LateralVelocity.sqrMagnitude > 0)
            {
                character.StateMachine.Enter<WalkCharacterState>();
            }
        }
    }
}