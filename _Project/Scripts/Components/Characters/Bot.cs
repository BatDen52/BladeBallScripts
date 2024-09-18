using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace _Project
{
    public class Bot : MonoBehaviour
    {
        [SerializeField] private BotConfig _config;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private BotInput _botInput;

        private Vector3 _destination;
        private float _preferredDistanceToBall;
        private float _tendencyToMove;
        private float _moveDecisionTimer;
        private float _moveDecisionInterval;
        private float _tendencyToChangeTarget;
        private float _changeTargetTimer;
        private float _changeTargetInterval;
        private float _tendencyToLockRotation;
        private float _lockRotationTimer;
        private float _lockRotationInterval;
        private float _jumpTimer;
        private float _jumpInterval;
        private float _jumpProbability;
        private float _doubleJumpProbability;
        private float _useSkillTimer;
        private float _useSkillInterval;
        private float _useSkillProbability;
        private bool _isStanding;
        private bool _isMoving;
        private Vector3 _lastPosition;
        private Transform _target;

        [field: SerializeField] public Character Character { get; private set; }

        public BladeBall BladeBall { get; set; }

        public BotConfig Config => _config;

        public int WeaponId => Character.WeaponId;
        public int ScorePoints { get => Character.ScorePoints; set => Character.ScorePoints = value; }

        public Skin UsedSkinPrefab => _skinPrefab;
        public Skill UsedSkillPrefab => _skillPrefab;

        private Vector3 VectorToDestination => _destination - transform.position;
        private Vector3 VectorToTarget => _target.position - transform.position;
        private float RemainingDistance => VectorToDestination.magnitude;
        private float BladeBallDistance => (BladeBall.transform.position - transform.position).magnitude;

        private Skin _skinPrefab;
        private Skill _skillPrefab;

        public void SetConfig(BotConfig config)
        {
            _config = config;
            SetParams();
        }

        public void SetRandomSkin()
        {
            var skin = _config.Skins[Random.Range(0, _config.Skins.Count)];
            SetSkin(skin);
        }
        
        public void SetRandomSkill()
        {
            var skill = _config.Skills[Random.Range(0, _config.Skills.Count)];
            SetSkill(skill);
        }

        public void SetSkill(Skill skill)
        {
            _skillPrefab = skill;
            Character.SkillActivator.SetSkill(skill);
        }

        public void SetSkin(Skin skin)
        {
            _skinPrefab = skin;
            Character.SetSkin(_skinPrefab);
        }

        private void Update()
        {
            Look();
            Move();
            Jump();
            UseSkill();
        }

        private void Start()
        {
            SetParams();
            _target = BladeBall.transform;
            _botInput.SetRotationLock(Random.value < _tendencyToLockRotation);
            BladeBall.Kill += ChangeTarget;
        }

        private void OnEnable()
        {
            Character.BladeBallCameClose += OnBladeBallCameClose;
        }

        private void OnDisable()
        {
            Character.BladeBallCameClose -= OnBladeBallCameClose;
            BladeBall.Kill -= ChangeTarget;
        }

        private void Move()
        {
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, VectorToDestination, Color.cyan);
#endif

            if (_isStanding)
            {
                return;
            }

            if (_isMoving)
            {
                if (_destination == Vector3.zero || RemainingDistance < _config.DestinationReachedDistance)
                {
                    SetNewRandomDestination();
                }
                else
                {
                    if (transform.position == _lastPosition && Time.timeScale > 0)
                    {
                        SetNewRandomDestination();
                    }

                    _botInput.MoveDirection = _navMeshAgent.velocity.normalized;
                    _lastPosition = transform.position;
                }
                
                return;
            }

            if (Random.value < _tendencyToMove)
            {
                Move(_moveDecisionInterval);
            }
            else
            {
                Stand(_moveDecisionInterval);
            }
        }
        
        private async void Move(float time)
        {
            _isMoving = true;
            await UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: false);
            _isMoving = false;
        }

        private void Look()
        {
#if UNITY_EDITOR
            Debug.DrawRay(transform.position + new Vector3(0f, 2f, 0f), VectorToTarget, Color.red);
#endif
            
            _changeTargetTimer += Time.deltaTime;
            _lockRotationTimer += Time.deltaTime;

            if (_changeTargetTimer > _changeTargetInterval)
            {
                _changeTargetTimer = 0f;

                if (Random.value < _tendencyToChangeTarget)
                {
                    ChangeTarget();
                }
            }
            
            if (_lockRotationTimer > _lockRotationInterval)
            {
                _lockRotationTimer = 0f;
                _botInput.SetRotationLock(Random.value < _tendencyToLockRotation);
            }

            Vector3 lookDirection = _target.position - transform.position;
            lookDirection.y = 0;
            _botInput.LookDirection = lookDirection;
        }

        private async void ChangeTarget()
        {
            List<ICharacter> availableTargets =
                BladeBall.AvailableTargets.Except(new ICharacter[] { Character }).ToList();

            if (availableTargets.Count == 0)
            {
                return;
            }
            
            _target = availableTargets[Random.Range(0, availableTargets.Count)].Transform;

            if (_isStanding)
            {
                _botInput.MoveDirection = _botInput.LookDirection.normalized;
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), ignoreTimeScale: false);
                _botInput.MoveDirection = Vector3.zero;
            }
        }

        private void SetNewRandomDestination()
        {
            _destination = GetRandomPointOnNavMesh();
            _navMeshAgent.SetDestination(_destination);
        }

        private async void Stand(float time)
        {
            _isStanding = true;
            _botInput.MoveDirection = Vector3.zero;
            await UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: false);
            _isStanding = false;
        }

        private void OnBladeBallCameClose(float speed)
        {
            var blockProbability = _config.BlockProbabilityCurve.Evaluate(speed);
            if (Random.value < blockProbability)
            {
                _botInput.ActivateBlock();
            }
        }

        private void UseSkill()
        {
            _useSkillTimer += Time.deltaTime;

            if (Character.SkillActivator.State != SkillState.Ready)
            {
                return;
            }

            if (_useSkillTimer > _useSkillInterval)
            {
                _useSkillTimer = 0f;
                if (Random.value < _useSkillProbability)
                {
                    _botInput.ActivateUseSkill();
                    if (Character.SkillActivator.Skill.Id == 1)
                    {
                        // Debug.Log("Use Platform");
                        // Time.timeScale = 0.01f;
                        Stand(Character.SkillActivator.Skill.ActiveTime - 2f);
                    }
                }
            }
        }

        private void Jump()
        {
            _jumpTimer += Time.deltaTime;
            if (_botInput.IsJumping == false && _jumpTimer > _jumpInterval)
            {
                _jumpTimer = 0f;
                if (Random.value < _jumpProbability)
                {
                    if (Random.value < _doubleJumpProbability)
                    {
                        Timing.RunCoroutine(_DoubleJump());
                    }
                    else
                    {
                        Timing.RunCoroutine(_Jump());
                    }
                }
            }
        }

        private IEnumerator<float> _Jump()
        {
            _botInput.IsJumping = true;
            yield return Timing.WaitForSeconds(_config.JumpTimings.FirstPress);
            _botInput.IsJumping = false;
        }

        private IEnumerator<float> _DoubleJump()
        {
            _botInput.IsJumping = true;
            yield return Timing.WaitForSeconds(_config.JumpTimings.FirstPress);
            _botInput.IsJumping = false;
            yield return Timing.WaitForSeconds(_config.JumpTimings.Pause);
            _botInput.IsJumping = true;
            yield return Timing.WaitForSeconds(_config.JumpTimings.SecondPress);
            _botInput.IsJumping = false;
        }

        public Vector3 GetRandomPointOnNavMesh()
        {
            Vector3 randomDirection = Random.insideUnitSphere * _config.NextDestinationRadius;
            randomDirection += transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, _config.NextDestinationRadius, 1))
            {
                return hit.position;
            }

            return Vector3.zero;
        }

        private void SetParams()
        {
            _jumpTimer -= Random.Range(_config.DecisionsDelay.Min, _config.DecisionsDelay.Max);
            _useSkillTimer -= Random.Range(_config.DecisionsDelay.Min, _config.DecisionsDelay.Max);
            _moveDecisionTimer -= Random.Range(_config.DecisionsDelay.Min, _config.DecisionsDelay.Max);
            _changeTargetTimer -= Random.Range(_config.DecisionsDelay.Min, _config.DecisionsDelay.Max);
            _lockRotationTimer -= Random.Range(_config.DecisionsDelay.Min, _config.DecisionsDelay.Max);


            _jumpInterval = Random.Range(_config.JumpInterval.Min, _config.JumpInterval.Max);
            _jumpProbability = Random.Range(_config.JumpProbability.Min, _config.JumpProbability.Max);
            _doubleJumpProbability = Random.Range(_config.DoubleJumpProbability.Min, _config.DoubleJumpProbability.Max);

            _useSkillInterval = Random.Range(_config.UseSkillInterval.Min, _config.UseSkillInterval.Max);
            _useSkillProbability = Random.Range(_config.UseSkillProbability.Min, _config.UseSkillProbability.Max);

            _moveDecisionInterval = Random.Range(_config.MoveDecisionInterval.Min, _config.MoveDecisionInterval.Max);
            _tendencyToMove = Random.Range(_config.TendencyToMove.Min, _config.TendencyToMove.Max);
            _preferredDistanceToBall =
                Random.Range(_config.PreferredDistanceFromBall.Min, _config.PreferredDistanceFromBall.Max);

            _changeTargetInterval = Random.Range(_config.ChangeTargetInterval.Min, _config.ChangeTargetInterval.Max);
            _tendencyToChangeTarget =
                Random.Range(_config.TendencyToChangeTarget.Min, _config.TendencyToChangeTarget.Max);

            _lockRotationInterval = Random.Range(_config.LockRotationInterval.Min, _config.LockRotationInterval.Max);
            _tendencyToLockRotation =
                Random.Range(_config.TendencyToLockRotation.Min, _config.TendencyToLockRotation.Max);
        }
    }
}