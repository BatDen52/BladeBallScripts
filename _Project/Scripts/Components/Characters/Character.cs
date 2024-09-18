using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using Cysharp.Threading.Tasks;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace _Project
{
    public class Character : Entity<Character>, ICharacter
    {
        [SerializeField] private GameObject _shield;
        [SerializeField] private Transform _skinParent;
        [SerializeField] private CharacterAnimator _characterAnimator;
        [SerializeField] private SkinnedMeshRenderer[] _renderers;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _deathSFXSpatialBlend = 0.5f;
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private Vector3 _deathVFXPosition;
        [SerializeField] private Color _whenTargetColor = new Color(235 / 255.0f, 122 / 255.0f, 122 / 255.0f);

        private bool _useGravity = true;

        public FloatReactiveProperty BlockCooldown { get; private set; } = new FloatReactiveProperty();
        public float MaxBlockCooldown => Config.BlockConfig.Cooldown;
        public bool IsRotationLocked;

        [field: SerializeField] public Skin Skin { get; private set; }
        [field: SerializeField] public SkillActivator SkillActivator { get; private set; }
        [field: SerializeField] public Transform CameraTarget { get; private set; }
        [field: SerializeField] public CharacterConfig Config { get; private set; }
        [field: SerializeField] public CharacterInput Input { get; private set; }

        public Transform Transform => transform;
        public int JumpCounter { get; protected set; }
        public bool IsBlockingByInput { get; private set; }
        public bool IsBlockingBySkill => _isBlockingBySkill;
        public bool IsLocalPlayer { get; set; }

        public event Action Died;
        public event Action<float> BladeBallCameClose;
        public event Action<ICharacter> DieByAccident;

        private List<Material> _initialMaterials = new List<Material>();
        private List<Material> _colorizedMaterials = new List<Material>();

        private BladeBall _bladeBall;
        private Settings _settings;
        private IAudio _audio;
        private IAnalytics _analytics;
        private IPersistentDataService _persistentDataService;
        private Vibrations _vibrations;
        public Weapon Weapon { get; private set; }

        public int ScorePoints
        {
            get => _scorePoints;
            set => _scorePoints = value;
        }

        public int WeaponId => _weaponData.Id;

        private WeaponData _weaponData;
        private CoroutineHandle _blockingCoroutine;
        private CoroutineHandle _returnWeaponToIdleCoroutine;
        private Transform _transform; //use this to avoid calling transform getter function calling
        private bool _isBallInRange;
        private bool _isDead = false;
        private bool _isBlockingBySkill;

        private int _scorePoints;

        private bool _isMaxSpeedOverriden = false;
        private float _maxSpeedOverrideValue = 1.0f;

        [Inject]
        private void Construct(Settings settings, IAudio audio, IAnalytics analytics,
            IPersistentDataService persistentDataService, Vibrations vibrations)
        {
            _settings = settings;
            _audio = audio;
            _analytics = analytics;
            _persistentDataService = persistentDataService;
            _vibrations = vibrations;
        }

        private void OnEnable()
        {
            Input.Block += OnBlock;
            Input.UseSkill += OnUseSkill;
            Input.RotationLock += OnRotationLock;
            _transform = transform;
        }

        protected override void Update()
        {
            base.Update();

            if (BlockCooldown.Value >= 0.001f)
            {
                BlockCooldown.Value -= Time.deltaTime;
            }

            if ((_transform.position.y < _settings.CharacterDeathHeightBottom) ||
                (_transform.position.y > _settings.CharacterDeathHeightTop))
            {
                PlayDeathEffects();
                DieByAccident?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            Input.Block -= OnBlock;
            Input.UseSkill -= OnUseSkill;
        }

        public void SetBladeBall(BladeBall bladeBall)
        {
            _bladeBall = bladeBall;
        }

        public void SetRandomWeapon()
        {
            int weaponId = Random.Range(0, _settings.Weapons.Count);
            SetWeapon(weaponId);
        }

        public void SetWeapon(int id)
        {
            if (Weapon != null)
            {
                Destroy(Weapon.gameObject);
            }

            _weaponData = _settings.Weapons[id];

            if (_settings.IsWeaponAlwaysInHand)
            {
                Weapon = Instantiate(_weaponData.Prefab, Skin.WeaponSlot);
                SetWeaponToActivePosition();
            }
            else
            {
                Weapon = Instantiate(_weaponData.Prefab, Skin.IdleWeaponSlot);
                SetWeaponToIdlePosition();
            }
        }

        private void SetWeaponToIdlePosition()
        {
            Weapon.transform.localPosition = _weaponData.Offsets.IdleOffset;
            Weapon.transform.localEulerAngles = _weaponData.Offsets.IdleRotation;
        }

        private void SetWeaponToActivePosition()
        {
            Weapon.transform.localPosition = _weaponData.Offsets.Offset;
            Weapon.transform.localEulerAngles = _weaponData.Offsets.Rotation;
        }

        private void OnBlock()
        {
            if (BlockCooldown.Value > 0.001f)
            {
                return;
            }

            if (IsLocalPlayer && _isBallInRange == false)
            {
                _audio.PlaySound(_audio.Sounds.Block, _audioSource);
                _vibrations.VibratePop();
            }

            BlockCooldown.Value = Config.BlockConfig.Cooldown;
            Timing.KillCoroutines(_returnWeaponToIdleCoroutine);
            _blockingCoroutine = Timing.RunCoroutine(_Block(Config.BlockConfig.Duration).CancelWith(gameObject));
        }

        public IEnumerator<float> _Block(float blockDuration)
        {
            IsBlockingByInput = true;
            _shield.SetActive(true);
            // _characterAnimator.Block();
            yield return Timing.WaitForSeconds(blockDuration);
            IsBlockingByInput = false;
            _shield.SetActive(false);
        }

        public ICharacter GetNextTarget(IEnumerable<ICharacter> availableTargets)
        {
            ICharacter closestTarget = availableTargets.First();
            float closestAngle = Mathf.Infinity;

            Vector3 forward = Input.LookDirection;
            forward.y = 0;

            var targetList = availableTargets.ToList();
            for (int i = 0; i < targetList.Count; i++)
            {
                Vector3 directionToTarget = targetList[i].GetPosition() - transform.position;
                directionToTarget.y = 0;

                float angle = Vector3.Angle(forward, directionToTarget.normalized);

                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closestTarget = targetList[i];
                }
            }

            return closestTarget;
        }

        public void Block()
        {
            _audio.PlaySound(_audio.Sounds.Hit, _audioSource);
            
            if (IsLocalPlayer)
            {
                _vibrations.VibratePeek();
            }
            
            if (Weapon.Vfx != null)
                Weapon.Vfx.SetActive(true);
            Timing.KillCoroutines(_blockingCoroutine);
            BlockCooldown.Value = 0f;
            IsBlockingByInput = false;
            _shield.SetActive(false);
            _isBallInRange = false;
            Weapon.transform.SetParent(Skin.WeaponSlot);
            SetWeaponToActivePosition();
            _characterAnimator.Block();

            if (_settings.IsWeaponAlwaysInHand == false)
            {
                _returnWeaponToIdleCoroutine = Timing.RunCoroutine(_ReturnWeaponToIdle().CancelWith(gameObject));
            }
        }


        public void SetBlockingBySkill(bool value)
        {
            _isBlockingBySkill = value;
        }

        public void PlaySound(Sound sound)
        {
            _audio.PlaySound(sound, _audioSource);
        }

        public void React(float speed)
        {
            BladeBallCameClose?.Invoke(speed);
            _isBallInRange = true;
        }

        private IEnumerator<float> _ReturnWeaponToIdle()
        {
            yield return Timing.WaitForSeconds(Config.BlockConfig.ReturnWeaponToIdleAfter);
            // if (_weapon.Vfx != null)
            //     _weapon.Vfx?.SetActive(false);
            Weapon.transform.SetParent(Skin.IdleWeaponSlot);
            SetWeaponToIdlePosition();
        }

        public void SetRandomSkin()
        {
            Skin skinPrefab = Config.Skins[Random.Range(0, Config.Skins.Count)];
            SetSkin(skinPrefab);
        }

        public void SetSkin(Skin skinPrefab)
        {
            foreach (Transform child in _skinParent)
            {
                Destroy(child.gameObject);
            }

            Skin = Instantiate(skinPrefab, _skinParent);
            _characterAnimator.Animator = Skin.Animator;
            _renderers = Skin.Renderers;
        }

        public void SetRandomSkill()
        {
            Skill skill = _settings.Skills[Random.Range(0, _settings.Skills.Count)];
            SkillActivator.SetSkill(skill);
        }

        private void OnUseSkill()
        {
            SkillActivator.Activate();

            if (IsLocalPlayer)
            {
                _analytics.LogEvent(AnalyticsEvents.skill_use,
                    (AnalyticsParameters.name, SkillActivator.Skill.Title));
            }
        }

        private void OnRotationLock(bool value)
        {
            IsRotationLocked = value;
        }

        public void OverrideMaxSpeed(float maxSpeed)
        {
            _isMaxSpeedOverriden = true;
            _maxSpeedOverrideValue = maxSpeed;
        }

        public void RemoveMaxSpeedOverride()
        {
            _isMaxSpeedOverriden = false;
        }

        public void Accelerate(Vector3 direction)
        {
            float turningDrag = Config.TurningDrag;
            var acceleration = IsGrounded ? Config.Acceleration : Config.AirAcceleration;
            var maxSpeed = _isMaxSpeedOverriden ? _maxSpeedOverrideValue : Config.MaxSpeed;

            Accelerate(direction, turningDrag, acceleration, maxSpeed);
        }

        public void AccelerateToInputDirection()
        {
            var inputDirection = Input.MoveDirection;
            Accelerate(inputDirection);
        }


        public void Decelerate() => Decelerate(Config.Deceleration);

        public void OnGravity()
        {
            _useGravity = true;
        }

        public void OffGravity()
        {
            _useGravity = false;
        }


        public void Gravity()
        {
            if (_useGravity && !IsGrounded && VerticalVelocity.y > -Config.GravityTopSpeed)
            {
                var speed = VerticalVelocity.y;
                var force = VerticalVelocity.y > 0 ? Config.Gravity : Config.FallGravity;
                speed -= force * Time.deltaTime;
                speed = Mathf.Max(speed, -Config.GravityTopSpeed);
                VerticalVelocity = new Vector3(0, speed, 0);
            }
        }

        public void ResetJumps() => JumpCounter = 0;


        //This is used to call jump from WalkState. To avoid Chicken-Egg problem
        //so animations are switched correctly
        private float _jumpIsRequested = 0.0f;
        private TimeSpan _fallStateDelay = TimeSpan.FromSeconds(0.1f);

        public void RequestJump(float height)
        {
            _jumpIsRequested = height;
        }

        public virtual void Jump()
        {
            bool canJump = JumpCounter > 0 && JumpCounter < SkillActivator.MaxJumps(Config);

            if (IsGrounded || canJump)
            {
                if (Input.GetJumpDown())
                {
                    Jump(SkillActivator.MaxJumpHeight(Config));
                }
                else if (_jumpIsRequested > 0.0f)
                {
                    Jump(_jumpIsRequested);
                }
            }

            // if (Input.GetJumpUp() && JumpCounter > 0 && VerticalVelocity.y > Config.MinJumpHeight)
            // {
            // VerticalVelocity = Vector3.up * Config.MinJumpHeight;
            // }

            _jumpIsRequested = 0.0f;
        }

        public async void Jump(float height)
        {
            if (IsLocalPlayer)
            {
                _vibrations.VibratePop();
            }
            
            JumpCounter++;
            VerticalVelocity = Vector3.up * height;
            await UniTask.Delay(_fallStateDelay, ignoreTimeScale: false);
            StateMachine.Enter<FallCharacterState>();
        }

        protected override void EnterGround(RaycastHit hit)
        {
            base.EnterGround(hit);
            ResetJumps();
        }

        public void Die()
        {
            PlayDeathEffects();
            Died?.Invoke();
        }

        private void PlayDeathEffects()
        {
            AudioSource audioSource = _audio.GetNewAudioSource(_deathSFXSpatialBlend);
            audioSource.transform.position = transform.position + _deathVFXPosition;
            _audio.PlaySound(_audio.Sounds.Death, audioSource);
            _vibrations.Vibrate(_vibrations.Patterns.Death);
            Instantiate(_deathVFXPrefab, transform.position + _deathVFXPosition, Quaternion.identity);
            gameObject.SetActive(false);

            DisableHoverOutline();
            EnableInSceneOutline();
            _isDead = true;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void MarkAsTarget()
        {
            CheckColorizedMaterials();
            for (int i = 0; i < _renderers.Length; ++i)
            {
                var colorizedMat = _colorizedMaterials[i];
                _renderers[i].sharedMaterial = colorizedMat; // Config.WhenTargetMaterial;
            }

            EnableHoverOutline();
        }

        private void CheckColorizedMaterials()
        {
            if (_colorizedMaterials.Count != _renderers.Length)
            {
                _colorizedMaterials.Clear();
                _initialMaterials.Clear();

                for (int i = 0; i < _renderers.Length; ++i)
                {
                    _initialMaterials.Add(_renderers[i].sharedMaterial);

                    var colorizedMaterial = new Material(_renderers[i].sharedMaterial);
                    colorizedMaterial.SetColor("_BaseColor", _whenTargetColor);
                    _colorizedMaterials.Add(colorizedMaterial);
                }
            }
        }

        public void UnmarkAsTarget()
        {
            ApplyNormalMaterial();

            DisableHoverOutline();
        }

        public void ApplyTransparentMaterial()
        {
            CheckColorizedMaterials();
            for (int i = 0; i < _renderers.Length; ++i)
            {
                _renderers[i].sharedMaterial = _settings.DiscardMaterial;
            }
        }

        public void ApplyNormalMaterial()
        {
            for (int i = 0; i < Math.Min(_renderers.Length, _initialMaterials.Count); ++i)
            {
                _renderers[i].sharedMaterial = _initialMaterials[i];
            }
        }

        private void EnableHoverOutline()
        {
            for (int i = 0; i < _renderers.Length; ++i)
            {
                var colorizedMat = _colorizedMaterials[i];
                _renderers[i].sharedMaterial = colorizedMat; // Config.WhenTargetMaterial;

                var m = _renderers[i].sharedMesh;
                int hash = _renderers[i].GetInstanceID();
                var position = transform.position;
                var rotation = transform.rotation;
                var scale = transform.lossyScale;
                OutlineRenderFeature.AddRendererForOutline(hash, _renderers[i], position, rotation, scale,
                    OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);
            }
        }

        private void DisableHoverOutline()
        {
            for (int i = 0; i < Math.Min(_renderers.Length, _initialMaterials.Count); ++i)
            {
                int hash = _renderers[i].GetInstanceID();
                OutlineRenderFeature.RemoveRendererForOutline(hash, OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);
            }
        }

        public void EnableInSceneOutline()
        {
            CheckColorizedMaterials();
            for (int i = 0; i < _renderers.Length; ++i)
            {
                var colorizedMat = _colorizedMaterials[i];
                _renderers[i].sharedMaterial = colorizedMat; // Config.WhenTargetMaterial;

                var m = _renderers[i].sharedMesh;
                int hash = _renderers[i].GetInstanceID();
                var position = transform.position;
                var rotation = transform.rotation;
                var scale = transform.lossyScale;
                OutlineRenderFeature.AddRendererForOutline(hash, _renderers[i], position, rotation, scale,
                    OutlineRenderFeature.OutlineType.IN_SCENE);
            }
        }

        public void DisableInSceneOutline()
        {
            //When dead, outline should be visible
            if (_isDead)
                return;

            for (int i = 0; i < Math.Min(_renderers.Length, _initialMaterials.Count); ++i)
            {
                int hash = _renderers[i].GetInstanceID();
                OutlineRenderFeature.RemoveRendererForOutline(hash, OutlineRenderFeature.OutlineType.IN_SCENE);
            }
        }

        public void AddScorePoints(int points)
        {
            _scorePoints += points;
        }
    }
}