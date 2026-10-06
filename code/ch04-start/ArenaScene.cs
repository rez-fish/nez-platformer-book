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

            var archer = CreateEntity("archer", new Vector2(160, 90));
            archer.AddComponent(new SpriteRenderer(Archer.CreatePlaceholderTexture()));
        }
    }
}
