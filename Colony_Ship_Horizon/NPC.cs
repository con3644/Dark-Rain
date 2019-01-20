using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TiledSharp;

namespace Colony_Ship_Horizon
{
    internal class NPC
    {
        private RandomNumberGen randomNumber = new RandomNumberGen();

        // camera data for translations
        private Vector2 _cameraCoords;

        // jump physics data
        private Vector2 velocity = new Vector2(0, 10);
        private Vector2 acceleration = new Vector2(0, -.45f);

        // control character animation speed
        private float timeSinceLastFrame = 0;
        private int millisecondsPerFrame = 100;

        // movement variables for the npc
        public bool isAtNpc = false;
        private bool hasJumped;
        private bool hasFallen;
        private bool onLadder;
        bool moveUpLadder = false;
        public Vector2 npcDirection;
        int npcWalkSpeed;
        int npcRunSpeed;
        // walking animation speed
        int walkSpeed;

        // character animation data
        SpriteMap spriteMap;
        private string currentAnimationFrame;
        private string previousAnimationFrame;
        private SpriteMap npcJumpingRight;
        private SpriteMap npcStandingRight;
        private SpriteMap npcWalkingRight;
        private SpriteMap npcAttackingRight;

        private Texture2D npcAttackingRightSheet;
        private Texture2D npcJumpingRightSheet;
        private Texture2D npcStandingRightSheet;
        private Texture2D npcWalkingRightSheet;

        // current action texture
        private Texture2D npcActionSheetToUse;
        private SpriteMap npcActionMapToUse;
        // action textures to load
        private Dictionary<string, Texture2D> actionSheets = new Dictionary<string, Texture2D>();
        private Dictionary<string, SpriteMap> actionMaps = new Dictionary<string, SpriteMap>();
        private SpriteEffects actionSpriteEffect;
        private int actionSpeed;
        private int actionPauseLength;
        bool newActionSpawn = true;
        
        // npc interactions
        public string _interaction;

        public Rectangle npcRect;
        public Rectangle npcHitBox;
        public Vector2 location;
        private int currentFrame;
        private int totalFrames;
        public int spriteSize;
        private Vector2 gunArmOffset;

        //  keyboard states used to determine key presses
        private KeyboardState currentKeyboardState;

        // /projectile data
        public Texture2D projectileTexture;
        private List<Projectile> projectiles;
        private Vector2 projectileDirection;
        private Vector2 gunLocation;
        private Vector2 _playerLocation;

        //  The rate of fire of the player laser
        private TimeSpan fireTime;
        private TimeSpan previousFireTime;

        // stats of npc
        public string _name;
        public string _type;
        public int npcHealth;
        private float npcMoveSpeed;

        // combat data
        public bool isEnemy;
        public bool isTalking = true;
        public bool isFiring = false;
        public bool attackPlayer = false;
        public int projectileDamage;
        public int meleeDamage;
        private Player _player;
        public bool outOfCombatWalking = false;
        public bool inCombatWalking = false;
        public int outOfCombatWalkDirection;
        float outOfCombatWalkingTimer;
        private List<Rectangle> _mapPlatforms;
        private Dictionary<string, Rectangle> _mapObjects;
        private List<Vector2> _mapStairs;
        private List<Rectangle> _mapLadders;
        private List<Rectangle> _mapDoors;
        string mapObjectsKey;
        int randomNumber100;
        public Rectangle agroRangeRect;
        int agroRange;
        float spriteSizeRectScale;
        public Animator npcDeathAnim;
        public Color npcColor;
        public bool isAtPlayer;

        /// <summary>
        /// Create NPC instance
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="mapPlatforms"></param>
        /// <param name="mapObjects"></param>
        /// <param name="mapStairs"></param>
        /// <param name="mapLadders"></param>
        /// <param name="mapDoors"></param>
        /// <param name="npcType">The data structure containing all data needed for each type</param>
        /// <param name="playerLoc">The player's current location</param>
        /// <param name="typeForNaming">The type name of this NPC ("Wave" for wave spawning, "Civilian" for civilians, etc.)</param>
        /// <param name="textureType">The name of the type of texture of this npc</param>
        /// <param name="spawnLocation">The integer representing which spawn point to use on the current map</param>
        /// <param name="name">The first name of this NPC</param>
        public NPC(GraphicsDevice graphics, List<Rectangle> mapPlatforms, Dictionary<string, Rectangle> mapObjects, 
            List<Vector2> mapStairs, List<Rectangle> mapLadders, List<Rectangle> mapDoors, NPC_Type npcType, 
            Vector2 playerLoc, string typeForNaming, string textureType, int spawnLocation = 0, string interaction = "", string actionToTake = "", 
            int actionSpeedInMilliseconds = 200, int actionPauseLengthInMilliseconds = 3000, string name = "")
        {
            // if no name is provided, generate a generic name for the NPC
            if (name == "")
            {
                _name = string.Concat(textureType, spawnLocation.ToString());
            }
            else
            {
                _name = string.Concat(textureType, "_", name);
            }
            _type = typeForNaming;
            
            _mapLadders = mapLadders;
            _mapStairs = mapStairs;
            _mapObjects = mapObjects;
            _mapPlatforms = mapPlatforms;
            _mapDoors = mapDoors;

            npcJumpingRight = npcType._npcJumpingRight;
            npcStandingRight = npcType._npcStandingRight;
            npcWalkingRight = npcType._npcWalkingRight;
            npcAttackingRight = npcType._npcAttackingRight;
            npcAttackingRightSheet = npcType._npcAttackingRightSheet;
            npcJumpingRightSheet = npcType._npcJumpingRightSheet;
            npcStandingRightSheet = npcType._npcStandingRightSheet;
            npcWalkingRightSheet = npcType._npcWalkingRightSheet;

            projectileTexture = npcType._projectileTexture;
            npcDeathAnim = npcType.npcDeathAnim;

            if (!textureType.Contains("alien"))
            {
                actionSheets = npcType._actionSheets;
                actionMaps = npcType._actionMaps;
                actionSpeed = actionSpeedInMilliseconds;
                actionPauseLength = actionPauseLengthInMilliseconds;
            }
            _interaction = interaction; // how the player can interact with this npc

            // NPC init.

            // set the action to take, determined by the property of the map object this npc will spawn on
            string actionString = textureType + actionToTake; // ex.: npcMale1 + SittingRight = a match to texture in actionTextures
            if (actionString.ToLower().Contains("bounty"))
            {
                isEnemy = true;
                isTalking = true;
            }
            else
                isEnemy = false;

            // if the action is left-facing, flip it and change to string so that it matches the texture in actionTextures
            if (actionToTake.Contains("Left"))
            {
                npcDirection = new Vector2(-1, 0);
                currentAnimationFrame = "StationaryLeft";
                spriteMap = npcStandingRight;

                actionSpriteEffect = SpriteEffects.FlipHorizontally; // flip the texture, if necessary
                // replace substring within actionString
                actionString = actionString.Remove(actionString.IndexOf("Left")); //remove the substring Left
                actionString += "Right"; //replace it with Right
                // replace substring within actionToTake
                actionToTake = actionToTake.Remove(actionToTake.IndexOf("Left")); //remove the substring Left
                actionToTake += "Right"; //replace it with Right
            }
            else // face it to the right
            {
                npcDirection = new Vector2(0, 1);
                currentAnimationFrame = "StationaryRight";
                spriteMap = npcStandingRight;
            }

            // an action will be set here, determined by the actionToTake
            if (actionSheets.ContainsKey(actionString))
            {
                npcActionSheetToUse = actionSheets[actionString];
            }
            if (actionMaps.ContainsKey(actionString))
            {
                npcActionMapToUse = actionMaps[actionString];
            }

            //---------------------------------- npc is a WAVE SPAWN ENEMY
            if (_type == "Wave" && spawnLocation > 0)
            {
                //int randomNumberColor = randomNumber.NextNumber(0, 4);
                int randomNumberColor = 3;
                switch (randomNumberColor)
                {
                    case 0:
                        {
                            spriteSize = 96;
                            npcColor = Color.Thistle;
                            break;
                        }
                    case 1:
                        {
                            spriteSize = 48;
                            npcColor = Color.WhiteSmoke;
                            break;
                        }
                    case 2:
                        {
                            spriteSize = 80;
                            npcColor = Color.Wheat;
                            break;
                        }
                    case 3:
                        {
                            spriteSize = 64;
                            npcColor = Color.White;
                            break;
                        }
                }

                mapObjectsKey = "Wave_Spawn" + spawnLocation.ToString();
            }//---------------- end wave spawners

            //------------------------------------npc is a CIVILIAN
            else if (_type == "Civilian" && spawnLocation > 0)
            {
                spriteSize = 96;
                npcColor = Color.White;
                mapObjectsKey = "NPC_Spawn" + spawnLocation.ToString();
            }//-----------------------end civilian

            location = new Vector2(_mapObjects[mapObjectsKey].Location.X, //get spawn point from map
                _mapObjects[mapObjectsKey].Location.Y);
            // npc rect is used for collision detection, spriteSize and scale are used for drawing and scaling the npc's hit box
            
            spriteSizeRectScale = .5f;
            npcRect = new Rectangle((int)location.X + (int)(spriteSize * spriteSizeRectScale), 
                (int)location.Y + (int)(spriteSize * spriteSizeRectScale), 
                (int)(spriteSize * spriteSizeRectScale), 
                (int)(spriteSize * spriteSizeRectScale));
            npcHitBox = new Rectangle((int)location.X + (int)(spriteSize), (int)location.Y + (int)(spriteSize), 
                (int)(spriteSize), (int)(spriteSize));
            
            npcRunSpeed = 250;
            npcWalkSpeed = 100;
            onLadder = false;
            npcHealth = 40;
            meleeDamage = 10;
            agroRange = 800; // size of agro around NPC
            agroRangeRect = new Rectangle((int)location.X - agroRange/2, (int)location.Y - agroRange/2, agroRange, agroRange); // rectangle of agro around NPC
            // gravity init.
            velocity = new Vector2(0, 0);
            acceleration = new Vector2(0, -1500f);
        }

        public void Initialize()
        {
            projectiles = new List<Projectile>();
        }

        public void Update(GameTime gameTime, Player player)
        {
            _player = player;
            // store camera coordinates for translations
            _cameraCoords = player.camera.Position;
            // store player's location for npc tracking
            _playerLocation = player.location;

            // call movement controls method for player
            Move(gameTime);
            Jump(gameTime);
            //update npcRect with new location
            npcRect = new Rectangle((int)location.X + (int)(spriteSize * spriteSizeRectScale), (int)location.Y + (int)(spriteSize * spriteSizeRectScale), (int)(spriteSize * spriteSizeRectScale), (int)(spriteSize * spriteSizeRectScale));

            int hitBoxX = spriteSize / 8;
            int hitBoxY = spriteSize / 2;
            npcHitBox = new Rectangle((int)location.X + hitBoxX, (int)location.Y + hitBoxY, spriteSize - hitBoxX, spriteSize - hitBoxY);
            // keep agro rect on NPC
            agroRangeRect.X = (int)location.X - agroRange / 2;
            agroRangeRect.Y = (int)location.Y - agroRange / 2;

            // find total number of frames for current sprite map
            totalFrames = spriteMap.TextureList.Count;
            // set framerate of background animation
            timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                timeSinceLastFrame = 0;
                
                currentFrame++;
                // if at last frame in sprite sheet, start at first frame
                if (currentFrame == totalFrames)
                {
                    currentFrame = 0;
                }
                if (currentAnimationFrame == "AimingRight")
                {
                    if (currentFrame == 1 || currentFrame == 7)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 2 || currentFrame == 8)
                        gunArmOffset = new Vector2(-1, -1);
                    else if (currentFrame == 3 || currentFrame == 9)
                        gunArmOffset = new Vector2(0, -1);
                    else if (currentFrame == 4 || currentFrame == 10)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 5 || currentFrame == 11)
                        gunArmOffset = new Vector2(1, 1);
                    else if (currentFrame == 6 || currentFrame == 12)
                        gunArmOffset = new Vector2(0, 0);
                }
                else if (currentAnimationFrame == "AimingBackRight")
                {
                    if (currentFrame == 1 || currentFrame == 7)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 2 || currentFrame == 8)
                        gunArmOffset = new Vector2(1, -1);
                    else if (currentFrame == 3 || currentFrame == 9)
                        gunArmOffset = new Vector2(0, -1);
                    else if (currentFrame == 4 || currentFrame == 10)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 5 || currentFrame == 11)
                        gunArmOffset = new Vector2(-1, 1);
                    else if (currentFrame == 6 || currentFrame == 12)
                        gunArmOffset = new Vector2(0, 0);
                }
                else if (currentAnimationFrame == "AimingLeft")
                {
                    if (currentFrame == 1 || currentFrame == 7)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 2 || currentFrame == 8)
                        gunArmOffset = new Vector2(-1, 1);
                    else if (currentFrame == 3 || currentFrame == 9)
                        gunArmOffset = new Vector2(0, 1);
                    else if (currentFrame == 4 || currentFrame == 10)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 5 || currentFrame == 11)
                        gunArmOffset = new Vector2(1, -1);
                    else if (currentFrame == 6 || currentFrame == 12)
                        gunArmOffset = new Vector2(0, 0);
                }
                else if (currentAnimationFrame == "AimingBackLeft")
                {
                    if (currentFrame == 1 || currentFrame == 7)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 2 || currentFrame == 8)
                        gunArmOffset = new Vector2(1, 1);
                    else if (currentFrame == 3 || currentFrame == 9)
                        gunArmOffset = new Vector2(0, 1);
                    else if (currentFrame == 4 || currentFrame == 10)
                        gunArmOffset = new Vector2(0, 0);
                    else if (currentFrame == 5 || currentFrame == 11)
                        gunArmOffset = new Vector2(-1, -1);
                    else if (currentFrame == 6 || currentFrame == 12)
                        gunArmOffset = new Vector2(0, 0);
                }
                else if (currentAnimationFrame.Contains("StandingAimingRight"))
                {
                    if (currentFrame == 1)
                        gunArmOffset = new Vector2(0, -2);
                    else if (currentFrame == 2)
                        gunArmOffset = new Vector2(0, -2);
                    else if (currentFrame == 3)
                        gunArmOffset = new Vector2(0, -3);
                    else if (currentFrame == 4)
                        gunArmOffset = new Vector2(0, -2);
                    else if (currentFrame == 5)
                        gunArmOffset = new Vector2(0, -1);
                    else if (currentFrame == 6)
                        gunArmOffset = new Vector2(0, -1);
                }
                else if (currentAnimationFrame.Contains("StandingAimingLeft"))
                {
                    if (currentFrame == 1)
                        gunArmOffset = new Vector2(0, 2);
                    else if (currentFrame == 2)
                        gunArmOffset = new Vector2(0, 2);
                    else if (currentFrame == 3)
                        gunArmOffset = new Vector2(0, 3);
                    else if (currentFrame == 4)
                        gunArmOffset = new Vector2(0, 2);
                    else if (currentFrame == 5)
                        gunArmOffset = new Vector2(0, 1);
                    else if (currentFrame == 6)
                        gunArmOffset = new Vector2(0, 1);
                }
            }


            fireTime = TimeSpan.FromSeconds(.5f);
            // this npc is an enemy and is not talking, fire at the player
            if (isEnemy && !isTalking)//attackPlayer)
            {
                //  fire only every interval we set as the fireTime
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    //  reset current time
                    previousFireTime = gameTime.TotalGameTime;
                    //  add the projectile
                    AddProjectile(new Vector2(location.X, location.Y));
                    isFiring = true;
                }
            }
            //  update the projectiles
            UpdateProjectiles(gameTime);
            UpdateCollision(gameTime);

            // update agro range
            agroRangeRect = new Rectangle((int)location.X - agroRange / 2, (int)location.Y - agroRange / 2, agroRange, agroRange); // rectangle of agro around NPC
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draws current frame from sprite sheet
            DrawCurrentFrame(spriteBatch);
            DrawProjectile(spriteBatch);
        }

        public void DrawCurrentFrame(SpriteBatch spriteBatch)
        {
            int walkSpeed = 75;
            int standSpeed = 225;

            bool isActingWithoutMap = false;
            if (npcActionSheetToUse != null) // don't draw normal animations, npc is acting
                currentAnimationFrame = "Acting";

            Texture2D texture;
            SpriteEffects effect;
            switch (currentAnimationFrame)
            {
                case "AttackingRight":
                    {
                        spriteMap = npcAttackingRight;
                        texture = npcAttackingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkSpeed;
                        break;
                    }
                case "AttackingLeft":
                    {
                        spriteMap = npcAttackingRight;
                        texture = npcAttackingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkSpeed;
                        break;
                    }
                case "PlayerRight":
                    {
                        spriteMap = npcWalkingRight;
                        texture = npcWalkingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkSpeed;
                        break;
                    }
                case "PlayerLeft":
                    {
                        spriteMap = npcWalkingRight;
                        texture = npcWalkingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkSpeed;
                        break;
                    }
                case "StationaryLeft":
                    {
                        spriteMap = npcStandingRight;
                        texture = npcStandingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "StationaryRight":
                    {
                        spriteMap = npcStandingRight;
                        texture = npcStandingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "JumpLeft":
                    {
                        spriteMap = npcJumpingRight;
                        texture = npcJumpingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "JumpRight":
                    {
                        spriteMap = npcJumpingRight;
                        texture = npcJumpingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "Acting":
                    {
                        effect = actionSpriteEffect;
                        texture = npcActionSheetToUse;
                        isActingWithoutMap = true;
                        
                        if (npcActionMapToUse != null)
                        {
                            spriteMap = npcActionMapToUse;
                            // delay the action animation by a randomized length of time
                            if (currentFrame == 0)
                            {
                                // a map was just loaded, randomize the current frame once
                                //if (newActionSpawn)
                                //{
                                //    currentFrame = randomNumber.NextNumber(0, totalFrames);
                                //    newActionSpawn = false;
                                //}
                                //else
                                millisecondsPerFrame = actionPauseLength;
                            }
                            else
                                millisecondsPerFrame = actionSpeed;
                            isActingWithoutMap = false;
                        }
                        break;
                    }
                default:
                    {
                        spriteMap = npcWalkingRight;
                        texture = npcWalkingRightSheet;
                        effect = SpriteEffects.None;
                        break;
                    }
            }
            if (currentFrame >= spriteMap.TextureList.Count) // ensure index is in bounds after switching animations
                currentFrame = 0;

            // move through texture list
            for (int i = 0; i < spriteMap.TextureList.Count; i++)
            {
                // get sprite data
                int width = spriteMap.TextureList[currentFrame].Width;
                int height = spriteMap.TextureList[currentFrame].Height;
                int x = spriteMap.TextureList[currentFrame].X;
                int y = spriteMap.TextureList[currentFrame].Y;
                // draw current frame from sprite data
                Rectangle sourceRect = new Rectangle(x, y, width, height);
                Rectangle destRect = new Rectangle((int)location.X, (int)location.Y, spriteSize, spriteSize);
                
                if (isActingWithoutMap)
                    spriteBatch.Draw(texture, destRect, null, npcColor, 0f, Vector2.Zero, effect, 0);
                else
                    spriteBatch.Draw(texture, destRect, sourceRect, npcColor, 0f, Vector2.Zero, effect, 0);
            }
            
            // store previous animation frame
            previousAnimationFrame = currentAnimationFrame;
        }

        public void DrawProjectile(SpriteBatch spriteBatch)
        {
            //  Draw the Projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(spriteBatch);
            }
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();

            // calculate direction needed for projectile to hit cursor position (do not add the world/camera position for npc's calculations)
            projectileDirection.X = _playerLocation.X - location.X;
            projectileDirection.Y = _playerLocation.Y - location.Y;

            int gunLocationOffset = 0;
            if (currentAnimationFrame.Contains("Right"))
                gunLocationOffset = 64;
            gunLocation = new Vector2(location.X + gunLocationOffset, location.Y + 52);

            // don't divide by zero and normalize the vector(convert to unit vector)
            if (projectileDirection != Vector2.Zero)
                projectileDirection.Normalize();
            projectile.Initialize(projectileTexture, projectileTexture, position, projectileDirection, 20, 1, 1000, gunLocation, 64, 1f, false);
            // damage of the projectile
            projectileDamage = projectile.Damage;
            // add a single projectile to list
            projectiles.Add(projectile);
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            //  Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update(gameTime, projectileDirection, gunLocation, location ,gunArmOffset, currentAnimationFrame);

                // will update projectile position to impact position
                if (projectiles[i].Texture.Name.Contains("lazer")
                        || projectiles[i].Texture.Name.Contains("gun"))
                {
                    //aimingLeft = projectiles[i].aimingLeft;
                    gunLocation = projectiles[i].Position;
                    projectileDirection.X = _playerLocation.X - location.X + _cameraCoords.X;
                    projectileDirection.Y = _playerLocation.Y - location.Y + _cameraCoords.Y;
                    // don't divide by zero and normalize the vector(convert to unit vector)
                    if (projectileDirection != Vector2.Zero)
                        projectileDirection.Normalize();
                }
                else if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void UpdateCollision(GameTime gameTime)
        {
            // if npc is launching projectiles, text collision against player
            Rectangle projectileRect;
            for (int i = 0; i < projectiles.Count; i++)
            {
                // Create the rectangles we need to determine if we collided with each other
                projectileRect = new Rectangle((int)projectiles[i].Position.X -
                projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);
                // Determine if the two objects collided with each other
                if (projectileRect.Intersects(_player.PlayerRect))
                {
                    _player.playerHealth -= projectiles[i].Damage;
                    projectiles[i].Active = false;
                }
            }
            // if npc is in range of melee attacking player then damage player at a single specific animation frame 
            if (attackPlayer && currentAnimationFrame.Contains("Attack") && currentFrame == 6)
            {
                // ensure the player is only damaged once during the attack animation cycle
                if (gameTime.TotalGameTime - previousFireTime > TimeSpan.FromMilliseconds(walkSpeed * totalFrames))
                {
                    previousFireTime = gameTime.TotalGameTime;
                    _player.playerHealth -= meleeDamage;
                }
            }
        }

        public void Move(GameTime gameTime)
        {
            bool atDoor = false;
            foreach (KeyValuePair<string, Rectangle> obj in _mapObjects)
            {
                if (obj.Key.ToLower().Contains("door") && obj.Value.Intersects(npcRect))
                {
                    atDoor = true;
                    break;
                }
            }
            inCombatWalking = false; // reset flag
            // finds a number between 0 and 100
            randomNumber100 = randomNumber.NextNumber(0, 100); 
            // time since the last frame is used to keep player movement smooth, in case of framerate drop
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // when not in combat, this timer keeps the npc stationary until the next movement is chosen
            outOfCombatWalkingTimer += elapsed;
            // represents the player's rectangle for collision detection
            Rectangle playerRect = new Rectangle((int)_playerLocation.X + 18, (int)_playerLocation.Y, 96, 96); // make the rectangle smaller than player's actual sprite size
            // determine if the npc is at the player's location
            isAtPlayer = false;
            bool isAboveBelowPlayer = false;
            if (npcRect.Intersects(playerRect))
                isAtPlayer = true;
            // don't attack player when player is above or below them
            else if (npcRect.Top  > playerRect.Bottom || npcRect.Bottom < playerRect.Top)
                isAboveBelowPlayer = true;
            // direction npc needs to get to player
            Vector2 directionToPlayer = _playerLocation - location;
            directionToPlayer.Normalize(); // normalize the vector before operating on it
            // dot product of direction to player and npc's current direction: determines whether player
            // is in the npc's field of view
            Vector2 directionProduct = directionToPlayer * npcDirection;
            // positive result means that player is in field of view, negative means is not, perpendicular is 0
            float fieldOfViewResult = directionProduct.X + directionProduct.Y;
            if (agroRangeRect.Intersects(playerRect) && fieldOfViewResult >= 0 && 
                (playerRect.Y >= npcRect.Y - spriteSize && playerRect.Y <= npcRect.Y))
            {
                attackPlayer = true;
                outOfCombatWalking = false;
            }
            else if (!agroRangeRect.Intersects(playerRect) || (playerRect.Y <= npcRect.Y - spriteSize*2 || playerRect.Y >= npcRect.Y)) // player is out of range or on a different level, exit combat
            {
                if (attackPlayer) // stop attacking
                {
                    currentAnimationFrame = "StationaryRight";
                    outOfCombatWalking = false;
                }
                attackPlayer = false;
            }

            if (!outOfCombatWalking && !attackPlayer)
            {
                npcMoveSpeed = npcWalkSpeed;
                outOfCombatWalkingTimer = 0; // reset timer
                if (randomNumber100 < 33) // walk left
                {
                    outOfCombatWalkDirection = -1;
                    outOfCombatWalking = true;
                }
                else if (randomNumber100 < 66) // don't move
                {
                    outOfCombatWalkDirection = 0;
                    outOfCombatWalking = true;
                }
                else // walk right
                {
                    outOfCombatWalkDirection = 1;
                    outOfCombatWalking = true;
                }
            }
            if (_type == "Civilian") // civilians act differently
            {
                currentAnimationFrame = "StationaryRight";
            }
            else if (attackPlayer)
            {
                // npc runs towards player until it reaches the player for attack
                npcMoveSpeed = npcRunSpeed;
                if (directionToPlayer.X > 0 && !isAtPlayer && !isAboveBelowPlayer && !IsAgainstWall(elapsed) && !atDoor) // follow player to the right
                {
                    location.X += npcMoveSpeed * elapsed;
                    currentAnimationFrame = "PlayerRight";
                    inCombatWalking = true;
                }
                else if (!isAtPlayer && !isAboveBelowPlayer && !IsAgainstWall(elapsed) && !atDoor) // follow player to the left
                {
                    currentAnimationFrame = "PlayerLeft";
                    location.X -= npcMoveSpeed * elapsed;
                    inCombatWalking = true;
                }
                else if (directionToPlayer.X > 0 && isAtPlayer && !isAboveBelowPlayer) // attack when at player's left, but not when above or below player
                {
                    currentAnimationFrame = "AttackingRight";
                    inCombatWalking = false;
                }
                else if (isAtPlayer && !isAboveBelowPlayer) // attack when at player's right, but not when above or below player
                {
                    currentAnimationFrame = "AttackingLeft";
                    inCombatWalking = false;
                }
                else
                    currentAnimationFrame = "StationaryRight";
            }
            //else if (IsOnLadder())
            //{
            //    location.Y += npcWalkSpeed * elapsed;
            //}
            else if (outOfCombatWalkingTimer > 3f) // while idle, NPC will move/not move for set time, then it will reset and move randomly
            {
                outOfCombatWalkingTimer = 0;
                outOfCombatWalking = false;
                outOfCombatWalkDirection = 0;
            }
            else if (outOfCombatWalking && outOfCombatWalkDirection == -1 && !isAtNpc && !atDoor) // move npc left
            {
                npcMoveSpeed = npcWalkSpeed;
                // hasten framerate for movement animation
                currentAnimationFrame = "PlayerLeft";

                if (!IsAgainstWall(elapsed) || npcDirection == new Vector2(0, 1))
                {
                    location.X -= npcMoveSpeed * elapsed;
                }
                else // turn around when at wall
                    outOfCombatWalkDirection = 1;
                npcDirection = new Vector2(-1, 0);
            }
            else if (outOfCombatWalking && outOfCombatWalkDirection == 1 && !isAtNpc && !atDoor)// move npc right
            {
                npcMoveSpeed = npcWalkSpeed;
                // hasten framerate for movement animation
                currentAnimationFrame = "PlayerRight";

                if (!IsAgainstWall(elapsed) || npcDirection == new Vector2(-1, 0))
                {
                    location.X += npcMoveSpeed * elapsed;
                }
                else // turn around when at wall
                    outOfCombatWalkDirection = -1;
                npcDirection = new Vector2(0, 1);
            }
            // move npc up when....
            //else if (IsOnLadder())
            //{
            //    location.Y -= npcMoveSpeed * elapsed;
            //}
            // move npc down when...
            

            // if player isnt moving, play stationary animations accordingly
            else
            {
                // slow framerate for stationary animation
                // if playing was moving left, play StationaryLeft movement animation
                if (currentAnimationFrame == "PlayerLeft")
                    currentAnimationFrame = "StationaryLeft";
                // if playing was moving right, play StationaryRight movement animation
                else if (currentAnimationFrame == "PlayerRight")
                    currentAnimationFrame = "StationaryRight";
            }
        }

        /// <summary>
        /// When a new map is loaded, the player must receive the new map data
        /// </summary>
        /// <param name="mapPlatforms"></param>
        /// <param name="mapObjects"></param>
        /// <param name="mapStairs"></param>
        /// <param name="mapLadders"></param>
        /// <param name="mapDoors"></param>
        public void LoadNewMapData(List<Rectangle> mapPlatforms,
            Dictionary<string, Rectangle> mapObjects, List<Vector2> mapStairs,
            List<Rectangle> mapLadders, List<Rectangle> mapDoors)
        {
            _mapDoors = mapDoors;
            _mapLadders = mapLadders;
            _mapStairs = mapStairs;
            _mapObjects = mapObjects;
            _mapPlatforms = mapPlatforms;

        }

        public void Jump(GameTime gameTime)
        {
            // time since the last frame is used to keep player movement smooth, in case of framerate drop
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // player is jumping
            //if (IsKeyPressed(Keys.Space) && hasJumped == false && hasFallen == false && onLadder == false)
            if (1==0)
            {
                velocity.Y = 500;
                acceleration = new Vector2(0, -1500f);
                location -= velocity * elapsed;

                hasJumped = true;
                if (npcDirection == new Vector2(0, 1))
                    currentAnimationFrame = "JumpRight";
                else
                    currentAnimationFrame = "JumpLeft";
            }
            // player is on ladder, do nothing
            else if (IsOnLadder())
            {
                onLadder = true;
                hasFallen = false;
                hasJumped = false;
                //// ladder climbing animation
            }
            // player is falling
            else if (!IsOnGround(elapsed) && hasJumped == false && hasFallen == false && !IsAboveLadder())
            {
                velocity.Y = 0;
                hasFallen = true;
                if (npcDirection == new Vector2(0, 1))
                    currentAnimationFrame = "JumpRight";
                else
                    currentAnimationFrame = "JumpLeft";
            }
            // implement gravity
            else
            {
                // stop gravity when player reaches floor
                if (IsOnGround(elapsed))
                {
                    // reset flags
                    hasJumped = false;
                    hasFallen = false;
                    // change animations when landing
                    if (npcDirection == new Vector2(0, 1) &&
                        (previousAnimationFrame == "JumpLeft" || previousAnimationFrame == "JumpRight"))
                        // if player just landed and was facing right
                        currentAnimationFrame = "StandingRight";
                    else if (npcDirection == new Vector2(-1, 0) &&
                        (previousAnimationFrame == "JumpLeft" || previousAnimationFrame == "JumpRight"))
                        // if player just landed and was facing right
                        currentAnimationFrame = "StandingLeft";
                }
                else
                {
                    // update player location by velocity
                    location -= velocity * elapsed;
                    velocity += acceleration * elapsed;
                    // move right while jumping if not against a wall
                    if (currentKeyboardState.IsKeyDown(Keys.D))
                    {
                        if (!IsAgainstWall(elapsed) || npcDirection == new Vector2(-1, 0))
                        {
                            location.X += npcMoveSpeed / 4 * elapsed;
                        }
                        npcDirection = new Vector2(0, 1);
                        currentAnimationFrame = "JumpRight";
                    }
                    // move left while jumping if not against a wall
                    else if (currentKeyboardState.IsKeyDown(Keys.A))
                    {
                        if (!IsAgainstWall(elapsed) || npcDirection == new Vector2(0, 1))
                        {
                            location.X -= npcMoveSpeed / 4 * elapsed;
                        }
                        npcDirection = new Vector2(-1, 0);
                        currentAnimationFrame = "JumpLeft";
                    }
                    // don't move left or right at all
                    else
                        velocity.X = 0f;
                }
            }
        }

        public bool IsOnGround(float time)
        {
            Vector2 newLoc = location - velocity * time;
            Rectangle newRect = new Rectangle((int)newLoc.X, (int)newLoc.Y, spriteSize, spriteSize);
            bool onGround = false;
            //playerMoveSpeed = setPlayerWalkSpeed; // reset move speed
            bool nextPointIsStart = false;
            foreach (var point in _mapStairs)
            {
                if (point == new Vector2(0, 0))
                {
                    nextPointIsStart = true; // set flag, indicating the next point is a starting point for stairs
                }
                else if (newRect.Contains(point))
                {
                    // playerMoveSpeed = setPlayerStairSpeed; // slow player when going down stairs
                    location.Y = point.Y - spriteSize + 5;
                    onGround = true;
                    if (nextPointIsStart) // top of stairs
                        break;
                }
                else if (nextPointIsStart)
                {
                    nextPointIsStart = false; // reset flag
                }
            }

            if (onGround == false)
            {
                foreach (var item in _mapPlatforms)
                {
                    if ((newRect.Y + spriteSize <= item.Bottom &&
                        newRect.Y + spriteSize >= item.Top && // npc is above platform
                        (int)(newRect.X) > (int)(item.Left - 50) &&
                        (int)(newRect.X + spriteSize) < (int)(item.Right + 50))) // npc is inside range of platform (range is increase because of small hitbox)
                    {
                        if (!moveUpLadder && onGround == false) // don't place player on platform if attempting to move up a ladder
                            location.Y = item.Y - spriteSize; // place player on top of platform
                        onGround = true;
                        break;
                    }
                    else
                        onGround = false;
                }
            }
            return onGround;
        }

        public bool IsAgainstWall(float time)
        {
            bool againstWall = true;
            float newLoc = 0;
            // find new location of player, if move is allowed
            if (currentAnimationFrame == "PlayerRight")
                newLoc = location.X + npcMoveSpeed * time;
            else
                newLoc = location.X - npcMoveSpeed * time;

            foreach (var item in _mapPlatforms)
            {
                if (item.Intersects(npcRect) && (npcRect.Left <= item.Right ||
                    npcRect.Right >= item.Left))
                {
                    againstWall = true;
                    break;
                }
                else
                    againstWall = false;
            }
            return againstWall;
        }

        public bool IsOnLadder()
        {
            bool onLadder = false;
            foreach (var item in _mapLadders)
            {
                if (npcRect.Intersects(item))
                {
                    if (npcRect.X + spriteSize <= item.Right + 64 &&
                        npcRect.X  >= item.Left - 64)
                    {
                        onLadder = true;
                        break;
                    }
                }
                else
                    onLadder = false;
            }
            return onLadder;
        }

        public bool IsAboveLadder()
        {
            bool aboveLadder = false;
            foreach (var item in _mapLadders)
            {
                if (npcRect.X + spriteSize <= item.Right + 50 &&
                    npcRect.X >= item.Left - 50 &&
                    (npcRect.Bottom < item.Top && npcRect.Bottom + 10 >= item.Top))
                {
                    aboveLadder = true;
                    break;
                }
                else
                    aboveLadder = false;
            }
            return aboveLadder;
        }
    
    }
}