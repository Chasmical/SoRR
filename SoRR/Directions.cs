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
        /// <summary>Represents the north, 0° clockwise.</summary>
        North,
        /// <summary>Represents the east, 90° clockwise.</summary>
        East,
        /// <summary>Represents the south, 180° clockwise.</summary>
        South,
        /// <summary>Represents the west, 270° clockwise.</summary>
        West,
    }
    /// <summary>
    ///   <para>Defines the four cardinal and four intercardinal directions: N, NE, E, SE, S, SW, W, NW.</para>
    /// </summary>
    public enum Dir8
    {
        /// <summary>Represents the north, 0° clockwise.</summary>
        North,
        /// <summary>Represents the north-east, 45° clockwise.</summary>
        NorthEast,
        /// <summary>Represents the east, 90° clockwise.</summary>
        East,
        /// <summary>Represents the south-east, 135° clockwise.</summary>
        SouthEast,
        /// <summary>Represents the south, 180° clockwise.</summary>
        South,
        /// <summary>Represents the south-west, 225° clockwise.</summary>
        SouthWest,
        /// <summary>Represents the west, 270° clockwise.</summary>
        West,
        /// <summary>Represents the north-west, 315° clockwise.</summary>
        NorthWest,
    }
    /// <summary>
    ///   <para>Defines 24 directions: four cardinal, four intercardinal and 16 in-between them (2 per every division).</para>
    /// </summary>
    public enum Dir24
    {
        // ReSharper disable InconsistentNaming
        /// <summary>Represents the north, 0° clockwise.</summary>
        N,
        /// <summary>Represents the north-north-north-east, 15° clockwise.</summary>
        NNNE,
        /// <summary>Represents the north-north-east, 30° clockwise.</summary>
        NNE,
        /// <summary>Represents the north-east, 45° clockwise.</summary>
        NE,
        /// <summary>Represents the north-east-east, 60° clockwise.</summary>
        NEE,
        /// <summary>Represents the north-east-east-east, 75° clockwise.</summary>
        NEEE,
        /// <summary>Represents the east, 90° clockwise.</summary>
        E,
        /// <summary>Represents the south-east-east-east, 105° clockwise.</summary>
        SEEE,
        /// <summary>Represents the south-east-east, 120° clockwise.</summary>
        SEE,
        /// <summary>Represents the south-east, 135° clockwise.</summary>
        SE,
        /// <summary>Represents the south-south-east, 150° clockwise.</summary>
        SSE,
        /// <summary>Represents the south-south-south-east, 165° clockwise.</summary>
        SSSE,
        /// <summary>Represents the south, 180° clockwise.</summary>
        S,
        /// <summary>Represents the south-south-south-west, 195° clockwise.</summary>
        SSSW,
        /// <summary>Represents the south-south-west, 210° clockwise.</summary>
        SSW,
        /// <summary>Represents the south-west, 225° clockwise.</summary>
        SW,
        /// <summary>Represents the south-west-west, 240° clockwise.</summary>
        SWW,
        /// <summary>Represents the south-west-west-west, 255° clockwise.</summary>
        SWWW,
        /// <summary>Represents the west, 270° clockwise.</summary>
        W,
        /// <summary>Represents the north-west-west-west, 285° clockwise.</summary>
        NWWW,
        /// <summary>Represents the north-west-west, 300° clockwise.</summary>
        NWW,
        /// <summary>Represents the north-west, 315° clockwise.</summary>
        NW,
        /// <summary>Represents the north-north-west, 330° clockwise.</summary>
        NNW,
        /// <summary>Represents the north-north-north-west, 345° clockwise.</summary>
        NNNW,
        // ReSharper restore InconsistentNaming
    }
}
