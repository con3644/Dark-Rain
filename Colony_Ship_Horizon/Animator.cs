using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Colony_Ship_Horizon
{
    public class Animator
    {
        private List<AnimatorInstance> instancePool = new List<AnimatorInstance>();
        private SpriteMap _spriteMap;
        private Texture2D _texture;
        private bool _isRepeating;
        private int _animSpeed;
        private bool _isPermanent;
        public bool _isTimed;
        public int totalFrames;
        public string type;
        public List<Rectangle> mechanicalAnimators = new List<Rectangle>();
        public List<Rectangle> clickedRectangles = new List<Rectangle>();
        public List<Rectangle> matureAnimClickedRectangles = new List<Rectangle>();
        public Vector2 worldPos = new Vector2(0, 0);
        int prevWave = 0;

        /// <summary>
        /// Create an instance of the animator class
        /// </summary>
        /// <param name="spriteMap"></param>
        /// <param name="texture"></param>
        /// <param name="animSpeed">Speed of animation in miliseconds</param>
        /// <param name="isRepeating">Will the animation restart at its first frame after reaching the last?</param>
        /// <param name="isPermanent">Will the animation remain in the game forever?</param>
        public Animator(SpriteMap spriteMap, Texture2D texture, int animSpeed, bool isRepeating, bool isPermanent = false)
        {
            _spriteMap = spriteMap;
            _texture = texture;
            _animSpeed = animSpeed;
            _isRepeating = isRepeating;
            _isPermanent = isPermanent;
            totalFrames = spriteMap.TextureList.Count;
            type = texture.Name;
        }

        /// <summary>
        ///  Add an instance of animation to the instance pool
        /// </summary>
        /// <param name="location"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <param name="animOrientation"></param>
        /// <param name="isTimed">Is the animation timed or is it synced to the current wave</param>
        public void AddAnimation(Vector2 location, Color color, float scale = 1, string animOrientation = "Right", 
            bool isTimed = true)
        {
            _isTimed = isTimed;
            instancePool.Add(new AnimatorInstance(_spriteMap, _texture, _isRepeating, _isPermanent,_animSpeed, location, scale, animOrientation, isTimed, color));
        }

        /// <summary>
        /// Update the instances, removing any that are inactive
        /// </summary>
        /// <param name="gametime"></param>
        /// <param name="newLocation"></param>
        /// <param name="scale"></param>
        /// <param name="currentWave"></param>
        public void Update(GameTime gametime, Vector2? newLocation = null, float scale = 1f, int currentWave = 0)
        {
            for (int i = 0; i < instancePool.Count; i++)
            {
                bool isInactive = false;
                if (!_isTimed) // animation is synced to the current wave number
                {
                    // after each wave, increment the wave syncing animations by a frame
                    if (instancePool[i].prevWaveNumber != currentWave)
                    {
                        // only progress animator instances that have been clicked on
                        if (clickedRectangles.Contains(instancePool[i].backgroundRect) || instancePool[i].currentFrameForWaveSync == 0)
                        {
                            instancePool[i].wavesSinceClickedOn = 0; // reset
                            instancePool[i].prevWaveNumber = currentWave;
                            if (instancePool[i].currentFrameForWaveSync < totalFrames)
                                instancePool[i].currentFrameForWaveSync++;
                        }
                        // determine if the animator is intersecting a mechanical animator
                        // if instance was not clicked or interacting with a mechanical animator, 
                        // then increment its counter
                        else
                        {
                            bool flag = false;
                            for (int j = 0; j < mechanicalAnimators.Count; j++)
                            {
                                if (instancePool[i].backgroundRect.Intersects(mechanicalAnimators[j]))
                                {
                                    instancePool[i].wavesSinceClickedOn = 0; // reset
                                    if (instancePool[i].currentFrameForWaveSync < totalFrames)
                                        instancePool[i].currentFrameForWaveSync++;
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                instancePool[i].wavesSinceClickedOn++;
                            }
                            instancePool[i].prevWaveNumber = currentWave;
                        }

                        // remove if instance has not been clicked on in 2 turns or is a mature instance
                        if (instancePool[i].wavesSinceClickedOn > 2 || instancePool[i].wavesSinceMature > 1)
                        {
                            isInactive = true;
                            instancePool.RemoveAt(i);
                        }
                        // number of waves since plant reached maturity (its final animation frame)
                        else if (instancePool[i].currentFrameForWaveSync == totalFrames)
                        {
                            // a mature animator was clicked on, add rectangle to dictionary to be used for spawning objects
                            matureAnimClickedRectangles.Add(instancePool[i].backgroundRect);
                            instancePool[i].wavesSinceMature++;
                        }
                    }
                }
                // update all other instances
                if (!isInactive)
                {
                    instancePool[i]._scale = scale;
                    if (instancePool[i].isActive)
                        instancePool[i].UpdateAnimator(gametime);
                    else if (!instancePool[i].isActive)
                        instancePool.RemoveAt(i);
                    if (newLocation != null)
                    {
                        instancePool[i]._location = (Vector2)newLocation;
                    }
                }
            }

            // when finished with the click event rectangles, clear the list
            if (prevWave != currentWave)
                clickedRectangles.Clear();
            prevWave = currentWave;
        }

        /// <summary>
        /// Gets all rectangles from current animator instances, to be checked for click events
        /// </summary>
        /// <param name="shouldModifyRectangle"></param>
        /// <returns></returns>
        public List<Rectangle> GetAnimRectangle(bool shouldModifyRectangle = false)
        {
            List<Rectangle> animRects = new List<Rectangle>();
            for (int i = 0; i < instancePool.Count; i++)
            {
                Rectangle rect = instancePool[i].backgroundRect;
                // prevent empty rectangles from being added
                if (!instancePool[i].backgroundRect.IsEmpty)
                {
                    if (shouldModifyRectangle)
                        animRects.Add(new Rectangle(rect.X+64, rect.Y, 32, rect.Height));
                    else if (!shouldModifyRectangle)
                        animRects.Add(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
                }
            }
            return animRects;
        }

        // draw each instance in the pool
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (AnimatorInstance instance in instancePool)
            {
                instance.DrawInstance(spriteBatch);
            }
        }
    }

    /// <summary>
    /// An instance of the animator class, stored in the instance pool to be removed and destroyed when animation is
    /// no longer needed.
    /// </summary>
    internal class AnimatorInstance
    {
        public bool isActive = true;
        private bool _isRepeating;
        private bool _isPermanent;
        private int currentFrame;
        private int totalFrames;
        private float timeSinceLastFrame = 0;
        private int millisecondsPerFrame = 600;
        private SpriteMap _spriteMap;
        private Texture2D _texture;
        public Vector2 _location;
        public Rectangle backgroundRect;
        public float _scale;
        string _animOrientation;
        bool _timed;
        public int prevWaveNumber = 0; // used for incrementing animations whose frames are tied to wave numbers
        public int currentFrameForWaveSync = 0;
        public int wavesSinceMature = 0;
        public int wavesSinceClickedOn = 0;
        Color _color;

        public AnimatorInstance(SpriteMap spriteMap, Texture2D texture, bool isRepeating, bool isPermanent, int animSpeed, Vector2 location,
            float scale, string animOrientation, bool timed, Color color)
        {
            _spriteMap = spriteMap;
            _texture = texture;
            _isRepeating = isRepeating;
            _isPermanent = isPermanent;
            millisecondsPerFrame = animSpeed;
            _location = location;
            totalFrames = _spriteMap.TextureList.Count;
            _scale = scale;
            _animOrientation = animOrientation;
            _timed = timed;
            _color = color;
        }

        public void UpdateAnimator(GameTime gameTime)
        {
            // set framerate of background animation
            timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame && _timed)
            {
                timeSinceLastFrame -= millisecondsPerFrame;
                currentFrame++;
                timeSinceLastFrame = 0;
                // if at last frame in sprite sheet, start at first frame or deactivate instance if applicable
                if (currentFrame == totalFrames)
                {
                    if (!_isRepeating) // non-repeating animations stop at the last frame
                    {
                        currentFrame = totalFrames - 1;
                        if (!_isPermanent) //non-permanent animations are removed when they finish their animation
                            isActive = false;
                    }
                    else // repeating animations loop to the first frame
                        currentFrame = 0;
                }
            }
            else if (!_timed)
            {
                if (currentFrameForWaveSync - 1 >= 0)
                    currentFrame = currentFrameForWaveSync - 1;
                else
                    currentFrame = currentFrameForWaveSync;
            }
        }

        public void DrawInstance(SpriteBatch spriteBatch)
        {
            // when needed, flip the frame
            SpriteEffects effect;
            if (_animOrientation.Contains("Right"))
                effect = SpriteEffects.FlipHorizontally;
            else
                effect = SpriteEffects.None;

            for (int i = 0; i < _spriteMap.TextureList.Count; i++)
            {
                // get sprite data
                int width = _spriteMap.TextureList[currentFrame].Width;
                int height = _spriteMap.TextureList[currentFrame].Height;
                int x = _spriteMap.TextureList[currentFrame].X;
                int y = _spriteMap.TextureList[currentFrame].Y;
                int mouseX, mouseY;
                if (width > 32)
                    mouseX = (int)_location.X - width / 2;
                else
                    mouseX = (int)_location.X;
                if (width > 32)
                    mouseY = (int)_location.Y - height / 2;
                else
                    mouseY = (int)_location.Y;
                // draw current frame from sprite data
                Rectangle sourceRect = new Rectangle(x, y, width, height);
                backgroundRect = new Rectangle(mouseX, mouseY, (int)(width * _scale), (int)(height * _scale));
                if (_location != new Vector2(0, 0))
                    spriteBatch.Draw(_texture, backgroundRect, sourceRect, _color, 0f, new Vector2(0, 0), effect, 0);
            }
        }
    }
}