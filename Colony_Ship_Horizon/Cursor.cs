using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Colony_Ship_Horizon
{
    internal class Cursor
    {
        public Vector2 worldPosition { get; set; }
        public Vector2 CursorPos { get; set; }
        private Texture2D CursorTexture;
        private Texture2D CursorTexture2;
        private MouseState mouseState;
        public Rectangle cursorRect;
        private Camera2d Camera;
        private int cursorSize;
        public bool WithinViewportBoundsX { get; set; }
        public bool WithinViewportBoundsY { get; set; }
        public bool DrawCursor { get; set; }

        public Cursor(Texture2D cursorTexture, GraphicsDevice graphics, Texture2D cursorTexture2)
        {
            Camera = new Camera2d(graphics.Viewport);
            CursorTexture = cursorTexture;
            CursorTexture2 = cursorTexture2;
            cursorRect.Height = CursorTexture.Height;
            cursorRect.Width = CursorTexture.Width;
            WithinViewportBoundsX = true;
            WithinViewportBoundsY = true;
            DrawCursor = true;
        }

        public void Update(Vector2 cameraPos, bool inBuildMode, bool inAgricultureMode, bool inMainMenu)
        {
            // get mouse coords to draw game's cursor
            mouseState = Mouse.GetState();

            // don't adjust world position if in a menu
            if (inBuildMode || inAgricultureMode)
            {
                cursorSize = 64;
            }
            // adjust for world position for normal gameplay
            else if (inMainMenu)
            {
                cursorSize = 32;
            }
            else
            {
                cursorSize = 32;
            }

            //if (!WithinViewportBoundsX && !WithinViewportBoundsY)
            //{
            //    CursorPos = new Vector2(CursorPos.X + cameraPos.X, CursorPos.Y + cameraPos.Y);
            //}
            //else if (WithinViewportBoundsY && WithinViewportBoundsX)
            //{
            //    CursorPos = new Vector2(mouseState.X + cameraPos.X, mouseState.Y + cameraPos.Y);
            //}
            //else
            //{
            //    if (!WithinViewportBoundsX)
            //        CursorPos = new Vector2(CursorPos.X + cameraPos.X, mouseState.Y + cameraPos.Y);
            //    if (!WithinViewportBoundsY)
            //        CursorPos = new Vector2(mouseState.X + cameraPos.X, CursorPos.Y + cameraPos.Y);
            //}

            CursorPos = new Vector2(mouseState.X + cameraPos.X, mouseState.Y + cameraPos.Y);

            if (WithinViewportBoundsX)
                cursorRect.X = mouseState.X;
            if (WithinViewportBoundsY)
                cursorRect.Y = mouseState.Y;

            // get world position via transforming mouse position by camera view matrix
            worldPosition = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(Camera.GetViewMatrix(new Vector2(1f))));
        }

        public void Draw(SpriteBatch spriteBatch, Color cursorColor, bool changeCursor = false)
        {
            if (DrawCursor)
            {
                if (!changeCursor)
                    spriteBatch.Draw(CursorTexture,
                        new Rectangle((int)CursorPos.X, (int)CursorPos.Y, cursorSize, cursorSize), cursorColor);
                else
                    spriteBatch.Draw(CursorTexture2,
                       new Rectangle((int)CursorPos.X, (int)CursorPos.Y, cursorSize, cursorSize), cursorColor);
            }
        }
    }
}