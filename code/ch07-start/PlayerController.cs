using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    enum MoveState { Normal, Dashing }

    /// <summary>
    /// Full Celeste-style moveset: run, variable jump, coyote time, buffering,
    /// wall slide, wall jump, and an 8-way dash on a tiny state machine.
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

        const float WallSlideSpeed = 45f;
        const float WallJumpX = 130f;
        const float WallJumpY = 235f;
        const float DashSpeed = 230f;
        const float DashTime = 0.15f;
        const float DashCooldown = 0.5f;

        MoveState _state;
        float _stateTimer;
        float _dashCooldown;
        float _coyoteTimer;
        float _bufferTimer;
        bool _jumpHeld;
        bool _grounded;
        int _facing = 1;  // 1 = right, -1 = left
        int _wallDir;     // 0 = no wall, else direction of the wall

        Mover _mover;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;
            _dashCooldown -= dt;

            // Dashing is its own branch: fixed velocity, no gravity, no input.
            // Early return keeps the two states from interfering.
            if (_state == MoveState.Dashing)
            {
                UpdateDash(dt);
                return;
            }

            var inputX = 0f;
            if (Input.IsKeyDown(Keys.Left)) inputX -= 1f;
            if (Input.IsKeyDown(Keys.Right)) inputX += 1f;
            var inputY = 0f;
            if (Input.IsKeyDown(Keys.Up)) inputY -= 1f;
            if (Input.IsKeyDown(Keys.Down)) inputY += 1f;
            if (inputX != 0f) _facing = (int)inputX;

            if (Input.IsKeyPressed(Keys.X) && _dashCooldown <= 0f)
            {
                var dir = new Vector2(inputX, inputY);
                if (dir == Vector2.Zero)
                    dir = new Vector2(_facing, 0f);
                else
                    dir = Vector2.Normalize(dir);

                Velocity = dir * DashSpeed;
                _state = MoveState.Dashing;
                _stateTimer = DashTime;
                MoveWithCollisions(dt);
                return;
            }

            var targetVx = inputX * MoveSpeed;
            var rate = inputX != 0f ? Acceleration : Friction;
            Velocity.X = Approach(Velocity.X, targetVx, rate * dt);

            var xDelta = Velocity.X * dt;
            var hitWall = xDelta != 0f && _mover.Move(new Vector2(xDelta, 0f), out _);
            if (hitWall) Velocity.X = 0f;
            _wallDir = hitWall && !_grounded ? Math.Sign(xDelta) : 0;

            // wall slide: airborne, pressed into the wall, moving down
            var wallSliding = _wallDir != 0 && inputX == _wallDir && Velocity.Y > 0f;

            if (Input.IsKeyPressed(Keys.Z))
            {
                if (wallSliding)
                {
                    Velocity = new Vector2(-_wallDir * WallJumpX, -WallJumpY);
                    _facing = -_wallDir;
                    _wallDir = 0;
                }
                else
                {
                    _bufferTimer = JumpBuffer;
                }
            }
            else
            {
                _bufferTimer -= dt;
            }

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
            if (wallSliding)
                Velocity.Y = Math.Min(Velocity.Y, WallSlideSpeed);

            var yDelta = Velocity.Y * dt;
            var hitY = _mover.Move(new Vector2(0f, yDelta), out _);
            _grounded = hitY && yDelta > 0f;
            if (hitY)
                Velocity.Y = 0f;

            if (_grounded)
            {
                _coyoteTimer = CoyoteTime;
                _dashCooldown = 0f; // landing refreshes the dash, Celeste-style
            }
            else
            {
                _coyoteTimer -= dt;
            }
        }

        void UpdateDash(float dt)
        {
            _stateTimer -= dt;
            MoveWithCollisions(dt);
            if (_stateTimer <= 0f)
            {
                _state = MoveState.Normal;
                _dashCooldown = DashCooldown;
                Velocity *= 0.35f; // keep some momentum, not all
            }
        }

        void MoveWithCollisions(float dt)
        {
            if (_mover.Move(new Vector2(Velocity.X * dt, 0f), out _))
                Velocity.X = 0f;
            if (_mover.Move(new Vector2(0f, Velocity.Y * dt), out _))
                Velocity.Y = 0f;
            _grounded = false;
        }

        static float Approach(float value, float target, float maxDelta)
        {
            if (value < target)
                return Math.Min(value + maxDelta, target);
            return Math.Max(value - maxDelta, target);
        }
    }
}
