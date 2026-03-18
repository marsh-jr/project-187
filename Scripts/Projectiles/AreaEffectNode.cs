using Godot;
using System.Collections.Generic;

namespace Project187
{
    /// Spawned by AreaAttack. Stays at position, pulses damage to all enemies within radius.
    public partial class AreaEffectNode : Area2D
    {
        private AttackInstance _ownerAttack;
        private AttackRuntimeStats _stats;
        private float _timeRemaining;
        private float _pulseTimer;

        public void Initialize(AttackInstance ownerAttack, AttackRuntimeStats stats)
        {
            _ownerAttack   = ownerAttack;
            _stats         = stats;
            _timeRemaining = stats.Duration;
            _pulseTimer    = 0f;

            // Scale the collision shape to match the radius
            var shape = GetNode<CollisionShape2D>("CollisionShape2D");
            if (shape?.Shape is CircleShape2D circle)
                circle.Radius = stats.Radius;
        }

        public override void _Process(double delta)
        {
            _timeRemaining -= (float)delta;
            if (_timeRemaining <= 0f)
            {
                QueueFree();
                return;
            }

            _pulseTimer -= (float)delta;
            if (_pulseTimer <= 0f)
            {
                _pulseTimer = _stats.PulseInterval;
                Pulse();
            }
        }

        private void Pulse()
        {
            // Collect hits first to avoid mutating while iterating
            var hits = new List<Node>();
            foreach (var body in GetOverlappingBodies())
            {
                if (body.IsInGroup("Enemies"))
                    hits.Add(body);
            }

            foreach (var body in hits)
                _ownerAttack.RegisterHit(body, _stats.BaseDamage);
        }
    }
}
