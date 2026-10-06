using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Follows the archer with velocity lookahead, clamped to the level, plus
    /// trauma-based screenshake. (Nez's Camera.Position is the world point at
    /// the center of the screen — verified in Camera's view-matrix source.)
    /// </summary>
    public class CameraFollow : Component, IUpdatable
    {
        const float FollowSharpness = 8f;    // higher = snappier
        const float LookaheadScale = 0.25f;  // seconds of velocity to look ahead
        const float MaxShakeOffset = 6f;     // px of shake at full trauma
        const float TraumaDecay = 1.6f;      // trauma lost per second

        const float LevelWidth = 640f;
        const float HalfViewWidth = 160f;

        public float Trauma;

        Entity _target;
        PlayerController _player;
        readonly Random _random = new Random();

        public override void OnAddedToEntity()
        {
            _target = Entity.Scene.FindEntity("archer");
            _player = _target?.GetComponent<PlayerController>();
        }

        public void AddTrauma(float amount)
        {
            Trauma = Math.Min(Trauma + amount, 1f);
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;
            var camera = Entity.Scene.Camera;

            if (_target != null)
            {
                var lookahead = _player != null
                    ? _player.Velocity * LookaheadScale
                    : Vector2.Zero;

                var desired = _target.Position + lookahead;
                desired.X = MathHelper.Clamp(desired.X, HalfViewWidth,
                                             LevelWidth - HalfViewWidth);
                desired.Y = 88f; // level is ~view height; vertical stays fixed

                // framerate-independent smoothing
                var t = 1f - (float)Math.Exp(-FollowSharpness * dt);
                camera.Position = Vector2.Lerp(camera.Position, desired, t);
            }

            Trauma = Math.Max(Trauma - TraumaDecay * dt, 0f);
            if (Trauma > 0f)
            {
                var mag = Trauma * Trauma * MaxShakeOffset;
                camera.Position += new Vector2(
                    (float)(_random.NextDouble() * 2 - 1) * mag,
                    (float)(_random.NextDouble() * 2 - 1) * mag);
            }
        }
    }
}
