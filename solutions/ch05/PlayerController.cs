using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Ground movement with game feel, resolved against real colliders via
    /// Nez's Mover. Moves one axis at a time so corners slide cleanly.
    /// </summary>
    public class PlayerController : Component, IUpdatable
    {
        public Vector2 Velocity;

        const float MoveSpeed = 90f;
        const float Acceleration = 900f;
        const float Friction = 1100f;
        const float JumpSpeed = 250f;
        const float Gravity = 800f;
        const float MaxFallSpeed = 300f;
        const float CoyoteTime = 0.10f;
        const float JumpBuffer = 0.12f;
        const float JumpCutMultiplier = 0.45f;

        float _coyoteTimer;
        float _bufferTimer;
        bool _jumpHeld;
        bool _grounded;

        Mover _mover;

        public override void OnAddedToEntity()
        {
            // Entity is assigned by now (it isn't in the constructor).
            // Fail fast here if the scene forgot the Mover.
            _mover = Entity.GetComponent<Mover>();
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;

            var inputX = 0f;
            if (Input.IsKeyDown(Keys.Left)) inputX -= 1f;
            if (Input.IsKeyDown(Keys.Right)) inputX += 1f;

            var targetVx = inputX * MoveSpeed;
            var rate = inputX != 0f ? Acceleration : Friction;
            Velocity.X = Approach(Velocity.X, targetVx, rate * dt);

            if (Input.IsKeyPressed(Keys.Z))
                _bufferTimer = JumpBuffer;
            else
                _bufferTimer -= dt;

            var jumpHeldNow = Input.IsKeyDown(Keys.Z);
            if (_jumpHeld && !jumpHeldNow && Velocity.Y < 0f)
                Velocity.Y *= JumpCutMultiplier;
            _jumpHeld = jumpHeldNow;

            if (_bufferTimer > 0f && _coyoteTimer > 0f)
            {
                Velocity.Y = -JumpSpeed;
                _bufferTimer = 0f;
                _coyoteTimer = 0f;
            }

            Velocity.Y = Math.Min(Velocity.Y + Gravity * dt, MaxFallSpeed);

            // axis-separated movement: X first, then Y. Move() returns true
            // when something was hit; the out CollisionResult is discarded
            // here (see Chapter 6 for using its Normal).
            if (_mover.Move(new Vector2(Velocity.X * dt, 0f), out _))
                Velocity.X = 0f;

            var yDelta = Velocity.Y * dt;
            var hitY = _mover.Move(new Vector2(0f, yDelta), out _);
            _grounded = hitY && yDelta > 0f;
            if (hitY)
                Velocity.Y = 0f; // landed, or bonked our head — either way, stop

            if (_grounded)
                _coyoteTimer = CoyoteTime;
            else
                _coyoteTimer -= dt;
        }

        static float Approach(float value, float target, float maxDelta)
        {
            if (value < target)
                return Math.Min(value + maxDelta, target);
            return Math.Max(value - maxDelta, target);
        }
    }
}
