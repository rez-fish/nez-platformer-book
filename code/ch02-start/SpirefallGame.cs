using Nez;

namespace Spirefall
{
    // Core is Nez's root class. It subclasses XNA's Game, so a Nez game
    // *is* an XNA game with Nez's Scene/Entity/Component machinery bolted on.
    public class SpirefallGame : Core
    {
        // Named arguments into Core's constructor configure the window.
        // Full signature (Nez net10-update @ 3f8cc40, Core.cs):
        //   Core(int width = 1280, int height = 720, bool isFullScreen = false,
        //        string windowTitle = "Nez", string contentDirectory = "Content",
        //        bool hardwareModeSwitch = true)
        public SpirefallGame() : base(width: 1280, height: 720, windowTitle: "Spirefall")
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Assigning Core.Scene for the first time wires it up immediately
            // and calls Scene.Begin(). A Scene with no entities just clears
            // the screen to Scene.ClearColor (CornflowerBlue by default).
            Scene = new Scene();
        }
    }
}
