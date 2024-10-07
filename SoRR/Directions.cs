using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides information about, and means to create direction enumerations.</para>
    /// </summary>
    public static class Direction
    {
        /// <summary>
        ///   <para>Returns the amount of directions defined in the <typeparamref name="T"/> direction enum.</para>
        /// </summary>
        /// <typeparam name="T">The direction enumeration type.</typeparam>
        /// <returns>The amount of directions defined in the <typeparamref name="T"/> direction enum.</returns>
        /// <exception cref="NotSupportedException">Type <typeparamref name="T"/> is not a direction enum.</exception>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountOf<T>()
        {
            if (typeof(T) == typeof(Dir4)) return 4;
            if (typeof(T) == typeof(Dir8)) return 8;
            if (typeof(T) == typeof(Dir24)) return 24;
            throw new NotSupportedException($"Type {typeof(T)} is not a direction enum.");
        }

        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="degrees"/>, to a <see cref="Dir4"/> direction.</para>
        /// </summary>
        /// <param name="degrees">The rotation angle, in degrees clockwise.</param>
        /// <returns>A <see cref="Dir4"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir4 DegreesToDir4(float degrees)
            => Normalize(UnitsToDirCore<Dir4>(degrees, 360f));
        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="degrees"/>, to a <see cref="Dir8"/> direction.</para>
        /// </summary>
        /// <param name="degrees">The rotation angle, in degrees clockwise.</param>
        /// <returns>A <see cref="Dir8"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir8 DegreesToDir8(float degrees)
            => Normalize(UnitsToDirCore<Dir8>(degrees, 360f));
        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="degrees"/>, to a <see cref="Dir24"/> direction.</para>
        /// </summary>
        /// <param name="degrees">The rotation angle, in degrees clockwise.</param>
        /// <returns>A <see cref="Dir24"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir24 DegreesToDir24(float degrees)
            => Normalize(UnitsToDirCore<Dir24>(degrees, 360f));

        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="radians"/>, to a <see cref="Dir4"/> direction.</para>
        /// </summary>
        /// <param name="radians">The rotation angle, in radians clockwise.</param>
        /// <returns>A <see cref="Dir4"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir4 RadiansToDir4(float radians)
            => Normalize(UnitsToDirCore<Dir4>(radians, 2f * Mathf.PI));
        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="radians"/>, to a <see cref="Dir8"/> direction.</para>
        /// </summary>
        /// <param name="radians">The rotation angle, in radians clockwise.</param>
        /// <returns>A <see cref="Dir8"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir8 RadiansToDir8(float radians)
            => Normalize(UnitsToDirCore<Dir8>(radians, 2f * Mathf.PI));
        /// <summary>
        ///   <para>Converts the specified rotation, in <paramref name="radians"/>, to a <see cref="Dir24"/> direction.</para>
        /// </summary>
        /// <param name="radians">The rotation angle, in radians clockwise.</param>
        /// <returns>A <see cref="Dir24"/> direction closest to the specified rotation.</returns>
        [Pure] public static Dir24 RadiansToDir24(float radians)
            => Normalize(UnitsToDirCore<Dir24>(radians, 2f * Mathf.PI));

        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TDir UnitsToDirCore<TDir>(float units, float fullRotation)
        {
            int value = Mathf.RoundToInt(units * (CountOf<TDir>() / fullRotation));
            return Unsafe.As<int, TDir>(ref value);
        }

        /// <summary>
        ///   <para>Determines whether the specified <paramref name="dir"/> specifies a valid <see cref="Dir4"/> direction.</para>
        /// </summary>
        /// <param name="dir">The direction value to validate.</param>
        /// <returns><see langword="true"/>, if <paramref name="dir"/> specifies a valid <see cref="Dir4"/> direction; otherwise, <see langword="false"/>.</returns>
        [Pure] public static bool IsValid(Dir4 dir)
            => IsValidCore(dir);
        /// <summary>
        ///   <para>Determines whether the specified <paramref name="dir"/> specifies a valid <see cref="Dir8"/> direction.</para>
        /// </summary>
        /// <param name="dir">The direction value to validate.</param>
        /// <returns><see langword="true"/>, if <paramref name="dir"/> specifies a valid <see cref="Dir8"/> direction; otherwise, <see langword="false"/>.</returns>
        [Pure] public static bool IsValid(Dir8 dir)
            => IsValidCore(dir);
        /// <summary>
        ///   <para>Determines whether the specified <paramref name="dir"/> specifies a valid <see cref="Dir24"/> direction.</para>
        /// </summary>
        /// <param name="dir">The direction value to validate.</param>
        /// <returns><see langword="true"/>, if <paramref name="dir"/> specifies a valid <see cref="Dir24"/> direction; otherwise, <see langword="false"/>.</returns>
        [Pure] public static bool IsValid(Dir24 dir)
            => IsValidCore(dir);

        [Pure] private static bool IsValidCore<TDir>(TDir dir)
            => Unsafe.As<TDir, uint>(ref dir) < CountOf<TDir>();

        /// <summary>
        ///   <para>Normalizes the specified <paramref name="dir"/> to the range of valid <see cref="Dir4"/> directions.</para>
        /// </summary>
        /// <param name="dir">The direction value to normalize.</param>
        /// <returns>The specified <paramref name="dir"/>, normalized to the range of valid <see cref="Dir4"/> directions.</returns>
        [Pure] public static Dir4 Normalize(Dir4 dir)
            => (Dir4)((int)dir & 3);
        /// <summary>
        ///   <para>Normalizes the specified <paramref name="dir"/> to the range of valid <see cref="Dir8"/> directions.</para>
        /// </summary>
        /// <param name="dir">The direction value to normalize.</param>
        /// <returns>The specified <paramref name="dir"/>, normalized to the range of valid <see cref="Dir8"/> directions.</returns>
        [Pure] public static Dir8 Normalize(Dir8 dir)
            => (Dir8)((int)dir & 7);
        /// <summary>
        ///   <para>Normalizes the specified <paramref name="dir"/> to the range of valid <see cref="Dir24"/> directions.</para>
        /// </summary>
        /// <param name="dir">The direction value to normalize.</param>
        /// <returns>The specified <paramref name="dir"/>, normalized to the range of valid <see cref="Dir24"/> directions.</returns>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dir24 Normalize(Dir24 dir)
        {
            int result = (int)dir % 24;
            return (Dir24)(result < 0 ? result + 24 : result);
        }
        /// <summary>
        ///   <para>Normalizes the specified <paramref name="dir"/> to the range of valid <see cref="Dir24"/> directions, using a fast-path that only normalizes <c>(Dir24)24</c> to <c>0</c>.</para>
        /// </summary>
        /// <param name="dir">The direction value to normalize.</param>
        /// <returns>The specified <paramref name="dir"/>, normalized to the range of valid <see cref="Dir24"/> directions.</returns>
        [Pure] public static Dir24 NormalizeFast(Dir24 dir)
        {
            Debug.Assert((int)dir is >= 0 and <= 24);
            return dir == (Dir24)24 ? 0 : dir;
        }

    }
    /// <summary>
    ///   <para>Defines the four cardinal directions: <see cref="North"/>, <see cref="East"/>, <see cref="South"/> and <see cref="West"/>.</para>
    /// </summary>
    public enum Dir4
    {
        North,
        East,
        South,
        West,
    }
    /// <summary>
    ///   <para>Defines the four cardinal and four intercardinal directions: N, NE, E, SE, S, SW, W, NW.</para>
    /// </summary>
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
    /// <summary>
    ///   <para>Defines 24 directions: four cardinal, four intercardinal and 16 in-between them (2 per every division).</para>
    /// </summary>
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
