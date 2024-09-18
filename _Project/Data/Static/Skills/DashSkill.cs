using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/DashSkill")]
    public class DashSkill : Skill
    {
        [field: SerializeField] public float Force { get; private set; }

        private Character _character;
        private CoroutineHandle _coroutine;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
                _character = target.GetComponent<Character>();

            _character.StateMachine.Enter<DashCharacterState>();
            //_character.Velocity = Vector3.zero;

            //_coroutine = Timing.RunCoroutine(Dash());

            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            //if (_coroutine.IsRunning)
            //{
            //    Timing.KillCoroutines(_coroutine);
            //}

            //_character.LateralVelocity = Vector3.ClampMagnitude(_character.LateralVelocity, _character.Config.MaxSpeed);
        }

        public override void Cancel()
        {
            Timing.KillCoroutines(_coroutine);
        }

        private IEnumerator<float> Dash()
        {
            while (_coroutine != null)
            {
                _character.LateralVelocity = _character.transform.forward * Force;
                yield return Timing.WaitForSeconds(Time.fixedDeltaTime);
            }
        }
    }
}