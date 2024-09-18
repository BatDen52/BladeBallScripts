using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder;

namespace _Project
{
    public abstract class Entity<T> : MonoBehaviour where T : Entity<T>
    {
        [field: SerializeField] public CharacterController Controller { get; protected set; }
        [field: SerializeField] public Transform LookAtTarget { get; set; }
        
        public EntityStateMachine<T> StateMachine { get; private set; }
        public bool IsGrounded { get; protected set; } = true;
        public float LastGroundedTime { get; protected set; }
        public float Height => Controller.height;
        public float Radius => Controller.radius;
        public Vector3 Position => transform.position + Center;
        public Vector3 Center => Controller.center;
        public Vector3 StepPosition => Position - transform.up * (Height * 0.5f - Controller.stepOffset);
        public bool IsPointUnderStep(Vector3 point) => StepPosition.y > point.y;
        public float GroundAngle { get; protected set; }
        public Vector3 GroundNormal { get; protected set; }
        public Vector3 LocalSlopeDirection { get; protected set; }
        
        protected readonly float GroundOffset = 0.1f;
        
        private void Awake()
        {
            StateMachine = GetComponent<EntityStateMachine<T>>();
        }

        protected virtual void Update()
        {
            StateMachine.Tick();
            Move();
            UpdateIsGrounded();
        }

        protected void UpdateIsGrounded()
        {
            float distance = (Height * 0.5f) + GroundOffset;

            if (SphereCast(Vector3.down, distance, out var hit) && VerticalVelocity.y <= 0)
            {
                if (!IsGrounded)
                {
                    if (EvaluateLanding(hit))
                    {
                        EnterGround(hit);
                    }
                }
                else if (IsPointUnderStep(hit.point))
                {
                    UpdateGroundedData(hit);

                    if (Vector3.Angle(hit.normal, Vector3.up) >= Controller.slopeLimit)
                    {
                        Slide(hit);
                    }
                }
            }
            else
            {
                ExitGround();
            }
        }
        protected virtual void Slide(RaycastHit hit) { }
        protected void UpdateGroundedData(RaycastHit hit)
        {
            if (IsGrounded)
            {
                GroundNormal = hit.normal;
                GroundAngle = Vector3.Angle(Vector3.up, hit.normal);
                LocalSlopeDirection = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
            }
        }
        
        protected virtual void EnterGround(RaycastHit hit)
        {
            if (!IsGrounded)
            {
                IsGrounded = true;
            }
        }
        
        protected void ExitGround()
        {
            if (IsGrounded)
            {
                IsGrounded = false;
                transform.parent = null;
                LastGroundedTime = Time.time;
                VerticalVelocity = Vector3.Max(VerticalVelocity, Vector3.zero);
            }
        }
        
        protected virtual bool EvaluateLanding(RaycastHit hit)
        {
            return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < Controller.slopeLimit;
        }
        
        public bool SphereCast(Vector3 direction, float distance,
            out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            var castDistance = Mathf.Abs(distance - Radius);
            return Physics.SphereCast(Position, Radius, direction,
                out hit, castDistance, layer, queryTriggerInteraction);
        }
        
        private void Move()
        {
            Controller.Move(Velocity * Time.deltaTime);
        }

        public Vector3 Velocity { get; set; }
        
        public Vector3 LateralVelocity
        {
            get => new(Velocity.x, 0, Velocity.z);
            set => Velocity = new Vector3(value.x, Velocity.y, value.z);
        }
        public Vector3 VerticalVelocity
        {
            get { return new Vector3(0, Velocity.y, 0); }
            set { Velocity = new Vector3(Velocity.x, value.y, Velocity.z); }
        }
        
        public void Accelerate(Vector3 direction, float turningDrag, float acceleration, float maxSpeed)
        {
            if (direction.sqrMagnitude <= 0)
            {
                return;
            }

            float speed = Vector3.Dot(direction, LateralVelocity);
            Vector3 velocity = direction * speed;
            Vector3 turningVelocity = LateralVelocity - velocity;
            float turningDelta = turningDrag * Time.deltaTime;

            if (LateralVelocity.magnitude < maxSpeed || speed < 0)
            {
                speed += acceleration * Time.deltaTime;
                speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);
            }

            velocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            LateralVelocity = velocity + turningVelocity;
        }
        
        public virtual void Decelerate(float deceleration)
        {
            var delta = deceleration * Time.deltaTime;
            LateralVelocity = Vector3.MoveTowards(LateralVelocity, Vector3.zero, delta);
        }
        
        public virtual void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = rotation;
            }
        }
        
        public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
        {
            if (direction != Vector3.zero)
            {
                var rotation = transform.rotation;
                var rotationDelta = degreesPerSecond * Time.deltaTime;
                var target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
            }
        }
    }
}