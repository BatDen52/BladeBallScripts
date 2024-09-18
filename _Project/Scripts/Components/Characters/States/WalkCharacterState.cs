using UnityEngine;

namespace _Project
{
    public class WalkCharacterState : CharacterState
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
            
            Vector3 inputDirection = character.Input.MoveDirection;
            
            if (inputDirection.sqrMagnitude > 0)
            {
                character.Accelerate(inputDirection);
                /*
                if (false) // if character.IsRotationLocked
                {
                    character.FaceDirection(character.Input.LookDirection);
                }
                else
                */
                {
                    character.FaceDirection(character.LateralVelocity, character.Config.RotationSpeed);
                }
            }
            else
            {
                character.Decelerate(character.Config.Friction);

                if (character.LateralVelocity.sqrMagnitude <= 0)
                {
                    character.StateMachine.Enter<IdleCharacterState>();
                }
            }
        }
    }
}