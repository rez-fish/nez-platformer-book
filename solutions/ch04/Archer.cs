using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// The player character. For now just a factory for its placeholder sprite;
    /// behavior arrives in Chapter 4. Swap CreatePlaceholderTexture for your own
    /// 16x16 art whenever you're ready — the call site doesn't change.
    /// </summary>
    public static class Archer
    {
        public const int SpriteSize = 16;

        public static Texture2D CreatePlaceholderTexture()
        {
            var tex = new Texture2D(Core.GraphicsDevice, SpriteSize, SpriteSize);
            var pixels = new Color[SpriteSize * SpriteSize];

            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(52, 52, 64); // dark body

            // a lighter "hood" band across the top rows
            for (var x = 3; x < 13; x++)
            for (var y = 2; y < 6; y++)
                pixels[y * SpriteSize + x] = new Color(139, 90, 43);

            tex.SetData(pixels);
            return tex;
        }
    }
}
