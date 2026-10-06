using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tiled;

namespace Spirefall
{
    /// <summary>
    /// Wider arena (40x11 tiles), a camera rig that follows the archer, and a
    /// reusable dust emitter for landing/dash puffs.
    /// </summary>
    public class ArenaScene : Scene
    {
        public override void Initialize()
        {
            base.Initialize();

            // 320x180, integer-scaled to the window: crisp pixels at any size.
            SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);

            var map = Content.LoadTiledMap("Content/Arena/arena.tmx");

            var tilemap = CreateEntity("tilemap");
            tilemap.AddComponent(new TiledMapRenderer(map, "Ground"));

            // camera rig and dust exist before the archer: its controller
            // looks them up in OnAddedToEntity.
            var rig = CreateEntity("camera-rig", new Vector2(160, 88));
            rig.AddComponent(new CameraFollow());

            var dust = CreateEntity("dust", Vector2.Zero);
            dust.AddComponent(new ParticleEmitter(CreateDustConfig(), playOnAwake: false));

            var archer = CreateEntity("archer", new Vector2(80, 100));
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture()));
            archer.AddComponent(new BoxCollider(16, 16));
            archer.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("Ground")));
            archer.AddComponent(new PlayerController());
        }

        // [unverified] field semantics (angle units, emission-rate units) are
        // from source inspection, not a runtime check — tune by eye.
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
