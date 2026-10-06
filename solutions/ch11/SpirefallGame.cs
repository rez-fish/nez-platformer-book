using Gum;
using Microsoft.Xna.Framework;
using Nez;

namespace Spirefall
{
    public class SpirefallGame : Core
    {
        public SpirefallGame() : base(width: 1280, height: 720, windowTitle: "Spirefall")
        {}

        protected override void Initialize()
        {
            base.Initialize();

            // Gum overlay UI. Verified against Gum source @ dbb35814:
            // GumService lives in namespace Gum; Initialize takes the Game.
            // (Gum.FNA 2026.10.1.1 on NuGet; add it as a PackageReference.)
            GumService.Default.Initialize(this, Gum.Forms.DefaultVisualsVersion.Newest);

            Scene = new TitleScene();
        }

        protected override void Update(GameTime gameTime)
        {
            GumService.Default.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GumService.Default.Draw(); // after the scene: UI draws on top
        }
    }
}
