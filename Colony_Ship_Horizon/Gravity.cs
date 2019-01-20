using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colony_Ship_Horizon
{
    class Gravity
    {
        List<Rectangle> _mapPlatforms;
        // new entry in list created for every new item that gravity is applied to
        List<Vector2> velocityList = new List<Vector2>();
        List<Vector2> accelerationList = new List<Vector2>();
        List<Vector2> directionList = new List<Vector2>(); // where left = (-1, 0) & right = (0, 1)
        List<bool> hasFallen = new List<bool>();
        List<Rectangle> rectanglesFalling = new List<Rectangle>();
        RandomNumberGen randomNumber = new RandomNumberGen();
        public int totalInstancesOfGravity = 0;

        /// <summary>
        /// Creates an object of gravity. One gravity object stores the gravity information for only one type.
        /// </summary>
        /// <param name="mapPlatforms"></param>
        public Gravity(List<Rectangle> mapPlatforms)
        {
            _mapPlatforms = mapPlatforms;
        }

        /// <summary>
        /// Creates a new instance of gravity to be applied to the given rectangle
        /// </summary>
        /// <param name="rectangleToAdd"></param>
        public void NewGravityInstance(Rectangle rectangleToAdd)
        {
            rectanglesFalling.Add(rectangleToAdd);
            totalInstancesOfGravity++;
            hasFallen.Add(true);
            velocityList.Add(new Vector2(randomNumber.NextNumber(-10, 10) / 10, 750));
            // Y acceleration remains constant while X acceleration is randomly generated and divided to 
            // create smaller, erratic horizontal movements
            float xAcceleration = randomNumber.NextNumber(-10, 10);
            accelerationList.Add(new Vector2(xAcceleration / 200, -75000));
        }

        /// <summary>
        /// Removes the matching rectangle from the gravity instances
        /// </summary>
        /// <param name="rectangleToRemove"></param>
        public void RemoveGravityInstance(Rectangle rectangleToRemove)
        {
            // determine the index to be removed
            int indexToRemove = rectanglesFalling.FindIndex(x => x == rectangleToRemove);

            // remove all current instances
            if (indexToRemove >= 0)
            {
                rectanglesFalling.Remove(rectangleToRemove);
                accelerationList.RemoveAt(indexToRemove);
                hasFallen.RemoveAt(indexToRemove);
                velocityList.RemoveAt(indexToRemove);
                totalInstancesOfGravity--;
            }
        }

        /// <summary>
        /// Applies gravity to the given index
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="location"></param>
        /// <param name="spriteSize"></param>
        /// <param name="gravityInstanceIndex"></param>
        /// <returns></returns>
        public Vector2 ApplyGravity(GameTime gameTime, Vector2 location, Point spriteSize, int gravityInstanceIndex)
        {
            
            // keep object movement smooth, in case of framerate drop
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // object is falling
            if (!IsOnGround(elapsed, location, spriteSize) && !hasFallen[gravityInstanceIndex])
            {
                velocityList[gravityInstanceIndex] = new Vector2(0, 0);
                hasFallen[gravityInstanceIndex] = true;
            }
            // implement gravity
            else if (hasFallen[gravityInstanceIndex])
            {
                // stop gravity when object reaches floor
                if (IsOnGround(elapsed, location, spriteSize))
                {
                    // reset flag
                    hasFallen[gravityInstanceIndex] = false;
                }
                else
                {
                    // update object location by velocityList
                    location -= velocityList[gravityInstanceIndex] * elapsed;
                    velocityList[gravityInstanceIndex] += accelerationList[gravityInstanceIndex] * elapsed;
                }
            }
            // update rectangle in list
            rectanglesFalling[gravityInstanceIndex] = new Rectangle((int)location.X, (int)location.Y, spriteSize.X, spriteSize.Y);
            
            return location;
        }

        public bool IsOnGround(float time, Vector2 location, Point spriteSize)
        {
            // determine where the object will be if move is allowed
            Rectangle newRect = new Rectangle((int)location.X, (int)location.Y, spriteSize.X / 2, spriteSize.Y / 2);
            bool onGround = false;
            // check is object will be standing on top of any platforms
            foreach (var platform in _mapPlatforms)
            {
                if (newRect.Bottom > platform.Top &&
                    newRect.Bottom < platform.Bottom &&// object is above platform
                    (int)(newRect.X) > (int)(platform.Left) &&
                    (int)(newRect.X + spriteSize.X) < (int)(platform.Right)) // object is inside range of platform
                {
                    location.Y = platform.Top; // place object on top of platform
                    onGround = true;
                    break;
                }
                else
                    onGround = false;
            }
            return onGround;
        }


    }
}
