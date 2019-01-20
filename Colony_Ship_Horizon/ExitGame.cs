using Microsoft.Xna.Framework.Input;
using System;

namespace Colony_Ship_Horizon
{
    /// <summary>
    /// ----------------https://msdn.microsoft.com/en-us/library/bb203874.aspx?f=255&MSPPError=-2147217396
    /// </summary>

    internal class ExitGame : Game1
    {
        public bool checkExitKey(KeyboardState keyboardState)
        {
            // Check to see whether ESC was pressed on the keyboard
            // or BACK was pressed on the controller.
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return true;
            }
            return false;
        }

        private void Game1_Exiting(object sender, EventArgs e)
        {
            // Add any code that must execute before the game ends.
        }
    }
}