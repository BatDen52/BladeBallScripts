using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/ForcefieldSkill")]
    public class ForcefieldSkill : Skill
    {
        [field: SerializeField] public GameObject ForcefieldAura { get; private set; }

        private Character _character;
        private GameObject _forcefieldAura;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
                _character = target.GetComponent<Character>();

            _character.SetBlockingBySkill(true);

            if (_forcefieldAura == null)
                _forcefieldAura = Instantiate(ForcefieldAura, _character.transform);

            _forcefieldAura.SetActive(true);

            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            _forcefieldAura.SetActive(false);
            _character.SetBlockingBySkill(false);
        }

        public override void Cancel()
        {
        }

    }
}