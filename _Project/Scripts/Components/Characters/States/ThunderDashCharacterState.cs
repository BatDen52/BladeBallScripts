using _Project.Data.Static.Skills;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace _Project
{
    public class ThunderDashCharacterState : CharacterState
    {
        private ThunderDashSkill _skill;
        private GameObject _effect;

        public override void Enter(Character character)
        {
            character.VerticalVelocity = Vector3.zero;


            _skill = character.SkillActivator.Skill as ThunderDashSkill;
            _effect = GameObject.Instantiate(_skill.ThunderEffect, character.transform);
        }

        public override void Exit(Character character)
        {
            character.LateralVelocity = Vector3.ClampMagnitude(
                character.LateralVelocity, character.Config.MaxSpeed);

            Timing.RunCoroutine(Dash());
        }

        private IEnumerator<float> Dash()
        {
            yield return Timing.WaitForSeconds(_skill.ActiveTime);
            GameObject.Destroy(_effect);
        }

        public override void Tick(Character character)
        {
            character.LateralVelocity = character.transform.forward * (character.SkillActivator.Skill as ThunderDashSkill).Force;

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