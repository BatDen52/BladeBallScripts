using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/FreezeSkill")]
    public class FreezeSkill : Skill
    {
        [field: SerializeField] public GameObject FreezeAura { get; private set; }

        private Character _character;
        private GameObject _freezeAura;

        public override bool Activate(MonoBehaviour target)
        {
            if (_character == null)
                _character = target.GetComponent<Character>();

            if (!GameFactory.BladeBall.Freeze(this.ActiveTime))
                return false;

            if (!base.Activate(target))
                return false;

            if (_freezeAura == null)
                _freezeAura = Instantiate(FreezeAura, _character.transform);
            _freezeAura.SetActive(true);

            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            _freezeAura.SetActive(false);
            //Destroy(_freezeAura);
        }

        public override void Cancel()
        {
        }

    }
}