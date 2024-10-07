using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static class Direction
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOf<T>()
        {
            if (typeof(T) == typeof(Dir4)) return 4;
            if (typeof(T) == typeof(Dir8)) return 8;
            if (typeof(T) == typeof(Dir24)) return 24;
            throw new ArgumentException($"Type {typeof(T)} is not a direction enum.", nameof(T));
        }

        [Pure] public static Dir4 DegreesToDir4(float degrees)
            => Normalize(UnitsToDirCore<Dir4>(degrees, 360f));
        [Pure] public static Dir8 DegreesToDir8(float degrees)
            => Normalize(UnitsToDirCore<Dir8>(degrees, 360f));
        [Pure] public static Dir24 DegreesToDir24(float degrees)
            => NormalizeFast(UnitsToDirCore<Dir24>(degrees, 360f));

        [Pure] public static Dir4 RadiansToDir4(float radians)
            => Normalize(UnitsToDirCore<Dir4>(radians, 2f * Mathf.PI));
        [Pure] public static Dir8 RadiansToDir8(float radians)
            => Normalize(UnitsToDirCore<Dir8>(radians, 2f * Mathf.PI));
        [Pure] public static Dir24 RadiansToDir24(float radians)
            => NormalizeFast(UnitsToDirCore<Dir24>(radians, 2f * Mathf.PI));

        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TDir UnitsToDirCore<TDir>(float units, float fullRotation)
        {
            int value = Mathf.RoundToInt(units * (CountOf<TDir>() / fullRotation));
            return Unsafe.As<int, TDir>(ref value);
        }

        [Pure] public static bool IsValid(Dir4 dir)
            => IsValidCore(dir);
        [Pure] public static bool IsValid(Dir8 dir)
            => IsValidCore(dir);
        [Pure] public static bool IsValid(Dir24 dir)
            => IsValidCore(dir);

        [Pure] private static bool IsValidCore<TDir>(TDir dir)
            => Unsafe.As<TDir, uint>(ref dir) < CountOf<TDir>();

        [Pure] public static Dir4 Normalize(Dir4 dir)
            => (Dir4)((int)dir & 3);
        [Pure] public static Dir8 Normalize(Dir8 dir)
            => (Dir8)((int)dir & 7);
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dir24 Normalize(Dir24 dir)
        {
            int result = (int)dir % 24;
            return (Dir24)(result < 0 ? result + 24 : result);
        }
        [Pure] public static Dir24 NormalizeFast(Dir24 dir)
            => dir == (Dir24)24 ? 0 : dir;

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
        // ReSharper disable InconsistentNaming
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
        // ReSharper restore InconsistentNaming
    }
}
