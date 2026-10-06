using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Pause overlay for the arena. Freezing is done with Time.TimeScale = 0:
    /// every movement in the game multiplies by Time.DeltaTime (= dt * scale),
    /// so zero scale stops the world while input polling keeps working.
    /// </summary>
    public class PauseController : Component, IUpdatable
    {
        bool _paused;
        TextRuntime _pauseText;
        bool _textShown;

        public override void OnAddedToEntity()
        {
            // [unverified] layout details — see TitleScene notes.
            _pauseText = new TextRuntime { Text = "PAUSED - esc: resume   T: title" };
            _pauseText.X = 160;
            _pauseText.Y = 80;
        }

        void IUpdatable.Update()
        {
            if (Input.IsKeyPressed(Keys.Escape))
            {
                _paused = !_paused;
                Time.TimeScale = _paused ? 0f : 1f;

                if (_paused && !_textShown)
                {
                    _pauseText.AddToRoot();
                    _textShown = true;
                }
                else if (!_paused && _textShown)
                {
                    _pauseText.RemoveFromRoot();
                    _textShown = false;
                }
            }

            if (_paused && Input.IsKeyPressed(Keys.T))
            {
                Time.TimeScale = 1f;
                if (_textShown)
                    _pauseText.RemoveFromRoot();
                ((ArenaScene)Entity.Scene).TeardownUi();
                Core.Scene = new TitleScene();
            }
        }
    }
}
