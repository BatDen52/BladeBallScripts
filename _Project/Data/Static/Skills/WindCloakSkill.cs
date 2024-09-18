using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/WindCloakSkill")]
    public class WindCloakSkill : Skill
    {
        [SerializeField] private float SpeedMultiplier = 2.0f;
        [SerializeField] private float JumpFactor = 2.0f;
        [SerializeField] private GameObject CloakEffect;

        private Character _character;
        private CharacterConfig _config;
        private CoroutineHandle _coroutine;

        private GameObject _cloakEffect = null;
        private bool _isActive = false;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
            {
                _character = target.GetComponent<Character>();
                _config = _character.Config;
            }

            _character.OverrideMaxSpeed(_config.MaxSpeed * SpeedMultiplier);

            _cloakEffect = GameObject.Instantiate(CloakEffect, _character.transform);
            _isActive = true;

            return true;
        }

        override public float MaxJumpHeight(CharacterConfig characterConfig)
        {
            return characterConfig.MaxJumpHeight * (_isActive ? JumpFactor : 1.0f);
        }

        public override void Deactive(MonoBehaviour target)
        {
            _character.RemoveMaxSpeedOverride();
            GameObject.Destroy(_cloakEffect);
            _isActive = false;
        }

        public override void Cancel()
        {
            Timing.KillCoroutines(_coroutine);
        }

    }
}