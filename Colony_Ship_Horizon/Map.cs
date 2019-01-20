using GameXML;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using TiledSharp;

namespace Colony_Ship_Horizon
{
    public struct Map
    {
        public List<Texture2D> _tilesets;
        public List<string> _tilesetIndexPerLayer;
        public TmxMap _map;
        public string _mapName;

        /// <summary>
        /// Create new map to be loaded
        /// </summary>
        /// <param name="tilesets">The tilesets to be used in this map</param>
        /// <param name="tilesetIndexPerLayer">Which tilesets to load in each layer of the tiled map. Index 0 of
        /// this list represents the first layer of the map. The integers of this list correspond with the index
        /// of the tileset to load in the tilesets list</param>
        /// <param name="map">The tiled map</param>
        /// <param name="mapName">The name of the map</param>
        public Map(List<Texture2D> tilesets, List<string> tilesetIndexPerLayer, TmxMap map, string mapName)
        {
            _tilesets = tilesets;
            _tilesetIndexPerLayer = tilesetIndexPerLayer;
            _map = map;
            _mapName = mapName;
        }
    }
}
