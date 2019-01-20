using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TiledSharp;

namespace Colony_Ship_Horizon
{
    public class Game1 : Game
    {
        // various states of the game
        private enum GameState
        {
            MenuSong,
            MainMenu,
            Gameplay,
            EndOfGame,
        }

        // states of the game
        private GameState gameState = GameState.MenuSong;

        // weapon states/data
        private enum GunType
        {
            NoGun,
            LazerGun,
            Pistol,
            Smg,
            Rocket,
            Shotgun,
            Sniper,
        }

        private int equippedPlayerGunIndex = 0;
        int playerEquippedGunTotalClips = 0; // must also have a list for inventory stacking
        private List<Gun> playerGuns = new List<Gun>();
        private Gun pistol;
        private Gun smg;
        private Gun shotgun;
        string currentSoundFromPool = "";
        string gunShotAnim = "";

        // phases of the gameplay state, a wave consists of one of each phase
        private enum GameplayPhase
        {
            Noncombat,
            Transition,
            Combat,
        }

        private GameplayPhase gameplayPhase = GameplayPhase.Noncombat;
        private int currentWave = 0;

        // main menu data
        private Texture2D newButton;
        private Texture2D exitButton;
        private Texture2D loadButton;
        private Texture2D mainLogo;
        private Rectangle newButtonRect = new Rectangle();
        private Rectangle loadButtonRect = new Rectangle();
        private Rectangle exitButtonRect = new Rectangle();
        bool exitMenu = false;

        // mouse
        private MouseState currentMouseState;
        private MouseState previousMouseState;
        private Vector2 prevMousePos;
        private Vector2 newMousePos;

        // menu sprite
        private Texture2D menuButton;
        private Texture2D menuButton2;
        private MenuGUI menu;
        Texture2D helpMenu;
        Texture2D controlsMenu;

        // hud elements
        private Texture2D hudHealthBarSheet;
        Texture2D hudBackground;
        private SpriteMap hudHealthBar;
        bool pauseMenuFlag;

        // used for ensuring objects are only drawn when visible to the player
        Rectangle viewPortBounds;

        // rainDrop collection/textures
        List<MovingObject> raindrops = new List<MovingObject>();
        Texture2D raindropTexture;

        // smog collection/textures
        List<MovingObject> smog = new List<MovingObject>();
        Texture2D smogTexture;

        // animator collection
        private List<Animator> effectsAnimators = new List<Animator>();

        private Animator raindropImpactAnim;
        private Animator projectileImpactAnim;
        private Animator bloodAnim;
        private Animator waterTankAnim;
        private Animator exhaustAnim1;
        private Animator exhaustAnim2;
        private Animator exhaustAnim3;
        private float menuCounter = 500;
        private Dictionary<string, Rectangle> AgriculturalButtons = new Dictionary<string, Rectangle>();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Sounds sounds;

        // GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;

        // cursor data
        private Cursor cursor;

        private Texture2D cursorTexture;
        private Background background;

        // to be drawn on top of player
        private Texture2D dockingBayExit;

        // background tiles/ maps
        private BackgroundTiles backgroundTiles = new BackgroundTiles();
        Dictionary<string, Map> mapsToLoad = new Dictionary<string, Map>();
        QueueMapToLoad queuedMap = new QueueMapToLoad(); // a queued map to load
        int playerSpawnToUse = 1; // default is 1, the first spawn in the map. this is changed to set a different spawn (when exiting building)

        // player interaction notification
        bool showInteractNotification = false;
        Rectangle interactionRectangle;
        string currentInteraction;
        Texture2D interactionButton;

        ///
        ///mission flags and other data
        ///
        string currentObjective = "";
        // mission 1 dialogue flags
        bool mission1 = true;
        bool talkedToHoloCall1 = false;
        bool talkedToBartender = false;
        bool talkedToBounty1 = false;
        // if (start == false && finish == true) { //objv hasn't begun, reverse the flags and objv is complete }
        bool mission1Objective1Start = false;
        bool mission1Objective1Finish = true;
        bool mission1Objective1ConvoDone = false;
        // mission call flags
        bool incomingCall = false;
        bool answerCall = false;
        Texture2D incomingCallImage;
        Dictionary<string, Texture2D> textBubbles = new Dictionary<string, Texture2D>();


        // text/halogenFont
        private SpriteFont agencyFont;
        private SpriteFont halogenFont;

        private int score = 0;

        // player sprite
        private PlayerType playerType = new PlayerType();
        private Player player;
        bool lookAtNpc = false;
        private bool deathNotify = false;
        private bool winNotify = false;
        private int deathNotifyCounter = 0;

        // NPC types
        private NPC_Type alienLeech;
        private NPC_Type npcBartender;
        private NPC_Type npcBounty1;
        private NPC_Type npcBouncer;
        private NPC_Type npcMale1;
        private NPC_Type npcMale2;
        private NPC_Type npcFemale1;
        private NPC_Type npcFemale2;
        private NPC_Type npcFemale3;

        private List<NPC> npcList = new List<NPC>();
        private int npcSpawnCount;
        int newNpcDamage = 0;
        private Timer gameplayPhaseTimer = new Timer(300);
        private Timer doorTimer = new Timer(300);

        // inventory and item data
        private Dictionary<string, List<Rectangle>> droppedAnimatorItems = new Dictionary<string, List<Rectangle>>();
        private Dictionary<string, List<Rectangle>> droppedNpcItems = new Dictionary<string, List<Rectangle>>();
        // used with clickEventFlag to determine whether a player has interacted with an event
        Dictionary<string, List<Rectangle>> clickEventCoords = new Dictionary<string, List<Rectangle>>();
        Dictionary<string, List<bool>> clickEventFlag = new Dictionary<string, List<bool>>();
        private List<Texture2D> itemTextures = new List<Texture2D>();
        private List<ItemStack> inventoryList = new List<ItemStack>();
        private List<Rectangle> inventoryRectangles = new List<Rectangle>();
        private Texture2D inventory;
        private bool isInventoryOpen = false;
        bool inventoryFullAlert = false;

        // travel map screen
        Vector2 travelMapPos;
        bool openTravelMap = false;
        Texture2D travelMapScreen;
        Texture2D travelMapPopup;
        Rectangle travelMapAsteroidColony;
        bool travelMapHighlightAsteroidColony = false;
        Rectangle travelMapMercury;
        bool travelMapHighlightMercury = false;
        Rectangle travelMapVenus;
        bool travelMapHighlightVenus = false;
        Rectangle travelMapEarth;
        bool travelMapHighlightEarth = false;
        Rectangle travelMapMars;
        bool travelMapHighlightMars = false;
        string prevCelestialObject = "";

        // player notifications/alerts
        bool isAnimationHighlighted = false;
        Texture2D waterAlert;
        Texture2D wateringCan;
        Texture2D hammer;
        bool wateringCanCursor;
        int sceneToDraw = 0;
        Rectangle currentSceneArea;
        bool drawCurrentScene = false;

        // gravity object, to apply gravity on items
        private Gravity npcItemGravity;
        private Gravity animatorItemGravity;

        // used in RandomNumber function
        private RandomNumberGen randomNumberGen = new RandomNumberGen();

        //ambient sound to play
        string ambienceToPlay;

        // is the player talking to an npc
        bool isPlayerInConversation = false;

        // allow dev hotkeys
        bool enableDevHotKeys = false;

        // toggle screens
        bool showFPS = false; // draw the fps counter
        bool showMenu = true; // draw the main menu buttons
        bool showHud = true; // draw the HUD
        bool showAmmo = true; // draw the ammo in inventory and clip
        bool showHealth = true; // draw the health bar
        bool showMode = false; // draw hammer/wateringCan and other icons

        public Game1()
        {
            IsMouseVisible = false;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            graphics.SynchronizeWithVerticalRetrace = true; // enable Vsync
            IsFixedTimeStep = true; // enable fixed time step
            //float targetFPS = 60.0f;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / targetFPS); // set custom framerate, causes jerky camera movement

            graphics.HardwareModeSwitch = false;
            graphics.IsFullScreen = true;

            // anti-aliasing
            //graphics.GraphicsProfile = GraphicsProfile.HiDef;
            //Graphics_PreparingDeviceSettings(this?, graphics.PreparingDeviceSettings += something); //function below
            //graphics.ApplyChanges();

            if (showFPS)
                Components.Add(new FrameRateCounter(this, new Vector2(0, 0), Color.WhiteSmoke, Color.White));
        }

        //possible way of enabling multisampling
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            graphics.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        // / <summary>
        // / Allows the game to perform any initialization it needs to before starting to run.
        // / This is where it can query for any required services and load any non-graphic
        // / related content.  Calling base.Initialize will enumerate through any components
        // / and initialize them as well.
        // / </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        // / <summary>
        // / LoadContent will be called once per game and is the place to load
        // / all of your content.
        // / </summary>
        protected override void LoadContent()
        {
            //  Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // interaction button to notify player
            interactionButton = Content.Load<Texture2D>("interactionButton");

            // load main menu textures
            newButton = Content.Load<Texture2D>("mainMenuNewGame");
            newButtonRect = new Rectangle(512, 750, 256, 256);
            loadButton = Content.Load<Texture2D>("mainMenuLoadGame");
            loadButtonRect = new Rectangle(832, 750, 256, 256);
            exitButton = Content.Load<Texture2D>("mainMenuExit");
            exitButtonRect = new Rectangle(1152, 750, 256, 256);
            mainLogo = Content.Load<Texture2D>("mainMenuLogo");
            helpMenu = Content.Load<Texture2D>("helpMenu");
            controlsMenu = Content.Load<Texture2D>("controlsMenu");

            // ---------------------inventory textures------------------
            // dropped animator items
            itemTextures.Add(Content.Load<Texture2D>("tomato"));
            itemTextures.Add(Content.Load<Texture2D>("chamomile"));
            itemTextures.Add(Content.Load<Texture2D>("blueberry"));
            // dropped NPC items
            itemTextures.Add(Content.Load<Texture2D>("shotgunAmmo")); // add to itemTextures
            droppedNpcItems.Add("shotgunAmmo", new List<Rectangle>()); // add to droppedNpcItems; when NPCs die, they may drop
            itemTextures.Add(Content.Load<Texture2D>("smgAmmo")); // add to itemTextures
            droppedNpcItems.Add("smgAmmo", new List<Rectangle>()); // add to droppedNpcItems; when NPCs die, they may drop
            itemTextures.Add(Content.Load<Texture2D>("pistolAmmo")); // add to itemTextures
            droppedNpcItems.Add("pistolAmmo", new List<Rectangle>()); // add to droppedNpcItems; when NPCs die, they may drop
            //load the inventory with item textures
            inventory = Content.Load<Texture2D>("inventory");

            // travel map image
            travelMapScreen = Content.Load<Texture2D>("travelMap");
            travelMapPopup = Content.Load<Texture2D>("travelMapPopup");

            // notification/alert textures
            waterAlert = Content.Load<Texture2D>("waterDrop");
            wateringCan = Content.Load<Texture2D>("wateringCan");
            hammer = Content.Load<Texture2D>("hammer");

            ///
            // ------------------------------------------------------- LOAD MAPS -------------------------------
            ///
            // ----- add the .tmx file to project via solution explorer and set to copy if newer-----
            TmxMap map = new TmxMap("Content/ShipRooms.tmx");
            // load the tilesets for the map
            List<Texture2D> tilesetTextures = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            // get the tileset index that corresponds with each layer of the map
            List<string> tilesetIndices = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices.Add(map.Layers[i].Properties["tileset"]);
            // create new map entry
            mapsToLoad.Add("ShipRooms", new Map(tilesetTextures, tilesetIndices, map, "ShipRooms"));

            map = new TmxMap("Content/personalStarshipMap.tmx");
            List<Texture2D> tilesetTextures2 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures2.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices2 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices2.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("personalStarship", new Map(tilesetTextures2, tilesetIndices2, map, "personalStarship"));

            map = new TmxMap("Content/asteroidColonyMap.tmx");
            List<Texture2D> tilesetTextures3 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures3.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices3 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices3.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("asteroidColony", new Map(tilesetTextures3, tilesetIndices3, map, "asteroidColony"));

            map = new TmxMap("Content/barMap.tmx");
            List<Texture2D> tilesetTextures4 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures4.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices4 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices4.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("bar", new Map(tilesetTextures4, tilesetIndices4, map, "bar"));

            map = new TmxMap("Content/playerRoomMap.tmx");
            List<Texture2D> tilesetTextures5 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures5.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices5 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices5.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("playerRoom", new Map(tilesetTextures5, tilesetIndices5, map, "playerRoom"));

            map = new TmxMap("Content/playerRoomMap2.tmx");
            List<Texture2D> tilesetTextures6 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures6.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices6 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices6.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("playerRoom2", new Map(tilesetTextures6, tilesetIndices6, map, "playerRoom2"));

            map = new TmxMap("Content/spaceStationMap.tmx");
            List<Texture2D> tilesetTextures7 = new List<Texture2D>();
            for (int i = 0; i < map.Tilesets.Count; i++)
                tilesetTextures7.Add(Content.Load<Texture2D>(map.Tilesets[i].Name.ToString()));
            List<string> tilesetIndices7 = new List<string>();
            for (int i = 0; i < map.Layers.Count; i++)
                tilesetIndices7.Add(map.Layers[i].Properties["tileset"]);
            mapsToLoad.Add("spaceStation", new Map(tilesetTextures7, tilesetIndices7, map, "spaceStation"));

            // initialize gravity with map platforms
            npcItemGravity = new Gravity(backgroundTiles.mapPlatforms);
            animatorItemGravity = new Gravity(backgroundTiles.mapPlatforms);

            // load fonts
            agencyFont = Content.Load<SpriteFont>("fonts/Agency");
            halogenFont = Content.Load<SpriteFont>("fonts/Halogen");

            // load sprite map xml files to spriteMap objects
            SpriteMap horizonSpriteMap = Content.Load<SpriteMap>("SpriteMaps/horizon");

            // create game cursor
            cursorTexture = Content.Load<Texture2D>("Cursor");
            cursor = new Cursor(cursorTexture, GraphicsDevice, wateringCan);
            cursor.CursorPos = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            // load player types
            playerType.playerJumpingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerJumpingRightSheet");
            playerType.playerJumpingRight = Content.Load<SpriteMap>("SpriteMaps/playerJumpingRight");
            playerType.playerWalkingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerWalkingRightSheet");
            playerType.playerWalkingRight = Content.Load<SpriteMap>("SpriteMaps/playerWalkingRight");
            playerType.playerStandingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerStandingRightSheet");
            playerType.playerStandingRight = Content.Load<SpriteMap>("SpriteMaps/playerStandingRight");
            playerType.playerAimingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerAimingRightSheet");
            playerType.playerAimingRight = Content.Load<SpriteMap>("SpriteMaps/playerAimingRight");
            playerType.playerAimingBackRightSheet = Content.Load<Texture2D>("SpriteSheets/playerAimingBackRightSheet");
            playerType.playerAimingBackRight = Content.Load<SpriteMap>("SpriteMaps/playerAimingBackRight");
            playerType.playerStandingAimingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerStandingAimingRightSheet");
            playerType.playerStandingAimingRight = Content.Load<SpriteMap>("SpriteMaps/playerStandingAimingRight");
            playerType.playerSmokingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerSmokingRightSheet");
            playerType.playerSmokingRight = Content.Load<SpriteMap>("SpriteMaps/playerSmokingRight");
            playerType.playerReceivingCallRightSheet = Content.Load<Texture2D>("SpriteSheets/playerReceivingCallRightSheet");
            playerType.playerReceivingCallRight = Content.Load<SpriteMap>("SpriteMaps/playerReceivingCallRight");
            playerType.playerWoundedRightSheet = Content.Load<Texture2D>("SpriteSheets/playerWoundedRightSheet");
            playerType.playerWoundedRight = Content.Load<SpriteMap>("SpriteMaps/playerWoundedRight");
            playerType.playerSittingRightSheet = Content.Load<Texture2D>("SpriteSheets/playerSittingRightSheet");
            playerType.playerSittingRight = Content.Load<SpriteMap>("SpriteMaps/playerSittingRight");
            playerType.lazer = Content.Load<Texture2D>("lazer");


            // create weapon objects
            //--------------scale is currently overwritten inside projetile
            // bulletCount is zero based
            // can make first bullet not have spread in player.cs addprojectile()
            pistol = new Gun("Pistol", 30, 0, 2500, false, TimeSpan.FromSeconds(.04f), Content.Load<Texture2D>("gun1"), Content.Load<Texture2D>("bullet2"),
                Content.Load<Texture2D>("muzzleFlash"), "impact2", "blasterShot2", 2, .5f, 200f, 0, 1, 8, 1);
            smg = new Gun("Smg", 5, 1, 2000, true, TimeSpan.FromSeconds(.1f), Content.Load<Texture2D>("gun3"), Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("muzzleFlash"), "impact1", "gunShot", 1, .25f, 25f, 0, 2f, 31, 1);
            shotgun = new Gun("Shotgun", 10, 2, 3000, false, TimeSpan.FromSeconds(.3f), Content.Load<Texture2D>("gun2"), Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("muzzleFlash"), "impact2", "sniperShot", 1, .35f, 7f, 7, 3f, 6, 1);

            // create background images
            background = new Background(horizonSpriteMap);
            background.earth = Content.Load<Texture2D>("earth");
            background.asteroidParallax = Content.Load<Texture2D>("asteroidParallax");
            background.citySkylineParallax = Content.Load <Texture2D>("citySkylineParallax");
            background.cityMidgroundParallax = Content.Load<Texture2D>("cityMidgroundParallax");
            background.cityForegroundParallax = Content.Load<Texture2D>("cityForegroundParallax");
            background.starsPurple = Content.Load<Texture2D>("starsPurple");//
            background.starsBlue = Content.Load<Texture2D>("starsBlue");
            background.starsBlack = Content.Load<Texture2D>("starsBlack");
            background.starsPink = Content.Load<Texture2D>("starsPink");
            background.starDust = Content.Load<Texture2D>("starDust");
            background.nebula = Content.Load<Texture2D>("nebula");
            background.planets = Content.Load<Texture2D>("planets");
            background.asteroids = Content.Load<Texture2D>("asteroids");
            background.flyingCar1 = Content.Load<Texture2D>("flyingCar1");
            background.flyingCar2 = Content.Load<Texture2D>("flyingCar2");
            background.flyingCar3 = Content.Load<Texture2D>("flyingCar3");
            background.flyingCar4 = Content.Load<Texture2D>("flyingCar4");
            background.flyingCar5 = Content.Load<Texture2D>("flyingCar5");
            background.ftlBeam = Content.Load<Texture2D>("jumpToFTL");
            background.Load();// load texture collections

            background.starship = new Animator(Content.Load<SpriteMap>("SpriteMaps/playerStarship"), Content.Load<Texture2D>("SpriteSheets/playerStarshipSheet"), 350, true);
            background.starship.AddAnimation(new Vector2(-501, 500), Color.White);

            // the docking bay exit, to be drawn on top of player
            dockingBayExit = Content.Load<Texture2D>("dockingBayExit");

            // create menu button image
            menuButton = Content.Load<Texture2D>("MenuButton");
            menuButton2 = Content.Load<Texture2D>("MenuButton2");

            // hud elements
            hudHealthBarSheet = Content.Load<Texture2D>("SpriteSheets/hudHealthBarSheet");
            hudHealthBar = Content.Load<SpriteMap>("SpriteMaps/hudHealthBar");
            hudBackground = Content.Load<Texture2D>("hudBackground");

            // raindrops
            raindropTexture = Content.Load<Texture2D>("rainDrop");
            
            // smog
            smogTexture = Content.Load<Texture2D>("smog");
            background.smogParallax = Content.Load<Texture2D>("smog");

            // ----------------------------------Load Animators

            // Agricultural animations
            // create Agricultural menu by loading content into each button. each button has an animator class which when clicked on will add animations to the game
            menu = new MenuGUI(Content, GraphicsDevice);
            int menuButtonY = 750;
            int menuButtonX = 100;

            menu.AddButton("chamomileAnim", "Agricultural", 3000, 32,
                new Rectangle(menuButtonX, menuButtonY, 86, 86));
            menuButtonX += 100;

            menu.AddButton("tomatoAnim", "Agricultural", 1000, 32,
                new Rectangle(menuButtonX, menuButtonY, 86, 86));
            menuButtonX += 100;

            menu.AddButton("blueberryAnim", "Agricultural", 1000, 32,
                new Rectangle(menuButtonX, menuButtonY, 86, 86));
            menuButtonX += 100;

            menu.AddButton("waterTankAnim", "Mechanical", 150, 32,
                new Rectangle(menuButtonX, menuButtonY, 86, 86));
            menuButtonX += 100;


            // effects animations
            Texture2D raindropImpactSheet = Content.Load<Texture2D>("SpriteSheets/raindropImpactSheet");
            SpriteMap raindropImpact = Content.Load<SpriteMap>("SpriteMaps/raindropImpact");
            raindropImpactAnim = new Animator(raindropImpact, raindropImpactSheet, 10, false);
            effectsAnimators.Add(raindropImpactAnim);

            //Texture2D projectileImpactSheet = Content.Load<Texture2D>("SpriteSheets/projectileImpactSheet");
            //SpriteMap projectileImpact = Content.Load<SpriteMap>("SpriteMaps/projectileImpact");
            Texture2D projectileImpactSheet = Content.Load<Texture2D>("SpriteSheets/gunShotSheet");
            SpriteMap projectileImpact = Content.Load<SpriteMap>("SpriteMaps/gunShot");
            projectileImpactAnim = new Animator(projectileImpact, projectileImpactSheet, 10, false);
            effectsAnimators.Add(projectileImpactAnim);

            Texture2D bloodSheet = Content.Load<Texture2D>("SpriteSheets/alien3ShotSheet");
            SpriteMap blood = Content.Load<SpriteMap>("SpriteMaps/alien3Shot");
            bloodAnim = new Animator(blood, bloodSheet, 15, false);
            effectsAnimators.Add(bloodAnim);

            Texture2D exhaustSheet = Content.Load<Texture2D>("SpriteSheets/exhaustSheet");
            SpriteMap exhaust = Content.Load<SpriteMap>("SpriteMaps/exhaust");
            exhaustAnim1 = new Animator(exhaust, exhaustSheet, 150, true);
            exhaustAnim1.AddAnimation(new Vector2(0, 0), Color.White, .1f, "Left");
            exhaustAnim2 = new Animator(exhaust, exhaustSheet, 150, true);
            exhaustAnim2.AddAnimation(new Vector2(0, 0), Color.White, .1f, "Left");
            exhaustAnim3 = new Animator(exhaust, exhaustSheet, 150, true);
            exhaustAnim3.AddAnimation(new Vector2(0, 0), Color.White, .1f, "Left");

            //------------------------------------------End Of Load Animators

            // create sounds
            sounds = new Sounds();
            sounds.doorSound = (Content.Load<SoundEffect>("Sounds/door"));
            sounds.leavesRustle = (Content.Load<SoundEffect>("Sounds/leavesRustle"));
            sounds.waterPour = (Content.Load<SoundEffect>("Sounds/waterPour"));

            sounds.menuOpen = (Content.Load<SoundEffect>("Sounds/Beeps/slowBeep"));
            sounds.menuClosed = (Content.Load<SoundEffect>("Sounds/Beeps/downBeep"));
            sounds.menuBuilt = (Content.Load<SoundEffect>("Sounds/pop"));
            sounds.menuSelect = (Content.Load<SoundEffect>("Sounds/menuSelect"));

            sounds.pulseGunShot = (Content.Load<SoundEffect>("Sounds/pulseRifle"));
            sounds.gunShot = (Content.Load<SoundEffect>("Sounds/gunShot"));
            sounds.blasterShot = (Content.Load<SoundEffect>("Sounds/blasterShot"));
            sounds.blasterShot2 = (Content.Load<SoundEffect>("Sounds/blasterShot2"));
            sounds.blasterReload = (Content.Load<SoundEffect>("Sounds/blasterReload"));
            sounds.blasterReload2 = (Content.Load<SoundEffect>("Sounds/blasterReload2"));
            sounds.sniperShot = (Content.Load<SoundEffect>("Sounds/sniperShot"));
            sounds.shotgunShot = (Content.Load<SoundEffect>("Sounds/shotgunShot"));
            sounds.shotgunPump = (Content.Load<SoundEffect>("Sounds/shotgunPump"));
            sounds.lazerGunShot = (Content.Load<SoundEffect>("Sounds/lazerGunShot"));

            sounds.bulletPing = (Content.Load<SoundEffect>("Sounds/bulletPing"));

            sounds.playerWalk = (Content.Load<SoundEffect>("Sounds/playerWalk"));

            sounds.gutCrunch = (Content.Load<SoundEffect>("Sounds/gutCrunch"));
            sounds.alienAttack = (Content.Load<SoundEffect>("Sounds/alienAttack"));
            sounds.alienWalk = (Content.Load<SoundEffect>("Sounds/alienWalk"));

            // map sounds
            sounds.starshipAmbience = (Content.Load<SoundEffect>("Sounds/Ambience/starship"));
            sounds.starshipJump = (Content.Load<SoundEffect>("Sounds/Ambience/starshipJump"));
            sounds.starshipArrival = (Content.Load<SoundEffect>("Sounds/Ambience/starshipArrival"));
            sounds.starshipLanding = (Content.Load<SoundEffect>("Sounds/Ambience/starshipLanding"));
            sounds.rainAmbience = (Content.Load<SoundEffect>("Sounds/Ambience/rain"));
            sounds.barAmbience = (Content.Load<SoundEffect>("Sounds/Ambience/bar"));

            sounds.incomingCallRing = Content.Load<SoundEffect>("Sounds/incomingCallRing");

            sounds.Load();

            //----------------------------------------------------------- LOAD NPC TYPES--------------------------------------------------
            // load content into the npc type "alien leech"
            alienLeech = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/alien3StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"));

            // ----------------------------------------------------------------------------LOAD NPC ACTIONS---------------------------------------------------
            //
            // load the next npc's action textures, before loading the npc itself; the texture used is determined by the "Action" property 
            // for each NPC_Spawn in the tilemap objects list; if an npc does not have an action set, it will stand by default
            //
            Dictionary<string, Texture2D> actionSheets = new Dictionary<string, Texture2D>();
            Dictionary<string, SpriteMap> actionMaps = new Dictionary<string, SpriteMap>();
            actionSheets.Add("npcBartenderCleaningRight", Content.Load<Texture2D>("SpriteSheets/npcBartenderCleaningRightSheet"));
            actionMaps.Add("npcBartenderCleaningRight", Content.Load<SpriteMap>("SpriteMaps/npcBartenderCleaningRight"));

            actionSheets.Add("npcBounty1SittingRight", Content.Load<Texture2D>("NpcActionTextures/npcBounty1SittingRight"));
            actionSheets.Add("npcBounty1AimingRight", Content.Load<Texture2D>("NpcActionTextures/npcBounty1AimingRight"));

            actionSheets.Add("npcMale1SittingRight", Content.Load<Texture2D>("NpcActionTextures/npcMale1SittingRight"));
            actionSheets.Add("npcMale1DrinkingRight", Content.Load<Texture2D>("SpriteSheets/npcMale1DrinkingRightSheet"));
            actionMaps.Add("npcMale1DrinkingRight", Content.Load<SpriteMap>("SpriteMaps/npcMale1DrinkingRight"));
            actionSheets.Add("npcMale1SmokingRight", Content.Load<Texture2D>("SpriteSheets/npcMale1SmokingRightSheet"));
            actionMaps.Add("npcMale1SmokingRight", Content.Load<SpriteMap>("SpriteMaps/npcMale1SmokingRight"));

            actionSheets.Add("npcMale2SittingRight", Content.Load<Texture2D>("NpcActionTextures/npcMale2SittingRight"));
            actionSheets.Add("npcMale2DrinkingRight", Content.Load<Texture2D>("SpriteSheets/npcMale2DrinkingRightSheet"));
            actionMaps.Add("npcMale2DrinkingRight", Content.Load<SpriteMap>("SpriteMaps/npcMale2DrinkingRight"));
            actionSheets.Add("npcMale2SmokingRight", Content.Load<Texture2D>("SpriteSheets/npcMale2SmokingRightSheet"));
            actionMaps.Add("npcMale2SmokingRight", Content.Load<SpriteMap>("SpriteMaps/npcMale2SmokingRight"));

            actionSheets.Add("npcFemale1SittingRight", Content.Load<Texture2D>("NpcActionTextures/npcFemale1SittingRight"));
            actionSheets.Add("npcFemale1DrinkingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale1DrinkingRightSheet"));
            actionMaps.Add("npcFemale1DrinkingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale1DrinkingRight"));
            actionSheets.Add("npcFemale1SmokingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale1SmokingRightSheet"));
            actionMaps.Add("npcFemale1SmokingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale1SmokingRight"));

            actionSheets.Add("npcFemale2SittingRight", Content.Load<Texture2D>("NpcActionTextures/npcFemale2SittingRight"));
            actionSheets.Add("npcFemale2DrinkingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale2DrinkingRightSheet"));
            actionMaps.Add("npcFemale2DrinkingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale2DrinkingRight"));
            actionSheets.Add("npcFemale2SmokingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale2SmokingRightSheet"));
            actionMaps.Add("npcFemale2SmokingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale2SmokingRight"));

            actionSheets.Add("npcFemale3DrinkingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale3DrinkingRightSheet"));
            actionMaps.Add("npcFemale3DrinkingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale3DrinkingRight"));
            actionSheets.Add("npcFemale3SmokingRight", Content.Load<Texture2D>("SpriteSheets/npcFemale3SmokingRightSheet"));
            actionMaps.Add("npcFemale3SmokingRight", Content.Load<SpriteMap>("SpriteMaps/npcFemale3SmokingRight"));


            // -------------------------------------------load civilian npcs---------------------------------------------
            npcBartender = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            npcBounty1 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/npcBounty1DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcBounty1DeathRight"),
                actionSheets, actionMaps);

            npcBouncer = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcBouncerStandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcBouncerStandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcBouncerStandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcBouncerStandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/npcBounty1DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcBounty1DeathRight"),
                actionSheets, actionMaps);

            npcMale1 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            npcMale2 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            npcFemale1 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            npcFemale2 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            npcFemale3 = new NPC_Type(Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/npcMale1StandingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3WalkingRightSheet"),
                Content.Load<Texture2D>("SpriteSheets/alien3AttackingRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/npcMale1StandingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3WalkingRight"),
                Content.Load<SpriteMap>("SpriteMaps/alien3AttackingRight"),
                Content.Load<Texture2D>("bullet1"),
                Content.Load<Texture2D>("SpriteSheets/alien3DeathRightSheet"),
                Content.Load<SpriteMap>("SpriteMaps/alien3DeathRight"),
                actionSheets, actionMaps);

            // --------------------end civilian load-------------

            // text bubble
            textBubbles.Add("player", Content.Load<Texture2D>("TextBubbles/textBubblePlayer"));
            textBubbles.Add("bartender", Content.Load<Texture2D>("TextBubbles/textBubbleBartender"));
            textBubbles.Add("bounty1", Content.Load<Texture2D>("TextBubbles/textBubbleBounty1"));
            textBubbles.Add("holoCall", Content.Load<Texture2D>("TextBubbles/textBubbleHoloCall"));

            incomingCallImage = Content.Load<Texture2D>("incomingCall");

            // load music
            Song engineRoom = Content.Load<Song>("Music/engineRoom");
            sounds.engineRoomSong = engineRoom;
            Song mainRoom = Content.Load<Song>("Music/mainRoom");
            sounds.mainRoomSong = mainRoom;
            Song gardenRoom = Content.Load<Song>("Music/gardenRoom");
            sounds.gardenRoomSong = gardenRoom;
            Song menuSong = Content.Load<Song>("Music/menuSong");
            sounds.menuSong = menuSong;
            Song attackSong1 = Content.Load<Song>("Music/attackSong1");
            sounds.attackSong1 = attackSong1;
            Song attackSong2 = Content.Load<Song>("Music/attackSong2");
            sounds.attackSong2 = attackSong2;
        }

        /// <summary>
        /// Load a new map, spawning the player there, despawning existing npcs and spawning new npcs
        /// </summary>
        /// <param name="mapToLoad">The map to be loaded next</param>
        public void LoadNewMap(Map mapToLoad, int playerSpawnToUse, GameTime gameTime)
        {
            // load new map into backgroundTiles, the object which draws the map's tiles
            backgroundTiles.mapName = mapToLoad._mapName;
            backgroundTiles.map = mapToLoad._map;
            backgroundTiles.currentlyLoadedTilesets = mapToLoad._tilesets;
            backgroundTiles.tilesetsIndexPerLayer = mapToLoad._tilesetIndexPerLayer;
            backgroundTiles.Load(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height); // load rectangles from tiled map into collections

            // remove all weather
            raindrops.Clear();
            smog.Clear();

            // stop all sounds
            sounds.Update(gameTime, "stopSound", background._mapName);

            Vector2 playerLoc;
            if (player != null)
            {
                playerLoc = player.location;

                player._npcDeathAnimList.Clear();
                player.npcDeathLocation = new Rectangle();
                player.npcImpactEffects = false;
                player.addCorpsePosToList = false;
                player._npcList.Clear();

                // spawn player in new map
                string playerSpawn = "Player_Spawn" + playerSpawnToUse.ToString();
                player.location = backgroundTiles.mapObjects[playerSpawn].Location.ToVector2();

                player.LoadNewMapData(backgroundTiles.mapPlatforms, backgroundTiles.mapObjects,
                backgroundTiles.mapStairs, backgroundTiles.mapLadders, backgroundTiles.mapDoors);
            }
            else
            {
                playerLoc = Vector2.Zero;
            }

            // despawn any existing npcs
            npcList.Clear();

            // copy dictionary before iteration, to allow editing of the original
            Dictionary<string, Rectangle> mapObjects = new Dictionary<string, Rectangle>(backgroundTiles.mapObjects);
            // spawn new npcs
            int spawnCount = 0;
            int npcTypeToSpawn = 0;

            // find different objects within map 
            foreach (KeyValuePair<string, Rectangle> spawn in mapObjects)
            {
                if (spawn.Key.Contains("NPC_Spawn")) // spawn civilians
                {
                    int interactionIndex = spawn.Key.LastIndexOf(';');
                    string npcInteraction = "";
                    if (interactionIndex != -1)
                    {
                        int i = 0;
                        npcInteraction = spawn.Key.Substring(interactionIndex + 1); // create substring from the action concatenated to the end of NPC_Spawn string
                        // now that the substring is no longer needed, remove it from the original
                        backgroundTiles.mapObjects.Remove(spawn.Key);
                        // and finally, update the dictionary with the new string sans substring
                        backgroundTiles.mapObjects.Add(spawn.Key.Remove(interactionIndex), spawn.Value);
                    }

                    // string is a property of the map objects, set in the tiled map editor, which determines where this npc will spawn and what action
                    // this npc will undertake and what type of npc it is (e.g. bounty target)
                    int typeIndex = spawn.Key.LastIndexOf(':');
                    string npcType = "";
                    if (typeIndex != -1)
                    {
                        int i = 0;
                        //npcType = spawn.Key.Substring(typeIndex + 1); // create substring from the action concatenated to the end of NPC_Spawn string
                        //// now that the substring is no longer needed, remove it from the original
                        //backgroundTiles.mapObjects.Remove(spawn.Key);
                        //// and finally, update the dictionary with the new string sans substring
                        //backgroundTiles.mapObjects.Add(spawn.Key.Remove(typeIndex + 1), spawn.Value);
                    }

                    string actionToTake = "";
                    char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    int actionIndex = spawn.Key.LastIndexOfAny(numbers); // find the end of the spawn name, which is the start of the actionToTake - 1
                    if (actionIndex != spawn.Key.Length - 1) // determine if index is the end of the string
                    {
                        // use temporary key and remove the end of the string to get the correct data
                        string tempKey = spawn.Key;
                        if (interactionIndex > 0)
                            tempKey = spawn.Key.Remove(interactionIndex);

                        actionToTake = tempKey.Substring(actionIndex + 1); // create substring from the action concatenated to the end of NPC_Spawn string
                        // now that the substring is no longer needed, remove it from the original
                        backgroundTiles.mapObjects.Remove(spawn.Key);

                        tempKey = spawn.Key.Remove(actionIndex + 1);
                        // and finally, update the dictionary with the new string sans substring
                        backgroundTiles.mapObjects.Add(tempKey, spawn.Value);
                    }
                    int actionPauseTime = randomNumberGen.NextNumber(2000, 10000);
                    int actionTime = randomNumberGen.NextNumber(200, 300);
                    if (actionToTake.Contains("Smoking"))
                    {
                        actionPauseTime = randomNumberGen.NextNumber(300, 600);
                    }

                    spawnCount++; // total number of npc spawns
                    int totalNpcTypes = 5; // the total number of different npc types to spawn

                    if (npcTypeToSpawn >= totalNpcTypes) // reached the limit of npc types to spawn
                        npcTypeToSpawn = 0; // reset the type

                    //int npcTypeSpawn = randomNumberGen.NextNumber(0, 5);

                    if (actionToTake.ToLower().Contains("cleaningright"))
                    {
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                            backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcBartender, playerLoc, "Civilian", "npcBartender", spawnCount,
                            npcInteraction , actionToTake, actionTime, 300));// don't pause this npc's actions
                    }
                    else if (actionToTake.ToLower().Contains("bounty") && talkedToBartender && !talkedToBounty1)
                    {
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                            backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcBounty1, playerLoc, "Civilian", "npcBounty1", spawnCount,
                            npcInteraction, actionToTake, actionTime, 300));// don't pause this npc's actions
                    }
                    else if (actionToTake.ToLower().Contains("bouncer"))
                    {
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                            backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcBouncer, playerLoc, "Civilian", "npcBouncer", spawnCount,
                            npcInteraction, actionToTake, actionTime, 300, "barBouncer"));// don't pause this npc's actions
                    }
                    else if (npcTypeToSpawn == 0 && !actionToTake.ToLower().Contains("bounty"))
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs, 
                            backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcMale1, playerLoc, "Civilian", "npcMale1",spawnCount,
                            npcInteraction, actionToTake, actionTime, actionPauseTime));
                    else if (npcTypeToSpawn == 1 && !actionToTake.ToLower().Contains("bounty"))
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                        backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcFemale1, playerLoc, "Civilian", "npcFemale1", spawnCount,
                        npcInteraction, actionToTake, actionTime, actionPauseTime));
                    else if (npcTypeToSpawn == 2 && !actionToTake.ToLower().Contains("bounty"))
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                            backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcMale2, playerLoc, "Civilian", "npcMale2", spawnCount,
                            npcInteraction, actionToTake, actionTime, actionPauseTime));
                    else if (npcTypeToSpawn == 3 && !actionToTake.ToLower().Contains("bounty"))
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                        backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcFemale2, playerLoc, "Civilian", "npcFemale2", spawnCount,
                        npcInteraction, actionToTake, actionTime, actionPauseTime));
                    else if (npcTypeToSpawn == 4 && !actionToTake.ToLower().Contains("bounty"))
                        npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                        backgroundTiles.mapLadders, backgroundTiles.mapDoors, npcFemale3, playerLoc, "Civilian", "npcFemale3", spawnCount,
                        npcInteraction, actionToTake, actionTime, actionPauseTime));


                    npcTypeToSpawn++; // the type of npc to spawn next
                }
                else if (spawn.Key.Contains("Wave_Spawn")) // spawn wave spawning enemies
                {
                    spawnCount++;
                    npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs, 
                        backgroundTiles.mapLadders, backgroundTiles.mapDoors, alienLeech, playerLoc, "Wave", "alienLeech", spawnCount));
                    // spawn different npc types per spawn location

                    //spawnCount++;
                    //npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects,
                    //backgroundTiles.mapStairs, backgroundTiles.mapLadders, backgroundTiles.mapDoors, alienwateva, playerLoc, "Wave", spawnCount));
                }
                else if (spawn.Key.Contains("Scene"))
                {
                    // the rectangle of the current scene in the new map
                    currentSceneArea = mapObjects[spawn.Key];
                    drawCurrentScene = true;
                }
            }

            // initialize npcs
            for (int i = 0; i < npcList.Count; i++)
                npcList[i].Initialize();
        }

        // / <summary>
        // / UnloadContent will be called once per game and is the place to unload
        // / game-specific content.
        // / </summary>

        protected override void UnloadContent()
        {
            //  TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// use the windows clipcursor function
        /// </summary>
        /// <param name="rect">The area the cursor will be clipped to</param>
        [DllImport("user32.dll")]
        static extern void ClipCursor(ref Rectangle rect);


        // / <summary>
        // / Allows the game to run logic such as updating the world,
        // / checking for collisions, gathering input, and playing audio.
        // / </summary>
        // / <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            switch (gameState)
            {
                case GameState.MenuSong:
                    {
                        //sounds.Update(gameTime, "menuSong");
                        gameState = GameState.MainMenu;
                        Mouse.SetPosition(newButtonRect.Center.X, newButtonRect.Center.Y);
                        break;
                    }
                case GameState.MainMenu:
                    {
                        UpdateMainMenu(gameTime);
                        break;
                    }

                case GameState.Gameplay:
                    {
                        //hide the mouse and cursor
                        IsMouseVisible = false;
                        cursor.DrawCursor = false;

                        ///detect when cursor leaves viewport bounds
                        //prevMousePos = newMousePos;
                        //newMousePos = new Vector2(Mouse.GetState().X + player.camera.Position.X, Mouse.GetState().Y + player.camera.Position.Y);
                        //if (player.camera.ViewportWorldBoundry().Right < newMousePos.X)
                        //    cursor.WithinViewportBoundsX = false;
                        //else
                        //    cursor.WithinViewportBoundsX = true;

                        //if (player.camera.ViewportWorldBoundry().Bottom < newMousePos.Y)
                        //    cursor.WithinViewportBoundsY = false;
                        //else
                        //    cursor.WithinViewportBoundsY = true;

                        ///clip the cursor to viewport bounds
                        //Rectangle rect = player.camera.ViewportWorldBoundry();
                        //ClipCursor(ref rect);

                        // update the game cursor and mouse
                        if (showMode)
                            cursor.Update(player.camera.Position, backgroundTiles.buildMode, menu.isAnimMenuOpen, false);
                        // don't hide the mouse when not showing build/farm menu
                        else
                            cursor.Update(player.camera.Position, false, false, false);

                        // check if the player has exited game
                        currentKeyboardState = Keyboard.GetState();
                        if (IsKeyPressed(Keys.Escape) && !pauseMenuFlag)
                        {
                            pauseMenuFlag = true;
                        }
                        else if (IsKeyPressed(Keys.Escape) && pauseMenuFlag)
                        {
                            pauseMenuFlag = false;
                        }
                        if (pauseMenuFlag && IsKeyPressed(Keys.RightControl) || IsKeyPressed(Keys.LeftControl))
                        {
                            gameState = GameState.EndOfGame;
                        }
                        
                        if (!pauseMenuFlag && !deathNotify && !winNotify)
                            UpdateGameplay(gameTime);

                        // //updating the viewport creates chaos!
                        //GraphicsDevice.Viewport = new Viewport(player.camera.ViewportWorldBoundry().X, player.camera.ViewportWorldBoundry().Y,
                        //    player.camera.ViewportWorldBoundry().Width, player.camera.ViewportWorldBoundry().Height);
                        

                        // player has died and is exiting game
                        if (deathNotify && (IsKeyPressed(Keys.E) || IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.Escape)))
                        {
                            gameState = GameState.EndOfGame;
                        }
                        // player has killed the bounty, finished mission 1 and has won
                        else if (npcList.Count == 0 && mission1 && talkedToBounty1)
                        {
                            winNotify = true;
                            if (IsKeyPressed(Keys.E) || IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.Escape))
                            {
                                gameState = GameState.EndOfGame;
                            }
                        }
                        break;
                    }

                case GameState.EndOfGame:
                    {
                        UpdateEndOfGame(gameTime);
                        break;
                    }
            }
        }

        public void UpdateMainMenu(GameTime gameTime)
        {
            cursor.Update(new Vector2(0, 0), false, false, true);

            if (menuCounter < GraphicsDevice.Viewport.Width + 100)
                menuCounter += .2f;
            else
                menuCounter = 0;
            background.Update(gameTime, GraphicsDevice, sounds,new Vector2(0, 0), false);
            //exhaustAnim.Update(gameTime, new Vector2(128 + menuCounter, 685), .1f);
            //exhaustAnim2.Update(gameTime, new Vector2(128 + menuCounter, 712), .1f);

            if (newButtonRect.Intersects(cursor.cursorRect) && IsMousePressed("LeftButton"))
            {
                gameState = GameState.Gameplay;

                // begin new game and load first map
                sounds.Update(gameTime, "stopSong", background._mapName);
                sounds.Update(gameTime, "stopSound", background._mapName);
                ambienceToPlay = "starshipAmbience";
                sounds.Update(gameTime, ambienceToPlay, background._mapName);
                background.celestialObjectToDraw ="earth";
                background.distanceToAdd = 5; // set initial speed of ship
                LoadNewMap(mapsToLoad["personalStarship"], 1, gameTime);

                // create player
                player = new Player(GraphicsDevice, cursor.worldPosition,
                    backgroundTiles.mapPlatforms, backgroundTiles.mapObjects,
                    backgroundTiles.mapStairs, backgroundTiles.mapLadders, backgroundTiles.mapDoors, playerType);
                // load default weapon data to player
                player.SwitchWeapon(pistol);
                currentSoundFromPool = pistol._gunShotSound;
                gunShotAnim = pistol._gunShotAnim;
                // give player some ammo
                droppedNpcItems["pistolAmmo"].Add(player.PlayerRect);
                droppedNpcItems["pistolAmmo"].Add(player.PlayerRect);
                // player starts smoking
                player.isSmoking = true;
                
                // init player
                player.Initialize();

                exitMenu = true;
            }
            else if (loadButtonRect.Intersects(cursor.cursorRect) && IsMousePressed("LeftButton"))
            {
                //gameState = GameState.Scoreboard;
            }
            else if (exitButtonRect.Intersects(cursor.cursorRect) && IsMousePressed("LeftButton"))
            {
                gameState = GameState.EndOfGame;
            }

            // draw rain on menu
            if (!showMenu && gameState == GameState.MainMenu)
            {
                Rectangle viewPortBounds = new Rectangle(-50, -50, GraphicsDevice.Viewport.Bounds.Width + 400, GraphicsDevice.Viewport.Bounds.Height + 50);
                if (raindrops.Count < 5000) // add new raindrops if below count
                {
                    bool willImpact = false;
                    if (randomNumberGen.NextNumber(0, 2) == 0)
                        willImpact = true;
                    raindrops.Add(new MovingObject(raindropTexture, 32, viewPortBounds, willImpact));
                }
                for (int i = 0; i < raindrops.Count; i++) // remove raindrops that hit a platform
                {
                    raindrops[i].Update(gameTime, new Vector2(50, 1000), viewPortBounds, backgroundTiles.mapPlatforms);
                    if (!raindrops[i].isActive)
                    {
                        if (raindrops[i].isImpact) // raindrop hit platform, show impact animation and remove from list
                            raindropImpactAnim.AddAnimation(new Vector2(raindrops[i].rectangle.Location.ToVector2().X,
                                raindrops[i].rectangle.Location.ToVector2().Y + 2),
                                Color.White);
                        raindrops.RemoveAt(i);
                    }
                }
                sounds.Update(gameTime, "rainAmbience", background._mapName);
            }
            else
            {
                raindrops.Clear();
            }
        }

        public void UpdateGameplay(GameTime gameTime)
        {
            if (exitMenu)
            {
                Mouse.SetPosition(GraphicsDevice.Viewport.Bounds.Width / 2, GraphicsDevice.Viewport.Bounds.Height / 2);
                exitMenu = false;
            }

            sounds.Update(gameTime, "", background._mapName);

            // if the starship is landing, do no allow player movement
            if (sounds.IsSoundEffectPlaying())
            {
                player.allowMovement = false;
            }
            else
            {
                player.allowMovement = true;
            }

            // on the first mission at start of game, player receives a call
            if (gameTime.TotalGameTime.Seconds >= 7.5f && mission1 && !talkedToHoloCall1 && !answerCall)
                incomingCall = true;

            // player is receiving a call and the phone rings
            if (incomingCall)
            {
                currentObjective = "Answer the call";
                sounds.Update(gameTime, "incomingCallRing", background._mapName);
            }
            // player is in a conversation
            else if (!incomingCall && sceneToDraw > 0)
            {
                sounds.Update(gameTime, "incomingCallAnswered", background._mapName);
                if (IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.E))
                {
                    sounds.Update(gameTime, "menuSelect", backgroundTiles.mapName);
                }
            }
            // player is not in a conversation
            else if (sceneToDraw == 0)
            {

                if (mission1)
                {
                    if (mission1Objective1ConvoDone && !mission1Objective1Finish) // first objective of mission 1 has been triggered
                        currentObjective = "Enter the storage room and clear it";
                    else if (talkedToHoloCall1 && backgroundTiles.mapName != "asteroidColony")
                        currentObjective = "Travel to Vesta";
                    else if (talkedToHoloCall1 && backgroundTiles.mapName == "asteroidColony" && !talkedToBartender)
                        currentObjective = "Enter The Drunken Cyborg bar";
                    else if (talkedToBartender)
                        currentObjective = "Leave the bar";
                }
                else
                    currentObjective = "";
            }

            // -------------play various menu sound effects
            if (showMode)
            {
                if ((IsKeyPressed(Keys.G) && !backgroundTiles.buildMode && !menu.isAnimMenuOpen) ||
                    (IsKeyPressed(Keys.F) && !menu.isAnimMenuOpen && !backgroundTiles.buildMode))
                {
                    Mouse.SetPosition(150, 790);
                    sounds.Update(gameTime, "menuOpen", background._mapName);
                }
                else if ((IsKeyPressed(Keys.G) && backgroundTiles.buildMode && !menu.isAnimMenuOpen) ||
                    (IsKeyPressed(Keys.F) && menu.isAnimMenuOpen && !backgroundTiles.buildMode))
                {
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    sounds.Update(gameTime, "menuClosed", background._mapName);
                }
                else if (IsMousePressed("LeftButton") && backgroundTiles.previewBuildItem && !menu.isAnimMenuOpen)
                {
                    sounds.Update(gameTime, "menuBuilt", background._mapName);
                }
                else if (backgroundTiles.menuClick && !menu.isAnimMenuOpen)
                {
                    sounds.Update(gameTime, "menuSelect", background._mapName);
                }
                else if (IsMousePressed("RightButton") && backgroundTiles.previewBuildItem && !menu.isAnimMenuOpen)
                {
                    sounds.Update(gameTime, "menuSelect", background._mapName);
                }
                backgroundTiles.allowBuildMode = true;
            }
            else
                backgroundTiles.allowBuildMode = false;

            // --------------------------------------INTERACTION WITH NPCs----------------------------
            for (int i = 0; i < npcList.Count; i++)
            {
                if (npcList[i]._interaction != "" && npcList[i].npcRect.Intersects(player.PlayerRect))
                {
                    interactionRectangle = npcList[i].npcRect;
                    currentInteraction = npcList[i]._interaction;

                    // start the next scene for npc interaction
                    if (IsKeyPressed(Keys.E) && sceneToDraw == 0 && drawCurrentScene)
                    {
                        sceneToDraw = 1;
                    }
                    break;
                }
                else
                {
                    interactionRectangle = Rectangle.Empty;
                    currentInteraction = "";
                }
            }

            // -----------------------------------MAP LOADING (set mapToLoad for LoadNextMap() func)----------------------------------------
            string mapToLoad = "";
            // don't check for map loading when player receives a call or is on objective
            if (sceneToDraw == 0 && !incomingCall)
            {
                foreach (KeyValuePair<string, Rectangle> mapObject in backgroundTiles.mapObjects)
                {
                    if (mapObject.Key.Contains("Load") && mapObject.Value.Intersects(player.PlayerRect) &&
                        background.celestialObjectToDraw != "jumpToFTL" && !background.jumpToFTL) // determine if player is on a map loading object
                    {
                        // the mapObject string ends with a number, this finds the index of that number
                        char[] charToFind = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                        int index = mapObject.Key.LastIndexOfAny(charToFind);

                        // find the spawn point number of the mapObject string
                        playerSpawnToUse = 1;
                        if (index > 0)
                        {
                            playerSpawnToUse = Int32.Parse(mapObject.Key.Substring(index));
                        }

                        index = mapObject.Key.IndexOf('_'); // locate substring

                        mapToLoad = mapObject.Key.Substring(index + 1); // store substring
                        mapToLoad = mapToLoad.TrimEnd(charToFind);
                        interactionRectangle = mapObject.Value;
                        showInteractNotification = true;
                        break;
                    }
                    else
                    {
                        mapToLoad = "";
                        playerSpawnToUse = 1;
                        showInteractNotification = false;
                    }
                }
                
                if (!IsKeyPressed(Keys.E))
                    playerSpawnToUse = 1; // prevent dev map loading shortcuts from crashing game by resetting this when intersecting an exit and not 'using' it

                if ((IsKeyPressed(Keys.E)) || IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.Escape) || IsMousePressed("RMB")) // close the travel map
                {
                    openTravelMap = false;
                    travelMapHighlightAsteroidColony = false;
                    travelMapHighlightMercury = false;
                    travelMapHighlightVenus = false;
                    travelMapHighlightEarth = false;
                    travelMapHighlightMars = false;
                }
                // only load the queued map if player is near the travel console and map is ready (allowing the load to take place even after player has entered another 
                // room inside the personal starship)
                if (mapToLoad == "travelMap" && background.readyToLoadMap &&  background._mapName == "personalStarship")
                {
                    if (mission1Objective1Start && mission1Objective1Finish)
                    {
                        // load the queued map
                        if (IsKeyPressed(Keys.E))
                            QueueNextMap(gameTime, true, new List<string>(), "asteroidColony", 1, "");
                    }
                }
                if ((mapToLoad == "travelMap" && IsKeyPressed(Keys.E) && (!mission1Objective1ConvoDone || mission1Objective1Finish)) || openTravelMap)
                {
                    // play sound when first opening the map
                    if (IsKeyPressed(Keys.E))
                        sounds.Update(gameTime, "menuOpen", background._mapName);

                    // draw the game cursor to interact with map
                    cursor.DrawCursor = true;

                    int gridIncrement = 32;
                    int size = 16; // size of buttons on travel map
                    travelMapPos = new Vector2(player.location.X + 135, player.location.Y);

                    travelMapAsteroidColony = new Rectangle((int)(travelMapPos.X + (gridIncrement * 5.5f)), (int)travelMapPos.Y + (gridIncrement * 3), size, size);
                    travelMapMercury = new Rectangle((int)(travelMapPos.X + (gridIncrement * 0f)), (int)(travelMapPos.Y + (gridIncrement * 2f)), size, size);
                    travelMapVenus = new Rectangle((int)(travelMapPos.X + (gridIncrement * 2.5f)), (int)(travelMapPos.Y + (gridIncrement * 2.5f)), size, size);
                    travelMapEarth = new Rectangle((int)(travelMapPos.X + (gridIncrement * 4f)), (int)(travelMapPos.Y + (gridIncrement * 1f)), size, size);
                    travelMapMars = new Rectangle((int)(travelMapPos.X + (gridIncrement * 6f)), (int)(travelMapPos.Y + (gridIncrement * 0f)), size, size);
                    // travel map is open, draw interaction highlights and load a map if the player has clicked on an interaction
                    openTravelMap = true;
                    Rectangle cursorRect = new Rectangle(cursor.cursorRect.X + (int)player.camera.Position.X, cursor.cursorRect.Y + (int)player.camera.Position.Y,
                        cursor.cursorRect.Width, cursor.cursorRect.Height);

                    if (cursorRect.Intersects(travelMapAsteroidColony))
                    {
                        travelMapHighlightAsteroidColony = true;
                        if (IsMousePressed("LMB"))
                        {
                            List<string> soundsToPlay = new List<string>();
                            soundsToPlay.Add("rainAmbience");
                            soundsToPlay.Add("starshipLanding");
                            QueueNextMap(gameTime, false, soundsToPlay, "asteroidColony", 1, "");
                            sounds.Update(gameTime, "menuOpen", background._mapName);
                            openTravelMap = false;
                            travelMapHighlightAsteroidColony = false;
                            travelMapHighlightMercury = false;
                            travelMapHighlightVenus = false;
                            travelMapHighlightEarth = false;
                            travelMapHighlightMars = false;
                        }
                    }
                    else if (cursorRect.Intersects(travelMapMercury))
                    {
                        travelMapHighlightMercury = true;
                        if (IsMousePressed("LMB"))
                        {
                            //LoadNewMap(mapsToLoad["asteroidColony"], 1);

                            //ambienceToPlay = "rainAmbience";
                            //if (sounds.previousAmbientSound != ambienceToPlay) // only stop the sound if it is not the same sound to be played next
                            //    sounds.Update(gameTime, "stopSound");
                            //sounds.Update(gameTime, "stopSong");
                            sounds.Update(gameTime, "menuClosed", background._mapName);
                            //openTravelMap = false;
                            //travelMapHighlightAsteroidColony = false;
                            //travelMapHighlightMercury = false;
                            //travelMapHighlightVenus = false;
                            //travelMapHighlightEarth = false;
                            //travelMapHighlightMars = false;
                        }
                    }
                    else if (cursorRect.Intersects(travelMapVenus))
                    {
                        travelMapHighlightVenus = true;
                        if (IsMousePressed("LMB"))
                        {
                            //LoadNewMap(mapsToLoad["asteroidColony"], 1);

                            //ambienceToPlay = "rainAmbience";
                            //if (sounds.previousAmbientSound != ambienceToPlay) // only stop the sound if it is not the same sound to be played next
                            //    sounds.Update(gameTime, "stopSound");
                            //sounds.Update(gameTime, "stopSong");
                            sounds.Update(gameTime, "menuClosed", background._mapName);
                            //openTravelMap = false;
                            //travelMapHighlightAsteroidColony = false;
                            //travelMapHighlightMercury = false;
                            //travelMapHighlightVenus = false;
                            //travelMapHighlightEarth = false;
                            //travelMapHighlightMars = false;
                        }
                    }
                    else if (cursorRect.Intersects(travelMapEarth))
                    {
                        travelMapHighlightEarth = true;
                        if (IsMousePressed("LMB"))
                        {
                            //LoadNewMap(mapsToLoad["asteroidColony"], 1);

                            //ambienceToPlay = "rainAmbience";
                            //if (sounds.previousAmbientSound != ambienceToPlay) // only stop the sound if it is not the same sound to be played next
                            //    sounds.Update(gameTime, "stopSound");
                            //sounds.Update(gameTime, "stopSong");
                            sounds.Update(gameTime, "menuClosed", background._mapName);
                            //openTravelMap = false;
                            //travelMapHighlightAsteroidColony = false;
                            //travelMapHighlightMercury = false;
                            //travelMapHighlightVenus = false;
                            //travelMapHighlightEarth = false;
                            //travelMapHighlightMars = false;
                        }
                    }
                    else if (cursorRect.Intersects(travelMapMars))
                    {
                        travelMapHighlightMars = true;
                        if (IsMousePressed("LMB"))
                        {
                            //LoadNewMap(mapsToLoad["asteroidColony"], 1);

                            //ambienceToPlay = "rainAmbience";
                            //if (sounds.previousAmbientSound != ambienceToPlay) // only stop the sound if it is not the same sound to be played next
                            //    sounds.Update(gameTime, "stopSound");
                            //sounds.Update(gameTime, "stopSong");
                            sounds.Update(gameTime, "menuClosed", background._mapName);
                            //openTravelMap = false;
                            //travelMapHighlightAsteroidColony = false;
                            //travelMapHighlightMercury = false;
                            //travelMapHighlightVenus = false;
                            //travelMapHighlightEarth = false;
                            //travelMapHighlightMars = false;
                        }
                    }
                    else // stop highlighting
                    {
                        travelMapHighlightAsteroidColony = false;
                        travelMapHighlightMercury = false;
                        travelMapHighlightVenus = false;
                        travelMapHighlightEarth = false;
                        travelMapHighlightMars = false;
                    }

                }
                else if ((enableDevHotKeys && currentKeyboardState.IsKeyDown(Keys.LeftShift) && IsKeyPressed(Keys.B)))
                {
                    ambienceToPlay = "starshipAmbience";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    sounds.Update(gameTime, "stopSong", background._mapName);

                    background.NextCelestialObjectToDraw = "spaceStation";
                    LoadNewMap(mapsToLoad["spaceStation"], playerSpawnToUse, gameTime);
                }
                else if ((enableDevHotKeys && IsKeyPressed(Keys.B)) || (mapToLoad == "bar" && IsKeyPressed(Keys.E)))
                {
                    ambienceToPlay = "barAmbience";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    //sounds.Update(gameTime, "stopSong", background._mapName);

                    LoadNewMap(mapsToLoad["bar"], playerSpawnToUse, gameTime);
                }
                else if ((enableDevHotKeys && currentKeyboardState.IsKeyDown(Keys.LeftShift) && IsKeyPressed(Keys.C)) || (mapToLoad == "playerRoom" && IsKeyPressed(Keys.E)))
                {
                    ambienceToPlay = "";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    sounds.Update(gameTime, "stopSong", background._mapName);

                    // store the current celestial object when exiting the starship
                    prevCelestialObject = background.celestialObjectToDraw;
                    LoadNewMap(mapsToLoad["playerRoom"], playerSpawnToUse, gameTime);
                }
                else if ((enableDevHotKeys && currentKeyboardState.IsKeyDown(Keys.LeftShift) && IsKeyPressed(Keys.V)) || (mapToLoad == "playerRoom2" && IsKeyPressed(Keys.E)))
                {
                    ambienceToPlay = "";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    sounds.Update(gameTime, "stopSong", background._mapName);
                    sounds.Update(gameTime, "starshipAmbience", background._mapName);

                    LoadNewMap(mapsToLoad["playerRoom2"], playerSpawnToUse, gameTime);
                }
                else if ((enableDevHotKeys && IsKeyPressed(Keys.C)) || (mapToLoad == "asteroidColony" && IsKeyPressed(Keys.E)))
                {
                    ambienceToPlay = "rainAmbience";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    sounds.Update(gameTime, "stopSong", background._mapName);

                    LoadNewMap(mapsToLoad["asteroidColony"], playerSpawnToUse, gameTime);
                }
                else if ((enableDevHotKeys && IsKeyPressed(Keys.V)) || (mapToLoad == "personalStarship" && IsKeyPressed(Keys.E)))
                {
                    ambienceToPlay = "starshipAmbience";
                    if (sounds.previousAmbientSound != ambienceToPlay)
                        sounds.Update(gameTime, "stopSound", background._mapName);
                    sounds.Update(gameTime, "stopSong", background._mapName);

                    // reset the celestial object when re-entering the starship
                    background.celestialObjectToDraw = prevCelestialObject;
                    LoadNewMap(mapsToLoad["personalStarship"], playerSpawnToUse, gameTime);
                }
            }

            //play the ambient sound
            sounds.Update(gameTime, ambienceToPlay, background._mapName);

            //  update the background to move with player
            background.Update(gameTime, GraphicsDevice, sounds,player.location, true, backgroundTiles.mapName);
            background.parallaxStart = backgroundTiles.parallaxStart;

            if (background._mapName == "spaceStation")
            {
                // update the engine exhaust animators
                int startY = 610;
                int startX = 680;
                Vector2 v1 = new Vector2(startX, startY);
                Vector2 v2 = new Vector2(startX, startY+175);
                Vector2 v3 = new Vector2(startX, startY+175*2);
                exhaustAnim1.Update(gameTime, v1, .6f);
                exhaustAnim2.Update(gameTime, v2, .6f);
                exhaustAnim3.Update(gameTime, v3, .6f);
            }


            if (!playerGuns.Contains(pistol))
                playerGuns.Add(pistol);
            if (!playerGuns.Contains(smg))
                playerGuns.Add(smg);
            if (!playerGuns.Contains(shotgun))
                playerGuns.Add(shotgun);

            // ------------------------------------------switch the player's gun-------------------------------------
            

            if ((IsKeyPressed(Keys.D1) || IsKeyPressed(Keys.D2) || IsKeyPressed(Keys.D3)) || 
                (MouseScrollValue() != 0 && currentMouseState.RightButton == ButtonState.Pressed))
            {
                // store used ammo in gun before switching weapons
                if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Pistol.ToString().ToLower())
                    pistol._bulletsInClip = player.currentlyEquippedGun._bulletsInClip;
                else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Smg.ToString().ToLower())
                    smg._bulletsInClip = player.currentlyEquippedGun._bulletsInClip;
                else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Shotgun.ToString().ToLower())
                    shotgun._bulletsInClip = player.currentlyEquippedGun._bulletsInClip;


                if (IsKeyPressed(Keys.D1))
                {
                    equippedPlayerGunIndex = 0;
                }
                else if (IsKeyPressed(Keys.D2))
                {
                    equippedPlayerGunIndex = 1;
                }
                else if (IsKeyPressed(Keys.D3))
                {
                    equippedPlayerGunIndex = 2;
                }
                // determine which weapon the player wants to equip next
                // scroll up
                else if (MouseScrollValue() > 0)
                {
                    if (equippedPlayerGunIndex < playerGuns.Count - 1)
                        equippedPlayerGunIndex++;
                    else
                        equippedPlayerGunIndex = 0;
                }
                // scroll down
                else if (MouseScrollValue() < 0)
                {
                    if (equippedPlayerGunIndex > 0)
                        equippedPlayerGunIndex--;
                    else
                        equippedPlayerGunIndex = playerGuns.Count - 1;
                }
                
                // equip the player's desired weapon
                //if (playerGuns[equippedPlayerGunIndex]._gunType == GunType.LazerGun.ToString())
                //{
                //    currentSoundFromPool = lazerGun._gunShotSound;
                //    gunShotAnim = lazerGun._gunShotAnim;
                //    player.SwitchWeapon(lazerGun);
                //}
                if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.NoGun.ToString().ToLower())
                {
                    currentSoundFromPool = pistol._gunShotSound;
                    gunShotAnim = pistol._gunShotAnim;
                    player.SwitchWeapon(pistol);
                }
                else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Pistol.ToString().ToLower())
                {
                    currentSoundFromPool = pistol._gunShotSound;
                    gunShotAnim = pistol._gunShotAnim;
                    player.SwitchWeapon(pistol);
                }
                else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Smg.ToString().ToLower())
                {
                    currentSoundFromPool = smg._gunShotSound;
                    gunShotAnim = smg._gunShotAnim;
                    player.SwitchWeapon(smg);
                }
                //else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Rocket.ToString().ToLower())
                //{
                //    currentSoundFromPool = rocket._gunShotSound;
                //    gunShotAnim = rocket._gunShotAnim;
                //    player.SwitchWeapon(rocket);
                //}
                else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Shotgun.ToString().ToLower())
                {
                    currentSoundFromPool = shotgun._gunShotSound;
                    gunShotAnim = shotgun._gunShotAnim;
                    player.SwitchWeapon(shotgun);
                }
                //else if (playerGuns[equippedPlayerGunIndex]._gunType.ToLower() == GunType.Sniper.ToString().ToLower())
                //{
                //    currentSoundFromPool = sniper._gunShotSound;
                //    gunShotAnim = sniper._gunShotAnim;
                //    player.SwitchWeapon(sniper);
                //}
            }

                // if player is firing, play sounds
            if (player.isFiring && player.currentlyEquippedGun._bulletsInClip >= 0)
            {
                sounds.Update(gameTime, currentSoundFromPool, background._mapName);
                // after playing sound effect for semi-auto weapon, stop
                if (!player.isGunAutomatic)
                    player.isFiring = false;
            }

            // mute the game volume
            if (IsKeyPressed(Keys.M))
            {
                sounds.AdjustVolume();
            }

            // play walking sound effect if player is moving and not in dialogue
            if (sceneToDraw > 0 && (currentKeyboardState.IsKeyDown(Keys.A) || currentKeyboardState.IsKeyDown(Keys.D)))
            {
                sounds.Update(gameTime, "playerWalk", background._mapName);
            }
            // player climbing sound effectif player is climbing and not in dialogue
            else if (sceneToDraw > 0 && ((currentKeyboardState.IsKeyDown(Keys.W) || currentKeyboardState.IsKeyDown(Keys.S)) && player.IsOnLadder()))
            {
                sounds.Update(gameTime, "playerWalk", background._mapName);
            }
            else
            {
                sounds.Update(gameTime, "playerStationary", background._mapName);
            }

            // remove the preview placement
            if (showMode && (backgroundTiles.buildMode || !menu.isAnimMenuOpen))
                menu.previewPlacement = false;
            
            // update the player and the trigger list in backgroundTiles
            player.Update(gameTime, npcList, backgroundTiles.buildMode, menu.isAnimMenuOpen);
            // player cocked the weapon (and is a weapon type that needs cocked), play the sound
            if (player.isWeaponCocked && IsMousePressed("LMB") && 
                currentMouseState.RightButton == ButtonState.Pressed && player.currentlyEquippedGun._gunType.ToLower().Contains("shotgun"))
            {
                sounds.Update(gameTime, "shotgunPump", background._mapName);
            }

            // update viewport bounds
            viewPortBounds = new Rectangle((int)(player.location.X - GraphicsDevice.Viewport.Height * .75f),
                (int)(player.location.Y - GraphicsDevice.Viewport.Height * .5f),
                GraphicsDevice.Viewport.Bounds.Width,
                GraphicsDevice.Viewport.Bounds.Height);

            // --------------draw raindrops and smog----------------------------
            if (backgroundTiles.mapName == "asteroidColony")
            {
                // ------_RAIN ---------
                if (raindrops.Count < 5000) // add new raindrops if below count
                {
                    bool willImpact = false;
                    if (randomNumberGen.NextNumber(0, 2) == 0)
                        willImpact = true;
                    raindrops.Add(new MovingObject(raindropTexture, 32, viewPortBounds, willImpact));
                }
                for (int i = 0; i < raindrops.Count; i++) // remove raindrops that hit a platform
                {
                    raindrops[i].Update(gameTime, new Vector2(50, 1000), viewPortBounds, backgroundTiles.mapPlatforms);
                    if (!raindrops[i].isActive)
                    {
                        if (raindrops[i].isImpact) // raindrop hit platform, show impact animation and remove from list
                            raindropImpactAnim.AddAnimation(new Vector2(raindrops[i].rectangle.Location.ToVector2().X, 
                                raindrops[i].rectangle.Location.ToVector2().Y + 2), 
                                Color.White, .5f);
                        raindrops.RemoveAt(i);
                    }
                }

                // ---------- SMOG -------------
                viewPortBounds.Y += 300;
                viewPortBounds.Height -= 900;
                if (smog.Count < 0) // add new smog if below count
                {
                    smog.Add(new MovingObject(smogTexture, 32, viewPortBounds, false, false));
                }
                for (int i = 0; i < smog.Count; i++) // remove smog that hit a platform
                {
                    smog[i].Update(gameTime, new Vector2(-randomNumberGen.NextNumber(100, 1000), 0), viewPortBounds, new List<Rectangle>());
                    if (!smog[i].isActive)
                    {
                        smog.RemoveAt(i);
                    }
                }
            }
            else
            {
                raindrops.Clear();
                smog.Clear();
            }

            // update the effectsAnimators
            foreach (Animator animator in effectsAnimators)
            {
                animator.Update(gameTime);
            }

            if (!mission1Objective1Start && mission1 && backgroundTiles.mapName == "personalStarship" && background.celestialObjectToDraw == "asteroidColony" &&
                !background.jumpToFTL)
            {
                // set first encounter flags for mission 1
                mission1Objective1Finish = false;
                mission1Objective1Start = true;
            }

            // on the first mission, player has killed all npcs, act accordingly
            if (mission1 && npcList.Count == 0 && backgroundTiles.mapName == "playerRoom")
            {
                mission1Objective1Finish = true;
            }

            // player has finished this part of the mission, despawn npcs within corresponding area
            if (mission1 && mission1Objective1Finish && backgroundTiles.mapName == "playerRoom")
            {
                npcList.Clear();
            }

            /// --------------------------------------------- switch gameplay phases ---------------------------------------------
            // wave based shiprooms map
            if (backgroundTiles.mapName == "ShipRooms")
            {
                switch (gameplayPhase)
                {
                    case GameplayPhase.Combat:
                        {
                            // NPC spawning and updating
                            for (int i = 0; i < npcSpawnCount; i++)
                            {
                                //if (npcList.Count < npcSpawnCount && !player.addCorpsePosToList)
                                //    npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects,
                                //    backgroundTiles.mapStairs, backgroundTiles.mapLadders, backgroundTiles.mapDoors, alienLeech, player.location, "Wave"));
                            }
                            List<Rectangle> currentNpcRects = new List<Rectangle>();
                            // update all NPCs in list
                            for (int i = 0; i < npcList.Count; i++)
                            {
                                npcList[i].Update(gameTime, player);
                                for (int k = 0; k < currentNpcRects.Count; k++)
                                {
                                    if (npcList[i].npcRect.X == currentNpcRects[k].X)
                                    {
                                        //if (!npcList[i].IsAgainstWall(gameTime.ElapsedGameTime.Milliseconds) && 
                                        //    npcList[i].npcDirection == npcList[k].npcDirection &&
                                        //    npcList[k].outOfCombatWalkDirection != 0) // keep npcs from stacking when walking in same direction
                                        //{
                                        //    npcList[i].location.X -= 10;
                                        //}
                                        if (npcList[i].npcDirection == npcList[k].npcDirection && npcList[i].outOfCombatWalkDirection != 0)
                                            npcList[i].isAtNpc = true;
                                    }
                                    else if (npcList[i].npcRect.X + npcList[i].spriteSize == currentNpcRects[k].X + npcList[i].spriteSize)
                                    {
                                        //if (!npcList[i].IsAgainstWall(gameTime.ElapsedGameTime.Milliseconds) &&
                                        //    npcList[i].npcDirection == npcList[k].npcDirection &&
                                        //    npcList[k].outOfCombatWalkDirection != 0) // keep npcs from stacking when walking in same direction
                                        //{
                                        //   npcList[i].location.X += 10;
                                        //}
                                        if (npcList[i].npcDirection == npcList[k].npcDirection && npcList[i].outOfCombatWalkDirection != 0)
                                            npcList[i].isAtNpc = true;
                                    }
                                    else
                                        npcList[i].isAtNpc = false;
                                }
                                currentNpcRects.Add(npcList[i].npcRect);
                                //if (npcList[i].playerWasHit)
                                //{
                                //    player.playerHealth -= npcList[i].meleeDamage;
                                //    npcList[i].playerWasHit = false;
                                //}
                            }

                            if (player.npcImpactEffects) // bullet hit an npc
                            {
                                bloodAnim.AddAnimation(player.projectilePos, Color.White);
                            }
                            // if the player killed an npc, draw a body and dropped items at the position of npc's death
                            if (player.addCorpsePosToList)
                            {
                                // chance of an item dropping (2 being a 1 in 2 chance, or 50%)
                                int itemDropChance = 2;
                                int random = randomNumberGen.NextNumber(0, droppedNpcItems.Count * itemDropChance);
                                

                                Rectangle npcCorpse = new Rectangle(player.npcDeathLocation.X, player.npcDeathLocation.Y, 64, 64);
                                // determine which items to drop for player
                                if (random == 0 && !droppedNpcItems["shotgunAmmo"].Contains(player.npcDeathLocation))
                                {
                                    droppedNpcItems["shotgunAmmo"].Add(npcCorpse);
                                }
                                else if (random == 1 && !droppedNpcItems["smgAmmo"].Contains(player.npcDeathLocation))
                                {
                                    droppedNpcItems["smgAmmo"].Add(npcCorpse);
                                }
                                else if (random == 2 && !droppedNpcItems["pistolAmmo"].Contains(player.npcDeathLocation))
                                {
                                    droppedNpcItems["pistolAmmo"].Add(npcCorpse);
                                }
                                npcSpawnCount--; // lower total spawn count (or enemies will continue to spawn after one has died)
                                string animOrientation = player.currentAnimationFrame;
                                //player.deadNpcType
                                player.addCorpsePosToList = false; // reset npc death flag
                                sounds.Update(gameTime, "gutCrunch", background._mapName);
                            }

                            if (npcList.Count < 1) // leave combat, enter noncombat
                            {
                                gameplayPhase = GameplayPhase.Noncombat;
                            }
                            // uncomment to skip combat phase
                            //gameplayPhase = GameplayPhase.Noncombat;
                            break;
                        }
                    case GameplayPhase.Noncombat:
                        {
                            // play room songs
                            if ((backgroundTiles.whichSongToPlay != sounds.previousType || sounds.isTransitioningSongs))
                                sounds.Update(gameTime, backgroundTiles.whichSongToPlay, background._mapName);

                            if (gameplayPhaseTimer.Update(300)) // leave noncombat, enter transition
                            {
                                currentWave++; // increment the current wave
                                player.addCorpsePosToList = false; // reset npc death flag
                                gameplayPhase = GameplayPhase.Transition;
                                // play combat music as next wave begins
                                if (currentWave % 2 == 0)
                                    sounds.Update(gameTime, "transitionSong", background._mapName);
                                else
                                    sounds.Update(gameTime, "combatSong", background._mapName);
                                //player._npcDeathAnimList.Clear();
                                npcList.RemoveRange(0, npcList.Count);
                                backgroundTiles.inCombatPhase = false;
                                player.addCorpsePosToList = false; // reset npc death flag
                            }
                            break;
                        }
                    case GameplayPhase.Transition:
                        {
                            if (gameplayPhaseTimer.Update(300)) // leave transition, enter combat
                            {
                                // clear click event collections
                                clickEventCoords.Clear();
                                clickEventFlag.Clear();
                                npcSpawnCount = 5; // set number of npcs to spawn in next wave
                                if (currentWave % 5 == 0) // every 5th wave, increase npc damage
                                {
                                    newNpcDamage += 10;
                                    for (int i = 0; i < npcList.Count; i++)
                                        npcList[i].meleeDamage += newNpcDamage;
                                }
                                if (currentWave == 0)
                                    npcSpawnCount++; // add npcs every wave
                                gameplayPhase = GameplayPhase.Combat;
                                //player._npcDeathAnimList.Clear();
                                backgroundTiles.inCombatPhase = true;
                            }
                            break;
                        }
                }
            }
            // not in wave based combat
            else
            { 
                // pass time
                if (enableDevHotKeys && IsKeyPressed(Keys.X))
                {
                    // reset event flags
                    clickEventCoords.Clear();
                    clickEventFlag.Clear();
                    // increment time counter for plants to proceed to next growth phase
                    currentWave++;
                }
                else if (enableDevHotKeys && IsKeyPressed(Keys.OemPlus))
                {
                    droppedNpcItems["shotgunAmmo"].Add(player.PlayerRect);
                    droppedNpcItems["smgAmmo"].Add(player.PlayerRect);
                    droppedNpcItems["pistolAmmo"].Add(player.PlayerRect);
                    droppedNpcItems["shotgunAmmo"].Add(player.PlayerRect);
                    droppedNpcItems["smgAmmo"].Add(player.PlayerRect);
                    droppedNpcItems["pistolAmmo"].Add(player.PlayerRect);
                }
                //else if (IsKeyPressed(Keys.OemMinus))
                //{
                //    npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                //        backgroundTiles.mapLadders, backgroundTiles.mapDoors, alienLeech, player.location, "Wave", "alienLeech", 0));
                //}
                else if (enableDevHotKeys && IsKeyPressed(Keys.OemMinus))
                {
                    player.playerHealth = player.playerTotalHealth;
                    //npcList.Add(new NPC(GraphicsDevice, backgroundTiles.mapPlatforms, backgroundTiles.mapObjects, backgroundTiles.mapStairs,
                    //    backgroundTiles.mapLadders, backgroundTiles.mapDoors, alienLeech, player.location, "Wave", "alienLeech", 0));
                }
                // hide the HUD
                else if (IsKeyPressed(Keys.PrintScreen))
                {
                    if (showHud)
                        showHud = false;
                    else
                        showHud = true;
                }
                
                if (player.npcImpactEffects) // bullet hit an npc
                {
                    bloodAnim.AddAnimation(player.projectilePos, Color.White);
                }
                // on npc death, drop npc items and ammo for player pickup
                if (player.addCorpsePosToList)
                {
                    // chance of an item dropping (2 being a 1 in 2 chance, or 50%)
                    int itemDropChance = 2;
                    int random = randomNumberGen.NextNumber(0, droppedNpcItems.Count * itemDropChance);

                    Rectangle npcCorpse = new Rectangle(player.npcDeathLocation.X, player.npcDeathLocation.Y, 64, 64);
                    // determine which items to drop for player
                    if (random == 0 && !droppedNpcItems["shotgunAmmo"].Contains(player.npcDeathLocation))
                    {
                        droppedNpcItems["shotgunAmmo"].Add(npcCorpse);
                    }
                    else if (random == 1 && !droppedNpcItems["smgAmmo"].Contains(player.npcDeathLocation))
                    {
                        droppedNpcItems["smgAmmo"].Add(npcCorpse);
                    }
                    else if (random == 2 && !droppedNpcItems["pistolAmmo"].Contains(player.npcDeathLocation))
                    {
                        droppedNpcItems["pistolAmmo"].Add(npcCorpse);
                    }
                    npcSpawnCount--; // lower total spawn count (or enemies will continue to spawn after one has died)
                    string animOrientation = player.currentAnimationFrame;
                    //player.deadNpcType
                    player.addCorpsePosToList = false; // reset npc death flag
                    sounds.Update(gameTime, "gutCrunch", background._mapName);
                }

                for (int i = 0; i < npcList.Count; i++)
                {
                    npcList[i].Update(gameTime, player);

                    if (!talkedToBounty1 && talkedToBartender && sceneToDraw == 0 && background._mapName == "asteroidColony")
                    {
                        isPlayerInConversation = true;
                        sceneToDraw = 1;
                    }
                    else if (talkedToBounty1)
                    {
                        isPlayerInConversation = false;
                    }

                    if (npcList[i].isEnemy && !isPlayerInConversation)
                    {
                        npcList[i].isTalking = false;
                    }
                    else if (npcList[i].isEnemy && isPlayerInConversation) // break conversation and attack player
                    {
                        npcList[i].isTalking = true;
                    }

                    // npc gun shot sounds
                    if (npcList[i].isFiring)
                    {
                        sounds.Update(gameTime, "gunShot", background._mapName);
                        npcList[i].isFiring = false; // reset flag
                    }
                    // alien npc walking and attack sounds
                    if ((npcList[i]._name.ToLower().Contains("alien")))
                    {
                        if (npcList[i].agroRangeRect.Contains(player.PlayerRect))
                        {
                            if (npcList[i].inCombatWalking || npcList[i].outOfCombatWalking)
                                sounds.Update(gameTime, "alienWalk", background._mapName, false, i); 
                            else if (npcList[i].attackPlayer && npcList[i].isAtPlayer)
                                sounds.Update(gameTime, "alienAttack", background._mapName, false, i);
                        }
                        else
                        {
                            sounds.Update(gameTime, "alienWalk", background._mapName, true, i);
                            sounds.Update(gameTime, "alienAttack", background._mapName, true, i);
                        }

                        // set alien npc damage
                        npcList[i].meleeDamage = 1;
                        npcList[i].projectileDamage = 1;
                    }
                }
            }

            // draw projectile impact animations at point of collision, regardless of gameplay phase
            if (player.roomImpactEffects) // bullet hit surface of room
            {
                projectileImpactAnim.AddAnimation(new Vector2(player.projectilePos.X - 16, player.projectilePos.Y - 16), Color.White);
            }

            // determine if any triggers are being activated
            TriggerActivation(gameTime);

            // update background tiles
            backgroundTiles.Update(gameTime, player.camera.Position, menu.isAnimMenuOpen);

            // update the GUI Menu (which updates the menu's animators as well)
            menu.UpdateMenu(gameTime, backgroundTiles.tileObjects, backgroundTiles.buildMode, player.location, player.camera.Position, currentWave);

            // -------------check if player is clicking on menu button
            foreach (KeyValuePair<string, Rectangle> button in backgroundTiles.menuButtons)
            {
                if (button.Value.Intersects(cursor.cursorRect) && IsMousePressed("LeftButton")
                    && backgroundTiles.menuOpen && !menu.isAnimMenuOpen)
                {
                    backgroundTiles.typeToBuild = button.Key;
                    backgroundTiles.previewBuildItem = true;
                    backgroundTiles.menuClick = true;
                    // center the mouse after click
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    break;
                }
                // reset menu click flag
                else
                    backgroundTiles.menuClick = false;
            }


            Dictionary<string, List<Rectangle>> agriculturalAnimators = menu.GetAnimatorRectangles();
            wateringCanCursor = false;
            // -------------check if player is clicking on a menu animator instance (i.e. to water a growing plant)
            foreach (KeyValuePair<string, List<Rectangle>> agriculturalAnimator in agriculturalAnimators)
            {
                // if new animators were added, add them to click event lists
                if (!clickEventCoords.ContainsKey(agriculturalAnimator.Key))
                    clickEventCoords.Add(agriculturalAnimator.Key, new List<Rectangle>());
                if (!clickEventFlag.ContainsKey(agriculturalAnimator.Key))
                    clickEventFlag.Add(agriculturalAnimator.Key, new List<bool>());

                // if animators were removed, remove them from click event lists
                if (agriculturalAnimator.Value.Count < clickEventCoords[agriculturalAnimator.Key].Count)
                {
                    for (int i = 0; i < clickEventCoords[agriculturalAnimator.Key].Count; i++)
                    {
                        int indexToRemove;
                        bool wasMatchFound = false;
                        for (int j = 0; j < agriculturalAnimator.Value.Count; j++)
                        {
                            if (clickEventCoords[agriculturalAnimator.Key][i] == (agriculturalAnimator.Value[j]))
                            {
                                wasMatchFound = true;
                            }
                        }
                        if (!wasMatchFound)
                        {
                            indexToRemove = clickEventCoords[agriculturalAnimator.Key].FindIndex(x =>
                            x == clickEventCoords[agriculturalAnimator.Key][i]);
                            clickEventCoords[agriculturalAnimator.Key].RemoveAt(indexToRemove);
                            clickEventFlag[agriculturalAnimator.Key].RemoveAt(indexToRemove);
                        }
                    }
                }

                // for all agricultural animations, check for clicks
                for (int i = 0; i < agriculturalAnimator.Value.Count; i++)
                {
                    // if player is clicking on an agricultural object and is in range of that object
                    if (agriculturalAnimator.Value[i].Intersects(new Rectangle(cursor.cursorRect.X + 
                        (int)player.camera.Position.X, cursor.cursorRect.Y + (int)player.camera.Position.Y,
                        cursor.cursorRect.Width, cursor.cursorRect.Height)) && IsMousePressed("LeftButton")
                        && !menu.previewPlacement && !player.combatMode)
                    {
                        // send the clicked-on rectangle to menu the click flags will be set and reset in each respective
                        // animator instance inside the menu object
                        menu.SetAnimatorRectangle(agriculturalAnimator.Key, agriculturalAnimator.Value[i]);
                        
                        // add new click event, for drawing player alerts/notifications
                        if (!clickEventCoords[agriculturalAnimator.Key].Contains(agriculturalAnimator.Value[i]))
                        {
                            clickEventCoords[agriculturalAnimator.Key].Add(agriculturalAnimator.Value[i]);
                            clickEventFlag[agriculturalAnimator.Key].Add(true);
                        }
                        else // update existing click event
                            clickEventFlag[agriculturalAnimator.Key][i] = true;
                        wateringCanCursor = true;
                        isAnimationHighlighted = true;
                        sounds.Update(gameTime, "waterPour", background._mapName);
                    }
                    // cursor is above a plant, change cursor to watering can
                    else if (agriculturalAnimator.Value[i].Intersects(new Rectangle(cursor.cursorRect.X + (int)player.camera.Position.X, cursor.cursorRect.Y + (int)player.camera.Position.Y,
                        cursor.cursorRect.Width, cursor.cursorRect.Height)) && !menu.previewPlacement && !player.combatMode)
                    {
                        if (player.PlayerRect.Intersects(agriculturalAnimator.Value[i]))
                            isAnimationHighlighted = true;
                        wateringCanCursor = true;
                    }
                    // add new rectangles to animator's lists
                    else
                    {
                        if (!clickEventCoords[agriculturalAnimator.Key].Contains(agriculturalAnimator.Value[i]))
                        {
                            clickEventCoords[agriculturalAnimator.Key].Add(agriculturalAnimator.Value[i]);
                            clickEventFlag[agriculturalAnimator.Key].Add(false);
                        }
                    }
                }
            }

            int totalDroppedAnimatorItems = 0;
            // get collection of all items that have been dropped by animators
            droppedAnimatorItems = menu.GetClickedMatureAnimRectangles();
            // pick up or apply gravity to dropped animators
            foreach (KeyValuePair<string, List<Rectangle>> entry in droppedAnimatorItems) // each entry in dictionary
            {
                for (int i = 0; i < entry.Value.Count; i++) // each rectangle in a value's list
                {
                    bool itemWasRemoved = false;
                    if (entry.Value[i].Intersects(player.PlayerRect) && inventoryList.Count < 18) // player is attempting to pick item up
                    {
                        // determine whether to stack the item
                        bool isItemInInventory = false;
                        int itemLocation = 0;
                        for (int j = 0; j < inventoryList.Count; j++)
                        {
                            if (inventoryList[j]._itemType == entry.Key && !inventoryList[j].IsStackFull())
                            {
                                isItemInInventory = true;
                                itemLocation = j;
                            }
                        }
                        // if there is room in the inventory then add it
                        if (!isItemInInventory)
                        {
                            inventoryFullAlert = false;
                            if (entry.Key == "SpriteSheets/chamomileSheet")
                                inventoryList.Add(new ItemStack(entry.Key, 5, "player"));
                            else if (entry.Key == "SpriteSheets/tomatoSheet")
                                inventoryList.Add(new ItemStack(entry.Key, 5, "player"));
                            else if (entry.Key == "SpriteSheets/blueberrySheet")
                                inventoryList.Add(new ItemStack(entry.Key, 5, "player"));
                            animatorItemGravity.RemoveGravityInstance(entry.Value[i]);
                            entry.Value.Remove(entry.Value[i]);
                            itemWasRemoved = true;
                        }
                        // stack the item if it is already held in inventory and there is room in a stack
                        else if (isItemInInventory)
                        {
                            inventoryFullAlert = false;
                            inventoryList[itemLocation].itemsInStack++;
                            animatorItemGravity.RemoveGravityInstance(entry.Value[i]);
                            entry.Value.Remove(entry.Value[i]);
                            itemWasRemoved = true;
                        }
                        else
                            inventoryFullAlert = true;
                        // if item was picked up, play sound
                        if (!inventoryFullAlert)
                            sounds.Update(gameTime, "itemPickUp", background._mapName);
                    }
                    else if (totalDroppedAnimatorItems + 1 > animatorItemGravity.totalInstancesOfGravity) // an item was dropped, create a new instance of gravity
                    {
                        animatorItemGravity.NewGravityInstance(entry.Value[i]);
                        sounds.Update(gameTime, "plantHarvest", background._mapName);
                    }
                    else // apply gravity to all dropped items
                    {
                        inventoryFullAlert = false; // reset alert flag when player is not attempting to pick item up
                        entry.Value[i] =
                            new Rectangle(animatorItemGravity.ApplyGravity(gameTime, 
                            new Vector2(entry.Value[i].X, entry.Value[i].Y), entry.Value[i].Size, totalDroppedAnimatorItems).ToPoint(),
                            new Point(entry.Value[i].Size.X, entry.Value[i].Size.Y));
                    }
                    if (!itemWasRemoved)
                        totalDroppedAnimatorItems++;
                }
            }


            // used for indexing and determining when to add new instances of gravity
            int totalDroppedNpcItems = 0;
            // -----------------------------------PICK UP ITEMS and apply gravity to dropped npc items---------------------------------------------------
            foreach (KeyValuePair<string, List<Rectangle>> entry in droppedNpcItems)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    bool itemWasRemoved = false;
                    if (entry.Value[i].Intersects(player.PlayerRect) && inventoryList.Count < 18) // player is attempting to pick item up
                    {
                        // determine whether to stack the item
                        bool isItemInInventory = false;
                        int itemLocation = 0;
                        for (int j = 0; j < inventoryList.Count; j++)
                        {
                            if (inventoryList[j]._itemType == entry.Key && !inventoryList[j].IsStackFull())
                            {
                                isItemInInventory = true;
                                itemLocation = j;
                            }
                        }
                        // if there is room in the inventory then add it
                        if (!isItemInInventory)
                        {
                            inventoryFullAlert = false;
                            if (entry.Key == "shotgunAmmo")
                                inventoryList.Add(new ItemStack(entry.Key, 6, "shotgun"));
                            if (entry.Key == "smgAmmo")
                                inventoryList.Add(new ItemStack(entry.Key, 31, "smg"));
                            if (entry.Key == "pistolAmmo")
                                inventoryList.Add(new ItemStack(entry.Key, 8, "pistol"));
                            npcItemGravity.RemoveGravityInstance(entry.Value[i]);
                            entry.Value.Remove(entry.Value[i]);
                            itemWasRemoved = true;
                        }
                        // stack the item if it is already held in inventory and there is room in a stack
                        else if (isItemInInventory)
                        {
                            inventoryFullAlert = false;
                            inventoryList[itemLocation].itemsInStack++;
                            npcItemGravity.RemoveGravityInstance(entry.Value[i]);
                            entry.Value.Remove(entry.Value[i]);
                            itemWasRemoved = true;
                        }
                        else
                            inventoryFullAlert = true;
                        if (!inventoryFullAlert)
                            sounds.Update(gameTime, "itemPickUp", background._mapName);
                    }
                    else if (totalDroppedNpcItems + 1 > npcItemGravity.totalInstancesOfGravity) // an item was dropped, create a new instance of gravity
                    {
                        npcItemGravity.NewGravityInstance(entry.Value[i]);
                    }
                    else // apply gravity to all dropped items
                    {
                        inventoryFullAlert = false; // reset alert flag when player is not attempting to pick item up
                        entry.Value[i] =
                            new Rectangle(npcItemGravity.ApplyGravity(gameTime, 
                            new Vector2(entry.Value[i].X, entry.Value[i].Y), entry.Value[i].Size, totalDroppedNpcItems).ToPoint(),
                            new Point(entry.Value[i].Size.X, entry.Value[i].Size.Y));
                    }
                    if (!itemWasRemoved)
                        totalDroppedNpcItems++;
                }
            }
            // open and close the inventory
            if (IsKeyPressed(Keys.I) && !isInventoryOpen)
            {
                UpdateInventory();
                isInventoryOpen = true;
            }
            else if (IsKeyPressed(Keys.I) && isInventoryOpen)
            {
                inventoryRectangles.Clear();
                isInventoryOpen = false;
            }
            playerEquippedGunTotalClips = 0;
            // get total reserve ammo held in player's inventory
            for (int i = 0; i < inventoryList.Count; i++)
            {
                // player has used a clip of ammunition, remove it
                if (player.useClip && player.currentlyEquippedGun._gunType.ToLower().Contains(inventoryList[i]._compatibleWith))
                {
                    // remove a clip from the stack
                    if (inventoryList[i].itemsInStack > 0)
                    {
                        player.currentlyEquippedGun._bulletsInClip = player.currentlyEquippedGun._clipSize;
                        player.currentGunAmmoClips--;
                        inventoryList[i].itemsInStack--;
                    }
                    // no clips left in stack, remove it from inventory
                    else
                        inventoryList.RemoveAt(i);
                    // reset reload flag
                    player.useClip = false;

                    // play reload sound
                    if (IsKeyPressed(Keys.R))
                    {
                        if (player.currentlyEquippedGun._gunType.ToLower().Contains("shotgun"))
                            sounds.Update(gameTime, "blasterReload", background._mapName);
                        else if (player.currentlyEquippedGun._gunType.ToLower().Contains("pistol"))
                            sounds.Update(gameTime, "blasterReload", background._mapName);
                        else if (player.currentlyEquippedGun._gunType.ToLower().Contains("smg"))
                            sounds.Update(gameTime, "blaster2Reload", background._mapName);
                    }
                }
                // get total ammo in inventory for each ammo type
                else if (inventoryList[i]._itemType.ToLower().Contains(player.currentlyEquippedGun._gunType.ToLower()))
                {
                    playerEquippedGunTotalClips += inventoryList[i].itemsInStack;                    
                }
            }
            // update the player's equipped gun clip count to the correct number
            player.SetAmmo(playerEquippedGunTotalClips);
            // check if player is using an item in the open inventory menu
            if (isInventoryOpen)
            {
                UpdateInventory();
                for (int i = 0; i < inventoryRectangles.Count; i++)
                {
                    if (inventoryRectangles[i].Intersects(new Rectangle(cursor.cursorRect.X + (int)player.camera.Position.X, 
                        cursor.cursorRect.Y + (int)player.camera.Position.Y, cursor.cursorRect.Width, cursor.cursorRect.Height)) &&
                        IsMousePressed("LeftButton"))
                    {
                        switch (inventoryList[i]._itemType)
                        {
                            case "SpriteSheets/chamomileSheet":
                                {
                                    player.playerHealth += 10;
                                    break;
                                }
                            case "SpriteSheets/tomatoSheet":
                                {
                                    player.playerHealth += 30;
                                    break;
                                }
                            case "SpriteSheets/blueberrySheet":
                                {
                                    player.playerHealth += 50;
                                    break;
                                }
                        }
                        // after using item, remove it and its rectangle from the lists
                        inventoryList.RemoveAt(i);
                        inventoryRectangles.RemoveAt(i);
                    }
                }
            }
        }

        public void UpdateEndOfGame(GameTime gameTime)
        {
            foreach (KeyValuePair<string, Rectangle> button in backgroundTiles.menuButtons)
            {
                if (button.Value.Intersects(cursor.cursorRect) && IsMousePressed("LeftButton")
                    && backgroundTiles.menuOpen && !menu.isAnimMenuOpen)
                {
                    backgroundTiles.typeToBuild = button.Key;
                    backgroundTiles.previewBuildItem = true;
                    backgroundTiles.menuClick = true;
                    // center the mouse after click
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    break;
                }
                // reset menu click flag
                else
                    backgroundTiles.menuClick = false;
            }
            this.Exit();
        }

        /// <summary>
        /// Queues a map to be loaded after a task is complete (when the player is using their starship to travel to another planet).
        /// If mapToLoad is not given a string, this method will check on the background object until it's task is complete (arrived at planet).
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="mapToLoad"></param>
        /// <param name="spawnToUse"></param>
        /// <param name="ambienceToPlay"></param>
        /// <param name="songToPlay"></param>
        /// <param name="backgroundToDraw">Which background should be drawn before the map is loaded</param>
        public void QueueNextMap(GameTime gameTime, bool readyToLoadMap, List<string> ambienceToPlay, string mapToLoad = "", int spawnToUse = 1,
            string songToPlay = "")
        {
            if (readyToLoadMap)
            { 
                LoadNewMap(mapsToLoad[queuedMap.mapToLoad], queuedMap.spawnToUse, gameTime);
                
                if (sounds.previousType != queuedMap.songToPlay)
                    sounds.Update(gameTime, "stopSong", background._mapName);
                sounds.Update(gameTime, queuedMap.songToPlay, background._mapName);

                sounds.Update(gameTime, "stopSound", background._mapName);

                if(queuedMap.ambienceToPlay != null)
                    for (int i = 0; i < queuedMap.ambienceToPlay.Count; i++)
                        sounds.Update(gameTime, queuedMap.ambienceToPlay[i], background._mapName);

                // additional sounds
                //sounds.Update(gameTime, "starshipLanding");
                background.readyToLoadMap = false;
                openTravelMap = false;
            }
            else if (mapToLoad != "") // queue the map
            {
                background.jumpToFTL = true;
                background.NextCelestialObjectToDraw = mapToLoad;

                queuedMap.mapToLoad = mapToLoad;
                queuedMap.songToPlay = songToPlay;
                queuedMap.ambienceToPlay = ambienceToPlay;
                queuedMap.spawnToUse = spawnToUse;
            }
        }

        // updates the inventory rectangles for click events
        public void UpdateInventory()
        {
            // clear old rectangles
            inventoryRectangles.Clear();
            // recreate all rectangles
            int spacerX = 0;
            int spacerY = 0;
            for (int i = 0; i < inventoryList.Count; i++)
            {
                if (i < 6) // first row of inventory
                {
                    spacerX = (i * 39) + 14;
                    spacerY = 15;
                }
                else if (i >= 6 && i < 12) // second row
                {
                    spacerX = (i * 39) - 220;
                    spacerY = 49;
                }
                else // third row
                {
                    spacerX = (i * 39) - 454;
                    spacerY = 82;
                }
                Rectangle inventoryRect = new Rectangle(500 + spacerX +
                    (int)player.camera.Position.X, 500 + spacerY +
                    (int)player.camera.Position.Y, 32, 32);
                inventoryRectangles.Add(inventoryRect);
            }
        }

        // check if triggers in map have been activated---------DOOR-------
        public void TriggerActivation(GameTime gameTime)
        {
            foreach (var item in backgroundTiles.mapTriggers)
            {
                if ((player.PlayerRect.Intersects(item._trigger) && item._activated == false))
                {
                    item._activated = true; // activate the trigger
                                            // only play door sound effect if player is close
                    if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                        sounds.Update(gameTime, item._type, background._mapName);
                    // tell player which doors are activated
                    if (item._type == "door")
                    {
                        player.activeDoorTrigger.Add(item._trigger);
                    }
                    break;
                }
                else if (!player.PlayerRect.Intersects(item._trigger) && item._activated == true
                    && doorTimer.Update(5000))
                {
                    item._activated = false; // activate the trigger
                                             // only play door sound effect if player is close
                    if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                        sounds.Update(gameTime, item._type, background._mapName);
                    // tell player which doors are activated
                    if (item._type == "door" && player.activeDoorTrigger.Count > 0)
                    {
                        int k = player.activeDoorTrigger.FindIndex(x => x == item._trigger); // find the door in player's list
                        player.activeDoorTrigger.RemoveAt(k); // remove the active door from player's list
                    }
                }

                if (false) //----------------------------------------------- player and npcs open door
                {
                    if (npcList.Count == 0) // if player is the only character in the map
                    {
                        if ((player.PlayerRect.Intersects(item._trigger) && item._activated == false))
                        {
                            item._activated = true; // activate the trigger
                                                    // only play door sound effect if player is close
                            if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                                sounds.Update(gameTime, item._type, background._mapName);
                            // tell player which doors are activated
                            if (item._type == "door")
                            {
                                player.activeDoorTrigger.Add(item._trigger);
                            }
                            break;
                        }
                        else if (!player.PlayerRect.Intersects(item._trigger) && item._activated == true
                            && doorTimer.Update(5000))
                        {
                            item._activated = false; // activate the trigger
                                                     // only play door sound effect if player is close
                            if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                                sounds.Update(gameTime, item._type, background._mapName);
                            // tell player which doors are activated
                            if (item._type == "door" && player.activeDoorTrigger.Count > 0)
                            {
                                int k = player.activeDoorTrigger.FindIndex(x => x == item._trigger); // find the door in player's list
                                player.activeDoorTrigger.RemoveAt(k); // remove the active door from player's list
                            }
                        }
                    }
                    // npc door triggers
                    else // if there are npcs in the map
                    {
                        // check if npcs or player are walking through door triggers
                        for (int i = 0; i < npcList.Count; i++)
                        {
                            if ((player.PlayerRect.Intersects(item._trigger) || npcList[i].npcRect.Intersects(item._trigger)) && item._activated == false)
                            {
                                item._activated = true; // activate the trigger
                                                        // only play door sound effect if player is close
                                if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                                    sounds.Update(gameTime, item._type, background._mapName);
                                // tell player which doors are activated
                                if (item._type == "door")
                                {
                                    player.activeDoorTrigger.Add(item._trigger);
                                }
                                break;
                            }
                            else if (!player.PlayerRect.Intersects(item._trigger) && !npcList[i].npcRect.Intersects(item._trigger) && item._activated == true
                                && doorTimer.Update(5000))
                            {
                                item._activated = false; // activate the trigger
                                                         // only play door sound effect if player is close
                                if (new Rectangle(item._trigger.X - 500, item._trigger.Y - 500, 1000, 1000).Contains(player.location))
                                    sounds.Update(gameTime, item._type, background._mapName);
                                // tell player which doors are activated
                                if (item._type == "door" && player.activeDoorTrigger.Count > 0)
                                {
                                    int k = player.activeDoorTrigger.FindIndex(x => x == item._trigger); // find the door in player's list
                                    player.activeDoorTrigger.RemoveAt(k); // remove the active door from player's list
                                }
                            }
                        }
                    }
                }
            }
        }

        // / <summary>
        // / This is called when the game should draw itself.
        // / </summary>
        // / <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (gameState)
            {
                case GameState.MainMenu:
                    DrawMainMenu(gameTime);
                    break;

                case GameState.Gameplay:

                    // while the starship is landing, do not draw gameplay
                    if (sounds.IsSoundEffectPlaying())
                    {
                        spriteBatch.Begin();
                        spriteBatch.Draw(background.starsPurple, Vector2.Zero, Color.White);
                        spriteBatch.Draw(background.starsPurple, new Vector2(background.starsPurple.Width, 0), Color.White);
                        spriteBatch.Draw(background.asteroids, new Vector2(background.starsPurple.Width / 2, background.starsPurple.Height/2 - 100), Color.White);
                        spriteBatch.Draw(background.asteroidParallax, new Rectangle(-1000, background.starsPurple.Height - 300, 4320, 1080), Color.White);
                        spriteBatch.End();
                    }
                    // player has died, don't draw gameplay and instead draw death notification
                    else if (deathNotify)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(halogenFont, "You have died. Press space to quit game.", new Vector2(700, 530), Color.Red);
                        spriteBatch.End();
                    }
                    else
                        DrawGameplay(gameTime);
                    break;

                case GameState.EndOfGame:
                    DrawEndOfGame(gameTime);
                    break;
            }
        }

        public void DrawMainMenu(GameTime gameTime)
        {
            // scale the menu by the size of the player's viewport
            Matrix scaleMatrix;
            scaleMatrix = Matrix.CreateScale(GraphicsDevice.Viewport.Width / 1920.0f, GraphicsDevice.Viewport.Height / 1080.0f, 1);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, scaleMatrix);

            // draw main menu background
            background.Draw(spriteBatch);

            if (showMenu)
            {
                // draw main menu buttons
                spriteBatch.Draw(newButton, newButtonRect, Color.White);
                //spriteBatch.Draw(loadButton, loadButtonRect, Color.White);
                spriteBatch.Draw(exitButton, exitButtonRect, Color.White);
                spriteBatch.Draw(mainLogo, new Rectangle(448, 200, 1024, 1024), Color.White);
            }
            else
            {
                spriteBatch.Draw(mainLogo, new Rectangle(448, 500, 1024, 1024), Color.White);
                // draw rain
                for (int i = 0; i < raindrops.Count; i++)
                {
                    raindrops[i].Draw(spriteBatch);
                }
            }

            //exhaustAnim.Draw(spriteBatch);
            //exhaustAnim2.Draw(spriteBatch);
            
            cursor.Draw(spriteBatch, Color.White);

            spriteBatch.End();
        }

        public void DrawGameplay(GameTime gameTime)
        {

            // place the hud elements at the same position across all display resolutions
            float hudWidthScale = GraphicsDevice.Viewport.Width / 1920.0f;
            if (GraphicsDevice.Viewport.Width > 2560)
                hudWidthScale /= 2;
            float hudHeightScale = GraphicsDevice.Viewport.Height / 1080.0f;
            if (GraphicsDevice.Viewport.Height > 1600)
                hudHeightScale /= 2;

            // ------------------------------------------ First SpriteBatch (World Objects)
            // parallax is for the movement of the background image as the player moves left and right
            Vector2 parallax = new Vector2(1f);
            // the gameplay images are scaled by the users viewport within the camera class
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, player.camera.GetViewMatrix(parallax));

            // draw background objects
            background.Draw(spriteBatch);

            if (background._mapName == "spaceStation")
            {
                exhaustAnim1.Draw(spriteBatch);
                exhaustAnim2.Draw(spriteBatch);
                exhaustAnim3.Draw(spriteBatch);
            }

            // draw background tiles
            backgroundTiles.Draw(spriteBatch, player.location, GraphicsDevice, viewPortBounds);


            // center player's camera on an npc's location
            if (currentKeyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T) && !lookAtNpc)
            {
                lookAtNpc = true;
                player.lookAtPlayer = false;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T) && lookAtNpc)
            {
                lookAtNpc = false;
                player.lookAtPlayer = true;
            }
            if (lookAtNpc && npcList.Count > 0)
            {
                player.camera.LookAt(npcList[0].location);
            }

            // draw animators, hud menu and inventory
            foreach (Animator animator in effectsAnimators)
            {
                animator.Draw(spriteBatch);
            }
            menu.DrawAnimators(spriteBatch);

            // draw player alerts/notifcations
            foreach (KeyValuePair<string, List<bool>> entry in clickEventFlag)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    // determine which alerts to draw for this entry by searching for indices matching type "Agricultural"
                    // only draw if the corresponding flag was not set
                    if (entry.Value[i] == false)
                    {
                        spriteBatch.Draw(waterAlert,
                            new Rectangle(clickEventCoords[entry.Key][i].X, clickEventCoords[entry.Key][i].Y, 
                            waterAlert.Width, waterAlert.Height), Color.White);
                    }
                }
            }

            // draw items dropped from mature animators
            foreach (KeyValuePair<string, List<Rectangle>> entry in droppedAnimatorItems)
            {
                for (int i = 0; i < itemTextures.Count; i++) // each item texture
                {
                    if (entry.Key.Contains(itemTextures[i].Name)) // compare to each entry in dictionary
                    {
                        for (int j = 0; j < entry.Value.Count; j++) // draw each dropped item with matching texture
                        {
                            spriteBatch.Draw(itemTextures[i],
                                new Rectangle(entry.Value[j].X, entry.Value[j].Y, itemTextures[i].Width / 2, itemTextures[i].Height / 2),
                                Color.White);
                        }
                    }
                }
            }

            // draw items dropped by npcs
            foreach (KeyValuePair<string, List<Rectangle>> entry in droppedNpcItems)
            {
                for (int i = 0; i < itemTextures.Count; i++)
                {
                    if (entry.Key.Contains(itemTextures[i].Name))
                    {
                        for (int j = 0; j < entry.Value.Count; j++)
                        {
                            spriteBatch.Draw(itemTextures[i],
                                new Rectangle(entry.Value[j].X, entry.Value[j].Y, itemTextures[i].Width / 2, itemTextures[i].Height / 2),
                                Color.White);
                        }
                    }
                }
            }

            // draw all NPCs in list
            if (npcList.Count > 0)
            {
                for (int i = 0; i < npcList.Count; i++)
                {
                    //npcList[i].DrawProjectile(spriteBatch);
                    npcList[i].Draw(spriteBatch);
                }
            }
            // draw player
            player.Draw(spriteBatch);
            player.DrawProjectile(spriteBatch);

            // if on the asteroid colony map, draw the docking bay exit over the player's layer
            if (background._mapName == "asteroidColony")
                spriteBatch.Draw(dockingBayExit, backgroundTiles.mapObjects["DockingBayExit"], Color.White);

            // draw smog
            for (int i = 0; i < smog.Count; i++)
            {
                smog[i].Draw(spriteBatch);
            }
            // draw rain
            for (int i = 0; i < raindrops.Count; i++)
            {
                raindrops[i].Draw(spriteBatch);
            }


            // draw the inventory background and the items the playing is holding
                if (isInventoryOpen)
            {
                int invX = 96;
                int invY = 128;
                spriteBatch.Draw(inventory, new Rectangle((int)player.location.X - invX, (int)player.location.Y - invY, 256, 256), Color.White);
                for (int i = 0; i < inventoryList.Count; i++)
                    for (int j = 0; j < itemTextures.Count; j++)
                        if (inventoryList[i]._itemType.Contains(itemTextures[j].Name))
                        {
                            int spacerX = 0;
                            int spacerY = 0;
                            if (i < 6) // first row of inventory
                            {
                                spacerX = (i * 39) + 14;
                                spacerY = 15;
                            }
                            else if (i >= 6 && i < 12) // second row
                            {
                                spacerX = (i * 39) - 220;
                                spacerY = 49;
                            }
                            else // third row
                            {
                                spacerX = (i * 39) - 454;
                                spacerY = 82;
                            }
                            Rectangle inventoryRect = new Rectangle((int)player.location.X - invX + spacerX, 
                                (int)player.location.Y - invY + spacerY, 32, 32);
                            spriteBatch.Draw(itemTextures[j], inventoryRect, Color.White);
                            spriteBatch.DrawString(halogenFont, inventoryList[i].itemsInStack.ToString(), new Vector2((int)player.location.X - invX + spacerX,
                                (int)player.location.Y - invY + spacerY), Color.White);
                        }
            }

            if (openTravelMap && background._mapName == "personalStarship")
            {
                // draw the travel map
                spriteBatch.Draw(travelMapScreen, travelMapPos, Color.White);
                // draw the planet selections
                if (travelMapHighlightAsteroidColony)
                {
                    spriteBatch.Draw(travelMapPopup, new Rectangle(travelMapAsteroidColony.X - 6, travelMapAsteroidColony.Y + 4, 64,32), Color.White);
                    spriteBatch.DrawString(agencyFont, "Vesta", new Vector2(travelMapAsteroidColony.X, travelMapAsteroidColony.Y), Color.WhiteSmoke);
                }
                else if (travelMapHighlightMercury)
                {
                    spriteBatch.Draw(travelMapPopup, new Rectangle(travelMapMercury.X - 6, travelMapMercury.Y + 4, 94, 36), Color.White);
                    spriteBatch.DrawString(agencyFont, "Mercury", new Vector2(travelMapMercury.X, travelMapMercury.Y), Color.WhiteSmoke);
                }
                else if (travelMapHighlightVenus)
                {
                    spriteBatch.Draw(travelMapPopup, new Rectangle(travelMapVenus.X - 6, travelMapVenus.Y + 4, 68, 32), Color.White);
                    spriteBatch.DrawString(agencyFont, "Venus", new Vector2(travelMapVenus.X, travelMapVenus.Y), Color.WhiteSmoke);
                }
                else if (travelMapHighlightEarth)
                {
                    spriteBatch.Draw(travelMapPopup, new Rectangle(travelMapEarth.X - 6, travelMapEarth.Y + 4, 64, 32), Color.White);
                    spriteBatch.DrawString(agencyFont, "Earth", new Vector2(travelMapEarth.X, travelMapEarth.Y), Color.WhiteSmoke);
                }
                else if (travelMapHighlightMars)
                {
                    spriteBatch.Draw(travelMapPopup, new Rectangle(travelMapMars.X - 6, travelMapMars.Y + 4, 64, 32), Color.White);
                    spriteBatch.DrawString(agencyFont, "Mars", new Vector2(travelMapMars.X, travelMapMars.Y), Color.WhiteSmoke);
                }
            }

            if (player.interactionFlag) // ---seats---
            {
                spriteBatch.Draw(interactionButton, new Rectangle(player.interactionRectangle.X + 20, player.interactionRectangle.Y + 20, 32, 32), Color.White);
            }
            else if ((showInteractNotification || player.interactionFlag) && !openTravelMap && !background.jumpToFTL) //--- doors/other objects---
            {
                if (!isPlayerInConversation)
                    spriteBatch.Draw(interactionButton, new Rectangle(interactionRectangle.X + 20, interactionRectangle.Y + 32, 32, 32), Color.White);
            }
            else if (currentInteraction == "Bartender" && !openTravelMap && !background.jumpToFTL) // ---people---
            {
                if (!isPlayerInConversation)
                    spriteBatch.Draw(interactionButton, new Rectangle(interactionRectangle.X - 16, interactionRectangle.Y - 16, 32, 32), Color.White);
            }

            DrawDialogue();
            
            // draw game cursor
            if (!backgroundTiles.buildMode && ((!menu.isAnimMenuOpen && !player.combatMode && !wateringCanCursor) || !showMode))
                cursor.Draw(spriteBatch, Color.White);
            // draw error watering can cursor when player is trying to water plant but out of range
            else if (showMode && wateringCanCursor && !isAnimationHighlighted
                    && !backgroundTiles.buildMode && !menu.isAnimMenuOpen)
            {
                cursor.Draw(spriteBatch, Color.IndianRed, true);
            }
            // draw watering can cursor
            else if (showMode && wateringCanCursor && !backgroundTiles.buildMode && !menu.isAnimMenuOpen)
            {
                cursor.Draw(spriteBatch, Color.White, true);
            }

            spriteBatch.End();// ----------------------------------------------- End SpriteBatch 1 ------------------------------------------------------










            //--------------------------------------------- Second SpriteBatch (GUI Objects) -------------------------------------------------

            // scale hud elements by the size of the player's viewport
            Matrix scaleMatrix;
            // greater than 1080P
            if (GraphicsDevice.Viewport.Width >= 1920 && GraphicsDevice.Viewport.Height >= 1080)
                scaleMatrix = Matrix.CreateScale(GraphicsDevice.Viewport.Width / 1920, GraphicsDevice.Viewport.Height / 1080, 1);
            // 1080P and smaller
            else
                scaleMatrix = Matrix.CreateScale(1920 / GraphicsDevice.Viewport.Width, 1080 / GraphicsDevice.Viewport.Height, 1);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, scaleMatrix);

            // player events / notifications
            if (winNotify)
            {
                spriteBatch.DrawString(halogenFont, "You have killed your bounty. Press space to quit game.", new Vector2(690, 530), Color.WhiteSmoke);
            }

            if (inventoryFullAlert)
            {
                spriteBatch.DrawString(halogenFont, "Inventory is full",
                    new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.Red);
            }
            // draw score
            //spriteBatch.DrawString(halogenFont, "Score " + score, new Vector2(0, 0), Color.White);

            // hud/gui top bar
            //spriteBatch.Draw(hudBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 110), Color.White);

            //if (gameplayPhase == GameplayPhase.Combat)
            //{
            //    spriteBatch.DrawString(halogenFont, "Enemies Remaining: " + npcList.Count, new Vector2(64, 5), Color.White);
            //    spriteBatch.DrawString(halogenFont, "Wave: " + currentWave, new Vector2(960 * hudWidthScale, 5), Color.White);
            //}
            //else if (gameplayPhase == GameplayPhase.Noncombat)
            //    spriteBatch.DrawString(halogenFont, "Time Remaining: " + gameplayPhaseTimer.TimeRemaining, new Vector2(64, 5), Color.White);
            //else if (gameplayPhase == GameplayPhase.Transition)
            //    spriteBatch.DrawString(halogenFont, "Next Wave In: " + gameplayPhaseTimer.TimeRemaining, new Vector2(64, 5), Color.White);

            if (showHud)
            {
                if (showAmmo)
                {
                    int playerBulletsInClip = player.currentlyEquippedGun._bulletsInClip; // bullets left in clip
                    if (playerBulletsInClip < 0)
                        playerBulletsInClip = 0;
                    int playerGunClipSize = player.gunClipSize; // size of clip of gun
                    if (playerGunClipSize < 0)
                        playerGunClipSize = 0;
                    int playerTotalAmmoClips = player.currentGunAmmoClips; // total ammo clips left to reload
                    if (playerTotalAmmoClips < 0)
                        playerTotalAmmoClips = 0;

                    // widen the background for larger clip sizes
                    int addWidth = 0;
                    // add width for first part of ammo HUD
                    if (playerBulletsInClip > 99)
                        addWidth+=16;
                    else if (playerBulletsInClip > 9)
                        addWidth+=8;
                    // add width for second part of ammo HUD
                    if (playerGunClipSize > 99)
                        addWidth += 16;
                    else if (playerGunClipSize > 9)
                        addWidth += 8;
                    // add width for third part of ammo HUD
                    if (playerTotalAmmoClips > 99)
                        addWidth += 16;
                    else if (playerTotalAmmoClips > 9)
                        addWidth += 8;


                    Vector2 startVect = new Vector2(64, GraphicsDevice.Viewport.Height - 128);
                    Vector2 startVectReload = new Vector2(32, GraphicsDevice.Viewport.Height - 128 - 64);
                    Rectangle startRect = new Rectangle((int)(startVect.X - 12), (int)(startVect.Y + 6), 
                        hudBackground.Width + addWidth, hudBackground.Height);

                    spriteBatch.Draw(hudBackground, startRect, Color.White);
                    // if player's gun is empty, show reload notification
                    if (playerBulletsInClip == 0 && playerTotalAmmoClips > 0)
                        spriteBatch.DrawString(halogenFont, "Press R to reload", startVectReload, Color.WhiteSmoke);
                    // gun is empty and no ammo to reload with
                    else if (playerBulletsInClip == 0 && playerTotalAmmoClips == 0)
                        spriteBatch.DrawString(halogenFont, "No ammo, press 1, 2, 3 or mouse wheel to switch weapons", startVectReload, Color.WhiteSmoke);
                    // display player's equipped weapon ammo count
                    spriteBatch.DrawString(halogenFont, playerBulletsInClip.ToString() + " / " + playerGunClipSize.ToString() +
                        "   " + playerTotalAmmoClips.ToString(), startVect, new Color(152, 21, 17));
                }

                if (showHealth)
                {
                    // player healthbar animation
                    int frame = 0;
                    if (player.playerHealth < 100 && player.playerHealth >= 90)
                        frame = 1;
                    else if (player.playerHealth < 90 && player.playerHealth >= 80)
                        frame = 2;
                    else if (player.playerHealth < 80 && player.playerHealth >= 70)
                        frame = 3;
                    else if (player.playerHealth < 70 && player.playerHealth >= 60)
                        frame = 4;
                    else if (player.playerHealth < 60 && player.playerHealth >= 50)
                        frame = 5;
                    else if (player.playerHealth < 50 && player.playerHealth >= 40)
                        frame = 6;
                    else if (player.playerHealth < 40 && player.playerHealth >= 30)
                        frame = 7;
                    else if (player.playerHealth < 30 && player.playerHealth >= 20)
                        frame = 8;
                    else if (player.playerHealth < 20)
                        frame = 9;
                    int width = hudHealthBar.TextureList[frame].Width;
                    int height = hudHealthBar.TextureList[frame].Height;
                    int x = hudHealthBar.TextureList[frame].X;
                    int y = hudHealthBar.TextureList[frame].Y;
                    // draw current frame from sprite data
                    Rectangle sourceRect = new Rectangle(x, y, width, height);
                    Rectangle backgroundRect = new Rectangle(64, 84, 256, 256);
                    spriteBatch.Draw(hudHealthBarSheet, backgroundRect, sourceRect, Color.White);
                }

                // draw current objective
                spriteBatch.DrawString(halogenFont, currentObjective, new Vector2(64, 150), Color.WhiteSmoke);

                if (showMode)
                {
                    // currently in the Agricultural mode
                    int hudIconHeight = 150;
                    if (menu.isAnimMenuOpen)
                    {
                        backgroundTiles.buildMode = false;
                        player.isBuildMode = true;
                        menu.DrawMenu(spriteBatch, GraphicsDevice, menuButton2);

                        int numberOfIcons = 2;
                        //spriteBatch.Draw(inventory, new Rectangle(48, hudIconHeight, hammer.Height + 32, hammer.Width * numberOfIcons + 32), Color.White);
                        spriteBatch.Draw(hammer, new Vector2(196, hudIconHeight), Color.SlateGray);
                        spriteBatch.Draw(wateringCan, new Vector2(64, hudIconHeight), Color.WhiteSmoke);

                        if (!menu.previewPlacement)
                            cursor.Draw(spriteBatch, Color.White);
                    }
                    // in build mode
                    else if (backgroundTiles.buildMode)
                    {
                        menu.isAnimMenuOpen = false;
                        player.isBuildMode = true;
                        spriteBatch.Draw(hammer, new Vector2(196, hudIconHeight), Color.WhiteSmoke);
                        spriteBatch.Draw(wateringCan, new Vector2(64, hudIconHeight), Color.SlateGray);
                        // draw the menu buttons
                        foreach (KeyValuePair<string, Rectangle> entry in backgroundTiles.menuButtons)
                        {
                            spriteBatch.Draw(menuButton, entry.Value, Color.White);
                        }
                        backgroundTiles.DrawMenu(spriteBatch);
                        // don't draw cursor when previewing the item to be built
                        if (!backgroundTiles.previewBuildItem)
                            cursor.Draw(spriteBatch, Color.White);
                    }
                    // no mode is active
                    else
                    {
                        player.isBuildMode = false;
                        spriteBatch.Draw(hammer, new Vector2(196, hudIconHeight), Color.SlateGray);
                        spriteBatch.Draw(wateringCan, new Vector2(64, hudIconHeight), Color.SlateGray);
                    }
                }
            }

            // player is pausing the game, draw pause menu
            if (pauseMenuFlag)
            {
                spriteBatch.DrawString(halogenFont, "Press Control to Exit Game", new Vector2(775, 850), Color.White);
                spriteBatch.DrawString(halogenFont, "Paused", new Vector2(900, 100), Color.White);

                // only show help menu for farming and builing when that mode is enabled
                if (showMode)
                {
                    spriteBatch.Draw(helpMenu,
                        new Rectangle(100, 200, 800, 600), Color.White);
                    spriteBatch.Draw(controlsMenu,
                         new Rectangle(1000, 200, 800, 600), Color.White);
                }
                else
                    spriteBatch.Draw(controlsMenu,
                        new Rectangle(550, 200, 800, 600), Color.White);
            }

            // aid in debugging scenes/dialogue
            if (enableDevHotKeys)
                spriteBatch.DrawString(halogenFont, sceneToDraw.ToString(), new Vector2(100,500), Color.White);

            spriteBatch.End();// -------- End SpriteBatch 2
            base.Draw(gameTime);
        }

        public void DrawEndOfGame(GameTime gameTime)
        {
            spriteBatch.Begin();
            background.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void DrawDialogue()
        {
            string currentMap = background._mapName;
           
            string textToDraw = "";
            string textBubbleToDraw = "";
            Vector2 vectorToUse = Vector2.Zero;

            Rectangle incomingCallRect = new Rectangle((int)player.location.X - 160, (int)player.location.Y - 64, 256, 256);
            Vector2 incomingCallText = new Vector2((int)player.location.X - 160, (int)player.location.Y + 175);
            Rectangle textBubbleRect = new Rectangle((int)player.location.X - 225, (int)player.location.Y + 55, 400, 400);
            Vector2 npcTextPos = new Vector2(player.location.X - 200, player.location.Y + 105); // for a one line npc text
            Vector2 playerTextPos = new Vector2(player.location.X - 150, player.location.Y + 105);

            // player is receiving a call and accepts it
            if (incomingCall && (IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.E)))
            {
                player.incomingCall = true;
                answerCall = true;
                incomingCall = false;
                sceneToDraw = 1;
            }
            // if the player is trying to continue with the conversation, increment the sceneToDraw counter
            else if ((IsKeyPressed(Keys.Space) || IsKeyPressed(Keys.E)) && sceneToDraw > 0 && currentSceneArea.Contains(player.location) && drawCurrentScene)
                sceneToDraw++;
            
            // receiving a call before dialogue, draw it
            if (incomingCall)
            {
                spriteBatch.Draw(incomingCallImage, incomingCallRect, Color.White);
                spriteBatch.DrawString(halogenFont, "Press E to interact", incomingCallText, Color.White);
            }
            else if (mission1)
            {
                if (currentMap == "personalStarship" || currentMap == "playerRoom")
                {
                    ///
                    /// Holocalls start at scene 0 to receive a call (with dialogue starting at 1), npcs start at 1 after rectangle intersection (with dialogue starting at 2)
                    ///
                    if (!mission1Objective1ConvoDone && !mission1Objective1Finish && mission1Objective1Start && talkedToHoloCall1)
                    {
                        if (sceneToDraw == 0)
                        {
                            // set flag to receive call
                            incomingCall = true;
                            // set flag to begin next dialogue scene
                            drawCurrentScene = true;
                        }
                        else if (sceneToDraw == 1)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "It appears we have been boarded.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw == 2)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "Scans indicate hostile lifeforms in storage.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw > 2)
                        {
                            // cancel call animation and reset standing animation
                            player.incomingCall = false;
                            if (player.currentAnimationFrame.Contains("Left"))
                                player.currentAnimationFrame = "StandingLeft";
                            else if (player.currentAnimationFrame.Contains("Right"))
                                player.currentAnimationFrame = "StandingRight";

                            sceneToDraw = 0;
                            talkedToHoloCall1 = true;
                            drawCurrentScene = false;
                            // signal that the conversation is finished
                            mission1Objective1ConvoDone = true;
                        }
                    }
                    else if (!talkedToHoloCall1)
                    {
                        if (sceneToDraw == 1)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "I have another job for you.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw == 2)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "Your target is Keelo Phiser.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw == 3)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "He was last seen at The Drunken Cyborg.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw == 4)
                        {
                            textBubbleToDraw = "holoCall";
                            textToDraw = "Sending details now.";
                            vectorToUse = npcTextPos;
                        }
                        else if (sceneToDraw > 4)
                        {
                            // cancel call animation and reset standing animation
                            player.incomingCall = false;
                            if (player.currentAnimationFrame.Contains("Left"))
                                player.currentAnimationFrame = "StandingLeft";
                            else if (player.currentAnimationFrame.Contains("Right"))
                                player.currentAnimationFrame = "StandingRight";

                            sceneToDraw = 0;
                            talkedToHoloCall1 = true;
                            drawCurrentScene = false;
                        }
                    }
                }
                else if (currentInteraction == "Bartender" && !talkedToBartender)
                {
                    if (sceneToDraw == 2)
                    {
                        textBubbleToDraw = "player";
                        textToDraw = "Have you seen this man?";
                        vectorToUse = playerTextPos;
                    }
                    else if (sceneToDraw == 3)
                    {
                        textBubbleToDraw = "bartender";
                        textToDraw = "He just left.";
                        vectorToUse = npcTextPos;
                    }
                    else if (sceneToDraw > 3)
                    {
                        sceneToDraw = 0;
                        talkedToBartender = true;
                        drawCurrentScene = false;
                    }
                }
                else if (!talkedToBounty1 && talkedToBartender && currentMap == "asteroidColony")
                {
                    if (sceneToDraw == 2)
                    {
                        textBubbleToDraw = "bounty1";
                        textToDraw = "I heard you were looking for me. ";
                        vectorToUse = npcTextPos;
                    }
                    else if (sceneToDraw == 3)
                    {
                        player.playerDirection = new Vector2(0, 1);
                        player.currentAnimationFrame = "StandingRight";
                        textBubbleToDraw = "bounty1";
                        textToDraw = "Well here I am...";
                        vectorToUse = npcTextPos;
                    }
                    else if (sceneToDraw > 3)
                    {
                        sceneToDraw = 0;
                        talkedToBounty1 = true;
                        drawCurrentScene = false;
                    }
                }

                // draw the text bubble and the text inside
                if (textBubbleToDraw != "" && textToDraw != "" && vectorToUse != Vector2.Zero)
                {
                    // when drawing text, don't let the player move
                    player.allowMovement = false;
                    // if the text is long, insert a new line and move the text position up
                    if (textToDraw.Length > 20)
                    {
                        int index = textToDraw.IndexOf(' ', 21);
                        if (index > 0)
                        {
                            vectorToUse.Y -= 20;
                            textToDraw = textToDraw.Insert(index + 1, "\r\n");
                        }
                    }

                    spriteBatch.Draw(textBubbles[textBubbleToDraw], textBubbleRect, Color.White);
                    spriteBatch.DrawString(agencyFont, textToDraw, vectorToUse, Color.WhiteSmoke);

                }
                // no text is being drawn, let player move
                else
                    player.allowMovement = true;
            }
        }


        // helper function to get player's mouse wheel input
        public int MouseScrollValue()
        {
            // positive value means scrolling up, negative value is scrolling down
            return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
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

        public bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }
    }
}