using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TiledSharp;

namespace Colony_Ship_Horizon
{
    internal class BackgroundTiles
    {

        // current map data
        float tileParallax = 0;
        public Rectangle parallaxStart;
        public int tileWidth;
        public int tileHeight;
        public int tilesetTilesWide;
        public int tilesetTilesHigh;
        public int tileCounter = 0;
        public TmxMap map;
        public Texture2D tileset;
        public string mapName;
        public List<Texture2D> currentlyLoadedTilesets = new List<Texture2D>();
        public List<string> tilesetsIndexPerLayer = new List<string>();

        // rectangle collections from tiled map
        public Dictionary<string, Rectangle> mapObjects = new Dictionary<string, Rectangle>();

        public List<Trigger> mapTriggers = new List<Trigger>();
        public List<Rectangle> mapPlatforms = new List<Rectangle>();
        public List<Vector2> mapStairs = new List<Vector2>();
        public List<Rectangle> mapLadders = new List<Rectangle>();
        public List<Rectangle> mapDoors = new List<Rectangle>();

        private MouseState previousMouseState;
        private MouseState currentMouseState = Mouse.GetState();
        private KeyboardState previousKeyboardState = Keyboard.GetState();
        private KeyboardState currentKeyboardState = Keyboard.GetState();
        private Vector2 _worldPos;
        public bool buildMode = false; // the mode activated by the player, indicates object creation/placement outside of combat
        public bool menuOpen = false; // drawing menu objects on the GUI layer
        private bool isBuilding = false; // an object is currently being placed
        public bool previewBuildItem = false; // an object has been chosed by the player, from the build menu, and is previewed at the cursor's position
        public bool buildItemPlacementClick = false; // an object has been placed, stop preview and don't allow more placements
        private bool addItemToList = false; // an object will be created at the cursor position and added to list
        private bool objectNotIntersecting = true;
        public bool placementError = false;
        public bool menuClick = false; // a player is clicking on a menu button
        private int loadCounter = 200; // spacing between GUI menu objects
        private bool uniqueType; // an item is unique and may be added to the tileObjects list
        public string typeToBuild; // the type of the object to be built, chosen by the player from the object menu
        public List<TileObject> tileObjects = new List<TileObject>(); // the list of all tile objects (includes menu and those built by the player)
        public Dictionary<string, Rectangle> menuButtons = new Dictionary<string, Rectangle>(); // the menu buttons to be clicked
        private bool itemWithinAllowedArea = false;
        Rectangle currentRoom = new Rectangle(0, 0, 0, 0);
        Rectangle previousRoom = new Rectangle(0, 0, 0, 0);
        GameTime gameTime;
        bool currentRoomChanged = false;
        TimeSpan roomChangeTime = TimeSpan.FromMilliseconds(500);
        TimeSpan prevRoomChangeTime;
        public string whichSongToPlay = "";
        public bool inCombatPhase;
        string previousType;
        bool continueBuilding = false;
        int menuButtonY = 750;
        public bool allowBuildMode = true;

        public void Load(int screenWidth, int screenHeight)
        {
            // clear all lists and reset data before loading new map
            mapDoors.Clear();
            mapLadders.Clear();
            mapObjects.Clear();
            mapPlatforms.Clear();
            mapStairs.Clear();
            mapTriggers.Clear();
            tileObjects.Clear();
            menuButtons.Clear();
            loadCounter = 200;

            int mapObjectsNpcSpawnCount = 0;
            // fill map rectangle dictionaries from .tmx file
            for (int i = 0; i < map.ObjectGroups[0].Objects.Count; i++)
            {
                if (map.ObjectGroups[0].Objects[i].Type == "Platform")
                {
                    mapPlatforms.Add(new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                        (int)map.ObjectGroups[0].Objects[i].Y,
                        (int)map.ObjectGroups[0].Objects[i].Width,
                        (int)map.ObjectGroups[0].Objects[i].Height));
                }
                else if (map.ObjectGroups[0].Objects[i].Type == "Stairs")
                {
                    //mapStairs.Add(new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                    //    (int)map.ObjectGroups[0].Objects[i].Y,
                    //    (int)map.ObjectGroups[0].Objects[i].Width,
                    //    (int)map.ObjectGroups[0].Objects[i].Height));
                    
                    // create vectors for each polyline labeled stairs
                    // start of polyline must be the TOP point of the stairs
                    Vector2 startPt = new Vector2((int)map.ObjectGroups[0].Objects[i].X,
                        (int)map.ObjectGroups[0].Objects[i].Y -5);
                    mapStairs.Add(new Vector2(0, 0)); // add zero vector to indicate that the next is a start
                    mapStairs.Add(startPt); // add the start
                    // the end point must be the BOTTOM, do not add it to list, as it will cause issues
                    Vector2 endPt = new Vector2((float)map.ObjectGroups[0].Objects[i].Points[1].X,
                        (float)map.ObjectGroups[0].Objects[i].Points[1].Y);
                    // create points from start point to end point and add to list, for collision
                    for (float j = 1; j < 20; j+=.05f)
                    {
                        Vector2 vector = new Vector2(endPt.X / j, endPt.Y / j);
                        vector += startPt;
                        mapStairs.Add(vector);
                    }
                }
                else if (map.ObjectGroups[0].Objects[i].Type == "Ladder")
                {
                    mapLadders.Add(new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                                    (int)map.ObjectGroups[0].Objects[i].Y,
                                    (int)map.ObjectGroups[0].Objects[i].Width,
                                    (int)map.ObjectGroups[0].Objects[i].Height));
                }
                else if (map.ObjectGroups[0].Objects[i].Type == "Door")
                {
                    mapDoors.Add(new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                                    (int)map.ObjectGroups[0].Objects[i].Y,
                                    (int)map.ObjectGroups[0].Objects[i].Width,
                                    (int)map.ObjectGroups[0].Objects[i].Height));
                }
                else if (map.ObjectGroups[0].Objects[i].Type == "Trigger")
                {
                    mapTriggers.Add(new Trigger(false, map.ObjectGroups[0].Objects[i].Name,
                        new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                        (int)map.ObjectGroups[0].Objects[i].Y,
                        (int)map.ObjectGroups[0].Objects[i].Width,
                        (int)map.ObjectGroups[0].Objects[i].Height),
                        map.ObjectGroups[0].Objects[i].Properties["soundEffectType"])); // retrieves the type of sound effect to play when triggered
                }
                else if (map.ObjectGroups[0].Objects[i].Type == "Object")
                {
                    if (map.ObjectGroups[0].Objects[i].Name.Contains("NPC_Spawn"))
                    {
                        string npcActionToTake = "";
                        string npcType = "";
                        string npcInteraction = "";
                        if (map.ObjectGroups[0].Objects[i].Properties.ContainsKey("Action"))
                            npcActionToTake = map.ObjectGroups[0].Objects[i].Properties["Action"];
                        if (map.ObjectGroups[0].Objects[i].Properties.ContainsKey("Type"))
                            npcType = ":" + map.ObjectGroups[0].Objects[i].Properties["Type"];
                        if (map.ObjectGroups[0].Objects[i].Properties.ContainsKey("Interaction"))
                            npcInteraction = ";" + map.ObjectGroups[0].Objects[i].Properties["Interaction"];
                        // count how many npc spawns have been added
                        mapObjectsNpcSpawnCount++;
                        // concatenate the current npc spawn object name with the total npc spawn count and add to mapObjects collection
                        mapObjects.Add(map.ObjectGroups[0].Objects[i].Name.Insert(map.ObjectGroups[0].Objects[i].Name.Length, 
                            mapObjectsNpcSpawnCount.ToString() + npcActionToTake + npcType + npcInteraction),// add the action of npc
                            new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                            (int)map.ObjectGroups[0].Objects[i].Y,
                            (int)map.ObjectGroups[0].Objects[i].Width,
                            (int)map.ObjectGroups[0].Objects[i].Height));
                    }
                    else if (map.ObjectGroups[0].Objects[i].Name.Contains("Parallax"))
                    {
                        parallaxStart = new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                            (int)map.ObjectGroups[0].Objects[i].Y,
                            (int)map.ObjectGroups[0].Objects[i].Width,
                            (int)map.ObjectGroups[0].Objects[i].Height);
                    }
                    else
                    {
                        mapObjects.Add(map.ObjectGroups[0].Objects[i].Name,
                            new Rectangle((int)map.ObjectGroups[0].Objects[i].X,
                            (int)map.ObjectGroups[0].Objects[i].Y,
                            (int)map.ObjectGroups[0].Objects[i].Width,
                            (int)map.ObjectGroups[0].Objects[i].Height));
                    }
                }
                // do nothing for triggertiles in the object layer
                else if (map.ObjectGroups[0].Objects[i].Type == "TriggerTiles") { }
                // for all other objects, add to list
                else if (allowBuildMode)
                {
                    bool tempBool = true;
                    // add first item
                    if (tileObjects.Count == 0 && map.ObjectGroups[0].Objects[i].Properties.ContainsKey("isOnGround"))
                    {
                        // is the object bound to the floor or can it be placed on a wall
                        if (map.ObjectGroups[0].Objects[i].Properties["isOnGround"] == "true")
                            tempBool = true;
                        else
                            tempBool = false;

                       tileObjects.Add(new TileObject(map.ObjectGroups[0].Objects[i].Type, 100, menuButtonY,
                            Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["height"]),
                            Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["width"]),
                            false, true, tempBool));
                        
                        menuButtons.Add(map.ObjectGroups[0].Objects[i].Type,
                            new Rectangle((int)tileObjects[tileObjects.Count - 1]._tileX,
                            (int)tileObjects[tileObjects.Count - 1]._tileY, 86, 86));
                    }
                    uniqueType = true;
                    // search for duplicates in list
                    foreach (var item in tileObjects)
                    {
                        if (item._type == map.ObjectGroups[0].Objects[i].Type)
                        {
                            uniqueType = false;
                        }
                    }
                    // if the type is unique, add it
                    if (uniqueType && map.ObjectGroups[0].Objects[i].Properties.ContainsKey("isOnGround"))
                    {
                        // is the object bound to the floor or can it be placed on a wall
                        if (map.ObjectGroups[0].Objects[i].Properties["isOnGround"] == "true")
                            tempBool = true;
                        else
                            tempBool = false;
                        tileObjects.Add(new TileObject(map.ObjectGroups[0].Objects[i].Type, loadCounter, menuButtonY,
                            Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["height"]),
                            Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["width"]),
                            false, true, tempBool));
                        loadCounter += 100;
                        // create rectangles for each TileObject for menu button click collision and offset the rectangles to center
                        // the TileObject to be drawn above it
                        menuButtons.Add(map.ObjectGroups[0].Objects[i].Type,
                            new Rectangle((int)tileObjects[tileObjects.Count - 1]._tileX,
                            (int)tileObjects[tileObjects.Count - 1]._tileY, 86, 86));

                        //reset flag
                        uniqueType = false;
                    }
                }
            } // end for loop
        }

        public void Update(GameTime elapsed, Vector2 worldPos, bool attemptSwitch)
        {
            gameTime = elapsed;
            _worldPos = worldPos;
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // toggle build mode
            if (allowBuildMode)
            {
                if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G) && !buildMode)
                {
                    buildMode = true;
                }
                else if ((currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) ||
                    currentKeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F) ||
                    (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released) &&
                    (buildMode))
                {
                    previewBuildItem = false;
                    buildMode = false;
                }
            }

            // controls transition of lighting/songs/sounds between rooms
            if (gameTime.TotalGameTime - prevRoomChangeTime > roomChangeTime && currentRoomChanged)
            {
                prevRoomChangeTime = gameTime.TotalGameTime;
                currentRoomChanged = false;
            }
            else if (previousRoom != currentRoom && !currentRoomChanged)
            {
                currentRoomChanged = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerLocation, GraphicsDevice graphicsDevice, Rectangle viewportBounds)
        {
            viewportBounds.Height *= 6;
            viewportBounds.Y -= 100;
            // ensure only one item is added to build list
            bool addedToBuildList = false;
            Color currentColor = Color.White;

            // find the room containing the player to determine which tiles should be drawn darker
            foreach (string currentKey in mapObjects.Keys)
            {
                // determine the type of room the player is in
                if (currentKey.Contains("engine") && mapObjects[currentKey].Contains(playerLocation))
                {
                    whichSongToPlay = "engineRoomSong";
                }
                else if (currentKey.Contains("garden") && mapObjects[currentKey].Contains(playerLocation))
                {
                    whichSongToPlay = "gardenRoomSong";
                }
                else if (currentKey.Contains("main") && mapObjects[currentKey].Contains(playerLocation))
                {
                    whichSongToPlay = "mainRoomSong";
                }
                else if (currentKey.Contains("tran") && mapObjects[currentKey].Contains(playerLocation))
                {
                    whichSongToPlay = "transitionBetweenSongs";
                }

                // determine which exact room the player is in 
                if (currentKey.Contains("BuildArea1") || currentKey.Contains("BuildArea2") ||
                    currentKey.Contains("BuildArea3") && mapObjects[currentKey].Contains(playerLocation))
                {
                    previousRoom = currentRoom;
                    currentRoom = mapObjects[currentKey];
                    break;
                }
            }
            
            //if (currentKeyboardState.IsKeyDown(Keys.A)) // move parallax background right (opposite of player)
            //{
            //    tileParallax += .2f;
            //}
            //else if (currentKeyboardState.IsKeyDown(Keys.D)) // move parallax background left (opposite of player) 
            //{
            //    tileParallax -= .2f;
            //}
            // the offset to calculate the correct gid for the current tileset
            int gidOffset = 0;
            // draw tiled map
            for (int i = 0; i < map.Layers.Count; i++)
            {
                // iterate through all loaded tilesets
                for (int j = 0; j < currentlyLoadedTilesets.Count; j++)
                {
                    // match the correct tileset to the layer
                    if (tilesetsIndexPerLayer[i] == currentlyLoadedTilesets[j].Name)
                    {
                        // find the correct tileset from the map's list the get the index
                        for (int k = 0; k < map.Tilesets.Count; k++)
                        {
                            if (map.Tilesets[k].Name == currentlyLoadedTilesets[j].Name)
                            {
                                // pass the correct tileset and it's index
                                LoadNewTileset(currentlyLoadedTilesets[j], k);
                                // subtract all preceding tiles to find the correct gid for the current tileset
                                gidOffset = map.Tilesets[k].FirstGid - 1;
                                break;
                            }
                        }
                    }
                }

                // draw each tile in the current layer
                for (int j = 0; j < map.Layers[i].Tiles.Count; j++)
                {
                    int gid = map.Layers[i].Tiles[j].Gid;
                    if (gid != 0) // don't draw empty tiles or tiles outside of view
                    {
                        gid -= gidOffset; // offset the gid
                        int tileFrame = gid - 1;
                        int column = tileFrame % tilesetTilesWide;
                        int row = (int)Math.Floor((double)tileFrame / (double)tilesetTilesWide);

                        float x = (j % map.Width) * map.TileWidth;
                        float y = (float)Math.Floor(j / (double)map.Width) * map.TileHeight;

                        //if (map.Layers[i].Name.Contains("Parallax"))
                        //{
                        //    x += tileParallax;
                        //}

                        // if the tile is flipped, apply effect
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (map.Layers[i].Tiles[j].HorizontalFlip)
                        {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (map.Layers[i].Tiles[j].VerticalFlip)
                        {
                            spriteEffects = SpriteEffects.FlipVertically;
                        }

                        Rectangle tilesetRect = new Rectangle(tileWidth * column, tileHeight * row, tileWidth, tileHeight);
                        Rectangle backRect = new Rectangle((int)x, (int)y, tileWidth, tileHeight);
                        //if (currentRoom.Contains(backRect) || (previousRoom.Contains(backRect) && currentRoomChanged))
                        currentColor = Color.White;
                        //else currentColor = Color.DarkGray;

                        if (viewportBounds.Contains(backRect))
                        {
                            spriteBatch.Draw(tileset, backRect, tilesetRect, currentColor, 0f,
                            Vector2.Zero, spriteEffects, 0);
                        }
                    }
                }
            }

            // draw object layer (object group)
            for (var i = 0; i < map.ObjectGroups[0].Objects.Count; i++)
            {
                // a door trigger is active, draw open door
                if (map.ObjectGroups[0].Objects[i].Properties.ContainsKey("Open") && IsTriggerActive(map.ObjectGroups[0].Objects[i].Name))
                {
                    isBuilding = false;
                    DrawTileObjects(spriteBatch, i, isBuilding, viewportBounds);
                }
                // a door trigger is inactive, draw closed door
                else if (map.ObjectGroups[0].Objects[i].Properties.ContainsKey("Closed") && !IsTriggerActive(map.ObjectGroups[0].Objects[i].Name))
                {
                    isBuilding = false;
                    DrawTileObjects(spriteBatch, i, isBuilding, viewportBounds);
                }
                // draw dynamic objects (player built)
                else
                {
                    addItemToList = false;
                    objectNotIntersecting = true;
                    TileObject tempTileObject = null;
                    foreach (TileObject item in tileObjects)
                    {
                        // temporarily draw object at cursor so that player may preview object placement
                        // tempTileObject is used to store a previewed object's data until it's placement is validated, where the data is sent to an
                        // item in the tileObjects list
                        if (map.ObjectGroups[0].Objects[i].Type == typeToBuild && map.ObjectGroups[0].Objects[i].Type == item._type
                            && buildMode && previewBuildItem)
                        {
                            isBuilding = true;
                            menuOpen = false;
                            tempTileObject = new TileObject(typeToBuild, 0, 0, item._height, item._width, false, false, item._isOnGround);
                            DrawTileObjects(spriteBatch, i, isBuilding, viewportBounds, tempTileObject);
                            // player is attempting to place the object, stop preview and proceed with placement validation
                            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                            previousMouseState.LeftButton == ButtonState.Released && itemWithinAllowedArea)
                            {
                                previewBuildItem = false;
                                buildItemPlacementClick = true;
                                //Mouse.SetPosition();
                            }
                            // player is cancelling object preview, exit preview mode
                            else if (currentMouseState.RightButton == ButtonState.Pressed &&
                            previousMouseState.RightButton == ButtonState.Released)
                            {
                                previewBuildItem = false;
                            }
                        }

                        // player is creating/placing a new object, add it to list and flag to draw it if
                        // the item is not built, to prevent duplicates from being added to the list
                        else if (map.ObjectGroups[0].Objects[i].Type == typeToBuild && item._type == typeToBuild && buildMode
                            && !item._isBuilt && buildItemPlacementClick)
                        {
                            previewBuildItem = false;
                            isBuilding = true;
                            menuOpen = false;
                            // create temporary TileObject with the corresponding type
                            tempTileObject = new TileObject(typeToBuild, 0, 0, item._height, item._width, false, false, item._isOnGround);
                            // find the tile object's info and current position from mouse position
                            DrawTileObjects(spriteBatch, i, isBuilding, viewportBounds, tempTileObject);

                            // no match, continue checking list
                            if (!addedToBuildList)
                            {
                                addItemToList = true;
                                addedToBuildList = true;
                            }
                        }
                        // object has been built, continue to draw it
                        if (map.ObjectGroups[0].Objects[i].Type == item._type && item._isBuilt)
                        {
                            isBuilding = true;
                            DrawTileObjects(spriteBatch, i, isBuilding, viewportBounds, item);
                        }
                    }
                    // if flagged, add the corresponding item to list (outside of foreach loop)
                    if (addItemToList)
                    {
                        addItemToList = false;
                        tempTileObject._isBuilt = true;
                        buildItemPlacementClick = false;
                        // update the object's rectangle with new point from mouse
                        tempTileObject.UpdateRectangle();
                        // add object to end of list
                        tileObjects.Add(tempTileObject);
                    }
                }
            } // end for loop
        }

        public void LoadNewTileset(Texture2D tilesetToLoad, int tilesetsIndex)
        {
            tileset = tilesetToLoad;
            tileWidth = map.Tilesets[tilesetsIndex].TileWidth; // data for drawing the tiles
            tileHeight = map.Tilesets[tilesetsIndex].TileHeight;
            tilesetTilesWide = tileset.Width / tileWidth;
            tilesetTilesHigh = tileset.Height / tileHeight;
        }

        // called from the GUI spritebatch to be displayed on the foreground with other menu/HUD elements
        public void DrawMenu(SpriteBatch spriteBatch)
        {
            bool buildMode = true;
            menuOpen = true;

            for (var i = 0; i < map.ObjectGroups[0].Objects.Count; i++)
            {
                foreach (TileObject item in tileObjects)
                {
                    if (map.ObjectGroups[0].Objects[i].Type == item._type && buildMode && item._isMenuObject)
                    {
                        DrawTileObjects(spriteBatch, i, buildMode, new Rectangle(0, 0, 5000, 5000), item, menuOpen);
                    }
                }
            }
        }

        public void DrawTileObjects(SpriteBatch spriteBatch, int i, bool buildMode, Rectangle viewportBounds, TileObject item = null, bool menuOpen = false)
        {
            float x = 0;
            float y = 0;
            // reset flag
            itemWithinAllowedArea = true;

            int gid = map.ObjectGroups[0].Objects[i].Tile.Gid;
            // only draw tiles that are not empty
            if (gid != 0)
            {
                int tileFrame = gid - 1;
                int column = tileFrame % tilesetTilesWide;
                int row = (int)Math.Floor((double)tileFrame / (double)tilesetTilesWide);
                Rectangle tilesetRec = new Rectangle(tileWidth * column, tileHeight * row, tileWidth, tileHeight);

                // if user is placing a tile object, enter build mode
                if (buildMode)
                {
                    int tileOffsetX = 0;
                    int tileOffsetY = 0;
                    // for objects larger than a single tile, get the offset of each tile to create full image of object
                    // a tile with a vector of 0,0 is the top left tile, or the only tile in the object
                    // x = 1 will add 32 to the tileOffsetX
                    try
                    {
                        int objectSectionX = Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["x"]);
                        int objectSectionY = Convert.ToInt32(map.ObjectGroups[0].Objects[i].Properties["y"]);

                        for (int j = 0; j < objectSectionX; j++)
                        {
                            tileOffsetX += 32;
                        }
                        for (int k = 0; k < objectSectionY; k++)
                        {
                            tileOffsetY += 32;
                        }
                    }
                    catch (FormatException) { }

                    float subtrahend;
                    float gridSizeX = 32;
                    float gridSizeY = 32;
                    // if player has placed object, get the mouse coords
                    if (!item._isBuilt && !item._isMenuObject && !previewBuildItem)
                    {
                        item._tileX = (int)currentMouseState.Position.X + _worldPos.X;
                        item._tileY = (int)currentMouseState.Position.Y + _worldPos.Y;
                        // align the object to grid of 32x32 tiles
                        if (item._tileX % gridSizeX != 0)
                        {
                            subtrahend = item._tileX % gridSizeX;
                            item._tileX += (gridSizeX - subtrahend);
                        }
                        if (item._tileY % gridSizeY != 0)
                        {
                            subtrahend = item._tileY % gridSizeY;
                            item._tileY += (gridSizeY - subtrahend);
                        }
                    }
                    // player is placing object, preview the placement
                    else if (!item._isBuilt && !item._isMenuObject && previewBuildItem)
                    {
                        item._tileX = (int)currentMouseState.Position.X + _worldPos.X;
                        item._tileY = (int)currentMouseState.Position.Y + _worldPos.Y;
                        // align the object to grid of 32x32 tiles
                        if (item._tileX % gridSizeX != 0)
                        {
                            subtrahend = item._tileX % gridSizeX;
                            item._tileX += (gridSizeX - subtrahend);
                        }
                        if (item._tileY % gridSizeY != 0)
                        {
                            subtrahend = item._tileY % gridSizeY;
                            item._tileY += (gridSizeY - subtrahend);
                        }

                        if (mapObjects["BuildFloor1"].Top != item._tileY + item._height &&
                            mapObjects["BuildFloor2"].Top != item._tileY + item._height &&
                            item._isOnGround)
                        //if (mapObjects["BuildFloor"].Top != item._tileY + item._height && item._isOnGround)
                        {
                            //item._tileY = mapObjects["Room"].Bottom - item._height;
                            itemWithinAllowedArea = false;
                        }
                        // ensure that the object will not intersect any platforms or other objects if placed
                        foreach (Rectangle platform in mapPlatforms)
                        {
                            // object intersects platform, stop check and do not allow placement
                            if (platform.Intersects(new Rectangle((int)item._tileX, (int)item._tileY,
                                item._width, item._height)))
                            {
                                objectNotIntersecting = false;
                                itemWithinAllowedArea = false;
                                break;
                            }
                        }
                        foreach (TileObject tileObject in tileObjects)
                        {
                            // object intersects another object, stop check and do not allow placement
                            if (tileObject.tileObjectRectangle.Intersects(new Rectangle((int)item._tileX,
                                (int)item._tileY, item._width, item._height)))
                            {
                                objectNotIntersecting = false;
                                itemWithinAllowedArea = false;
                                break;
                            }
                        }
                    }
                    // place object at coords
                    x = item._tileX + tileOffsetX;
                    y = item._tileY + tileOffsetY;
                }
                // if not in build move, item is static pre-placed object, draw at coords specified in tiled map
                else
                {
                    x = (float)map.ObjectGroups[0].Objects[i].X;
                    y = (float)map.ObjectGroups[0].Objects[i].Y - 32;
                }

                // don't draw outside of the viewport
                //if (viewportBounds.Contains(x, y))
                //{
                    // draw static objects (when an item == null you cannot test it as you can in the else if below)
                    if (!buildMode)
                    {
                        spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.White);
                    }
                    // draw placed objects, but don't draw when adding new item to list (new item will be drawn here later)
                    else if (item._isBuilt || item._isMenuObject || previewBuildItem)
                    {
                        // ensure the item is within the allowed area
                        //if ((mapObjects["BuildArea1"].Contains(new Point((int)x, (int)y)) ||
                        //    mapObjects["BuildArea2"].Contains(new Point((int)x, (int)y))  ||
                        //    mapObjects["BuildArea3"].Contains(new Point((int)x, (int)y))) && itemWithinAllowedArea)
                        if (mapObjects["BuildArea"].Contains(new Point((int)x, (int)y)) && itemWithinAllowedArea)
                        {
                            itemWithinAllowedArea = true;
                        }
                        else
                        {
                            itemWithinAllowedArea = false;
                        }

                        // if previewing an object placement and object is out of allowed area, draw as red
                        if (previewBuildItem && !item._isBuilt && !menuOpen && !itemWithinAllowedArea)
                        {
                            spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.Red);
                        }
                        // if previewing an object placement and object is within allowed area, draw as green
                        else if (previewBuildItem && !item._isBuilt && !menuOpen && itemWithinAllowedArea)
                        {
                            spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.LightGreen);
                        }
                        else if (menuOpen)
                        {
                            // draw tile menu
                            int xInc = 10;
                            if (item._width == 32)
                                xInc = 25;
                            int yInc = 10;
                            if (item._height == 32)
                                yInc = 25;
                            spriteBatch.Draw(tileset, new Rectangle((int)x + xInc, (int)y + yInc, tileWidth, tileHeight), tilesetRec, Color.White);
                        }
                        // draw built/placed objects as their normal colors
                        else
                            spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.White);
                    }
                //}
            }
        }

        public bool IsTriggerActive(string name)
        {
            bool triggerActive = false;
            foreach (Trigger item in mapTriggers)
            {
                // if the corresponding trigger was activated, return true
                if (name.Contains(item._name) && item._activated == true)
                {
                    triggerActive = true;
                }
            }
            return triggerActive;
        }
    }

    /// <summary>
    /// class for trigger data, to be added to list for activation/deactivation by player and npcs
    /// and subsequently that information is used to draw corresponding object tiles to the spritebatch
    /// </summary>
    internal class Trigger
    {
        public bool _activated;
        public string _name;
        public Rectangle _trigger;
        public string _type;

        public Trigger(bool activated, string name, Rectangle trigger, string type)
        {
            _activated = activated;
            _name = name;
            _trigger = trigger;
            _type = type;
        }
    }

    internal class TileObject
    {
        public string _type;

        public float _tileX = 0;
        public float _tileY = 0;
        public int _height;
        public int _width;
        public Rectangle tileObjectRectangle;

        public bool _isBuilt;
        public bool _isMenuObject;
        public bool _isOnGround;

        public TileObject(string type, int tileX, int tileY, int height, int width, bool isBuilt, bool isMenuObject, bool isOnGround)
        {
            _type = type;

            _tileX = tileX;
            _tileY = tileY;
            _height = height;
            _width = width;
            tileObjectRectangle = new Rectangle((int)_tileX, (int)_tileY, _width, _height);

            _isBuilt = isBuilt;
            _isMenuObject = isMenuObject;
            _isOnGround = isOnGround;
        }

        public void UpdateRectangle()
        {
            tileObjectRectangle = new Rectangle((int)_tileX, (int)_tileY, _width, _height);
        }
    }
}