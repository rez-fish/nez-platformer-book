using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum.GueDeriving;
using Nez;
using Nez.Tiled;

namespace Spirefall
{
    /// <summary>
    /// The arena: tilemap level, archer, target dummies, score HUD.
    /// Owns its Gum elements and tears them down when leaving.
    /// </summary>
    public class ArenaScene : Scene
    {
        public int Score;

        TextRuntime _scoreText;
        readonly Random _random = new Random();

        static readonly Vector2[] TargetSpots =
        {
            new Vector2(60, 60),
            new Vector2(200, 40),
            new Vector2(320, 90),
            new Vector2(480, 50),
            new Vector2(580, 100),
        };

        public override void Initialize()
        {
            base.Initialize();

            // 320x180, integer-scaled to the window: crisp pixels at any size.
            SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);

            Sfx.Load();

            var map = Content.LoadTiledMap("Content/Arena/arena.tmx");

            var tilemap = CreateEntity("tilemap");
            tilemap.AddComponent(new TiledMapRenderer(map, "Ground"));

            CreateEntity("camera-rig", new Vector2(160, 88))
                .AddComponent(new CameraFollow());

            var dust = CreateEntity("dust", Vector2.Zero);
            dust.AddComponent(new ParticleEmitter(CreateDustConfig(), playOnAwake: false));

            CreateEntity("pause").AddComponent(new PauseController());

            var archer = CreateEntity("archer", new Vector2(80, 100));
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture()));
            archer.AddComponent(new BoxCollider(16, 16));
            archer.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("Ground")));
            archer.AddComponent(new PlayerController());

            SpawnTarget(TargetSpots[0]);

            SpawnEnemy(280, 240, 340);
            SpawnEnemy(520, 470, 570);

            // [unverified] layout details — see Chapter 9 notes.
            _scoreText = new TextRuntime { Text = "SCORE 0" };
            _scoreText.X = 16;
            _scoreText.Y = 16;
            _scoreText.AddToRoot();
        }

        public void TeardownUi()
        {
            _scoreText?.RemoveFromRoot();
        }

        public void OnEnemyKilled(Enemy enemy)
        {
            Score += 2;
            _scoreText.Text = $"SCORE {Score}";
            Sfx.PlayHit();
            FindEntity("camera-rig")?.GetComponent<CameraFollow>()?.AddTrauma(0.35f);
            enemy.Die();
        }

        public void OnPlayerDeath()
        {
            var archer = FindEntity("archer");
            var controller = archer.GetComponent<PlayerController>();
            archer.Position = new Vector2(80, 100);
            controller.Velocity = Vector2.Zero;
            controller.GrantInvulnerability(1.5f);
            Sfx.PlayDeath();
            FindEntity("camera-rig")?.GetComponent<CameraFollow>()?.AddTrauma(0.5f);
        }

        public void SpawnArrow(Vector2 position, Vector2 direction)
        {
            var arrow = CreateEntity("arrow", position);
            arrow.AddComponent(new SpriteRenderer(Archer.CreateArrowTexture()));
            arrow.AddComponent(new Arrow(direction));
        }

        public void OnTargetHit(Target target)
        {
            Score++;
            _scoreText.Text = $"SCORE {Score}";
            Sfx.PlayHit();

            var rig = FindEntity("camera-rig")?.GetComponent<CameraFollow>();
            rig?.AddTrauma(0.3f);

            // respawn the dummy at a different spot
            target.Entity.Destroy();
            Vector2 spot;
            do
            {
                spot = TargetSpots[_random.Next(TargetSpots.Length)];
            } while ((spot - target.Entity.Position).LengthSquared() < 1f);
            SpawnTarget(spot);
        }

        void SpawnEnemy(float x, float minX, float maxX)
        {
            var enemy = CreateEntity("enemy", new Vector2(x, 100));
            enemy.AddComponent(new SpriteRenderer(Archer.CreateEnemyTexture()));
            enemy.AddComponent(new Enemy(minX, maxX));
        }

        void SpawnTarget(Vector2 position)
        {
            var target = CreateEntity("target", position);
            target.AddComponent(new SpriteRenderer(Archer.CreateTargetTexture()));
            target.AddComponent(new BoxCollider(12, 12));
            target.AddComponent(new Target());
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
