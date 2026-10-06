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

        /// <param name="variant">0 = orange hood, 1 = teal hood.</param>
        public static Texture2D CreatePlaceholderTexture(int variant = 0)
        {
            var tex = new Texture2D(Core.GraphicsDevice, SpriteSize, SpriteSize);
            var pixels = new Color[SpriteSize * SpriteSize];

            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(52, 52, 64); // dark body

            // a lighter "hood" band across the top rows (per-player color)
            var hood = variant == 0 ? new Color(139, 90, 43) : new Color(45, 130, 140);
            for (var x = 3; x < 13; x++)
            for (var y = 2; y < 6; y++)
                pixels[y * SpriteSize + x] = hood;

            tex.SetData(pixels);
            return tex;
        }
    }

        /// <summary>8x2 arrow shaft placeholder.</summary>
        public static Texture2D CreateArrowTexture()
        {
            var tex = new Texture2D(Core.GraphicsDevice, 8, 2);
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(230, 200, 120);
            // darker tip
            pixels[6] = pixels[7] = pixels[14] = pixels[15] = new Color(180, 60, 50);
            tex.SetData(pixels);
            return tex;
        }

        /// <summary>12x12 target dummy placeholder.</summary>
        public static Texture2D CreateTargetTexture()
        {
            const int size = 12;
            var tex = new Texture2D(Core.GraphicsDevice, size, size);
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = x - size / 2;
                var dy = y - size / 2;
                var r = dx * dx + dy * dy;
                // concentric rings: red / white / red
                pixels[y * size + x] = r < 4 ? new Color(200, 50, 50)
                    : r < 12 ? new Color(230, 230, 230)
                    : r < 22 ? new Color(200, 50, 50)
                    : new Color(0, 0, 0, 0);
            }
            tex.SetData(pixels);
            return tex;
        }
    }

        /// <summary>16x12 enemy blob placeholder.</summary>
        public static Texture2D CreateEnemyTexture()
        {
            const int w = 16, h = 12;
            var tex = new Texture2D(Core.GraphicsDevice, w, h);
            var pixels = new Color[w * h];
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var dx = x - w / 2;
                var dy = y - h / 2;
                var edge = dx * dx / 49f + dy * dy / 25f; // ellipse
                pixels[y * w + x] = edge > 1f ? new Color(0, 0, 0, 0)
                    : y < 4 ? new Color(150, 80, 180)   // purple top
                    : new Color(110, 55, 140);
            }
            // angry eyes
            pixels[4 * w + 5] = pixels[4 * w + 10] = Color.White;
            pixels[5 * w + 5] = pixels[5 * w + 10] = new Color(200, 30, 30);
            tex.SetData(pixels);
            return tex;
        }
    }
}