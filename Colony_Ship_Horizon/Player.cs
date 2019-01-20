using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TiledSharp;

namespace Colony_Ship_Horizon
{
    internal class Player
    {
        // used in RandomNumber function
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        // jump physics data
        int accelerationScale;
        readonly Vector2 velocityConstant;
        readonly Vector2 accelerationConstant;
        private Vector2 velocity = new Vector2(0, 0);
        private Vector2 acceleration = new Vector2(0, 0);

        // control character animation speed
        private float timeSinceLastFrame = 0;
        private int millisecondsPerFrame = 50;

        // movement variables for the player
        private bool hasJumped;
        private bool hasFallen;
        private bool onLadder;
        private bool moveUpLadder;
        public Vector2 playerDirection;
        public bool allowMovement = true; // is the player allowed to move

        // player camera data
        public Camera2d camera;

        // character animation data
        public string currentAnimationFrame;
        private string previousAnimationFrame;

        public bool isSmoking = false;
        public bool incomingCall = false;

        private SpriteMap spriteMap; // will store current spritemap to be used
        private SpriteMap playerJumpingRight;
        private SpriteMap playerStandingRight;
        private SpriteMap playerWalkingRight;
        private SpriteMap playerAimingRight;
        private SpriteMap playerStandingAimingRight;
        private SpriteMap playerAimingBackRight;
        private SpriteMap playerSmokingRight;
        private SpriteMap playerReceivingCallRight;
        private SpriteMap playerWoundedRight;
        private SpriteMap playerSittingRight;

        private Texture2D playerSittingRightSheet;
        private Texture2D playerWoundedRightSheet;
        private Texture2D playerReceivingCallRightSheet;
        private Texture2D playerSmokingRightSheet;
        private Texture2D playerAimingBackRightSheet;
        private Texture2D playerStandingAimingRightSheet;
        private Texture2D playerAimingRightSheet;
        private Texture2D playerJumpingRightSheet;
        private Texture2D playerStandingRightSheet;
        private Texture2D playerWalkingRightSheet;
        public Rectangle PlayerRect;
        public Vector2 location;
        private int currentFrame;
        private int totalFrames;
        private int spriteSize;
        private int spriteSizeForLadders;

        //  keyboard states used to determine key presses
        private KeyboardState currentKeyboardState;

        private KeyboardState previousKeyboardState;

        //  gamepad states used to determine button presses
        private GamePadState currentGamePadState;

        private GamePadState previousGamePadState;

        //  mouse states used to track Mouse button press
        private MouseState currentMouseState;

        private MouseState previousMouseState;

        //  The rate of fire of the player laser
        public TimeSpan fireTime;
        public TimeSpan halfOfFireTime;

        private TimeSpan previousFireTime;

        // stats
        // stats of npc
        public int playerTotalHealth;
        public int playerHealth;
        private float playerMoveSpeed;
        private float setPlayerStairSpeed;
        private float setPlayerWalkSpeed;
        int walkAnimSpeed;
        public bool addCorpsePosToList = false;

        // combat data
        private Texture2D _bulletTexture;

        private Texture2D _muzzleFlash;
        private List<Projectile> projectiles;
        private Vector2 projectileDirection;
        private Vector2 _cursorWorldPosition;
        private bool drawgun;
        private bool aimingLeft = false;
        private bool drawLazer;
        public bool combatMode;
        private Texture2D lazer;
        private Texture2D gun;
        private Vector2 gunPos;
        public Gun currentlyEquippedGun;
        private Gun previouslyEquippedGun = new Gun();
        public int currentGunAmmoClips = 0;
        public bool useClip;
        private int gunDamage;
        private int gunForce;
        private float bulletSpeed;
        private int bulletCount;
        private float bulletSpread;
        public bool isGunAutomatic;
        private int bulletSize = 64;
        private float bulletScale = 1f;
        private float gunReloadSpeed;
        public int gunClipSize;
        private int gunPenetrationPower;
        private Vector2 gunArmOffset = new Vector2(0, 0);
        public bool roomImpactEffects;
        public bool npcImpactEffects;
        public Vector2 projectilePos;
        public List<NPC> _npcList = new List<NPC>();
        public Dictionary<string, Animator> _npcDeathAnimList = new Dictionary<string, Animator>();
        public Rectangle npcDeathLocation = new Rectangle();
        public bool isFiring;
        public bool isWeaponCocked = true;
        public bool lookAtPlayer = true;

        private List<Rectangle> _mapPlatforms;
        private Dictionary<string, Rectangle> _mapObjects;
        private List<Vector2> _mapStairs;
        Vector2 stairDirection; // when using stairs, this is the direction to go up or down
        private List<Rectangle> _mapLadders;
        private List<Rectangle> _mapDoors;
        private List<Trigger> _mapTriggers;
        public List<Rectangle> activeDoorTrigger = new List<Rectangle>();
        public bool isBuildMode = false;
        public Rectangle interactionRectangle;
        public bool interactionFlag;

        /// <summary>
        /// / Player Constructor with Camera and Player Data Initialization
        /// </summary>
        public Player(GraphicsDevice graphics, Vector2 cursorWorldPosition, List<Rectangle> mapPlatforms,
            Dictionary<string, Rectangle> mapObjects, List<Vector2> mapStairs,
            List<Rectangle> mapLadders, List<Rectangle> mapDoors, PlayerType playerType)
        {
            _mapDoors = mapDoors;
            _mapLadders = mapLadders;
            _mapStairs = mapStairs;
            _mapObjects = mapObjects;
            _mapPlatforms = mapPlatforms;
            _cursorWorldPosition = cursorWorldPosition;

            // load the player type
            playerJumpingRightSheet = playerType.playerJumpingRightSheet;
            playerJumpingRight = playerType.playerJumpingRight;
            playerWalkingRightSheet = playerType.playerWalkingRightSheet;
            playerWalkingRight = playerType.playerWalkingRight;
            playerStandingRightSheet = playerType.playerStandingRightSheet;
            playerStandingRight = playerType.playerStandingRight;
            playerAimingRightSheet = playerType.playerAimingRightSheet;
            playerAimingRight = playerType.playerAimingRight;
            playerAimingBackRightSheet = playerType.playerAimingBackRightSheet;
            playerAimingBackRight = playerType.playerAimingBackRight;
            playerStandingAimingRightSheet = playerType.playerStandingAimingRightSheet;
            playerStandingAimingRight = playerType.playerStandingAimingRight;
            playerSmokingRightSheet = playerType.playerSmokingRightSheet;
            playerSmokingRight = playerType.playerSmokingRight;
            playerReceivingCallRightSheet = playerType.playerReceivingCallRightSheet;
            playerReceivingCallRight = playerType.playerReceivingCallRight;
            playerWoundedRightSheet = playerType.playerWoundedRightSheet;
            playerWoundedRight = playerType.playerWoundedRight;
            playerSittingRightSheet = playerType.playerSittingRightSheet;
            playerSittingRight = playerType.playerSittingRight;
            lazer = playerType.lazer;

            // camera init.
            camera = new Camera2d(graphics.Viewport);
            camera.Zoom = 2.15f; // larger numbers zoom in, smaller numbers zoom out

            // player init.

            string playerSpawnPoint = "Player_Spawn1";
            //int randomNumber = RandomNumber(0, 3);
            //switch (randomNumber)
            //{
            //    case 0:
            //        {
            //            playerSpawnPoint = playerSpawnPoint.Insert(playerSpawnPoint.Length, "1");
            //            break;
            //        }
            //    case 1:
            //        {
            //            playerSpawnPoint = playerSpawnPoint.Insert(playerSpawnPoint.Length, "2");
            //            break;
            //        }
            //    case 2:
            //        {
            //            playerSpawnPoint = playerSpawnPoint.Insert(playerSpawnPoint.Length, "3");
            //            break;
            //        }
            //}
            location = new Vector2(_mapObjects[playerSpawnPoint].Location.X,
               _mapObjects[playerSpawnPoint].Location.Y);
            playerDirection = new Vector2(0, 1);
            currentAnimationFrame = "StandingRight";

            setPlayerWalkSpeed = 350f;
            setPlayerStairSpeed = 275f;
            walkAnimSpeed = 75;
            playerMoveSpeed = setPlayerWalkSpeed;

            spriteSize = 96;
            spriteSizeForLadders = 64;

            PlayerRect = new Rectangle((int)location.X, (int)location.Y, spriteSize, spriteSize);
            // used to reset player's health
            playerTotalHealth = 100;
            playerHealth = playerTotalHealth;
            // set the projectile fire time
            isFiring = false;
            isWeaponCocked = true;
            // jump physics flags
            hasJumped = false;
            hasFallen = false;
            onLadder = false;

            // gravity init.
            accelerationScale = 3;
            velocityConstant.Y = 1000;
            accelerationConstant = new Vector2(0, -4000 * accelerationScale);

            velocity = velocityConstant;
            acceleration = accelerationConstant;
        }

        public void Initialize()
        {
            projectiles = new List<Projectile>();
            //  set the fire time
            fireTime = TimeSpan.FromSeconds(.2f);
            halfOfFireTime = TimeSpan.FromSeconds(.1f);
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

        public void Update(GameTime gameTime, List<NPC> npcList, bool isNotAllowedToFire, bool isNotAllowedToFire2)
        {
            _npcList = npcList;
            // add new npc death animations to list, to be drawn when player kills an npc
            for (int i = 0; i < npcList.Count; i++)
            {
                if (!_npcDeathAnimList.ContainsKey(npcList[i]._name))
                {
                    _npcDeathAnimList.Add(_npcList[i]._name, _npcList[i].npcDeathAnim);
                }
            }
            // respawn player
            //if (currentKeyboardState.IsKeyDown(Keys.RightControl) || playerHealth <= 0)
            //{
            //    // reset player health
            //    playerHealth = playerTotalHealth;
            //    // get spawn point from map
            //    location = new Vector2(_mapObjects["Player_Spawn"].Location.X,
            //    _mapObjects["Player_Spawn"].Location.Y);
            //    // spawn player at point
            //    hasFallen = false;
            //    PlayerRect = new Rectangle((int)location.X, (int)location.Y, spriteSize, spriteSize);
            //}

            // call movement controls method for player
            Move(gameTime);
            Jump(gameTime);

            // find total number of frames for current sprite map
            if (spriteMap != null)
                totalFrames = spriteMap.TextureList.Count;
            else
                totalFrames = 0;

            // set framerate of background animation
            timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                currentFrame++;
                timeSinceLastFrame -= millisecondsPerFrame;
                timeSinceLastFrame = 0;
                // if at last frame in sprite sheet, start at first frame or deactivate instance if applicable
                if (currentFrame >= totalFrames)
                {
                    currentFrame = 0;
                }

                // move the gun arm with the player's gait when walking and aiming
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
            // stop firing weapon
            if (previousMouseState.LeftButton == ButtonState.Released || currentMouseState.LeftButton == ButtonState.Released)
                isFiring = false;
            // player is equipping a weapon
            if (currentMouseState.RightButton == ButtonState.Pressed && !combatMode && !isBuildMode)
            {
                combatMode = true;
                drawLazer = true;
                drawgun = true;
                if (currentAnimationFrame.Contains("Right"))
                    currentAnimationFrame = "StandingAimingRight";
                else
                    currentAnimationFrame = "StandingAimingLeft";
            }
            else if (currentMouseState.RightButton == ButtonState.Released && combatMode)
            {
                // closing combat mode, reset flags
                combatMode = false;
                drawLazer = false;
                drawgun = false;

                // reload gun textures when entering combat mode
                if (combatMode)
                {
                    foreach (Projectile projectile in projectiles)
                    {
                        if (projectile.Texture.Name.Contains("gun"))
                        {
                            projectile.UpdateWeapon(currentlyEquippedGun._gunTexture, currentlyEquippedGun._damage, currentlyEquippedGun._bulletSpeed);
                        }
                    }
                }
                else
                    gun = currentlyEquippedGun._gunTexture;

                // not in combat mode, clear the list of lazer and gun
                for (int i = projectiles.Count - 1; i >= 0; i--)
                {
                    if (projectiles[i].Texture.Name.Contains("lazer"))
                    {
                        projectiles.RemoveAt(i);
                    }
                    else if (projectiles[i].Texture.Name.Contains("gun"))
                    {
                        projectiles.RemoveAt(i);
                    }
                }
                if (currentAnimationFrame.Contains("Right"))
                    currentAnimationFrame = "StandingRight";
                else
                    currentAnimationFrame = "StandingLeft";
            }
            // if player allowed to fire, and is trying to fire a projectile, add a projectile to list
            else if (!isNotAllowedToFire && !isNotAllowedToFire2 && combatMode)
            {
                // the weapon is automatic
                if (currentMouseState.LeftButton == ButtonState.Pressed && isGunAutomatic)
                {
                    //  fire only every interval we set as the fireTime
                    if (gameTime.TotalGameTime - previousFireTime > fireTime)
                    {
                        isFiring = true;
                        // reset the current time
                        previousFireTime = gameTime.TotalGameTime;
                        //  add the projectile
                        AddProjectile(new Vector2(location.X + spriteSize, location.Y + 16));
                    }
                    else
                    {
                        isFiring = false;
                    }
                }
                // the weapon is semi-automatic and firing
                else if (IsMousePressed("LeftButton") && !isGunAutomatic)
                {
                    // only the shotgun needs cocked
                    if (!currentlyEquippedGun._gunType.ToLower().Contains("shotgun"))
                        isWeaponCocked = true;

                    //  fire only every interval we set as the fireTime
                    if (gameTime.TotalGameTime - previousFireTime > fireTime && isWeaponCocked)
                    {
                        // if a shotgun's ammo is full, it doesn't need cocked
                        if (currentlyEquippedGun._gunType.ToLower().Contains("shotgun") && currentlyEquippedGun._bulletsInClip == gunClipSize)
                            isWeaponCocked = true;
                        else
                            isWeaponCocked = false;

                        isFiring = true;
                        // reset the current time
                        previousFireTime = gameTime.TotalGameTime;
                        //  add the projectile
                        AddProjectile(new Vector2(location.X + spriteSize, location.Y + 16));
                    }
                    else if (!isWeaponCocked)
                    {
                        isFiring = false;
                        isWeaponCocked = true;
                    }
                }
                // draw lazer, gun and arm
                else
                {
                    // only draw one lazer projectile
                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        if (projectiles[i].Texture.Name.Contains("lazer") && drawLazer)
                        {
                            drawLazer = false;
                        }
                        else if (projectiles[i].Texture.Name.Contains("gun") && drawgun)
                        {
                            drawgun = false;
                        }
                    }
                    //  add the gun and lazer
                    if (drawLazer)
                        AddProjectile(new Vector2(location.X + spriteSize, location.Y + 16));
                    if (drawgun)
                        AddProjectile(new Vector2(location.X + spriteSize, location.Y + 16));
                }
            }
            else // reset all flags
            {
                isFiring = false;
                combatMode = false;
                drawgun = false;
                drawLazer = false;
            }
            //  update the projectiles
            UpdateCollision();
            UpdateProjectiles(gameTime);

            foreach (Animator deathAnim in _npcDeathAnimList.Values)
            {
                deathAnim.Update(gameTime);
            }

            //  lock camera onto player
            if (lookAtPlayer)
                camera.LookAt(location);
            //  camera zoom in/zoom out, if not aiming weapon
            if (!currentAnimationFrame.Contains("aim") && !currentAnimationFrame.Contains("Aim"))
                CameraZoomer();


            foreach (KeyValuePair<string, Rectangle> obj in _mapObjects)
            {
                if (obj.Key == "Seat" && PlayerRect.Intersects(obj.Value))
                {
                    interactionFlag = true;
                    interactionRectangle = obj.Value;
                }
                else
                {
                    interactionFlag = false;
                    interactionRectangle = Rectangle.Empty;
                }
            }

            // player is smoking, stop smoking when moving or aiming
            if (isSmoking && !((previousKeyboardState.IsKeyDown(Keys.A) || previousKeyboardState.IsKeyDown(Keys.D) ||
               previousKeyboardState.IsKeyDown(Keys.W) || previousKeyboardState.IsKeyDown(Keys.S) || currentMouseState.RightButton == ButtonState.Pressed ||
               previousKeyboardState.IsKeyDown(Keys.Space))))
            {
                if (currentAnimationFrame.Contains("Left"))
                    currentAnimationFrame = "SmokingLeft";
                else if (currentAnimationFrame.Contains("Right"))
                    currentAnimationFrame = "SmokingRight";
            }
            else if (isSmoking)
                isSmoking = false;

            // player is receiving call
            if (incomingCall && !((previousKeyboardState.IsKeyDown(Keys.A) || previousKeyboardState.IsKeyDown(Keys.D) ||
               previousKeyboardState.IsKeyDown(Keys.W) || previousKeyboardState.IsKeyDown(Keys.S) || currentMouseState.RightButton == ButtonState.Pressed)))
            {
                if (currentAnimationFrame.Contains("Left"))
                    currentAnimationFrame = "ReceivingCallLeft";
                else if (currentAnimationFrame.Contains("Right"))
                    currentAnimationFrame = "ReceivingCallRight";
            }

            // sit player in seat
            if (((IsKeyPressed(Keys.E) && !interactionRectangle.IsEmpty) ||
                (currentAnimationFrame == "SittingLeft" || currentAnimationFrame == "SittingRight")) &&
                (!IsKeyPressed(Keys.A) && !IsKeyPressed(Keys.D) &&
               !IsKeyPressed(Keys.W) && !IsKeyPressed(Keys.S) && !IsMousePressed("RMB")))
            {
                interactionFlag = false;
                location = interactionRectangle.Location.ToVector2();
                //if (currentAnimationFrame.Contains("Left"))
                //    currentAnimationFrame = "SittingLeft";
                //else if (currentAnimationFrame.Contains("Right"))
                currentAnimationFrame = "SittingRight";
            }

            // player is wounded when he has 50% total health
            bool isWounded = false;
            if (playerHealth + playerTotalHealth / 2 < playerTotalHealth)
                isWounded = true;
            if (isWounded && !((previousKeyboardState.IsKeyDown(Keys.A) || previousKeyboardState.IsKeyDown(Keys.D) ||
               previousKeyboardState.IsKeyDown(Keys.W) || previousKeyboardState.IsKeyDown(Keys.S) || currentMouseState.RightButton == ButtonState.Pressed)))
            {
                if (currentAnimationFrame.Contains("Left"))
                    currentAnimationFrame = "WoundedLeft";
                else if (currentAnimationFrame.Contains("Right"))
                    currentAnimationFrame = "WoundedRight";
            }

            // player is reload their weapon
            if (IsKeyPressed(Keys.R))
                ReloadWeapon();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draws current frame from sprite sheet
            DrawCurrentFrame(spriteBatch);
        }

        public void DrawCurrentFrame(SpriteBatch spriteBatch)
        {
            //walkAnimSpeed = 75; //--set in isOnGround()
            int standSpeed = 225;
            int smokeSpeed = 300;
            int callSpeed = 150;
            int woundedSpeed = 150;
            int sittingSpeed = 150;

            Texture2D texture;
            SpriteEffects effect;
            switch (currentAnimationFrame)
            {
                case "SittingLeft":
                    {
                        spriteMap = playerSittingRight;
                        texture = playerSittingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = sittingSpeed;
                        break;
                    }
                case "SittingRight":
                    {
                        spriteMap = playerSittingRight;
                        texture = playerSittingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = sittingSpeed;
                        break;
                    }
                case "WoundedLeft":
                    {
                        spriteMap = playerWoundedRight;
                        texture = playerWoundedRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = woundedSpeed;
                        break;
                    }
                case "WoundedRight":
                    {
                        spriteMap = playerWoundedRight;
                        texture = playerWoundedRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = woundedSpeed;
                        break;
                    }
                case "ReceivingCallLeft":
                    {
                        spriteMap = playerReceivingCallRight;
                        texture = playerReceivingCallRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = callSpeed;
                        break;
                    }
                case "ReceivingCallRight":
                    {
                        spriteMap = playerReceivingCallRight;
                        texture = playerReceivingCallRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = callSpeed;
                        break;
                    }
                case "SmokingLeft":
                    {
                        spriteMap = playerSmokingRight;
                        texture = playerSmokingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = smokeSpeed;
                        break;
                    }
                case "SmokingRight":
                    {
                        spriteMap = playerSmokingRight;
                        texture = playerSmokingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = smokeSpeed;
                        break;
                    }
                case "AimingBackLeft":
                    {
                        spriteMap = playerAimingBackRight;
                        texture = playerAimingBackRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "AimingBackRight":
                    {
                        spriteMap = playerAimingBackRight;
                        texture = playerAimingBackRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "StandingAimingRight":
                    {
                        spriteMap = playerStandingAimingRight;
                        texture = playerStandingAimingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "StandingAimingLeft":
                    {
                        spriteMap = playerStandingAimingRight;
                        texture = playerStandingAimingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "AimingRight":
                    {
                        spriteMap = playerAimingRight;
                        texture = playerAimingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "AimingLeft":
                    {
                        spriteMap = playerAimingRight;
                        texture = playerAimingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "PlayerRight":
                    {
                        spriteMap = playerWalkingRight;
                        texture = playerWalkingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "PlayerLeft":
                    {
                        spriteMap = playerWalkingRight;
                        texture = playerWalkingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "StandingLeft":
                    {
                        spriteMap = playerStandingRight;
                        texture = playerStandingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "StandingRight":
                    {
                        spriteMap = playerStandingRight;
                        texture = playerStandingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = standSpeed;
                        break;
                    }
                case "JumpLeft":
                    {
                        spriteMap = playerJumpingRight;
                        texture = playerJumpingRightSheet;
                        effect = SpriteEffects.FlipHorizontally;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                case "JumpRight":
                    {
                        spriteMap = playerJumpingRight;
                        texture = playerJumpingRightSheet;
                        effect = SpriteEffects.None;
                        millisecondsPerFrame = walkAnimSpeed;
                        break;
                    }
                default:
                    {
                        spriteMap = playerWalkingRight;
                        texture = playerWalkingRightSheet;
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
                PlayerRect = new Rectangle((int)location.X, (int)location.Y, spriteSize, spriteSize);
                spriteBatch.Draw(texture, PlayerRect, sourceRect, Color.White, 0f, new Vector2(0, 0), effect, 0);
            }
            // store previous animation frame
            previousAnimationFrame = currentAnimationFrame;

            foreach (Animator deathAnim in _npcDeathAnimList.Values)
            {
                deathAnim.Draw(spriteBatch);
            }
        }

        public void DrawProjectile(SpriteBatch spriteBatch)
        {
            //  gun and lazer in beginning of list, draw from end of list to draw them above projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Draw(spriteBatch);
            }
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile(); 
            // find distance between player and last point of impact of a player's projectile
            float distance = Vector2.Distance(projectilePos, location);
            // calculate direction needed for projectile to hit cursor position,
            // then add camera position to translate from screen to world coordinates
            projectileDirection.X = currentMouseState.Position.X - location.X + camera.Position.X;
            projectileDirection.Y = currentMouseState.Position.Y - location.Y + camera.Position.Y;
            // don't divide by zero and normalize the vector(convert to unit vector)
            if (projectileDirection != Vector2.Zero)
                projectileDirection.Normalize();

            // don't fire if clip is empty
            if (currentlyEquippedGun._bulletsInClip > 0 && isFiring)
            {
                // remove bullet from gun's clip
                currentlyEquippedGun._bulletsInClip--;
                // if weapon needs cocked after firing, flag it
                if (currentlyEquippedGun._gunType.ToLower().Contains("shotgun"))
                    isWeaponCocked = false;

                for (int i = 0; i <= bulletCount; i++)
                {
                    // get random x value in range to adjust each projectile's direction, creating a bullet spread
                    double val = random.NextDouble(); // range 0.0 to 1.0
                    val -= .5; // range -.5 to .5
                    val *= 2; // range -1 to 1
                    val /= bulletSpread; // in set bullet spread range (bulletSpread=1 is least inaccurate, bulletSpread=200 is most accurate)
                    float x = (float)val;

                    // get random y value in range
                    val = random.NextDouble();
                    val -= .5;
                    val *= 2;
                    val /= bulletSpread;
                    float y = (float)val;

                    Vector2 spreadDirection = new Vector2(projectileDirection.X + x, projectileDirection.Y + y);
                    Projectile projectileSpread = new Projectile();

                    // first shot out of gun is treated differently
                    if (i == 0)
                    {
                        // if a single bullet count, spread the bullet and flash
                        //if (bulletCount < 2)
                        //    projectileSpread.Initialize(_bulletTexture, _muzzleFlash, position, spreadDirection, gunDamage, gunForce,
                        //        bulletSpeed, gunPos, bulletSize, bulletScale, true);
                        //// if firing multiple bullets (bulletCount>0) then dont spread the bullet or flash
                        //else
                        projectileSpread.Initialize(_bulletTexture, _muzzleFlash, position, projectileDirection, gunDamage, gunForce,
                            bulletSpeed, gunPos, bulletSize, bulletScale, true);
                        projectiles.Add(projectileSpread);
                    }
                    // create all other bullets with spread and without muzzle flash
                    else
                    {
                        projectileSpread.Initialize(_bulletTexture, _muzzleFlash, position, spreadDirection, gunDamage, gunForce,
                            bulletSpeed, gunPos, bulletSize, bulletScale, false);
                        projectiles.Add(projectileSpread);
                    }
                }
            } // end if clip size > 0

            // gun's clip is empty
            else if (currentlyEquippedGun._bulletsInClip == 0)
            {
                currentlyEquippedGun._bulletsInClip = -1;
            }

            if (drawLazer)// draw the lazer
            {
                projectile.Initialize(lazer, _muzzleFlash, position, projectileDirection, 0, 0, 0, gunPos, 64, .5f);
                projectiles.Add(projectile);
            }
            else if (drawgun)// draw the gun
            {
                projectile.Initialize(gun, _muzzleFlash, position, projectileDirection, 0, 0, 0, gunPos, 64, 1f);
                projectiles.Add(projectile);
            }
        }

        public void SetAmmo(int totalClips)
        {
            currentGunAmmoClips = totalClips;
        }

        public void UseItem()
        {

        }

        public void ReloadWeapon()
        {
            // attempt to use ammo to reload weapon
            if (currentGunAmmoClips > 0)
            {
                // flag to notify main method to reload
                useClip = true;
            }
            // do not allow reload
            else
            {
                useClip = false;
            }

            // weapon doesn't need cocked
            if (currentlyEquippedGun._gunType.ToLower().Contains("shotgun"))
                isWeaponCocked = true;
        }

        public void SwitchWeapon(Gun gunToEquip)
        {
            // gun was switched
            if (!gunToEquip.Equals(currentlyEquippedGun))
            {
                previouslyEquippedGun = currentlyEquippedGun;
                currentlyEquippedGun = gunToEquip;
                // do not unintentionally reload when switching guns
                useClip = false;
            }

            _bulletTexture = gunToEquip._bulletTexture;
            _muzzleFlash = gunToEquip._muzzleFlash;
            isGunAutomatic = gunToEquip._isAutomatic;
            fireTime = gunToEquip._rateOfFire;
            halfOfFireTime = new TimeSpan(fireTime.Milliseconds/2);
            gunDamage = gunToEquip._damage;
            gunForce = gunToEquip._force;
            bulletSpeed = gunToEquip._bulletSpeed;
            bulletSize = gunToEquip._bulletSize;
            bulletScale = gunToEquip._bulletScale;
            bulletSpread = gunToEquip._bulletSpread;
            bulletCount = gunToEquip._bulletCount;
            gunReloadSpeed = gunToEquip._reloadSpeed;
            gunClipSize = gunToEquip._clipSize;
            gunPenetrationPower = gunToEquip._penetrationPower;

            // when player is switching weapons, find the gun texture in the list and load it to draw correct gun
            if (combatMode)
            {
                foreach (Projectile projectile in projectiles)
                {
                    if (projectile.Texture.Name.Contains("gun"))
                    {
                        projectile.UpdateWeapon(gunToEquip._gunTexture, gunToEquip._damage, gunToEquip._bulletSpeed);
                    }
                }
            }
            else
                gun = gunToEquip._gunTexture;
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            //  Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update(gameTime, projectileDirection, location, gunPos, gunArmOffset, currentAnimationFrame);
                
                // will update projectile position to impact position
                if (projectiles[i].Texture.Name.Contains("lazer")
                        || projectiles[i].Texture.Name.Contains("gun"))
                {
                    aimingLeft = projectiles[i].aimingLeft;
                    gunPos = projectiles[i].Position;
                    projectileDirection.X = currentMouseState.Position.X - location.X + camera.Position.X;
                    projectileDirection.Y = currentMouseState.Position.Y - location.Y + camera.Position.Y;
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

        /// <summary>
        /// Projectile collision with platforms and NPCs
        /// </summary>
        private void UpdateCollision()
        {
            Rectangle projectileRect;
            roomImpactEffects = false;
            npcImpactEffects = false;
            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                // Create the rectangles needed to determine if there was a collision
                projectileRect = new Rectangle((int)projectiles[i].Position.X, (int)projectiles[i].Position.Y,
                    projectiles[i].rectangleSize, projectiles[i].rectangleSize);
                for (int k = 0; k < _npcList.Count; k++)
                {
                    // npc was hit, subtract from npc health and 'push' it in the direction of the projectile 
                    if (_npcList[k].npcHitBox.Intersects(projectileRect) && projectiles[i].Active && !projectiles[i].Texture.Name.Contains("lazer")
                        && !projectiles[i].Texture.Name.Contains("gun"))
                    {
                        _npcList[k].npcHealth -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                        projectilePos = new Vector2(projectiles[i].Position.X, projectiles[i].Position.Y);
                        npcImpactEffects = true;

                        /// push the npc in the projectile's direction by the force of the projectile
                        //if (projectiles[i].Direction.X < 0)
                        //{
                        //    _npcList[k].location.X -= 10 * projectiles[i].Force;
                        //    for (int j = 0; j < _mapPlatforms.Count; j++)
                        //    {
                        //        // don't push the npc into a wall
                        //        if (_mapPlatforms[j].Contains(new Point(_npcList[k].npcRect.Left, _npcList[k].npcRect.Y)))
                        //        {
                        //            _npcList[k].location.X += 10 * projectiles[i].Force;
                        //        }
                        //    }
                        //}
                        //else if (projectiles[i].Direction.Y > 0)
                        //{
                        //    _npcList[k].location.X += 10 * projectiles[i].Force;
                        //    for (int j = 0; j < _mapPlatforms.Count; j++)
                        //    {
                        //        // don't push the npc into a wall
                        //        if (_mapPlatforms[j].Contains(new Point(_npcList[k].npcRect.Left, _npcList[k].npcRect.Y)))
                        //        {
                        //            _npcList[k].location.X -= 10 * projectiles[i].Force;
                        //        }
                        //    }
                        //}
                    }
                    // npc has no health left, remove them from list
                    if (_npcList[k].npcHealth <= 0)
                    {
                        int deathY = (int)_npcList[k].location.Y;
                        float deathScale = .5f;
                        switch (_npcList[k].spriteSize)
                        {
                            case 48:
                                {
                                    deathY -= 16;
                                    deathScale = .375f;
                                    break;
                                }
                            case 64:
                                {
                                    deathScale = .5f;
                                    break;
                                }
                            case 80:
                                {
                                    deathY += 16;
                                    deathScale = .625f;
                                    break;
                                }
                            case 96:
                                {
                                    deathY += 48;
                                    deathScale = 1f;
                                    break;
                                }
                        }
                        // play death animation
                        foreach (string deathAnimKey in _npcDeathAnimList.Keys)
                        {
                            if (deathAnimKey == _npcList[k]._name)
                                _npcDeathAnimList[deathAnimKey].AddAnimation(new Vector2((int)_npcList[k].location.X, deathY), 
                                    _npcList[k].npcColor, deathScale);
                        }
                        addCorpsePosToList = true;
                        // drop npc items
                        npcDeathLocation = _npcList[k].npcRect;
                        _npcList.RemoveAt(k);
                    }
                }
                // remove when projectile collides with platforms
                for (int k = 0; k < _mapPlatforms.Count; k++)
                {
                    if ((_mapPlatforms[k].Contains(projectileRect)) && projectiles[i].Active)
                    {
                        projectiles[i].Active = false;
                        projectilePos = new Vector2(projectiles[i].Position.X, projectiles[i].Position.Y);
                        roomImpactEffects = true;
                        break;
                    }
                }
                // remove when projectile collides with walls
                for (int k = 0; k < _mapPlatforms.Count; k++)
                {
                    if ((_mapPlatforms[k].Contains(projectileRect)) && projectiles[i].Active)
                    {
                        projectiles[i].Active = false;
                        projectilePos = new Vector2(projectiles[i].Position.X, projectiles[i].Position.Y);
                        roomImpactEffects = true;
                    }
                    break;
                }
                // collides with closed doors, not open doors
                //for (int k = 0; k < _mapDoors.Count; k++)
                //{
                //    bool doorIsActive = false;
                //    for (int j = 0; j < activeDoorTrigger.Count; j++)
                //    {
                //        if (activeDoorTrigger[j].Contains(_mapDoors[k]))
                //            doorIsActive = true;
                //        break;
                //    }
                //    if (_mapDoors[k].Contains(projectileRect) && projectiles[i].Active && !doorIsActive)
                //    {
                //        projectiles[i].Active = false;
                //        projectilePos = new Vector2(projectiles[i].Position.X, projectiles[i].Position.Y);
                //        roomImpactEffects = true;
                //        break;
                //    }
                //}
            }
        }

        public void Move(GameTime gameTime)
        {
            // time since the last frame is used to keep player movement smooth, in case of framerate drop
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // reset moveUpLadder flag
            moveUpLadder = false;

            // if player is aiming one direction but walking in the opposite direction
            if ((!aimingLeft || currentAnimationFrame == "AimingLeft") && currentKeyboardState.IsKeyDown(Keys.A) && combatMode && allowMovement)
                currentAnimationFrame = "AimingBackRight";
            else if ((aimingLeft || currentAnimationFrame == "AimingRight") && currentKeyboardState.IsKeyDown(Keys.D) && combatMode && allowMovement)
                currentAnimationFrame = "AimingBackLeft";

            if (currentKeyboardState.IsKeyDown(Keys.A) && !moveUpLadder && allowMovement)// move player left
            {
                if (!combatMode)
                    currentAnimationFrame = "PlayerLeft";
                else if (aimingLeft)
                    currentAnimationFrame = "AimingLeft";

                if (!IsAgainstWall(elapsed) || playerDirection == new Vector2(0, 1))
                {
                    location.X -= playerMoveSpeed * elapsed;
                }
                playerDirection = new Vector2(-1, 0);

                MoveToNextArea();
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D) && !moveUpLadder && allowMovement)// move player right
            {
                if (!combatMode)
                    currentAnimationFrame = "PlayerRight";
                else if (!aimingLeft)
                    currentAnimationFrame = "AimingRight";

                if (!IsAgainstWall(elapsed) || playerDirection == new Vector2(-1, 0))
                {
                    location.X += playerMoveSpeed * elapsed;
                }
                playerDirection = new Vector2(0, 1);

                MoveToNextArea();
            }
            else if (currentKeyboardState.IsKeyDown(Keys.W) && (IsOnLadder()) && allowMovement)// move player up ladder if on ladder
            {
                moveUpLadder = true;
                if (combatMode && !aimingLeft)
                    currentAnimationFrame = "AimingRight";
                else if (combatMode && aimingLeft)
                    currentAnimationFrame = "AimingLeft";
                else
                    currentAnimationFrame = "PlayerRight";
                location.Y -= playerMoveSpeed * elapsed;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.S) && (IsOnLadder() || IsAboveLadder()) && allowMovement)// move player down ladder if on or above ladder
            {
                if (combatMode && !aimingLeft)
                    currentAnimationFrame = "AimingRight";
                else if (combatMode && aimingLeft)
                    currentAnimationFrame = "AimingLeft";
                else
                currentAnimationFrame = "PlayerRight";
                if (!IsOnGround(elapsed))
                    location.Y += playerMoveSpeed * elapsed;
            }

            // if player isnt moving, play stationary animations accordingly
            else
            {
                // if playing was moving left, play StandingLeft movement animation
                if (currentAnimationFrame == "PlayerLeft" && !combatMode)
                    currentAnimationFrame = "StandingLeft";
                // if playing was moving right, play StandingRight movement animation
                else if (currentAnimationFrame == "PlayerRight" && !combatMode)
                    currentAnimationFrame = "StandingRight";
                else if ((!aimingLeft || currentAnimationFrame == "AimingRight") && !IsKeyPressed(Keys.D) && combatMode)
                    currentAnimationFrame = "StandingAimingRight";
                else if ((aimingLeft || currentAnimationFrame == "AimingLeft") && !IsKeyPressed(Keys.A) && combatMode)
                    currentAnimationFrame = "StandingAimingLeft";
            }

            // save previous state
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;
            // update current state
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            // // // currentGamePadState = GamePad.GetState(PlayerIndex); ------need playerindex
        }

        /// <summary>
        /// When a player is moving towards and colliding with a transition object, move to the next area
        /// </summary>
        public void MoveToNextArea()
        {
            foreach (KeyValuePair<string, Rectangle> entry in _mapObjects)
            {
                // player is intersecting the left side of a transition object, move to the right
                if (entry.Key.Contains("To") && PlayerRect.Intersects(entry.Value) &&
                    PlayerRect.Right >= entry.Value.Left && PlayerRect.Left < entry.Value.Left)
                {
                    location = new Vector2(entry.Value.Right + spriteSize, entry.Value.Top);
                    break;
                }
                // player is intersecting the right side of a transition object, move to the left
                else if (entry.Key.Contains("To") && PlayerRect.Intersects(entry.Value) &&
                    PlayerRect.Left <= entry.Value.Right && PlayerRect.Right > entry.Value.Right)
                {
                    location = new Vector2(entry.Value.Left - spriteSize, entry.Value.Top);
                    break;
                }
            }
        }

        //public bool IsOnStairs(float elapsed)
        //{
        //    /////
        //    /////////// find direction to mouse and launch player towards it, special ability
        //    /////
        //    bool isOnStairs = false;
        //    for (int i = 0; i < _mapStairs.Count; i++)
        //    {
        //        if (PlayerRect.Intersects(_mapStairs[i]))
        //        {
        //            if (stairDirection == new Vector2(0, 0)) // find direction to end of stair
        //            {
        //                if (PlayerRect.Bottom <= _mapStairs[i].Top + 33) // player is at top of stair
        //                {
        //                    if (PlayerRect.Right <= _mapStairs[i].Left + 31)
        //                        stairDirection = new Vector2(_mapStairs[i].Bottom - _mapStairs[i].Top,
        //                            _mapStairs[i].Right - _mapStairs[i].Left);
        //                    else if (PlayerRect.Left >= _mapStairs[i].Right - 31)
        //                        stairDirection = new Vector2(_mapStairs[i].Bottom - _mapStairs[i].Top,
        //                            _mapStairs[i].Left - _mapStairs[i].Right);

        //                    isOnStairs = true;
        //                    break;
        //                }
        //                else if (PlayerRect.Bottom <= _mapStairs[i].Bottom + 33) // player is at bottom of stair
        //                {
        //                    if (PlayerRect.Right <= _mapStairs[i].Left + 31)
        //                        stairDirection = new Vector2(_mapStairs[i].Top - _mapStairs[i].Bottom,
        //                            _mapStairs[i].Right - _mapStairs[i].Left);
        //                    else if (PlayerRect.Left >= _mapStairs[i].Right - 31)
        //                        stairDirection = new Vector2(_mapStairs[i].Top - _mapStairs[i].Bottom,
        //                            _mapStairs[i].Left - _mapStairs[i].Right);

        //                    isOnStairs = true;
        //                    break;
        //                }
        //                else isOnStairs = false;
        //            }
        //            else // move along stair
        //            {
        //                if (stairDirection.X > 0 && playerDirection.X < 0)
        //                    stairDirection = new Vector2(0, 0);
        //                else if (stairDirection.X < 0 && playerDirection.Y > 0)
        //                    stairDirection = new Vector2(0, 0);
        //                else
        //                    location += (stairDirection) * elapsed;
        //            }
        //        }
        //    }
        //    return isOnStairs;
        //}

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
            //{
            //    velocity.Y = velocityConstant.Y;
            //    acceleration = accelerationConstant;
            //    location -= velocity * elapsed;
            //    hasJumped = true;
            //    if (playerDirection == new Vector2(0, 1))
            //        currentAnimationFrame = "JumpRight";
            //    else
            //        currentAnimationFrame = "JumpLeft";
            //}
            // player is on ladder, do nothing

            //else if (IsOnLadder())
            if (IsOnLadder())
            {
                onLadder = true;
                hasFallen = false;
                hasJumped = false;
                // ladder climbing animation
            }
            // player is falling
            else if (!IsOnGround(elapsed) && hasJumped == false && hasFallen == false && !IsAboveLadder())
            {
                velocity.Y = 0;
                hasFallen = true;
                if (playerDirection == new Vector2(0, 1) && !combatMode)
                    currentAnimationFrame = "JumpRight";
                else if (!combatMode)
                    currentAnimationFrame = "JumpLeft";
            }
            // player is above ladder, do nothing
            else if (IsAboveLadder())
            {
                hasFallen = false;
                onLadder = false;
                hasJumped = false;
                if (currentAnimationFrame.Contains("Jump"))
                {
                    if (playerDirection == new Vector2(0, 1) && !combatMode)
                        currentAnimationFrame = "StandingRight";
                    else if (!combatMode)
                        currentAnimationFrame = "StandingLeft";
                }
            }
            // implement gravity
            else
            {
                // stop gravity when player reaches floor
                if (IsOnGround(elapsed))
                {
                    // reset flags
                    moveUpLadder = false;
                    hasJumped = false;
                    hasFallen = false;
                    onLadder = false;
                    // change animations when landing
                    if (playerDirection == new Vector2(0, 1) &&
                        (previousAnimationFrame == "JumpLeft" || previousAnimationFrame == "JumpRight"))
                        // if player just landed and was facing right
                        currentAnimationFrame = "StandingRight";
                    else if (playerDirection == new Vector2(-1, 0) &&
                        (previousAnimationFrame == "JumpLeft" || previousAnimationFrame == "JumpRight"))
                        // if player just landed and was facing right
                        currentAnimationFrame = "StandingLeft";
                }
                else if (!IsOnLadder()) // implement gravity if not on a ladder
                {
                    moveUpLadder = false;
                    onLadder = false; // reset flag
                    // update player location by velocity
                    location -= velocity * elapsed;
                    velocity += acceleration * elapsed;
                    // move right while jumping if not against a wall
                    if (currentKeyboardState.IsKeyDown(Keys.D))
                    {
                        if (!IsAgainstWall(elapsed) && playerDirection == new Vector2(-1, 0))
                        {
                            location.X += playerMoveSpeed / 4 * elapsed;
                        }
                        playerDirection = new Vector2(0, 1);
                        if (!combatMode)
                            currentAnimationFrame = "JumpRight";
                    }
                    // move left while jumping if not against a wall
                    else if (currentKeyboardState.IsKeyDown(Keys.A))
                    {
                        if (!IsAgainstWall(elapsed) && playerDirection == new Vector2(0, 1))
                        {
                            location.X -= playerMoveSpeed / 4 * elapsed;
                        }
                        playerDirection = new Vector2(-1, 0);
                        if (!combatMode)
                            currentAnimationFrame = "JumpLeft";
                    }
                    // don't move left or right at all
                    else
                        velocity.X = 0f;
                }
            }
        }


        /// <summary>
        /// Controls the camera zoom level, zoom of 0 being farthest zoom out and larger numbers zooming in
        /// </summary>
        public void CameraZoomer()
        {
            if (currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
            {
                if (camera.Zoom >= 2.15)// limit zoom out
                {
                    camera.Zoom -= .05f;// zoom out
                }
            }
            else if (currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
            {
                if (camera.Zoom <= 3)// limit zoom in
                {
                    camera.Zoom += .05f;// zoom in
                }
            }
        }

        public bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }

        public bool IsMousePressed(string button)
        {
            bool mouseButton;
            if (button == "LeftMouseButton" || button == "LeftButton" || button == "Left" || button == "LMB")
            {
                mouseButton = currentMouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released;
            }
            else if (button == "RightMouseButton" || button == "RightButton" || button == "Right" || button == "RMB")
            {
                mouseButton = currentMouseState.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Released;
            }
            else if (button == "MiddleMouseButton" || button == "MiddleButton" || button == "Middle" || button == "MMB")
            {
                mouseButton = currentMouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Released;
            }
            else
                mouseButton = false;
            return mouseButton;
        }

        /// <summary>
        /// Determines whether the player is on the ground or stairs and if so, will set the player
        /// at ground/stair level
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Is on ground or stairs</returns>
        public bool IsOnGround(float time)
        {
            // determine where the player will be if move is allowed
            Vector2 newLoc = location - velocity * time;
            Rectangle newRect = new Rectangle((int)newLoc.X, (int)newLoc.Y, spriteSize, spriteSize);
            bool onGround = false;
            //playerMoveSpeed = setPlayerWalkSpeed; // reset move speed
            bool nextPointIsStart = false;

            // player is moving on stairs
            foreach (var point in _mapStairs)
            {
                if (point == new Vector2(0, 0))
                {
                    nextPointIsStart = true; // set flag, indicating the next point is a starting point for stairs
                }
                else if (newRect.Contains(point))
                {
                    // slow player when going down stairs
                    playerMoveSpeed = setPlayerStairSpeed; 
                    walkAnimSpeed = 100;

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
                // speed walk up when on flat ground
                playerMoveSpeed = setPlayerWalkSpeed;
                walkAnimSpeed = 75;

                // check is player will be standing on top of any platforms
                foreach (var item in _mapPlatforms)
                {
                    if (newRect.Y + spriteSize + camera.Position.Y <= item.Bottom + camera.Position.Y &&
                        newRect.Y + spriteSize + camera.Position.Y >= item.Top + camera.Position.Y && // player is above platform

                        (int)(newRect.X + spriteSize + camera.Position.X) > (int)(item.Left + camera.Position.X - 0) &&
                        (int)(newRect.X + camera.Position.X) < (int)(item.Right + camera.Position.X + 0)) // player is inside range of platform
                    {
                        if (!moveUpLadder && onGround == false && newRect.Y < item.Top) // check if on ladder and verify platform is below player
                        {
                            location.Y = item.Y - spriteSize; // place player on top of platform
                            onGround = true;
                            break;
                        }
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
            //float newLoc = 0;
            //// find new location of player, if move is allowed
            //if (currentAnimationFrame == "PlayerRight")
            //    newLoc = location.X + playerMoveSpeed * time;
            //else
            //    newLoc = location.X - playerMoveSpeed * time;

            // check if player is walking against a wall
            foreach (var item in _mapPlatforms)
            {
                if (item.Intersects(new Rectangle((int)location.X, PlayerRect.Y + 32, 32, 32)) && 
                    (PlayerRect.Left <= item.Right ||
                    PlayerRect.Right >= item.Left))
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
                if (PlayerRect.Intersects(item))
                {
                    // width of the ladder for the player to latch on to
                    if (PlayerRect.X + spriteSizeForLadders + camera.Position.X <= item.Right + camera.Position.X + 15 &&
                        PlayerRect.X + camera.Position.X >= item.Left + camera.Position.X - 45)
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
                if (PlayerRect.X + spriteSizeForLadders + camera.Position.X <= item.Right + camera.Position.X + 50 &&
                    PlayerRect.X + camera.Position.X >= item.Left + camera.Position.X - 50 &&
                    (PlayerRect.Bottom < item.Top + 1 && PlayerRect.Bottom + 10 >= item.Top))
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