using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colony_Ship_Horizon
{
    public class Camera2d
    {
        public Camera2d(Viewport viewport)
        {
            _viewport = viewport;
            Origin = new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);
            Zoom = 2.15f;
            ViewportWidth = viewport.Width;
            ViewportHeight = viewport.Height;
        }

        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }
        public Viewport _viewport { get; set; }

        public Rectangle screenBounds { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }

        public Matrix GetViewMatrix(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public void LookAt(Vector2 position)
        {
            Position = position - new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);

            float inverseZoom = 1 / Zoom;
            screenBounds = new Rectangle((int)(Position.X + 600 -(Zoom*10)), (int)(Position.Y), (int)(_viewport.Width * inverseZoom), (int)(_viewport.Height * inverseZoom* 2));
        }


        public Rectangle ViewportWorldBoundry()
        {
            Vector2 viewPortCorner = ScreenToWorld(new Vector2(0, 0));
            Vector2 viewPortBottomCorner =
               ScreenToWorld(new Vector2(ViewportWidth, ViewportHeight));

            return new Rectangle((int)viewPortCorner.X,
               (int)viewPortCorner.Y,
               (int)(viewPortBottomCorner.X - viewPortCorner.X),
               (int)(viewPortBottomCorner.Y - viewPortCorner.Y));
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition,
                Matrix.Invert(GetViewMatrix(new Vector2(1f))));
        }


        /// <summary>
        /// Determines whether the target is in view given the specified position.
        /// This can be used to increase performance by not drawing objects
        /// directly in the viewport
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>
        ///     <c>true</c> if [is in view] [the specified position]; otherwise, <c>false</c>.
        /// </returns>
        /// 
        //public bool IsInView(Vector2 position)
        //{
        //    Rectangle texture = new Rectangle(0, 0, 32, 32);
        //    // If the object is not within the horizontal bounds of the screen

        //    if ((position.X + texture.Width) < (Position.X - Origin.X) || (position.X) > (Position.X + Origin.X))
        //        return false;

        //    // If the object is not within the vertical bounds of the screen
        //    if ((position.Y + texture.Height) < (Position.Y - Origin.Y) || (position.Y) > (Position.Y + Origin.Y))
        //        return false;

        //    // In View
        //    return true;
        //}
    }
}