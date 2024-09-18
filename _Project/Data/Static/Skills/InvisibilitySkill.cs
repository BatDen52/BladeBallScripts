using _Project;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{
    [CreateAssetMenu(menuName = "Skills/InvisibilitySkill")]
    public class InvisibilitySkill : Skill
    {
        [field: SerializeField] public GameObject DisappearEffect { get; private set; }
        private Character _character;
        private BladeBall _bladeBall;
        private GameObject _effect;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
                _character = target.GetComponent<Character>();

            if (_character.IsLocalPlayer)
                _character.EnableInSceneOutline();

            if (_bladeBall == null)
                _bladeBall = GameFactory.BladeBall;

            _bladeBall.InvisibleTargets.Add(_character);

            if (_bladeBall.CurrentTarget == (ICharacter)_character)
                _bladeBall.GetNextTarget(true);

            SetVisibility(false);

            _effect = GameObject.Instantiate(DisappearEffect, target.transform.position, target.transform.rotation);

            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            _bladeBall?.InvisibleTargets.Remove(_character);
            SetVisibility(true);

            if (_character.IsLocalPlayer)
                _character.DisableInSceneOutline();

            GameObject.Destroy(_effect);
        }

        public override void Cancel()
        {
        }

        private void SetVisibility(bool visible)
        {
            if (visible)
                _character.ApplyNormalMaterial();
            else
                _character.ApplyTransparentMaterial();
            _character.Weapon.gameObject.SetActive(visible);
        }
    }
}