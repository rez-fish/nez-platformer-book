using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Title screen. Menu visuals are Gum elements (global overlay, not scene
    /// children), so the scene creates them in Initialize and removes them
    /// when leaving — Gum does not know about Nez scenes.
    /// </summary>
    public class TitleScene : Scene
    {
        TextRuntime _title;
        TextRuntime _prompt;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);

            // [unverified] layout details (exact centering/anchor props):
            // element API confirmed (TextRuntime.Text, AddToRoot/RemoveFromRoot),
            // visual tuning left to Gum's docs + Exercise 2.
            _title = new TextRuntime { Text = "SPIREFALL" };
            _title.X = 160;
            _title.Y = 60;
            _title.AddToRoot();

            _prompt = new TextRuntime { Text = "press Z to start" };
            _prompt.X = 160;
            _prompt.Y = 110;
            _prompt.AddToRoot();

            CreateEntity("title-input").AddComponent(new TitleInput());
        }

        public void HideMenu()
        {
            _title?.RemoveFromRoot();
            _prompt?.RemoveFromRoot();
        }

        class TitleInput : Component, IUpdatable
        {
            void IUpdatable.Update()
            {
                if (Input.IsKeyPressed(Keys.Z))
                {
                    ((TitleScene)Entity.Scene).HideMenu();
                    Core.Scene = new ArenaScene();
                }
            }
        }
    }
}
