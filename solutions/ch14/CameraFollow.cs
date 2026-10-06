using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Versus camera: frames the MIDPOINT of the two archers, with lookahead
    /// from their average velocity. Clamped to the level like before.
    /// </summary>
    public class CameraFollow : Component, IUpdatable
    {
        const float FollowSharpness = 8f;
        const float LookaheadScale = 0.25f;
        const float MaxShakeOffset = 6f;
        const float TraumaDecay = 1.6f;

        const float LevelWidth = 640f;
        const float HalfViewWidth = 160f;

        public float Trauma;

        Entity _p0;
        Entity _p1;
        PlayerController _c0;
        PlayerController _c1;
        readonly Random _random = new Random();

        public override void OnAddedToEntity()
        {
            _p0 = Entity.Scene.FindEntity("archer-0");
            _p1 = Entity.Scene.FindEntity("archer-1");
            _c0 = _p0?.GetComponent<PlayerController>();
            _c1 = _p1?.GetComponent<PlayerController>();
        }

        public void AddTrauma(float amount)
        {
            Trauma = Math.Min(Trauma + amount, 1f);
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;
            var camera = Entity.Scene.Camera;

            if (_p0 != null && _p1 != null)
            {
                var midpoint = (_p0.Position + _p1.Position) / 2f;
                var avgVel = (_c0?.Velocity ?? Vector2.Zero)
                           + (_c1?.Velocity ?? Vector2.Zero);
                var desired = midpoint + avgVel / 2f * LookaheadScale;
                desired.X = MathHelper.Clamp(desired.X, HalfViewWidth,
                                             LevelWidth - HalfViewWidth);
                desired.Y = 88f;

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
