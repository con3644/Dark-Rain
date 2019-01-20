using System;
using System.Collections.Generic;

namespace SpriteMap
{
    public class XML
    {
        public List<Item> items { get; set; } = new List<Item>();
    }

    public class Item
    {
        public string Name { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }

    }
}
