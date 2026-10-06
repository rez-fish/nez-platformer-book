using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Ground movement with game feel: acceleration, friction, variable jump
    /// height, coyote time, and jump buffering. No collisions yet — the ground
    /// is a flat line at GroundLineY. Chapter 5 replaces it with real colliders.
    /// </summary>
    public class PlayerController : Component, IUpdatable
    {
        public Vector2 Velocity;

        // the entire "feel" of the game lives in these numbers
        const float MoveSpeed = 90f;      // px/s
        const float Acceleration = 900f;  // px/s^2
        const float Friction = 1100f;     // px/s^2
        const float JumpSpeed = 250f;     // px/s, upward
        const float Gravity = 800f;       // px/s^2
        const float MaxFallSpeed = 300f;  // px/s, terminal velocity
        const float CoyoteTime = 0.10f;   // s of grace after leaving ground
        const float JumpBuffer = 0.12f;   // s jump presses are remembered
        const float JumpCutMultiplier = 0.45f; // early release cuts rise

        // the entity origin is the sprite center; feet sit 8px below it
        const float FeetOffset = 8f;
        const float GroundLineY = 164f;

        float _coyoteTimer;
        float _bufferTimer;
        bool _jumpHeld;

        // explicit interface implementation: Update is only visible as
        // IUpdatable, keeping the component's public surface clean.
        // (Same pattern as the Nez sample's Caveman component.)
        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;

            var inputX = 0f;
            if (Input.IsKeyDown(Keys.Left)) inputX -= 1f;
            if (Input.IsKeyDown(Keys.Right)) inputX += 1f;

            var targetVx = inputX * MoveSpeed;
            var rate = inputX != 0f ? Acceleration : Friction;
            Velocity.X = Approach(Velocity.X, targetVx, rate * dt);

            var grounded = Entity.Position.Y + FeetOffset >= GroundLineY;
            if (grounded)
            {
                Entity.Position = new Vector2(Entity.Position.X, GroundLineY - FeetOffset);
                if (Velocity.Y > 0f) Velocity.Y = 0f;
                _coyoteTimer = CoyoteTime;
            }
            else
            {
                _coyoteTimer -= dt;
            }

            if (Input.IsKeyPressed(Keys.Z))
                _bufferTimer = JumpBuffer;
            else
                _bufferTimer -= dt;

            // variable jump height: releasing Z early cuts the upward velocity
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
            Entity.Position += Velocity * dt;
        }

        static float Approach(float value, float target, float maxDelta)
        {
            if (value < target)
                return Math.Min(value + maxDelta, target);
            return Math.Max(value - maxDelta, target);
        }
    }
}
