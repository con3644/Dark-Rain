using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Colony_Ship_Horizon
{
    internal class Background
    {
        public Animator starship;
        private SpriteMap _spriteMap;
        public Rectangle parallaxStart;
        Vector2 parallaxScrollDistance;
        
        public Texture2D earth;

        private Texture2D textureToDraw;
        public Texture2D asteroidParallax;
        public Texture2D smogParallax;
        public Texture2D citySkylineParallax;
        public Texture2D cityMidgroundParallax;
        public Texture2D cityForegroundParallax;
        public Texture2D starsPurple;
        public Texture2D starsBlue;
        public Texture2D starsBlack;
        public Texture2D starsPink;
        public Texture2D starDust;
        public Texture2D nebula;
        public Texture2D asteroids;
        public Texture2D planets;

        public Texture2D flyingCar1;
        public Texture2D flyingCar2;
        public Texture2D flyingCar3;
        public Texture2D flyingCar4;
        public Texture2D flyingCar5;
        Vector2[] flyingCars = new Vector2[100];
        Vector2[] flyingCars2 = new Vector2[100];
        Texture2D[] flyingCarTextures = new Texture2D[100];
        int flyingObjectTimeSinceMovement = 0; // the time since flying objects moved
        float flyingObjectTimeCounter = 0; // milliseconds until next movement of a flying object on screen, set in update
        int flyingObjectXMovement = 0;
        int flyingCarEndPt;

        List<MovingObject> ftlBeams = new List<MovingObject>();
        public Texture2D ftlBeam;

        private Vector2 backgroundCoords;
        private int rows;
        private int columns;
        private int currentFrame;
        private int totalFrames;
        public string _mapName;
        // player's location
        Vector2 playerLocation;
        Vector2 prevLocation;
        Vector2 directionToNewLocation;
        // each vector is moved dynamically to create a scrolling background
        Vector2 leftScroller;
        Vector2 midScroller;
        Vector2 rightScroller;
        Vector2 spaceScroller;
        // represents the distance to scroll the background as determined by the time passed
        Vector2 distanceToScroll;
        Vector2 distanceToScroll2;
        private float timeSinceLastUpdate = 0;
        private float distanceToMove = 5;
        private float distanceToMove2 = 5;
        public float distanceToAdd = 0;
        public bool jumpToFTL = false;
        private int numberOfTexturesPassed = 0; // the times a texture has been passed while moving in space
        float timeTilNextUpdate = 1f; // how fast to update the movement of textures in space
        bool changeBackground = false;
        int backgroundCounter = 0;
        public string celestialObjectToDraw = ""; // set to the current planet when the player first enters the starship
        public string NextCelestialObjectToDraw = ""; // set when a player decides to travel to another planet from the travel map
        public bool readyToLoadMap = false;

        // scale of background image
        private float scale = 1f;

        private int timeSinceLastFrame = 0;
        private int millisecondsPerFrame = 600;
        bool _inGame;

        RandomNumberGen randomNumberGen = new RandomNumberGen();
        Sounds _sounds;

        public Background(SpriteMap spriteMap)
        {
            _spriteMap = spriteMap;

            int carSpacing = 200;
            int numberOfCars = 50;
            RandomNumberGen random = new RandomNumberGen();
            for (int i = 0; i < numberOfCars; i++) // add flying cars to list, flying left to right
            {
                flyingCars[i] = new Vector2(i * carSpacing, random.NextNumber(600, 650));
            }
            for (int i = numberOfCars-1; i > 0; i--) // add flying cars to list, flying right to left
            {
                flyingCars2[i] = new Vector2(i * carSpacing, random.NextNumber(810, 860));
            }
            flyingCarEndPt = carSpacing * numberOfCars;
        }

        /// <summary>
        /// Load textures into texture collections, called only after textures have been loaded from main game function
        /// </summary>
        public void Load()
        {
            RandomNumberGen random = new RandomNumberGen();
            for (int i = 0; i < 100; i++)
            {
                int randInt = random.NextNumber(0, 5);
                if (randInt == 0)
                    flyingCarTextures[i] = flyingCar1;
                else if (randInt == 1)
                    flyingCarTextures[i] = flyingCar2;
                else if (randInt == 2)
                    flyingCarTextures[i] = flyingCar3;
                else if (randInt == 3)
                    flyingCarTextures[i] = flyingCar4;
                else
                    flyingCarTextures[i] = flyingCar5;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gametime"></param>
        /// <param name="location"></param>
        /// <param name="inGame"></param>
        /// <param name="mapName"></param>
        public void Update(GameTime gametime, GraphicsDevice graphics, Sounds sounds, Vector2 location, bool inGame, string mapName = "")
        {
            // adjust for delta time
            float elapsed = (float)gametime.ElapsedGameTime.TotalSeconds;

            _mapName = mapName;
            _inGame = inGame;
            _sounds = sounds;
            prevLocation = playerLocation;
            playerLocation = location;
            directionToNewLocation = playerLocation - prevLocation;

            // update flying cars
            flyingObjectTimeSinceMovement += gametime.ElapsedGameTime.Milliseconds;
            if (flyingObjectTimeSinceMovement > flyingObjectTimeCounter)
            {
                flyingObjectTimeSinceMovement = 0;

                if (!_inGame)
                {
                    flyingObjectTimeCounter = 1; // set the time until next update
                    flyingObjectXMovement += 10;
                    // move starship across menu
                    starship.Update(gametime, new Vector2(-501 + flyingObjectXMovement, 500)); // -501 to start it on edge of screen
                    if (flyingObjectXMovement > graphics.Viewport.Width + 600)
                    {
                        flyingObjectXMovement = 0;
                    }
                }
                else
                {
                    flyingObjectTimeCounter = 10; // set the time until next update
                    // move flying cars across map
                    for (int i = 0; i < flyingCars.Length; i++)
                    {
                        if (directionToNewLocation.X > 0) // player is moving right, accelerate the cars with player
                        {
                            flyingCars[i].X += 10;
                            flyingCars2[i].X -= 2.5f;
                        }
                        else if (directionToNewLocation.X < 0) // player is moving left
                        {
                            flyingCars[i].X += 2.5f;
                            flyingCars2[i].X -= 10;
                        }
                        else // player is not moving, move cars at normal rate
                        {
                            flyingCars[i].X += 5;
                            flyingCars2[i].X -= 5;
                        }

                        if (flyingCars[i].X >= flyingCarEndPt)
                        {
                            flyingCars[i].X = 0; // reset movement at edge of screen
                        }
                        if (flyingCars2[i].X <= 0)
                        {
                            flyingCars2[i].X = flyingCarEndPt; // reset movement at edge of screen
                        }
                    }
                }
            }

            if (!_inGame)
            {
            }
            else
            {
                // set background to update to player's location - offset
                backgroundCoords.X = location.X - 512;
                backgroundCoords.Y = location.Y - 1500;
                // move background slightly as player moves
                backgroundCoords -= location / 8;

                parallaxScrollDistance = backgroundCoords;
                parallaxScrollDistance.Y = 0;
            }

            // set framerate of background animation
            timeSinceLastFrame += gametime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                currentFrame++;
                timeSinceLastFrame = 0;
                // if at last frame in sprite sheet, start at first frame
                if (currentFrame == totalFrames)
                    currentFrame = 0;
            }

            // move background from right to left when moving in space --------------scrolling background------
            if (mapName == "personalStarship")
            {
                string songToPlay = "engineRoomSong";
                if (_sounds.previousType != songToPlay)
                    _sounds.Update(gametime, songToPlay, _mapName);

                if (jumpToFTL)
                {
                    timeSinceLastUpdate += (float)gametime.ElapsedGameTime.TotalMilliseconds;
                    if (timeSinceLastUpdate > timeTilNextUpdate)
                    {
                        timeSinceLastUpdate = 0;
                        distanceToMove += distanceToAdd;
                        // this isn't reset (for certain objects that will move off of viewport)
                        distanceToMove2 += distanceToAdd;

                        if (numberOfTexturesPassed == 0)
                            _sounds.Update(gametime, "starshipJump", _mapName);

                        if (textureToDraw != null && distanceToMove > textureToDraw.Width + 64)
                        {
                            // reset the distance when it has exceeded the width of the texture
                            distanceToMove = 0;
                            numberOfTexturesPassed++;

                            // moving away from current celestial object, speed up
                            if (numberOfTexturesPassed == 1)
                            {
                                distanceToAdd = 10f;
                            }
                            else if (numberOfTexturesPassed == 2)
                            {
                                celestialObjectToDraw = "jumpToFTL";
                                distanceToAdd = 20f;
                            }
                            // slow down for approach to celestial object and prepare to draw the object
                            else if (numberOfTexturesPassed == 10)
                                distanceToAdd = 10f;
                            else if (numberOfTexturesPassed == 11)
                            {
                                _sounds.Update(gametime, "starshipArrival", _mapName);
                                celestialObjectToDraw = NextCelestialObjectToDraw;
                                NextCelestialObjectToDraw = "";
                                distanceToAdd = 5f;
                            }
                            else if (numberOfTexturesPassed == 12)
                            {
                                numberOfTexturesPassed = 0;
                                jumpToFTL = false;
                                readyToLoadMap = true;
                                // stop the ship
                                distanceToAdd = 0f;
                            }
                        }
                    }

                    // don't adjust the scroll distance when arriving/preparing to land at object
                    if (!readyToLoadMap)
                    {
                        // update the scroller by the time passed since the current background was set
                        distanceToScroll = new Vector2(distanceToMove, 0);
                        // scroller for objects that move past viewport
                        distanceToScroll2 = new Vector2(distanceToMove2, 0);
                        float backgroundYCoordScalar = 0f;
                        leftScroller = new Vector2(0 - textureToDraw.Width,
                            backgroundCoords.Y * backgroundYCoordScalar);
                        midScroller = new Vector2(0,
                            backgroundCoords.Y * backgroundYCoordScalar);
                        rightScroller = new Vector2(0 + textureToDraw.Width,
                            backgroundCoords.Y * backgroundYCoordScalar);
                    }

                    if (celestialObjectToDraw == "jumpToFTL")
                    {
                        Rectangle viewPortBounds = new Rectangle((int)(location.X - graphics.Viewport.Height * .75f),
                            (int)(location.Y - graphics.Viewport.Height * .5f),
                            graphics.Viewport.Bounds.Width + 200, // increase the width of viewport bounds for drawing FTL beams from the right to left
                            graphics.Viewport.Bounds.Height);
                        if (ftlBeams.Count < 500) // add new ftlBeams if below count
                        {
                            ftlBeams.Add(new MovingObject(ftlBeam, 32, viewPortBounds, false, false));
                        }
                        for (int i = 0; i < ftlBeams.Count; i++) // remove ftlBeams that hit a platform
                        {
                            ftlBeams[i].Update(gametime, new Vector2(-randomNumberGen.NextNumber(1200, 1500), 0), viewPortBounds, new List<Rectangle>());
                            if (!ftlBeams[i].isActive)
                            {
                                ftlBeams.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        ftlBeams.Clear();
                    }
                }

                // not moving through FTL
                else { }
            }

            else if (_mapName == "playerRoom2")
            {
                string songToPlay = "mainRoomSong";
                if (_sounds.previousType != songToPlay)
                    _sounds.Update(gametime, songToPlay, _mapName);
            }

            else if (_mapName == "playerRoom")
            {
                string songToPlay = "engineRoomSong";
                if (_sounds.previousType != songToPlay)
                    _sounds.Update(gametime, songToPlay, _mapName);
            }

            else if (_mapName == "spaceStation")
            {
                string songToPlay = "attackSong2";
                if (_sounds.previousType != songToPlay)
                    _sounds.Update(gametime, songToPlay, _mapName);
            }
            else if (_mapName == "asteroidColony")
            {
                string songToPlay = "attackSong2";
                if (_sounds.previousType != songToPlay)
                    _sounds.Update(gametime, songToPlay, _mapName);
            }

            else // ------------------------- parallax background-----------------------------------------
            {
                // distance to move background y coord. useful for positioning background to be seen by player
                scale = 2f;
                float backgroundYCoordScalar = 0f; // 0 out the y
                // infinite scrollers init
                if (textureToDraw != null)
                {
                    leftScroller = new Vector2(backgroundCoords.X - textureToDraw.Width, backgroundCoords.Y * backgroundYCoordScalar);
                    midScroller = new Vector2(backgroundCoords.X, backgroundCoords.Y * backgroundYCoordScalar);
                    rightScroller = new Vector2(backgroundCoords.X + textureToDraw.Width, backgroundCoords.Y * backgroundYCoordScalar);
                }
                spaceScroller = new Vector2(backgroundCoords.X, backgroundCoords.Y * backgroundYCoordScalar);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw menu textures
            if (!_inGame)
            {
                textureToDraw = starsPurple;
                spriteBatch.Draw(textureToDraw, midScroller - distanceToScroll,
                      new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                      0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                // draw starship
                //starship.Draw(spriteBatch);
            }

            // infinite scrollers will continuously adjust around the player to ensure they never see the edge of the screen
            if (playerLocation.X > midScroller.X + textureToDraw.Width) // player is moving right, past the middleScroller, move the leftScroller to the right of the rightScroller
                leftScroller.X = rightScroller.X + textureToDraw.Width;
            else if (playerLocation.X < midScroller.X)
                rightScroller.X = leftScroller.X - textureToDraw.Width;// player is moving left, past the middleScroller, move the rightScroller to the left of the leftScroller
           
            // ------------------------------------ON STARSHIP, DRAW VARIOUS SPACE TEXTURES-----------------------------------
            if (_mapName == "personalStarship")
            {
                // set the space background to draw
                if (celestialObjectToDraw == "asteroidColony")
                    textureToDraw = starsPurple;
                else if (celestialObjectToDraw == "earth")
                    textureToDraw = starsBlue;
                else if (celestialObjectToDraw == "jumpToFTL")
                {
                    //if (numberOfTexturesPassed >= 8)
                    //    textureToDraw = starsPurple;
                    //else
                    textureToDraw = starsBlue;
                }

                // horizontal adjustment of scrollers
                Vector2 horizMove = new Vector2(300, 0);
                // adjust for jerky movements when a celestial object switches coord vectors
                Vector2 backgroundObjectAdjustments = new Vector2(600, 250);

                // draw the infinite scrollers
                spriteBatch.Draw(textureToDraw, leftScroller - distanceToScroll + horizMove - parallaxScrollDistance * .05f,
                        new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                        0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.Draw(textureToDraw, midScroller - distanceToScroll + horizMove - parallaxScrollDistance * .05f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.Draw(textureToDraw, rightScroller - distanceToScroll + horizMove - parallaxScrollDistance * .05f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);

                ///
                // ------------------adjust this vector to move celestial objects----------------------------
                Vector2 backgroundObjectCoords = new Vector2(rightScroller.X + 1200 - distanceToScroll.X, rightScroller.Y) + horizMove
                    - parallaxScrollDistance * .1f;

                Vector2 backgroundObjectCoords2 = new Vector2(rightScroller.X + 1200 - distanceToScroll2.X, rightScroller.Y) + backgroundObjectAdjustments
                    - parallaxScrollDistance * .1f;


                if (celestialObjectToDraw == "jumpToFTL")
                {
                    // while in FTL, draw FTL animation and don't draw any celestial objects
                    for (int i = 0; i < ftlBeams.Count; i++)
                    {
                        spriteBatch.Draw(ftlBeam, ftlBeams[i].rectangle, Color.White);
                    }
                }
                else if (celestialObjectToDraw == "earth")
                {
                    // earth
                    backgroundObjectCoords.X += 290;
                    backgroundObjectCoords.Y += 250;

                    if (numberOfTexturesPassed == 0)
                        spriteBatch.Draw(earth, backgroundObjectCoords, new Rectangle(0, 0, earth.Width, earth.Height), Color.White,
                            0.0f, Vector2.Zero, 1, SpriteEffects.None, 0);
                    else
                        spriteBatch.Draw(earth, backgroundObjectCoords2, new Rectangle(0, 0, earth.Width, earth.Height), Color.White,
                            0.0f, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else if (celestialObjectToDraw == "asteroidColony")
                {
                    backgroundObjectCoords.X += 800;
                    backgroundObjectCoords.Y += 300;
                    // asteroid colony from space
                    spriteBatch.Draw(asteroids, backgroundObjectCoords, new Rectangle(0, 0, asteroids.Width, asteroids.Height), Color.White,
                        0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
                }
            }

            // -------------------------------------parallax planet background-------------------------------
            else if (_mapName == "asteroidColony")
            {
                // scale the back-most parallax image
                float backScale = 1.5f;
                // scale the middle parallax image
                float midScale = 2;
                // scale the front parallax image
                float frontScale = 2;
                // adjust the height of the parallax image
                int adjustParaHeight = 400; 
                // set the texture of the parallax image
                textureToDraw = citySkylineParallax;

                // background parallax needs moved upwards
                Vector2 backgroundParallaxStart = new Vector2(parallaxStart.X, parallaxStart.Y - 64);
                Vector2 backgroundParallaxStart2 = new Vector2(parallaxStart.X + 250, parallaxStart.Y - 64);
                //for the far right background
                Vector2 backgroundParallaxStart3 = new Vector2(parallaxStart.X + 250, parallaxStart.Y);
                // each background parallax gets a unique vector
                Vector2 parStartLeft = new Vector2(backgroundParallaxStart.X - (textureToDraw.Width * backScale),
                    backgroundParallaxStart.Y + adjustParaHeight);
                Vector2 parStartMiddle = new Vector2(backgroundParallaxStart.X, backgroundParallaxStart.Y + adjustParaHeight);
                Vector2 parStartTop = new Vector2(backgroundParallaxStart.X, 
                    backgroundParallaxStart.Y + adjustParaHeight - (textureToDraw.Height * backScale));
                Vector2 parStartBottom = new Vector2(backgroundParallaxStart.X, 
                    backgroundParallaxStart.Y + adjustParaHeight + (textureToDraw.Height * backScale));

                Vector2 parStartRight = new Vector2(backgroundParallaxStart.X + (textureToDraw.Width * backScale),
                   backgroundParallaxStart.Y + adjustParaHeight);
                Vector2 parStartTopRight = new Vector2(backgroundParallaxStart.X + (textureToDraw.Width * backScale),
                    backgroundParallaxStart.Y + adjustParaHeight - (textureToDraw.Height * backScale));
                Vector2 parStartBottomRight = new Vector2(backgroundParallaxStart.X + (textureToDraw.Width * backScale),
                    backgroundParallaxStart.Y + adjustParaHeight + (textureToDraw.Height * backScale));

                Vector2 parStartRight2 = new Vector2(backgroundParallaxStart2.X + (textureToDraw.Width * backScale),
                   backgroundParallaxStart2.Y + adjustParaHeight);
                Vector2 parStartRight3 = new Vector2(backgroundParallaxStart3.X + (textureToDraw.Width * backScale),
                  backgroundParallaxStart3.Y + adjustParaHeight);
                Vector2 parStartTopRight2 = new Vector2(backgroundParallaxStart2.X + (textureToDraw.Width * backScale),
                    backgroundParallaxStart2.Y + adjustParaHeight - (textureToDraw.Height * backScale));
                Vector2 parStartTopRight3 = new Vector2(backgroundParallaxStart3.X + (textureToDraw.Width * backScale),
                    backgroundParallaxStart3.Y + adjustParaHeight - (textureToDraw.Height * backScale));
                Vector2 parStartBottomRight2 = new Vector2(backgroundParallaxStart2.X + (textureToDraw.Width * backScale),
                    backgroundParallaxStart2.Y + adjustParaHeight + (textureToDraw.Height * backScale));

                Vector2 parSpace = new Vector2(parallaxStart.X, parallaxStart.Y);

                Vector2 midPara = new Vector2(parallaxStart.X - 800 + (cityMidgroundParallax.Width * backScale),
                    parallaxStart.Y -128);
                Vector2 frontPara = new Vector2(parallaxStart.X - 256 + (cityForegroundParallax.Width * frontScale),
                    parallaxStart.Y - 128);

                //spriteBatch.Draw(starsBlue, spaceScroller,
                //new Rectangle(0, 0, starsBlue.Width, starsBlue.Height), Color.White,
                //0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);

                // background textures LEFT-------------
                //spriteBatch.Draw(textureToDraw, parStartLeft + parallaxScrollDistance,
                //    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                //    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                //spriteBatch.Draw(citySkylineParallax, parStartLeft + parallaxScrollDistance,
                //    new Rectangle(0, 0, citySkylineParallax.Width, citySkylineParallax.Height), Color.White,
                //    0.0f, Vector2.Zero, paraScale, SpriteEffects.None, 0);

                // old skyline
                //spriteBatch.Draw(citySkylineParallax, parStartMiddle + parallaxScrollDistance,
                //    new Rectangle(0, 0, citySkylineParallax.Width, citySkylineParallax.Height), Color.White,
                //    0.0f, Vector2.Zero, paraScale, Sprite Effects.None, 0);

                /// far left backgrounds
                // ---background textures MIDDLE------------
                spriteBatch.Draw(textureToDraw, parStartMiddle + parallaxScrollDistance,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.DarkGray,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures TOP-----------
                spriteBatch.Draw(textureToDraw, parStartTop + parallaxScrollDistance,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.DarkGray,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures BOTTOM-----------
                spriteBatch.Draw(textureToDraw, parStartBottom + parallaxScrollDistance,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.DarkGray,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);

                /// far right backgrounds
                // ---background textures RIGHT3----------
                spriteBatch.Draw(textureToDraw, parStartRight3 + parallaxScrollDistance,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.DarkGray,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures TOP RIGHT3-----------
                spriteBatch.Draw(textureToDraw, parStartTopRight3 + parallaxScrollDistance,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.DarkGray,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);

                /// middle backgrounds
                // ---background textures TOP RIGHT1-----------
                spriteBatch.Draw(textureToDraw, parStartTopRight + parallaxScrollDistance * .9f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures RIGHT1-----------
                spriteBatch.Draw(textureToDraw, parStartRight + parallaxScrollDistance * .9f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures TOP RIGHT2-----------
                spriteBatch.Draw(textureToDraw, parStartTopRight2 + parallaxScrollDistance * .9f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures BOTTOM RIGHT2-----------
                spriteBatch.Draw(textureToDraw, parStartBottomRight2 + parallaxScrollDistance * .9f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
                // ---background textures RIGHT2----------
                spriteBatch.Draw(textureToDraw, parStartRight2 + parallaxScrollDistance * .9f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);


                // -----------------------------------------FLYING CARS--------------------------------------------
                // list for each type of car
                for (int i = 0; i < flyingCars.Length; i++)
                {
                    spriteBatch.Draw(flyingCarTextures[i], new Rectangle(flyingCars[i].ToPoint(),
                        new Point(32, 32)), Color.WhiteSmoke);
                    spriteBatch.Draw(flyingCarTextures[i], new Rectangle(flyingCars2[i].ToPoint(),
                        new Point(32, 32)), null, Color.WhiteSmoke, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                }


                // --------midground right
                midPara.X += cityMidgroundParallax.Width - 100; // move right
                spriteBatch.Draw(cityMidgroundParallax, midPara + (parallaxScrollDistance * .85f),
                    new Rectangle(0, 0, cityMidgroundParallax.Width, cityMidgroundParallax.Height), Color.LightGray,
                    0.0f, Vector2.Zero, midScale, SpriteEffects.None, 0);

                // --------midground
                midPara.X -= cityMidgroundParallax.Width; // move back
                spriteBatch.Draw(cityMidgroundParallax, midPara + (parallaxScrollDistance * .75f),
                    new Rectangle(0, 0, cityMidgroundParallax.Width, cityMidgroundParallax.Height), Color.White,
                    0.0f, Vector2.Zero, midScale, SpriteEffects.None, 0);

                //----------foreground
                spriteBatch.Draw(cityForegroundParallax, frontPara + (parallaxScrollDistance * .55f),
                    new Rectangle(0, 0, cityForegroundParallax.Width, cityForegroundParallax.Height), Color.White,
                    0.0f, Vector2.Zero, frontScale, SpriteEffects.None, 0);


                // ------------ smog --------------
                spriteBatch.Draw(smogParallax, parStartMiddle - new Vector2(0, 115) + parallaxScrollDistance * .65f,
                    new Rectangle(0, 0, smogParallax.Width * 10, (int)(smogParallax.Height * 1.5f)), Color.White,
                    0.0f, Vector2.Zero, backScale, SpriteEffects.None, 0);
            }
            else if (_mapName == "playerRoom2")
            {
                spriteBatch.Draw(starsPink, new Vector2(0, 150) + parallaxScrollDistance * .85f,
                    new Rectangle(0, 0, starsPink.Width, (int)(starsPink.Height)), Color.White,
                    0.0f, Vector2.Zero, 1.25f, SpriteEffects.None, 0);
                spriteBatch.Draw(asteroidParallax, new Vector2(-100, 750) + parallaxScrollDistance * .65f,
                    new Rectangle(0, 0, asteroidParallax.Width, (int)(asteroidParallax.Height)), Color.White,
                    0.0f, Vector2.Zero, 3, SpriteEffects.None, 0);
            }
            else if (_mapName == "spaceStation")
            {
                textureToDraw = starsBlack;
                
                // draw the infinite scrollers
                //spriteBatch.Draw(textureToDraw, leftScroller - distanceToScroll + horizMove - parallaxScrollDistance * .05f,
                //        new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                //        0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.Draw(textureToDraw, midScroller - distanceToScroll - parallaxScrollDistance * .05f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.Draw(textureToDraw, rightScroller - distanceToScroll - parallaxScrollDistance * .05f,
                    new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height), Color.White,
                    0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);

                // adjust this vector to move celestial objects
                Vector2 backgroundObjectCoords = new Vector2(rightScroller.X - distanceToScroll.X - 600, rightScroller.Y + 250)
                    - parallaxScrollDistance * .075f;
                
                // star
                spriteBatch.Draw(planets, backgroundObjectCoords, new Rectangle(0, 0, earth.Width, earth.Height), Color.White,
                    0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            }
        }
    }
}