using _Project;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/ShadowStep")]
    public class ShadowStepSkill : Skill
    {
        [field: SerializeField] public GameObject AuraPrefab { get; private set; }
        [field: SerializeField] public float SpeedMultiplier { get; private set; }

        private GameObject _aura;
        private Character _character;
        private CharacterConfig _config;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
            {
                _character = target.GetComponent<Character>();
                _config = _character.Config;
            }

            if (_aura == null)
                _aura = Instantiate(AuraPrefab, _character.transform);
            _aura.SetActive(true);

            _character.OverrideMaxSpeed(_config.MaxSpeed * SpeedMultiplier);
            
            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            _aura.SetActive(false);
            _character.RemoveMaxSpeedOverride();
        }

        public override void Cancel()
        {
        }

    }
}