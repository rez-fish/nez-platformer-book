using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// A simple AI: patrols between two x bounds, chases the archer when it
    /// has line of sight within aggro range. Same enum-state-machine shape as
    /// the player's dash logic, driven by sensing instead of input.
    /// </summary>
    public class Enemy : Component, IUpdatable
    {
        enum AiState { Patrol, Chase }

        const float PatrolSpeed = 35f;
        const float ChaseSpeed = 70f;
        const float AggroRange = 110f;
        const float Gravity = 800f;
        const float MaxFallSpeed = 300f;
        const float HopSpeed = 220f;
        const float TouchDistance = 14f;

        readonly float _minX;
        readonly float _maxX;

        AiState _state;
        float _dir = 1f;
        Vector2 _velocity;
        Mover _mover;
        Entity _player;

        public Enemy(float minX, float maxX)
        {
            _minX = minX;
            _maxX = maxX;
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(new BoxCollider(16, 12));
            _mover = Entity.AddComponent(new Mover());
            _player = Entity.Scene.FindEntity("archer");
        }

        public void Die()
        {
            Entity.Destroy();
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;

            // --- sense ---
            var toPlayer = _player.Position - Entity.Position;
            var dist = toPlayer.Length();
            var seen = false;
            if (dist < AggroRange)
            {
                // line of sight: clear, or the first thing hit IS the player
                var hit = Physics.Linecast(Entity.Position, _player.Position);
                seen = hit.Collider == null || hit.Collider.Entity == _player;
            }
            _state = seen ? AiState.Chase : AiState.Patrol;

            // --- think ---
            var speed = _state == AiState.Chase ? ChaseSpeed : PatrolSpeed;
            if (_state == AiState.Chase && toPlayer.X != 0f)
                _dir = Math.Sign(toPlayer.X);

            // --- act ---
            _velocity.Y = Math.Min(_velocity.Y + Gravity * dt, MaxFallSpeed);
            _velocity.X = _dir * speed;

            var xDelta = _velocity.X * dt;
            if (_mover.Move(new Vector2(xDelta, 0f), out _))
            {
                if (_state == AiState.Patrol)
                    _dir *= -1f;          // bounce off walls while patrolling
                else
                    _velocity.Y = -HopSpeed; // hop over obstacles while chasing
            }

            if (Entity.Position.X < _minX)
            {
                Entity.Position = new Vector2(_minX, Entity.Position.Y);
                _dir = 1f;
            }
            else if (Entity.Position.X > _maxX)
            {
                Entity.Position = new Vector2(_maxX, Entity.Position.Y);
                _dir = -1f;
            }

            _mover.Move(new Vector2(0f, _velocity.Y * dt), out _);

            // --- touch kills (if the player isn't spawn-protected) ---
            var controller = _player.GetComponent<PlayerController>();
            if (dist < TouchDistance && controller.IsVulnerable)
                ((ArenaScene)Entity.Scene).OnPlayerDeath();
        }
    }
}
