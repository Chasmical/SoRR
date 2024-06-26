using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static class Direction
    {
        // Note: Dir4 and Dir8 are normalized by &'ing with 3 (0b11) and 7 (0b111) respectively

        [Pure] public static Dir4 DegreesToDir4(float degrees)
        {
            Debug.Assert(degrees is >= 0 and < 360);
            return (Dir4)(Mathf.RoundToInt(degrees * (4 / 360f)) & 3);
        }
        [Pure] public static Dir8 DegreesToDir8(float degrees)
        {
            Debug.Assert(degrees is >= 0 and < 360);
            return (Dir8)(Mathf.RoundToInt(degrees * (8 / 360f)) & 7);
        }
        [Pure] public static Dir24 DegreesToDir24(float degrees)
        {
            Debug.Assert(degrees is >= 0 and < 360);
            int result = Mathf.RoundToInt(degrees * (24 / 360f));
            return result == 24 ? 0 : (Dir24)result;
        }

        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOf<T>()
        {
            if (typeof(T) == typeof(Dir4)) return 4;
            if (typeof(T) == typeof(Dir8)) return 8;
            if (typeof(T) == typeof(Dir24)) return 24;
            return 0;
        }

        [Pure] public static bool IsValid(Dir4 dir)
            => (uint)dir < 4;
        [Pure] public static bool IsValid(Dir8 dir)
            => (uint)dir < 8;
        [Pure] public static bool IsValid(Dir24 dir)
            => (uint)dir < 24;

    }
    public enum Dir4
    {
        North,
        East,
        South,
        West,
    }
    public enum Dir8
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
    }
    public enum Dir24
    {
        // ReSharper disable InconsistentNaming IdentifierTypo
        N,
        NNNE,
        NNE,
        NE,
        NEE,
        NEEE,
        E,
        SEEE,
        SEE,
        SE,
        SSE,
        SSSE,
        S,
        SSSW,
        SSW,
        SW,
        SWW,
        SWWW,
        W,
        NWWW,
        NWW,
        NW,
        NNW,
        NNNW,
        // ReSharper restore InconsistentNaming IdentifierTypo
    }
}
