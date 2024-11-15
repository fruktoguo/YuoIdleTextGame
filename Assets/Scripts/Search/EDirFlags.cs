using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace YuoTools.Search
{
    [Flags]
    public enum EDirFlags
    {
        NONE = 0,
        NORTH = 1,
        SOUTH = 2,
        EAST = 4,
        WEST = 8,
        NORTHEAST = 16,
        NORTHWEST = 32,
        SOUTHEAST = 64,
        SOUTHWEST = 128,
        ALL = NORTH | SOUTH | EAST | WEST | NORTHEAST | NORTHWEST | SOUTHEAST | SOUTHWEST,
    }

    public static class DirFlags
    {
        public static int ToArrayIndex(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.NORTHWEST:
                    return 0;
                case EDirFlags.NORTH:
                    return 1;
                case EDirFlags.NORTHEAST:
                    return 2;
                case EDirFlags.WEST:
                    return 3;
                case EDirFlags.EAST:
                    return 4;
                case EDirFlags.SOUTHWEST:
                    return 5;
                case EDirFlags.SOUTH:
                    return 6;
                case EDirFlags.SOUTHEAST:
                    return 7;
                default:
                    return -1;
            }
        }

        public static bool IsStraight(EDirFlags dir)
        {
            return (dir & (EDirFlags.NORTH | EDirFlags.SOUTH | EDirFlags.EAST | EDirFlags.WEST)) != EDirFlags.NONE;
        }

        public static bool IsDiagonal(EDirFlags dir)
        {
            return (dir & (EDirFlags.NORTHEAST | EDirFlags.NORTHWEST | EDirFlags.SOUTHEAST | EDirFlags.SOUTHWEST)) !=
                   EDirFlags.NONE;
        }

        public static EDirFlags DiagonalToEastWest(EDirFlags dir)
        {
            if ((dir & (EDirFlags.NORTHEAST | EDirFlags.SOUTHEAST)) != EDirFlags.NONE)
            {
                return EDirFlags.EAST;
            }

            if ((dir & (EDirFlags.NORTHWEST | EDirFlags.SOUTHWEST)) != EDirFlags.NONE)
            {
                return EDirFlags.WEST;
            }

            return EDirFlags.NONE;
        }

        public static EDirFlags DiagonalToNorthSouth(EDirFlags dir)
        {
            if ((dir & (EDirFlags.NORTHEAST | EDirFlags.NORTHWEST)) != EDirFlags.NONE)
            {
                return EDirFlags.NORTH;
            }

            if ((dir & (EDirFlags.SOUTHEAST | EDirFlags.SOUTHWEST)) != EDirFlags.NONE)
            {
                return EDirFlags.SOUTH;
            }

            return EDirFlags.NONE;
        }

        private static readonly Dictionary<EDirFlags, int2> DirToPos = new Dictionary<EDirFlags, int2>()
        {
            { EDirFlags.NORTH, new int2(0, -1) },
            { EDirFlags.SOUTH, new int2(0, 1) },
            { EDirFlags.EAST, new int2(1, 0) },
            { EDirFlags.WEST, new int2(-1, 0) },
            { EDirFlags.NORTHEAST, new int2(1, -1) },
            { EDirFlags.NORTHWEST, new int2(-1, -1) },
            { EDirFlags.SOUTHEAST, new int2(1, 1) },
            { EDirFlags.SOUTHWEST, new int2(-1, 1) },
        };

        public static int2 ToPos(EDirFlags dir)
        {
            return DirToPos[dir];
        }
    }
    
    public static class ExInt2
    {
        public static int2 Foward(this int2 x, EDirFlags dir)
        {
            return x + DirFlags.ToPos(dir);
        }

        public static int2 Backward(this int2 x, EDirFlags dir)
        {
            return x - DirFlags.ToPos(dir);
        }
    }
}