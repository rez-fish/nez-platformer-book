using Microsoft.Xna.Framework;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// The one scene of the game (for now). Owns the arena and everything in it.
    /// </summary>
    public class ArenaScene : Scene
    {
        public override void Initialize()
        {
            base.Initialize();

            // 320x180, integer-scaled to the window: crisp pixels at any size.
            SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);

            var archer = CreateEntity("archer", new Vector2(160, 100));
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture()));
            archer.AddComponent(new BoxCollider(16, 16)); // centered on the entity
            archer.AddComponent(new Mover());
            archer.AddComponent(new PlayerController());

            // platforms: entities whose only job is holding a BoxCollider.
            // Position is the top-left; the collider offset (0,0) matches.
            CreatePlatform(0, 164, 320, 16);    // ground
            CreatePlatform(60, 120, 64, 8);     // floating platforms
            CreatePlatform(196, 96, 64, 8);
            CreatePlatform(128, 140, 40, 8);

            // a local function: declared inside a method, invisible outside it.
            // Perfect for small scene-assembly helpers.
            void CreatePlatform(float x, float y, float w, float h)
            {
                var platform = CreateEntity("platform", new Vector2(x, y));
                platform.AddComponent(new BoxCollider(0, 0, w, h));
            }
        }
    }
}
