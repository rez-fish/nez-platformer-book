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
            Scene = new ArenaScene();
        }
    }
}
