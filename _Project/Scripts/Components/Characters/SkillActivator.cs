using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using _Project.UI;
using UniRx;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class SkillActivator : MonoBehaviour
    {
        [SerializeField] private KeyCode _key;
        [field: SerializeField] public Skill Skill { get; private set; }
        public FloatReactiveProperty SkillCooldown { get; private set; } = new FloatReactiveProperty();
        public IntReactiveProperty SkillId { get; private set; } = new IntReactiveProperty();
        public IntReactiveProperty SkillActivationsCountLeft { get; private set;} = new IntReactiveProperty();
        public SkillState State { get; set; } = SkillState.Ready;

        private IGameFactory _gameFactory;
        private Vibrations _vibrations;
        private CoroutineHandle _coroutineActive;
        private CoroutineHandle _coroutineCooldown;
        private IUIService _uiService;
        private Character _myCharacter;

        public float MaxSkillCooldown => Skill.CooldownTime + Skill.ActiveTime;

        public event Action Activated;
        public event Action CooldownStarted;
        public event Action CooldownEnded;

        [Inject]
        public void Constructor(IObjectResolver container)
        {
            _gameFactory = container.Resolve<IGameFactory>();
            _uiService = container.Resolve<IUIService>();
            _vibrations = container.Resolve<Vibrations>();
        }

        private void OnEnable()
        {
            _myCharacter = GetComponent<Character>();
        }

        private void Update()
        {
            if (SkillCooldown.Value >= 0.001f)
            {
                SkillCooldown.Value -= Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            Timing.KillCoroutines(_coroutineActive);
            Skill?.Cancel();
        }

        public int MaxJumps(CharacterConfig characterConfig)
        {
            return Skill.MaxJumps(characterConfig);
        }

        public float MaxJumpHeight(CharacterConfig characterConfig)
        {
            return Skill.MaxJumpHeight(characterConfig);
        }

        public void Activate()
        {
            if (State == SkillState.Active)
                return;
            if (Skill.ActivatedTimesCount() >= Skill.ActivationsCount)
                return;

            bool isActivated = Skill.Activate(this);
            if (!isActivated)
                return;

            SkillActivationsCountLeft.Value -= 1;

            if (Skill.ActivateSound != null)
                _myCharacter.PlaySound(Skill.ActivateSound);

            if (_myCharacter.IsLocalPlayer)
            {
                _vibrations.VibratePeek();
            }
            
            State = SkillState.Active;

            if (Skill.ActivatedTimesCount() == 1)
                SkillCooldown.Value = MaxSkillCooldown;

            Activated?.Invoke();

            _coroutineActive = Timing.RunCoroutine(RunActiveTimer());
        }

        public void SetSkill(Skill skill)
        {
            Skill = Instantiate(skill);
            Skill.Init(_gameFactory);
            SkillId.Value = skill.Id;

            SkillActivationsCountLeft.Value = skill.ActivationsCount;

            State = skill.IsPassive ? SkillState.Passive : SkillState.Ready;
        }

        private IEnumerator<float> RunActiveTimer()
        {
            yield return Timing.WaitForSeconds(Skill.ActiveTime);

            Skill.Deactive(this);

            if (_myCharacter.IsLocalPlayer && Skill.ActiveTime > 2)
            {
                _vibrations.VibratePeek();
            }
            
            if (Skill.DeactivateSound != null)
                _myCharacter.PlaySound(Skill.DeactivateSound);

            State = SkillState.Cooldown;
            CooldownStarted?.Invoke();
            if (!_coroutineCooldown.IsRunning)
                _coroutineCooldown = Timing.RunCoroutine(RunCooldown(Skill.CooldownTime));
        }

        private IEnumerator<float> RunCooldown(float cooldownTime)
        {
            yield return Timing.WaitForSeconds(cooldownTime);

            Skill.CooldownEnded(this);
            State = SkillState.Ready;
            CooldownEnded?.Invoke();
            
            if (_myCharacter.IsLocalPlayer)
            {
                _vibrations.Vibrate(_vibrations.Patterns.Reload);
            }

            SkillActivationsCountLeft.Value += 1;

            if (Skill.ActivatedTimesCount() > 0)
            {
                SkillCooldown.Value = MaxSkillCooldown;
                _coroutineCooldown = Timing.RunCoroutine(RunCooldown(MaxSkillCooldown));
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Skill.OnDrawGizmos();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_myCharacter.Position, _myCharacter.Radius * 4.0f);
        }
#endif
    }
}

