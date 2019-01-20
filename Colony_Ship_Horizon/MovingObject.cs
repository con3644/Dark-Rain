using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Colony_Ship_Horizon
{
    class MovingObject
    {
        Texture2D _texture;
        Vector2 location;
        public Rectangle rectangle;
        public bool isActive = true;
        public bool _willImpact;
        public bool isImpact = false;
        RandomNumberGen randomNumber = new RandomNumberGen(); // sync-locked random number generator defined within project
        float timeSinceLastFrame = 0;
        float millisecondsPerUpdate;

        /// <summary>
        /// Creates a single instance of a texture which moves across the screen until it exits the viewport
        /// </summary>
        /// <param name="texture">The texture to be drawn</param>
        /// <param name="size">The size of the texture</param>
        /// <param name="viewPortBounds">The bounds of the viewport</param>
        /// <param name="willImpact">Will this object impact the rectangles given in the update method?</param>
        /// <param name="starFromTopOfViewPort">Will this object originate from the top of the view port or the right side?</param>
        public MovingObject(Texture2D texture, int size, Rectangle viewPortBounds, bool willImpact, bool starFromTopOfViewport = true)
        {
            int originY, originX;
            if (starFromTopOfViewport) // start the object from a random point on the top of the screen
            {
                originX = randomNumber.NextNumber(0, viewPortBounds.Width + viewPortBounds.X);
                location = new Vector2(originX, viewPortBounds.Y);
            }
            else // start the object from a random point on the right side of the screen
            {
                originY = randomNumber.NextNumber(0, viewPortBounds.Height + viewPortBounds.Y);
                location = new Vector2(viewPortBounds.Width + viewPortBounds.X - 100, originY);
            }

            rectangle = new Rectangle((int)location.X, (int)location.Y, size, size);
            _willImpact = willImpact;

            millisecondsPerUpdate = 1; // how often the object is update

            _texture = texture;
        }

        public void Update(GameTime gameTime, Vector2 direction, Rectangle viewPortBounds, List<Rectangle> platforms)
        {
            // adjust for delta time
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // only update the object after enough time has passed
            timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastFrame > millisecondsPerUpdate)
            {
                // reset counter
                timeSinceLastFrame = 0;

                // if the object is meant to impact platforms, then check if it intersects any
                if (_willImpact)
                {
                    for (int i = 0; i < platforms.Count; i++)
                    {
                        // if object intersects platform, deactivate it  and flag an impact at this location
                        if (platforms[i].Intersects(rectangle))
                        {
                            isImpact = true;
                            isActive = false;
                            location.Y += 8; // offset for whitespace on textures
                            rectangle.Location = location.ToPoint();
                        }
                    }
                }

                // check if the object is still active
                if (isActive)
                {
                    //check if it is still within the viewport bounds. if it is not, then deactivate the object
                    if (!viewPortBounds.Contains(rectangle))
                    {
                        isActive = false;
                    }
                    // move the object in the specified direction and update the rectangle
                    else
                    {
                        location += direction * elapsed;
                        rectangle.Location = location.ToPoint();
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the MovingObject to the screen
            spriteBatch.Draw(_texture, rectangle, Color.White);
        }
    }
}
