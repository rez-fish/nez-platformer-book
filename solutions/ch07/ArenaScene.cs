using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace Spirefall
{
    /// <summary>
    /// The one scene of the game (for now). Level geometry comes from a Tiled
    /// tilemap; the archer collides with it via TiledMapMover.
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

            var archer = CreateEntity("archer", new Vector2(160, 100));
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture()));
            archer.AddComponent(new BoxCollider(16, 16));
            // TiledMapMover IS a Mover — PlayerController keeps working unchanged.
            archer.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("Ground")));
            archer.AddComponent(new PlayerController());
        }
    }
}
