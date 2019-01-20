using System.Collections.Generic;

namespace GameXML
{
    public class SpriteMap
    {
        public List<Item> TextureList = new List<Item>();
    }

    public class Item
    {
        public string Name;
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
}