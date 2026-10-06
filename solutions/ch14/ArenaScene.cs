using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using Nez;
using Nez.Tiled;

namespace Spirefall
{
    /// <summary>
    /// Versus arena: two archers (keyboard vs gamepad), first to 5 hits wins.
    /// Enemies and targets are gone — the duel is the game now. (Single-player
    /// still exists in the ch12 solution if you want both: see exercises.)
    /// </summary>
    public class ArenaScene : Scene
    {
        public const int WinningScore = 5;

        public readonly int[] Scores = new int[2];
        public bool HasWinner { get; private set; }

        static readonly Vector2[] Spawns =
        {
            new Vector2(80, 100),
            new Vector2(560, 100),
        };

        TextRuntime _scoreText;
        TextRuntime _winnerText;

        public override void Initialize()
        {
            base.Initialize();

            // 320x180, integer-scaled to the window: crisp pixels at any size.
            SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);

            Sfx.Load();

            var map = Content.LoadTiledMap("Content/Arena/arena.tmx");

            CreateEntity("tilemap")
                .AddComponent(new TiledMapRenderer(map, "Ground"));

            CreateEntity("camera-rig", new Vector2(320, 88))
                .AddComponent(new CameraFollow());

            var dust = CreateEntity("dust", Vector2.Zero);
            dust.AddComponent(new ParticleEmitter(CreateDustConfig(), playOnAwake: false));

            CreateEntity("pause").AddComponent(new PauseController());
            CreateEntity("win-input").AddComponent(new WinInput());

            var collisionLayer = map.GetLayer<TmxLayer>("Ground");
            for (var i = 0; i < 2; i++)
                SpawnPlayer(i, collisionLayer);

            // [unverified] layout details — see Chapter 9 notes.
            _scoreText = new TextRuntime { Text = ScoreLine() };
            _scoreText.X = 16;
            _scoreText.Y = 16;
            _scoreText.AddToRoot();
        }

        public void TeardownUi()
        {
            _scoreText?.RemoveFromRoot();
            _winnerText?.RemoveFromRoot();
        }

        void SpawnPlayer(int index, TmxLayer collisionLayer)
        {
            // $"archer-{index}": string interpolation with a hole for the index.
            var archer = CreateEntity($"archer-{index}", Spawns[index]);
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture(index)));
            archer.AddComponent(new BoxCollider(16, 16));
            archer.AddComponent(new TiledMapMover(collisionLayer));
            var controller = archer.AddComponent(new PlayerController(new PlayerInput(index)));
            controller.PlayerIndex = index;
        }

        public void SpawnArrow(Vector2 position, Vector2 direction, int ownerIndex)
        {
            var arrow = CreateEntity("arrow", position);
            arrow.AddComponent(new SpriteRenderer(Archer.CreateArrowTexture()));
            arrow.AddComponent(new Arrow(direction, ownerIndex));
        }

        public void OnEnemyKilled(Enemy enemy)
        {
            // no enemies in versus — kept so shared code (Arrow) compiles.
            enemy.Die();
        }

        public void OnPlayerHit(int shooterIndex, PlayerController victim)
        {
            if (HasWinner)
                return;

            Scores[shooterIndex]++;
            _scoreText.Text = ScoreLine();
            Sfx.PlayHit();
            FindEntity("camera-rig")?.GetComponent<CameraFollow>()?.AddTrauma(0.4f);

            if (Scores[shooterIndex] >= WinningScore)
            {
                HasWinner = true;
                // [unverified] layout details — see Chapter 9 notes.
                _winnerText = new TextRuntime
                {
                    Text = $"PLAYER {shooterIndex + 1} WINS - press Z"
                };
                _winnerText.X = 160;
                _winnerText.Y = 80;
                _winnerText.AddToRoot();
                Time.TimeScale = 0f; // freeze the duel; input still polls
                return;
            }

            victim.Entity.Position = Spawns[victim.PlayerIndex];
            victim.Velocity = Vector2.Zero;
            victim.GrantInvulnerability(1.5f);
            Sfx.PlayDeath();
        }

        // kept for shared code (Enemy); unused in versus.
        public void OnPlayerDeath()
        {
        }

        public void OnTargetHit(Target target)
        {
        }

        string ScoreLine() => $"P1 {Scores[0]} : {Scores[1]} P2";

        void SpawnTarget(Vector2 position)
        {
        }

        class WinInput : Component, IUpdatable
        {
            void IUpdatable.Update()
            {
                var arena = (ArenaScene)Entity.Scene;
                if (arena.HasWinner && Input.IsKeyPressed(Keys.Z))
                {
                    Time.TimeScale = 1f;
                    arena.TeardownUi();
                    Core.Scene = new TitleScene();
                }
            }
        }

        // [unverified] field semantics — see Chapter 8.
        static ParticleEmitterConfig CreateDustConfig()
        {
            var dot = new Texture2D(Core.GraphicsDevice, 4, 4);
            var white = new Color[16];
            for (var i = 0; i < white.Length; i++)
                white[i] = Color.White;
            dot.SetData(white);

            return new ParticleEmitterConfig
            {
                Sprite = new Sprite(dot),
                EmitterType = ParticleEmitterType.Gravity,
                BlendFuncSource = Blend.SourceAlpha,
                BlendFuncDestination = Blend.InverseSourceAlpha,
                MaxParticles = 32,
                EmissionRate = 160f,
                ParticleLifespan = 0.45f,
                ParticleLifespanVariance = 0.15f,
                Speed = 45f,
                SpeedVariance = 25f,
                Angle = 90f,
                AngleVariance = 55f,
                Gravity = new Vector2(0f, 260f),
                StartColor = new Color(210, 210, 220, 200),
                FinishColor = new Color(210, 210, 220, 0),
                StartParticleSize = 3f,
                FinishParticleSize = 0.5f,
                Duration = 0.12f,
            };
        }
    }
}
