using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace _Project
{
    public class BladeBall : MonoBehaviour
    {
        public event Action<ICharacter> AllCharactersKilled;

        //[Kill] is called always when Ball kills Character
        public event Action Kill;
        //[KillWithBlock] called additinoally to [Kill] if Characted is killed by another
        //Character using shield
        public event Action<ICharacter> KillWithBlock;

        public List<ICharacter> Targets = new List<ICharacter>();
        public HashSet<ICharacter> InvisibleTargets = new HashSet<ICharacter>();
        public IEnumerable<ICharacter> AvailableTargets = new List<ICharacter>();
        public ICharacter LastBlockingCharacter;

        [SerializeField] private BladeBallConfig _config;
        [SerializeField] private GameObject _chargeVFX;
        [SerializeField] private GameObject _beamVFX;
        [SerializeField] private GameObject _frozenVFXPrefab;
        [SerializeField] private Color _activeColor;
        [SerializeField] private GameObject _trail;

        private List<Material> _initialMaterials = new List<Material>();
        private List<Material> _colorizedMaterials = new List<Material>();

        private Vector3 _startPosition;
        private float _speed;
        private MeshRenderer[] _renderers;
        private ICharacter _player;
        private bool _isChoosingFirstTarget;
        private bool _isCurrentTargetReacted;
        private float _freezeTimeLeft = 0.0f;

        private GameObject _frozenVFXInstance;

        public ICharacter CurrentTarget { get; private set; }

        public float RespawnTime => _config.RespawnTime;


        private enum State
        {
            RespawningState,
            ActiveState,
            FrozenState
        }

        private State _state;

        private void Start()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            EnableOutline();
            _startPosition = transform.position;
            StartCoroutine(Respawn());
        }

        private void Update()
        {
            switch (_state)
            {
                case State.RespawningState:
                    break;
                case State.ActiveState:
                    ActiveState();
                    break;
                case State.FrozenState:
                    UpdateFrozenState();
                    break;
            }
        }

        public void GetNextTarget(bool playBlockAnimation)
        {
            _isCurrentTargetReacted = false;
            AvailableTargets = Targets.Except(new ICharacter[] { CurrentTarget });

            if (AvailableTargets.Except(InvisibleTargets).Any())
            {
                AvailableTargets = AvailableTargets.Except(InvisibleTargets);
            }
            
            if (AvailableTargets.Count() > 0)
            {
                CurrentTarget?.UnmarkAsTarget();
                if (playBlockAnimation)
                {
                    CurrentTarget?.Block();
                }
                ChooseTarget();
                CurrentTarget?.MarkAsTarget();
                if (CurrentTarget.IsLocalPlayer)
                {
                    SetActiveColor();
                }
                else
                {
                    SetInitialColor();
                }
            }
            else
            {
                CurrentTarget = null;
                // DisableOutline();
            }
        }

        private void ChooseTarget()
        {
            if (_isChoosingFirstTarget)
            {
                int index = Random.Range(0, AvailableTargets.Count());
                CurrentTarget = AvailableTargets.ElementAt(index);
                return;
            }

            if (AvailableTargets.Contains(_player) && Random.value < _config.PlayerChooseProbability)
            {
                CurrentTarget = _player;
                return;
            }
            
            ICharacter selectedTarget = CurrentTarget.GetNextTarget(AvailableTargets);
            CurrentTarget = selectedTarget;
        }

        public void CharacterDied(ICharacter character)
        {
            Targets.Remove(character);
            if (character == CurrentTarget)
            {
                if (Targets.Count > 0)
                {
                    StartCoroutine(Respawn());
                }
            }
        }

        public void SetPlayer(ICharacter player)
        {
            _player = player;
        }

        private void ActiveState()
        {
            if (CurrentTarget == null)
            {
                StartCoroutine(Respawn());
            }

            MoveToTarget();
            float distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.GetPosition() + _config.TargetOffset);

            if (distanceToTarget <= _config.TargetBlockRadius)
            {
                if (_isCurrentTargetReacted == false)
                {
                    CurrentTarget.React(_speed);
                    _isCurrentTargetReacted = true;
                }
                if (CurrentTarget.IsBlockingByInput)
                {
                    LastBlockingCharacter = CurrentTarget;
                    _speed += _config.SpeedIncrement;
                    GetNextTarget(true);
                    return;
                }
                if (CurrentTarget.IsBlockingBySkill &&
                        !IsStandoff())
                {
                    LastBlockingCharacter = CurrentTarget;
                    _speed += _config.SpeedIncrement;
                    GetNextTarget(false);
                    return;
                }
            }
            
            if (distanceToTarget <= _config.TargetKillRadius)
            {
                KillTarget();
            }
        }

        private void MoveToTarget()
        {
            _beamVFX.SetActive(true);
            var targetPosition = CurrentTarget.GetPosition() + _config.TargetOffset;
            transform.LookAt(targetPosition);
            float step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }

        private void KillTarget()
        {
            Kill?.Invoke();
            Targets.Remove(CurrentTarget);
            if (LastBlockingCharacter != null)
            {
                KillWithBlock?.Invoke(LastBlockingCharacter);
            }
            CurrentTarget.Die();
            CurrentTarget = null;
            // DisableOutline();

            if (Targets.Count > 0)
            {
                StartCoroutine(Respawn());
            }
        }

        private IEnumerator Respawn()
        {
            _state = State.RespawningState;
            _trail.SetActive(false);
            LastBlockingCharacter = null;
            _speed = _config.StartSpeed;
            _beamVFX.SetActive(false);
            transform.position = _startPosition;
            _chargeVFX.SetActive(true);
            // DisableOutline();
            if (Targets.Count == 1)
            {
                AllCharactersKilled?.Invoke(Targets[0]);
                yield break;
            }
            yield return new WaitForSeconds(_config.RespawnTime);
            _chargeVFX.SetActive(false);
            _isChoosingFirstTarget = true;
            GetNextTarget(true);
            _isChoosingFirstTarget = false;
            if (CurrentTarget != null)
            {
                _state = State.ActiveState;
                _trail.SetActive(true);
            }
        }

        private void EnableOutline()
        {
            for (int i = 0; i < _renderers.Length; ++i)
            {
                int hash = _renderers[i].GetInstanceID();
                var position = transform.position;
                var rotation = transform.rotation;
                var scale = transform.lossyScale;
                OutlineRenderFeature.AddRendererForOutline(hash, _renderers[i], position, rotation, scale, OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);
            }
        }

        private void DisableOutline()
        {
            for (int i = 0; i < _renderers.Length; ++i)
            {
                int hash = _renderers[i].GetInstanceID();
                OutlineRenderFeature.RemoveRendererForOutline(hash, OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);
            }
        }
        
        public void SetInitialColor()
        {
            for (int i = 0; i < Math.Min(_renderers.Length, _initialMaterials.Count); ++i)
            {
                _renderers[i].sharedMaterial = _initialMaterials[i];
            }
        }
        
        public void SetActiveColor()
        {
            CheckColorizedMaterials();
            for (int i = 0; i < _renderers.Length; ++i)
            {
                var colorizedMat = _colorizedMaterials[i];
                _renderers[i].sharedMaterial = colorizedMat;
            }
        }

        private void UpdateFrozenState()
        {
            if (CurrentTarget == null)
            {
                _freezeTimeLeft = -1.0f;
                _frozenVFXInstance.SetActive(false);
                StartCoroutine(Respawn());
            }

            _freezeTimeLeft -= Time.deltaTime;
            if (_freezeTimeLeft < 0.0f)
            {
                _state = State.ActiveState;
                _frozenVFXInstance.SetActive(false);
            }

            float distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.GetPosition() + _config.TargetOffset);

            if (distanceToTarget <= _config.TargetBlockRadius)
            {
                if (_isCurrentTargetReacted == false)
                {
                    CurrentTarget.React(_speed);
                    _isCurrentTargetReacted = true;
                }
                if (CurrentTarget.IsBlockingByInput)
                {
                    _state = State.ActiveState;
                    _frozenVFXInstance.SetActive(false);

                    LastBlockingCharacter = CurrentTarget;
                    _speed += _config.SpeedIncrement;
                    GetNextTarget(true);
                }
            }
        }

        public bool Freeze(float time)
        {
            if (_state == State.RespawningState)
                return false;

            _state = State.FrozenState;
            _freezeTimeLeft = time;
            if (_frozenVFXInstance == null)
                _frozenVFXInstance = Instantiate(_frozenVFXPrefab, this.transform);
            _frozenVFXInstance.SetActive(true);
            return true;
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
                    colorizedMaterial.SetColor("_BaseColor", _activeColor);
                    _colorizedMaterials.Add(colorizedMaterial);
                }
            }

        }

        private bool IsStandoff()
        {
            return Targets.Count < 3;
        }
    }
}