using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    public class SpirefallGame : Core
    {
        // XNA has no "draw a rectangle" primitive. The standard trick is a
        // 1x1 white texture, stretched to whatever rect you want.
        Texture2D _pixel;

        Vector2 _squarePos = new Vector2(100, 100);
        const float SquareSize = 32f;
        const float MoveSpeed = 240f; // pixels per second

        public SpirefallGame() : base(width: 1280, height: 720, windowTitle: "Spirefall")
        {}

        protected override void Initialize()
        {
            base.Initialize();

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            Scene = new Scene();
        }

        protected override void Update(GameTime gameTime)
        {
            var move = Vector2.Zero;
            if (Input.IsKeyDown(Keys.Left)) move.X -= 1;
            if (Input.IsKeyDown(Keys.Right)) move.X += 1;
            if (Input.IsKeyDown(Keys.Up)) move.Y -= 1;
            if (Input.IsKeyDown(Keys.Down)) move.Y += 1;

            // frame-rate independent: scale by seconds elapsed since last frame
            _squarePos += move * MoveSpeed * Time.DeltaTime;

            _squarePos.X = MathHelper.Clamp(_squarePos.X, 0, 1280 - SquareSize);
            _squarePos.Y = MathHelper.Clamp(_squarePos.Y, 0, 720 - SquareSize);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime); // renders the Scene (still just the clear color)

            var batcher = Graphics.Instance.Batcher;
            batcher.Begin();
            batcher.Draw(_pixel,
                new Rectangle((int)_squarePos.X, (int)_squarePos.Y,
                              (int)SquareSize, (int)SquareSize),
                Color.OrangeRed);
            batcher.End();
        }
    }
}
