using Microsoft.Xna.Framework.Input;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Per-player input. Player 0 is keyboard, player 1 is gamepad 0.
    /// Virtual inputs register with Nez's Input system and update themselves
    /// every frame — no polling code needed beyond reading them.
    /// </summary>
    public class PlayerInput
    {
        public readonly VirtualIntegerAxis MoveX = new VirtualIntegerAxis();
        public readonly VirtualIntegerAxis MoveY = new VirtualIntegerAxis();
        public readonly VirtualButton Jump = new VirtualButton();
        public readonly VirtualButton Dash = new VirtualButton();
        public readonly VirtualButton Shoot = new VirtualButton();

        public PlayerInput(int playerIndex)
        {
            if (playerIndex == 0)
            {
                MoveX.AddKeyboardKeys(OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
                MoveY.AddKeyboardKeys(OverlapBehavior.TakeNewer, Keys.Up, Keys.Down);
                Jump.AddKeyboardKey(Keys.Z);
                Dash.AddKeyboardKey(Keys.X);
                Shoot.AddKeyboardKey(Keys.C);
            }
            else
            {
                const int pad = 0;
                MoveX.AddGamePadLeftStickX(pad);
                MoveX.AddGamePadDPadLeftRight(pad);
                MoveY.AddGamePadLeftStickY(pad);
                MoveY.AddGamePadDPadUpDown(pad);
                Jump.AddGamePadButton(pad, Buttons.A);
                Dash.AddGamePadButton(pad, Buttons.X);
                Shoot.AddGamePadButton(pad, Buttons.B);
            }
        }
    }
}
