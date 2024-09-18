using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{
    [CreateAssetMenu(menuName = "Skills/SuperJumpSkill")]
    public class SuperJumpSkill : Skill
    {
        [SerializeField] private float _height;
        [SerializeField] private GameObject _effect;

        private Character _character;
        private GameObject _instantiatedEffect;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
                _character = target.GetComponent<Character>();

            _instantiatedEffect = GameObject.Instantiate(_effect, _character.transform);
            target.StartCoroutine(Jump());

            return true;
        }

        public override void Deactive(MonoBehaviour target)
        {
            // _character.OnGravity();
        }

        public override void Cancel()
        {
        }

        private IEnumerator Jump()
        {
            // _character.RequestJump(_height);
            yield return new WaitForEndOfFrame();
            _character.Jump(_height);

            yield return new WaitWhile(() => _character.VerticalVelocity.y > 0.001);

            // _character.OffGravity();
            // _character.VerticalVelocity = Vector3.zero;
            GameObject.Destroy(_instantiatedEffect);
        }
    }
}