using Repkarbinhake.Core.Data;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxClimber : MonoBehaviour
    {
        private const float GroundProbeShrink = 0.9f;
        private const float AirControl = 0.25f;
        private const float HalfFactor = 0.5f;
        private const float AirTimeGrowthWindow = 1f;
        private const float FallingSpeed = -1f;

        [SerializeField] private Rigidbody _body;
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private Transform _ropePoint;
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private PhysicsMaterial _gripMaterial;
        [SerializeField] private PhysicsMaterial _slideMaterial;
        [SerializeField, Min(0f)] private float _maxSpeed = 4.5f;
        [SerializeField, Min(0f)] private float _moveAcceleration = 30f;
        [SerializeField, Min(0f)] private float _jumpSpeed = 5.5f;
        [SerializeField, Min(0f)] private float _uprightTorque = 60f;
        [SerializeField, Min(0f)] private float _uprightDamping = 10f;
        [SerializeField, Min(0f)] private float _groundProbeDistance = 0.15f;

        [Header("PEAK-like air gravity")]
        [SerializeField, Min(0f)] private float _groundGravity = 9.81f;
        [SerializeField, Min(0f)] private float _airGravityStart = 4f;
        [SerializeField, Min(0f)] private float _airGravityMax = 20f;
        [SerializeField, Min(0.01f)] private float _airGravityRampSeconds = 1f;

        [Header("Stamina (PEAK: full bar = 1)")]
        [SerializeField, Min(0f)] private float _staminaRegenPerSecond = 0.25f;
        [SerializeField, Min(0f)] private float _staminaRegenDelay = 1f;

        private LayerMask _groundMask;
        private Vector3 _moveDirection;
        private Vector3 _spawnPosition;
        private float _fallenTimer;
        private float _unconsciousTimer;
        private float _sinceUsedStamina;

        public int Actor { get; private set; }
        public bool IsGrounded { get; private set; }
        public float SinceGrounded { get; private set; }
        public float CurrentGravity { get; private set; }
        public float Stamina { get; private set; } = 1f;
        public Footing Footing => IsDead || IsUnconscious ? Footing.Unconscious : IsGrounded ? Footing.Grounded : Footing.Airborne;
        public bool IsDead { get; private set; }
        public bool IsUnconscious => _unconsciousTimer > 0f;
        public bool IsLimp => IsFallen || IsDead || IsUnconscious;
        public bool IsPresent => gameObject.activeSelf;
        public bool IsFallen => _fallenTimer > 0f;
        public Vector3 RopePoint => _ropePoint.position;
        public Vector3 Center => _body.worldCenterOfMass;
        public Vector3 Velocity => _body.linearVelocity;
        public Renderer BodyRenderer => _bodyRenderer;

        public void Initialize(int actor, LayerMask groundMask)
        {
            Actor = actor;
            _groundMask = groundMask;
            _spawnPosition = _body.position;
            _body.useGravity = false;
        }

        public void Tick(float deltaTime)
        {
            IsGrounded = ProbeGround();
            ApplySurface();
            TrackAirTime(deltaTime);
            ApplyGravity();
            RegenerateStamina(deltaTime);

            if (IsLimp)
            {
                _fallenTimer -= deltaTime;
                _unconsciousTimer -= deltaTime;
                return;
            }

            KeepUpright();
            ApplyMovement();
        }

        public void Move(Vector3 worldDirection)
        {
            _moveDirection = Vector3.ClampMagnitude(new Vector3(worldDirection.x, 0f, worldDirection.z), 1f);
        }

        public void Jump()
        {
            if (!IsGrounded || IsLimp)
                return;

            Vector3 velocity = _body.linearVelocity;
            velocity.y = _jumpSpeed;
            _body.linearVelocity = velocity;
        }

        public void Fall(float seconds)
        {
            _fallenTimer = seconds;
        }

        public void KnockOut(float seconds)
        {
            _unconsciousTimer = seconds;
        }

        public void Kill()
        {
            IsDead = true;
        }

        public void Leave()
        {
            gameObject.SetActive(false);
        }

        public void Rejoin()
        {
            gameObject.SetActive(true);
        }

        public void Push(Vector3 velocityChange)
        {
            _body.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        public void UseStamina(float amount)
        {
            if (amount <= 0f)
                return;

            Stamina = Mathf.Max(Stamina - amount, 0f);
            _sinceUsedStamina = 0f;
        }

        public void AddAcceleration(Vector3 acceleration)
        {
            _body.AddForce(acceleration, ForceMode.Acceleration);
        }

        public void ClampAirTime(float maxSeconds)
        {
            SinceGrounded = Mathf.Min(SinceGrounded, maxSeconds);
        }

        public void Teleport(Vector3 position)
        {
            _body.position = position;
            _body.rotation = Quaternion.identity;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _fallenTimer = 0f;
            _unconsciousTimer = 0f;
            IsDead = false;
            SinceGrounded = 0f;
            Stamina = 1f;
        }

        public void Respawn()
        {
            Teleport(_spawnPosition);
        }

        private bool ProbeGround()
        {
            float radius = _capsule.radius * GroundProbeShrink;
            float castDistance = _capsule.height * HalfFactor - radius + _groundProbeDistance;
            return Physics.SphereCast(_body.worldCenterOfMass, radius, Vector3.down, out _, castDistance, _groundMask, QueryTriggerInteraction.Ignore);
        }

        private void ApplySurface()
        {
            PhysicsMaterial surface = IsGrounded && !IsLimp ? _gripMaterial : _slideMaterial;

            if (_capsule.sharedMaterial != surface)
                _capsule.sharedMaterial = surface;
        }

        private void TrackAirTime(float deltaTime)
        {
            if (IsGrounded)
            {
                SinceGrounded = 0f;
                return;
            }

            if (SinceGrounded < AirTimeGrowthWindow || _body.linearVelocity.y < FallingSpeed)
                SinceGrounded += deltaTime;
        }

        private void RegenerateStamina(float deltaTime)
        {
            _sinceUsedStamina += deltaTime;

            if (IsGrounded && _sinceUsedStamina >= _staminaRegenDelay)
                Stamina = Mathf.Min(Stamina + _staminaRegenPerSecond * deltaTime, 1f);
        }

        private void ApplyGravity()
        {
            CurrentGravity = IsGrounded ? _groundGravity : Mathf.Lerp(_airGravityStart, _airGravityMax, SinceGrounded / _airGravityRampSeconds);
            _body.AddForce(Vector3.down * CurrentGravity, ForceMode.Acceleration);
        }

        private void KeepUpright()
        {
            Vector3 tiltAxis = Vector3.Cross(transform.up, Vector3.up);
            _body.AddTorque(tiltAxis * _uprightTorque - _body.angularVelocity * _uprightDamping, ForceMode.Acceleration);
        }

        private void ApplyMovement()
        {
            Vector3 velocity = _body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 change = _moveDirection * _maxSpeed - horizontal;
            float control = IsGrounded ? 1f : AirControl;

            if (_moveDirection.sqrMagnitude < Mathf.Epsilon && !IsGrounded)
                return;

            _body.AddForce(Vector3.ClampMagnitude(change * _moveAcceleration, _moveAcceleration) * control, ForceMode.Acceleration);
        }
    }
}
