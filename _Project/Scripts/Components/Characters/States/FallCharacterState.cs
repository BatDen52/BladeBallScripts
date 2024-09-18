using UnityEngine;

namespace _Project
{
    public class FallCharacterState : CharacterState
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
            /*
            if (false) // if character.IsRotationLocked
            {
                character.FaceDirection(character.Input.LookDirection);
            }
            else
            */
            {
                character.FaceDirection(character.LateralVelocity);
            }
            character.AccelerateToInputDirection();
            character.Jump();
            
            if (character.IsGrounded)
            {
                character.StateMachine.Enter<IdleCharacterState>();
            }
        }
    }
}