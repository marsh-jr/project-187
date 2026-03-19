using Godot;

namespace Project187
{
    /// Fired by a ProjectileAttack. Moves in a straight line, reports hits to its owner AttackInstance.
    public partial class ProjectileNode : Area2D
    {
        public const float DefaultMaxLifetime = 5f;

        private AttackInstance _ownerAttack;
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private int   _piercesLeft;
        private bool  _isHoming;
        private int   _bouncesRemaining;
        private Node  _homingTarget;

        private float _maxLifetime;
        private float _lifetime;

        public void Initialize(
            AttackInstance ownerAttack,
            Vector2        direction,
            float          speed,
            float          damage,
            int            pierceCount,
            bool           isHoming,
            int            bounces,
            float          maxLifetime = DefaultMaxLifetime)
        {
            _ownerAttack      = ownerAttack;
            _direction        = direction.Normalized();
            _speed            = speed;
            _damage           = damage;
            _piercesLeft      = pierceCount;
            _isHoming         = isHoming;
            _bouncesRemaining = bounces;
            _maxLifetime      = maxLifetime > 0f ? maxLifetime : DefaultMaxLifetime;
        }

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
        }

        public override void _PhysicsProcess(double delta)
        {
            _lifetime += (float)delta;
            if (_lifetime >= _maxLifetime)
            {
                QueueFree();
                return;
            }

            if (_isHoming && _homingTarget == null)
                _homingTarget = FindNearestEnemy();

            if (_isHoming && _homingTarget != null && IsInstanceValid(_homingTarget))
            {
                var targetPos = (_homingTarget as Node2D)?.GlobalPosition ?? GlobalPosition;
                _direction = (targetPos - GlobalPosition).Normalized();
            }

            GlobalPosition += _direction * _speed * (float)delta;
        }

        private void OnBodyEntered(Node body)
        {
            if (!body.IsInGroup("Enemies")) return;

            var result = _ownerAttack.RegisterHit(body, _damage);

            if (result.ShouldRicochet && _bouncesRemaining > 0)
            {
                _bouncesRemaining--;
                var nearest = FindNearestEnemy();
                if (nearest is Node2D n2d)
                    _direction = (n2d.GlobalPosition - GlobalPosition).Normalized();
                else
                    _direction = -_direction;
                return;
            }

            if (result.ShouldSplit)
                SpawnSplitProjectiles();

            _piercesLeft--;
            if (_piercesLeft < 0)
                QueueFree();
        }

        private void SpawnSplitProjectiles()
        {
            for (int i = -1; i <= 1; i += 2)
            {
                var proj = Duplicate() as ProjectileNode;
                if (proj == null) continue;
                proj._direction       = _direction.Rotated(Mathf.DegToRad(30f * i));
                proj._ownerAttack     = _ownerAttack;
                proj._speed           = _speed;
                proj._damage          = _damage * 0.6f;
                proj._piercesLeft     = 0;
                proj._isHoming        = false;
                proj._bouncesRemaining = 0;
                proj.GlobalPosition   = GlobalPosition;
                GetParent().AddChild(proj);
            }
        }

        private Node FindNearestEnemy()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Node nearest = null;
            float minDist = float.MaxValue;
            foreach (Node e in enemies)
            {
                if (e is Node2D e2d)
                {
                    float d = GlobalPosition.DistanceSquaredTo(e2d.GlobalPosition);
                    if (d < minDist) { minDist = d; nearest = e; }
                }
            }
            return nearest;
        }
    }
}
