using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Colony_Ship_Horizon
{
    internal class Projectile
    {
        public Texture2D Texture;
        Texture2D MuzzleFlash;
        public Vector2 Position;
        public bool Active;
        public int Damage;
        public int Force;
        public Vector2 Direction;
        public int Width
        {
            get { return Texture.Width; }
        }
        public int Height
        {
            get { return Texture.Height; }
        }
        private float projectileMoveSpeed;
        private int count = 0;

        float RotationAngle = 0f;
        float gunRotationAngle = 0f;
        Vector2 _gunArmOffset;
        float _scale;
        public int rectangleSize;
        public bool aimingLeft = false;
        public bool aimingUp = false;
        public bool aimingDown = false;
        SpriteEffects effect;
        bool DrawMuzzleFlash;
        Vector2 _gunPos;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="muzzleFlash"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="damage"></param>
        /// <param name="force"></param>
        /// <param name="speed"></param>
        /// <param name="gunPos">Where the projectile will start</param>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        /// <param name="drawMuzzleFlash"></param>
        public void Initialize(Texture2D texture, Texture2D muzzleFlash, Vector2 position, Vector2 direction, 
            int damage, int force, float speed, Vector2 gunPos, int size, float scale, bool drawMuzzleFlash = true)
        {
            Texture = texture;
            MuzzleFlash = muzzleFlash;
            Position = position;
            Direction = direction;
            Active = true;
            Damage = damage;
            Force = force;
            projectileMoveSpeed = speed;
            rectangleSize = size;
            _scale = scale;
            DrawMuzzleFlash = drawMuzzleFlash;
            if (Texture.Name.Contains("lazer") || Texture.Name.Contains("gun"))
                Position = Direction; // point the lazer in the direction of the cursor
            else
                Position = gunPos; // start projectile at tip of the gun (lazer's Position)
            _gunPos = gunPos; // for the muzzle flash
        }

        public void UpdateWeapon(Texture2D texture, int damage, float speed)
        {
            Texture = texture;
            Damage = damage;
            projectileMoveSpeed = speed;
        }

            public void Update(GameTime gameTime, Vector2 direction, Vector2 playerPos, Vector2 gunPos, 
                Vector2 gunArmOffset, string currentFrame)
        {
            _gunPos = gunPos; // for the muzzle flash
            _gunArmOffset = gunArmOffset;
            //  projectiles fire in the direction of the cursor
            if (!Texture.Name.Contains("lazer") && !Texture.Name.Contains("gun"))
                Position += Direction * projectileMoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //  multiply by delta seconds to keep a consistent speed on all computers.
            else if (Texture.Name.Contains("lazer") || Texture.Name.Contains("gun"))
            {
                Direction = direction; // don't change the direction of projectiles mid-flight, just the gun
                if (currentFrame == "AimingRight" || currentFrame == "AimingBackLeft")
                    Position = playerPos + new Vector2(46, 37); // gun does not have a velocity, so much follow the player
                else if (currentFrame == "AimingLeft" || currentFrame == "AimingBackRight")
                    Position = playerPos + new Vector2(50, 37);
                else if (currentFrame == "StandingAimingRight")
                    Position = playerPos + new Vector2(45, 37); // gun does not have a velocity, so much follow the player
                else if (currentFrame == "StandingAimingLeft")
                    Position = playerPos + new Vector2(52, 37);
            }
            // ---determine rotation angle for sprite----
            //to find the direction vector:
            //V.x = cos(A)
            //V.y = sin(A)  ---- Atan2 gives a result in the range -pi to +pi
            // to get the number in the range 0 to 2pi, just add 2pi if the result is less than 0
            //to find the radians for a flipped texture, negate direction to change the rotation:
            RotationAngle = (float)Math.Atan2(Direction.Y, Direction.X);
            // for regular textures, do the following:
            gunRotationAngle = (float)Math.Atan2(Direction.Y, Direction.X);

            float pi = (float)Math.PI;
            if (gunRotationAngle < 0)
                gunRotationAngle += 2 * pi; // get negative values in range 0 to 2pi
            // if user is aiming past the left side of the character, flip the texture and set flag
            if (gunRotationAngle > pi / 2 && gunRotationAngle < (3 * pi) / 2)
            {
                effect = SpriteEffects.FlipVertically;
                aimingLeft = true;
            }
            else
            {
                effect = SpriteEffects.None;
                aimingLeft = false;
            }
            // if use is aiming up or down, set flag
            if (gunRotationAngle < (5 * pi) / 6 && gunRotationAngle > pi / 6) // slightly wider than down area
            {
                aimingDown = true;
            }
            else if (gunRotationAngle > (5 * pi) / 4 && gunRotationAngle < (7 * pi) / 4) // slightly thinnger than up area
            {
                aimingUp = true;
            }
            else
            {
                aimingUp = false;
                aimingDown = false;
            }
            //  deactivate bullet if too many on screen
            if (count > 350 && (!Texture.Name.Contains("lazer") && !Texture.Name.Contains("gun")))
            {
                Active = false;
            }

            // increment count of proj instance movement
            count++;
            // remove muzzle flash after so long
            if (count > 1)
                DrawMuzzleFlash = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // determine where to move arm/gun/lazer when aiming up/down/left/right
            if ((aimingDown && !aimingLeft) || (aimingUp && aimingLeft))
            {
                _gunArmOffset = new Vector2(_gunArmOffset.Y, _gunArmOffset.X);
            }
            else if ((aimingUp && !aimingLeft) || (aimingDown && aimingLeft))
            {
                _gunArmOffset = new Vector2(_gunArmOffset.Y * -1, _gunArmOffset.X);
            }

            // vector of (16,32) is used as the origin to manipulate textures of lazer and gun to draw and rotate them properly 
            if (Texture.Name.Contains("lazer"))
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, 500, 4), // width and height of the draw lazer
                    new Rectangle(0, 0, Width, Height), Color.White, RotationAngle,
                    new Vector2(0, 28), SpriteEffects.None, 0f);
            else if (Texture.Name.Contains("gun"))
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, 64, 58), // width and height of the draw gun
                    new Rectangle(0, 0, Width, Height), Color.White, gunRotationAngle,
                    new Vector2(0, 32) + _gunArmOffset, effect, 0f);
            else
            {
                Color muzzleFlashColor = Color.White;
                Color projectileColor = Color.White;
                if (Texture.Name == "bullet2")
                {
                    muzzleFlashColor = new Color(15, 215, 255);
                    projectileColor = new Color(255, 255, 255);
                }
                
                _scale = .75f; // overwriting the scale here, adjustments for the offset are below

                if (aimingLeft)
                {
                    // adjust the projectile and muzzle flash
                    Vector2 projOriginOffset = new Vector2(-80 * _scale, 13); // slightly higher y coord when aiming left
                    Vector2 muzzleOriginOffset = new Vector2(-80 * _scale, 13);
                    int projSize = (int)(32 * _scale);
                    int muzzleFlashSize = (int)(32 * _scale);

                    // draw the projectile
                    spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, projSize, projSize), // width and height of the draw gun
                            new Rectangle(0, 0, Width, Height), projectileColor, gunRotationAngle,
                            projOriginOffset + _gunArmOffset, SpriteEffects.None, 0f);

                    // draw a muzzle flash when the projectile is first being fired, and rotate it with the gun and lazer
                    if (DrawMuzzleFlash)
                        spriteBatch.Draw(MuzzleFlash, new Rectangle((int)_gunPos.X, (int)_gunPos.Y, muzzleFlashSize, muzzleFlashSize), // width and height of the draw gun
                            new Rectangle(0, 0, Width, Height), muzzleFlashColor, gunRotationAngle,
                            muzzleOriginOffset + _gunArmOffset, SpriteEffects.None, 0f);
                }
                else if (!aimingLeft)
                {
                    // adjust the projectile and muzzle flash
                    Vector2 projOriginOffset = new Vector2(-80 * _scale, 18); // slightly lower y coor when aiming right
                    Vector2 muzzleOriginOffset = new Vector2(-80 * _scale, 18);
                    int projSize = (int)(32 * _scale);
                    int muzzleFlashSize = (int)(32 * _scale);
                   
                    // draw the projectile
                    spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, projSize, projSize), // width and height of the draw gun
                            new Rectangle(0, 0, Width, Height), projectileColor, gunRotationAngle,
                            projOriginOffset + _gunArmOffset, SpriteEffects.None, 0f);

                    //draw a muzzle flash when the projectile is first being fired, and rotate it with the gun and lazer
                    if (DrawMuzzleFlash)
                        spriteBatch.Draw(MuzzleFlash, new Rectangle((int)_gunPos.X, (int)_gunPos.Y, muzzleFlashSize, muzzleFlashSize), // width and height of the draw gun
                            new Rectangle(0, 0, Width, Height), muzzleFlashColor, gunRotationAngle,
                            muzzleOriginOffset + _gunArmOffset, SpriteEffects.None, 0f);
                }
            }
        }
    }
}