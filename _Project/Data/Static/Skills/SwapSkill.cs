using _Project;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Linq;
using static UnityEngine.UI.Image;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/SwapSkill")]
    public class SwapSkill : Skill
    {
        [field: SerializeField] public float DelayBeforeTelepor { get; private set; } = 20;
        [field: SerializeField] public GameObject TeleportFromEffect { get; private set; }
        [field: SerializeField] public GameObject TeleportToEffect { get; private set; }

        [field: SerializeField] public GameObject TeleportFromEffectOther { get; private set; }
        [field: SerializeField] public GameObject TeleportToEffectOther { get; private set; }

        private Character _character;
        private Character _otherCharacter;
        private CoroutineHandle _coroutine;

        private Vector3 _lastTeleportFrom = Vector3.zero;
        private Vector3 _lastTeleportTo = Vector3.zero;

        private GameObject[] TeleportFromEffectsArray = new GameObject[2];

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            if (_character == null)
            {
                _character = target.GetComponent<Character>();
            }

            var availableTargets = GameFactory.BladeBall.Targets.Except(new ICharacter[] { _character });

            _otherCharacter = (Character)_character.GetNextTarget(availableTargets);

            _coroutine = Timing.RunCoroutine(Teleport());

            return true;
        }


        private IEnumerator<float> Teleport()
        {
            TeleportFromEffectsArray[0] = GameObject.Instantiate(TeleportFromEffect, _character.transform);
            TeleportFromEffectsArray[1] = GameObject.Instantiate(TeleportFromEffectOther, _otherCharacter.transform);

            yield return Timing.WaitForSeconds(DelayBeforeTelepor);

            var origin = _character.transform.position;
            var otherOrigin = _otherCharacter.transform.position;

            var movementVector = otherOrigin - origin;
            var newPosition = origin + movementVector;

            _lastTeleportFrom = origin;
            _lastTeleportTo = newPosition;

            _character.Controller.Move(movementVector);
            _otherCharacter.Controller.Move(-movementVector);

            GameObject.Destroy(TeleportFromEffectsArray[0]);
            GameObject.Destroy(TeleportFromEffectsArray[1]);

            GameObject.Instantiate(TeleportToEffect, _character.transform);
            GameObject.Instantiate(TeleportToEffectOther, _otherCharacter.transform);
        }

        public override void Deactive(MonoBehaviour target)
        {
        }

        public override void Cancel()
        {
        }

#if UNITY_EDITOR
        override public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(_lastTeleportFrom, _lastTeleportTo);
        }
#endif

    }
}