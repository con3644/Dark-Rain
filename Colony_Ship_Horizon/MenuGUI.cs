using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Colony_Ship_Horizon
{
    internal class MenuGUI
    {
        private ContentManager _content;
        private GraphicsDevice _graphics;
        private List<Animator> animators = new List<Animator>();
        private List<string> animatorTypes = new List<string>();
        public bool previewPlacement = false;
        public bool isAnimMenuOpen = false;
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private Point mousePosOnGrid;
        private MouseState currentMouseState;
        private MouseState previousMouseState;
        private List<string> buttonNames = new List<string>();
        private List<string> buttonTypes = new List<string>(); // does the button represent a decoration, engineering or agricultural object
        private List<int> buttonAnimSpeeds = new List<int>(); // holds animation speed for animation to be drawn
        private List<int> buttonSpriteSizes = new List<int>(); // sprite size for animations/ button
        private List<Rectangle> buttonRects = new List<Rectangle>(); // for click events
        private List<bool> wasClicked = new List<bool>();
        private List<int> totalFrames = new List<int>();
        private List<SpriteMap> spriteMaps = new List<SpriteMap>();
        private List<Texture2D> textures = new List<Texture2D>();
        private int buttonCount = 0;
        private Texture2D chamomileSheet;
        private SpriteMap chamomile;
        private Texture2D tomatoSheet;
        private SpriteMap tomato;
        private Texture2D blueberrySheet;
        private SpriteMap blueberry;
        private Texture2D waterTankSheet;
        private SpriteMap waterTank;
        List<Rectangle> mechanicalAnimators = new List<Rectangle>();
        Vector2 _worldPos;
        List<TileObject> _tileObjects;
        bool invalidPlantPlacement = false; // flag to show player if they can place a plant in an area
        int currentPlantHeight; // used for validating plant placement

        public MenuGUI(ContentManager content, GraphicsDevice graphics)
        {
            _graphics = graphics;
            _content = content;
            chamomileSheet = _content.Load<Texture2D>("SpriteSheets/chamomileSheet");
            chamomile = _content.Load<SpriteMap>("SpriteMaps/chamomile");
            tomatoSheet = _content.Load<Texture2D>("SpriteSheets/tomatoSheet");
            tomato = _content.Load<SpriteMap>("SpriteMaps/tomato");
            blueberrySheet = _content.Load<Texture2D>("SpriteSheets/blueberrySheet");
            blueberry = _content.Load<SpriteMap>("SpriteMaps/blueberry");
            waterTankSheet = _content.Load<Texture2D>("SpriteSheets/waterTankSheet");
            waterTank = _content.Load<SpriteMap>("SpriteMaps/waterTank");
        }

        //// returns indices of types that match currentType
        //public bool GetListOfIndicesOfType(string currentType, string typeToMatch)
        //{
        //    bool doesTypeMatch = false;
        //    for (int i = 0; i < animators.Count; i++)
        //    {
        //        if (currentType == animators[i].type && animatorTypes[i] == typeToMatch)
        //        {
        //            doesTypeMatch = true;
        //            break;
        //        }
        //    }
        //    return doesTypeMatch;
        //}

        public void AddButton(string buttonName, string buttonType, int buttonAnimSpeed, int buttonSpriteSize, Rectangle buttonRect)
        {
            bool isShortLived = true;
            bool isPermanent = false;
            if (buttonType == "Agricultural")
            {
                isShortLived = false;
            }
            else if (buttonType == "Mechanical")
            {
                isPermanent = true;
            }
            animatorTypes.Add(buttonType);

            if (buttonCount == 0)
            {
                animators.Add(new Animator(chamomile, chamomileSheet, buttonAnimSpeed, isShortLived));
                buttonCount++;
                spriteMaps.Add(chamomile);
                textures.Add(chamomileSheet);
                totalFrames.Add(chamomile.TextureList.Count); // get last frame from spritemap and add to list
            }
            else if (buttonCount == 1)
            {
                animators.Add(new Animator(tomato, tomatoSheet, buttonAnimSpeed, isShortLived));
                spriteMaps.Add(tomato);
                textures.Add(tomatoSheet);
                buttonCount++;
                totalFrames.Add(tomato.TextureList.Count); // get last frame from spritemap and add to list
            }
            else if (buttonCount == 2)
            {
                animators.Add(new Animator(blueberry, blueberrySheet, buttonAnimSpeed, isShortLived));
                spriteMaps.Add(blueberry);
                textures.Add(blueberrySheet);
                buttonCount++;
                totalFrames.Add(blueberry.TextureList.Count); // get last frame from spritemap and add to list
            }
            else if (buttonCount == 3)
            {
                animators.Add(new Animator(waterTank, waterTankSheet, buttonAnimSpeed, isShortLived, isPermanent));
                buttonCount++;
                spriteMaps.Add(waterTank);
                textures.Add(waterTankSheet);
                totalFrames.Add(waterTank.TextureList.Count); // get last frame from spritemap and add to list
            }
            buttonNames.Add(buttonName);
            buttonTypes.Add(buttonType);
            buttonAnimSpeeds.Add(buttonAnimSpeed);
            buttonSpriteSizes.Add(buttonSpriteSize);
            buttonRects.Add(buttonRect);
            wasClicked.Add(false);
        }

        public void UpdateMenu(GameTime gameTime, List<TileObject> tileObjects, bool attemptSwitch, Vector2 playerLoc, Vector2 worldPos, int currentWave)
        {
            _tileObjects = tileObjects;
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            _worldPos = worldPos;
            // toggle animator menu mode mode
            if (currentKeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F) && !isAnimMenuOpen && !attemptSwitch)
            {
                isAnimMenuOpen = true;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F)) ||
                currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G) ||
                (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                && (isAnimMenuOpen || attemptSwitch))
            {
                previewPlacement = false;
                isAnimMenuOpen = false;
                for (int i = 0; i < wasClicked.Count; i++)
                {
                    wasClicked[i] = false; // leaving Agricultural mode, turn off all flags
                }
            }
            // check for user clicking events on menu buttons
            if (!previewPlacement)
            {
                for (int i = 0; i < buttonRects.Count; i++)
                {
                    if (buttonRects[i].Contains(currentMouseState.Position) && currentMouseState.LeftButton == ButtonState.Pressed
                        && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        wasClicked[i] = true;
                        previewPlacement = true;
                        Mouse.SetPosition(_graphics.Viewport.Width / 2, _graphics.Viewport.Height / 2);
                    }
                }
            }
            // user is previewing object placement, align to grid and check for click to place object
            else if (previewPlacement)
            {
                // player is placing object, iterate through wasClicked to determine which object
                for (int i = 0; i < wasClicked.Count; i++)
                {
                    Rectangle objectRect = new Rectangle(mousePosOnGrid, new Point(32, 64));
                    if (animatorTypes[i] == "Agricultural")
                        objectRect = new Rectangle(mousePosOnGrid, new Point(32, 64));
                    else if (animatorTypes[i] == "Mechanical")
                        objectRect = new Rectangle(mousePosOnGrid, new Point(32, 96));

                    // if planting an object, ensure it is above a planter and not being placed on an existing object
                    if (isAnimMenuOpen && wasClicked[i])
                    {
                        foreach (TileObject tileObject in _tileObjects)
                        {
                            Rectangle planter = new Rectangle((int)tileObject._tileX, (int)tileObject._tileY, tileObject._width, tileObject._height);
                            // determine if above a planter
                            if (tileObject._type.ToLower().Contains("planter") && objectRect.Left == planter.Left
                                && objectRect.Right == planter.Right && objectRect.Bottom == planter.Top)
                            {
                                List<Rectangle> allMatchingAnimRectangles = new List<Rectangle>();
                                // determine if being placed on another object of the same type
                                if (animatorTypes[i] == "Agricultural")
                                {
                                    for (int j = 0; j < animators.Count; j++)
                                    {
                                        // match the current type
                                        if (animatorTypes[j] == "Agricultural")
                                        {
                                            // collect all rectangles and add to list
                                            List<Rectangle> tempList = animators[j].GetAnimRectangle();
                                            for (int k = 0; k < tempList.Count; k++)
                                            {
                                                allMatchingAnimRectangles.Add(tempList[k]);
                                            }
                                        }
                                    }
                                    // determine if a matching object exists
                                    if (!allMatchingAnimRectangles.Contains(objectRect))
                                    {
                                        invalidPlantPlacement = false;
                                    }
                                }
                                else if (animatorTypes[i] == "Mechanical")
                                {
                                    for (int j = 0; j < animators.Count; j++)
                                    {
                                        // match the current type
                                        if (animatorTypes[j] == "Mechanical")
                                        {
                                            // collect all rectangles and add to list
                                            List<Rectangle> tempList = animators[j].GetAnimRectangle();
                                            for (int k = 0; k < tempList.Count; k++)
                                            {
                                                allMatchingAnimRectangles.Add(tempList[k]);
                                            }
                                        }
                                    }
                                    // determine if a matching object exists
                                    if (!allMatchingAnimRectangles.Contains(objectRect))
                                    {
                                        invalidPlantPlacement = false;
                                    }
                                }
                                break;
                            }
                            else
                                invalidPlantPlacement = true;
                        }
                    }
                    // player is attempting to place an animator
                    if (currentMouseState.LeftButton == ButtonState.Pressed && 
                        previousMouseState.LeftButton == ButtonState.Released && wasClicked[i])
                    {
                        // create agricultural animators
                        if (isAnimMenuOpen && !invalidPlantPlacement && animatorTypes[i] == "Agricultural")
                        {
                            if (!currentKeyboardState.IsKeyDown(Keys.LeftShift))
                            {
                                wasClicked[i] = false; // reset flags
                                previewPlacement = false;
                            }
                            animators[i].AddAnimation(mousePosOnGrid.ToVector2(), Color.White, 1, "Right", false);
                        }
                        // create mechanical animators
                        else if (!invalidPlantPlacement && animatorTypes[i] == "Mechanical")
                        {
                            // if player is holding shift, continue to place the same object
                            if (!currentKeyboardState.IsKeyDown(Keys.LeftShift))
                            {
                                wasClicked[i] = false; // reset flags
                                previewPlacement = false;
                            }
                            // correct the position misplacement cause by the larger size of the 'mechanical' sprites
                            mousePosOnGrid = new Point(mousePosOnGrid.X + 8, mousePosOnGrid.Y + 56);
                            animators[i].AddAnimation(mousePosOnGrid.ToVector2(), Color.White, 1, "Right");
                        }
                        else if (!isAnimMenuOpen)
                        {
                            animators[i].AddAnimation(mousePosOnGrid.ToVector2(), Color.White, 1, "Right");
                            // if player is holding shift, continue to place the same object
                            if (!currentKeyboardState.IsKeyDown(Keys.LeftShift))
                            {
                                wasClicked[i] = false; // reset flags
                                previewPlacement = false;
                            }
                        }
                        break;
                    }
                    // cancel object placement
                    else if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                    {
                        wasClicked[i] = false; // reset flags
                        previewPlacement = false;
                    }
                }
            }

            // update the animators
            for (int i = 0; i < animators.Count; i++)
            {
                // update Agricultural animators by the wave count
                if (animatorTypes[i] == "Agricultural")
                {
                    // get the current wave number in a range usable by this specific animator
                    int currentWaveInRange = (currentWave % animators[i].totalFrames);
                    animators[i].Update(gameTime, null, 1, currentWaveInRange);
                }
                // update all other animators normally and update the mechanical animator list of rectangles
                else
                {
                    // update list of mechanical animator rectangles, used to manage interaction
                    // with other animators
                    if (animatorTypes[i] == "Mechanical")
                    {
                        List<Rectangle> rectanglesFromCurrentAnim = animators[i].GetAnimRectangle(true);
                        // add new rectangles to list
                        for (int j = 0; j < rectanglesFromCurrentAnim.Count; j++)
                        {
                            if (!mechanicalAnimators.Contains(rectanglesFromCurrentAnim[j]))
                            {
                                mechanicalAnimators.Add(rectanglesFromCurrentAnim[j]);
                            }
                        }

                        // remove old rectangles from list
                        //for (int j = 0; j < mechanicalAnimators.Count; j++)
                        //{
                        //    if (mechanicalAnimators.Contains(rectanglesFromCurrentAnim[j]))
                        //    {

                        //    }
                        //}
                    }
                    animators[i].Update(gameTime);
                }
            }
        }

        // returns a dictionary with an entry for every clicked-on animator rectangle in the menu
        public Dictionary<string, List<Rectangle>> GetClickedMatureAnimRectangles()
        {
            Dictionary<string, List<Rectangle>> matureAnimDictionary = new Dictionary<string, List<Rectangle>>();
            for (int i = 0; i < animators.Count; i++)
            {
                if (animators[i].type != "Mechanical")
                {
                    // tell each non-mechanical animator where mechanical animators are, for interaction
                    animators[i].mechanicalAnimators = mechanicalAnimators;
                }
                matureAnimDictionary.Add(animators[i].type, animators[i].matureAnimClickedRectangles);
            }
            return matureAnimDictionary;
        }

        // returns a dictionary with an entry for every animator in the menu
        public Dictionary<string, List<Rectangle>> GetAnimatorRectangles()
        {
            Dictionary<string, List<Rectangle>> agriculturalAnimators = new Dictionary<string, List<Rectangle>>();
            Dictionary<string, List<Rectangle>> mechanicalAnimators = new Dictionary<string, List<Rectangle>>();
            for (int i = 0; i < animators.Count; i++)
            {
                // separate different animators into their own dictionaries
                if (animatorTypes[i] == "Agricultural")
                {
                    agriculturalAnimators.Add(animators[i].type, animators[i].GetAnimRectangle());
                }
                else if (animatorTypes[i] == "Mechanical")
                {
                    mechanicalAnimators.Add(animators[i].type, animators[i].GetAnimRectangle(true));
                }
            }

            List<Rectangle> rectanglesToRemove = new List<Rectangle>();
            // allow mechanical animators to act on the agricultural animators
            foreach (KeyValuePair<string, List<Rectangle>> agriAnim in agriculturalAnimators)
            {
                for (int i = 0; i < agriAnim.Value.Count; i++)
                {
                    foreach (KeyValuePair<string, List<Rectangle>> mechAnim in mechanicalAnimators)
                    {
                        for (int j = 0; j < mechAnim.Value.Count; j++)
                        {
                            if (agriAnim.Value[i].Intersects(mechAnim.Value[j]) || 
                                mechAnim.Value[j].Contains(agriAnim.Value[i]))
                            {
                                // allow mechanical anim to act on the agriculural anim
                                rectanglesToRemove.Add(agriAnim.Value[i]);
                            }
                        }
                    }
                }
                // remove all matching rectangles from the list
                for (int k = 0; k < rectanglesToRemove.Count; k++)
                    agriAnim.Value.RemoveAll(x => x == rectanglesToRemove[k]);
            }

            return agriculturalAnimators;
        }

        public void SetAnimatorRectangle(string animatorType, Rectangle clickedRectangle)
        {
            // find the corresponding animator
            foreach (Animator anim in animators)
            {
                if (anim.type == animatorType && !anim.clickedRectangles.Contains(clickedRectangle))
                {
                    // add the clicked rectangle to the animator's list
                    anim.clickedRectangles.Add(clickedRectangle);
                }
            }
        }

        // draw animators on world layer
        public void DrawAnimators(SpriteBatch spriteBatch)
        {
            int subtrahend;
            // the position of the mouse, aligned to the 32x32 grid (+world position)
            mousePosOnGrid = currentMouseState.Position + _worldPos.ToPoint();
            int gridSizeX = 32;
            int gridSizeY = 32;
            // align objects to a 32x32 grid
            if (mousePosOnGrid.X % gridSizeX != 0)
            {
                subtrahend = mousePosOnGrid.X % gridSizeX;
                mousePosOnGrid.X += (gridSizeX - subtrahend);
            }
            if (mousePosOnGrid.Y % gridSizeY != 0)
            {
                subtrahend = mousePosOnGrid.Y % gridSizeY;
                mousePosOnGrid.Y += (gridSizeY - subtrahend);
            }

            // draw mechanical animators in the background
            for (int i = 0; i < animators.Count; i++)
            {
                if (animatorTypes[i] == "Mechanical")
                    animators[i].Draw(spriteBatch);
            }
            // draw agriculural animators in the foreground
            for (int i = 0; i < animators.Count; i++)
            {
                if (animatorTypes[i] == "Agricultural")
                    animators[i].Draw(spriteBatch);
            }

            // draw a preview of the object clicked on in the menu
            for (int i = 0; i < wasClicked.Count; i++)
            {
                if (previewPlacement)
                {
                    if (wasClicked[i])
                    {
                        currentPlantHeight = spriteMaps[i].TextureList[totalFrames[i] - 1].Height;
                        int width = spriteMaps[i].TextureList[totalFrames[i] - 1].Width;
                        int height = spriteMaps[i].TextureList[totalFrames[i] - 1].Height;
                        int x = spriteMaps[i].TextureList[totalFrames[i] - 1].X;
                        int y = spriteMaps[i].TextureList[totalFrames[i] - 1].Y;
                        // draw current frame from sprite data
                        Rectangle sourceRect = new Rectangle(x, y, width, height);
                        Rectangle imgRect = new Rectangle(mousePosOnGrid.X, mousePosOnGrid.Y, width, height);
                        Color color;
                        if (invalidPlantPlacement)
                            color = Color.Red;
                        else
                            color = Color.LightGreen;
                        spriteBatch.Draw(textures[i], imgRect, sourceRect, color);
                    }
                }
                else
                    wasClicked[i] = false; // if not in preview, reset flag
            }
        }

        // draw menu buttons on GUI layer
        public void DrawMenu(SpriteBatch spriteBatch, GraphicsDevice graphics, Texture2D buttonImg)
        {
            if (buttonNames.Count > 0)
            {
                // using a single index, draw each button from the data in the lists
                for (int i = 0; i < buttonNames.Count; i++)
                {
                    // get sprite data for button image
                    int width = spriteMaps[i].TextureList[totalFrames[i] - 1].Width;
                    int height = spriteMaps[i].TextureList[totalFrames[i] - 1].Height;
                    int x = spriteMaps[i].TextureList[totalFrames[i] - 1].X;
                    int y = spriteMaps[i].TextureList[totalFrames[i] - 1].Y;
                    // draw current frame from sprite data
                    Rectangle sourceRect = new Rectangle(x, y, width, height);
                    Rectangle imgRect;
                    // make this button smaller
                    if (buttonNames[i].ToLower().Contains("tank"))
                        imgRect = new Rectangle(buttonRects[i].X + 16, buttonRects[0].Y, width - 10, height - 10);
                    else
                        imgRect = new Rectangle(buttonRects[i].X + 16, buttonRects[0].Y - 32, width*2 - 10, height*2 - 10);
                    spriteBatch.Draw(buttonImg, buttonRects[i], Color.White);
                    spriteBatch.Draw(textures[i], imgRect, sourceRect, Color.White);
                }
            }
        }
    }
}