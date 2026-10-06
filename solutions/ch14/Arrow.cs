using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// A fired arrow. Moves in a straight line, dies on any collision or
    /// after its lifetime. Hitting a Target scores for the arena.
    /// </summary>
    public class Arrow : Component, IUpdatable
    {
        public const float Speed = 260f;
        const float Lifetime = 1.5f;

        public Vector2 Direction;
        readonly int _ownerIndex;
        float _life = Lifetime;
        Mover _mover;

        public Arrow(Vector2 direction, int ownerIndex)
        {
            Direction = direction;
            _ownerIndex = ownerIndex;
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(new BoxCollider(8, 2));
            _mover = Entity.AddComponent(new Mover());
        }

        void IUpdatable.Update()
        {
            var dt = Time.DeltaTime;
            _life -= dt;
            if (_life <= 0f)
            {
                Entity.Destroy();
                return;
            }

            if (_mover.Move(Direction * Speed * dt, out var hit))
            {
                // what did we hit? Targets score; everything else just stops us.
                var arena = (ArenaScene)Entity.Scene;

                var target = hit.Collider.Entity.GetComponent<Target>();
                if (target != null)
                    arena.OnTargetHit(target);

                var enemy = hit.Collider.Entity.GetComponent<Enemy>();
                if (enemy != null)
                    arena.OnEnemyKilled(enemy);

                // versus: arrows hit the OTHER player, never their owner
                var player = hit.Collider.Entity.GetComponent<PlayerController>();
                if (player != null && player.PlayerIndex != _ownerIndex && player.IsVulnerable)
                    arena.OnPlayerHit(_ownerIndex, player);

                Entity.Destroy();
            }
        }
    }
}
