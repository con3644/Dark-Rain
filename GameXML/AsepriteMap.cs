using System.Collections.Generic;

namespace GameXML
{
    public class AsepriteMap
    {
        public List<Flower> TextureList = new List<Flower>();
    }

    public class Flower
    {
        public frame frame = new frame();
        public string rotated;
        public string trimmed;
        public List<spriteSourceSize> spriteSourceSize = new List<spriteSourceSize>();
        public List<sourceSize> sourceSize = new List<sourceSize>();
    }

    public class frame
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    public class spriteSourceSize
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    public class sourceSize
    {
        public int w;
        public int h;
    }
}