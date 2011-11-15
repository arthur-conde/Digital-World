using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Helpers
{
    public class Position
    {
        public short Map = 1;
        public int PosX = 0;
        public int PosY = 0;

        public Position()
        {
            Map = 1;
            PosX = 50;
            PosY = 50;
        }

        public Position(short map, int x, int y)
        {
            Map = map;
            PosX = x;
            PosY = y;
        }
    }
}
