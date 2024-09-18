using _Project.Data.Static.Skills;
using UnityEngine;

namespace _Project
{
    public class DashCharacterState : CharacterState
    {
        public override void Enter(Character character)
        {
            character.VerticalVelocity = Vector3.zero;
        }

        public override void Exit(Character character)
        {
            character.LateralVelocity = Vector3.ClampMagnitude(
                character.LateralVelocity, character.Config.MaxSpeed);
        }

        public override void Tick(Character character)
        {
            character.LateralVelocity = character.transform.forward * (character.SkillActivator.Skill as DashSkill).Force;

            if (TimeSinceEntered > character.SkillActivator.Skill.ActiveTime)
            {
                if (character.IsGrounded)
                    character.StateMachine.Enter<WalkCharacterState>();
                else
                    character.StateMachine.Enter<FallCharacterState>();
            }
        }
    }
}