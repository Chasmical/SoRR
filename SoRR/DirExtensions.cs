using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static class DirExtensions
    {
        [Pure] public static int ToDegrees(this Dir4 dir)
            => ToDegreesCore(dir);
        [Pure] public static int ToDegrees(this Dir8 dir)
            => ToDegreesCore(dir);
        [Pure] public static int ToDegrees(this Dir24 dir)
            => ToDegreesCore(dir);

        [Pure] private static int ToDegreesCore<TDir>(TDir dir)
            => Unsafe.As<TDir, int>(ref dir) * (360 / Direction.CountOf<TDir>());

        [Pure] public static float ToRadians(this Dir4 dir)
            => ToRadiansCore(dir);
        [Pure] public static float ToRadians(this Dir8 dir)
            => ToRadiansCore(dir);
        [Pure] public static float ToRadians(this Dir24 dir)
            => ToRadiansCore(dir);

        [Pure] private static float ToRadiansCore<TDir>(TDir dir)
            => Unsafe.As<TDir, int>(ref dir) * (2f * Mathf.PI / Direction.CountOf<TDir>());

        private static readonly string[] directions8 = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

        [Pure] public static string ToLetters(this Dir4 dir)
            => directions8[(int)dir * 2];
        [Pure] public static string ToLetters(this Dir8 dir)
            => directions8[(int)dir];

        [Pure] public static Dir4 Rotate(this Dir4 dir, int delta)
            => Direction.Normalize(RotateCore(dir, delta));
        [Pure] public static Dir8 Rotate(this Dir8 dir, int delta)
            => Direction.Normalize(RotateCore(dir, delta));
        [Pure] public static Dir24 Rotate(this Dir24 dir, int delta)
            => Direction.Normalize(RotateCore(dir, delta));

        [Pure] public static Dir4 Clockwise(this Dir4 dir)
            => Direction.Normalize(RotateCore(dir, 1));
        [Pure] public static Dir8 Clockwise(this Dir8 dir)
            => Direction.Normalize(RotateCore(dir, 1));
        [Pure] public static Dir24 Clockwise(this Dir24 dir)
            => Direction.NormalizeFast(RotateCore(dir, 1));

        [Pure] private static TDir RotateCore<TDir>(TDir dir, int delta)
        {
            int result = Unsafe.As<TDir, int>(ref dir) + delta;
            return Unsafe.As<int, TDir>(ref result);
        }

        // the conversion methods can't be generalized properly without losing performance

        [Pure] public static Dir8 ToDir8(this Dir4 dir)
            => (Dir8)((int)dir * 2);
        [Pure] public static Dir24 ToDir24(this Dir4 dir)
            => (Dir24)((int)dir * 6);

        [Pure] public static Dir4 ToDir4(this Dir8 dir)
            => (Dir4)((int)dir / 2);
        [Pure] public static Dir24 ToDir24(this Dir8 dir)
            => (Dir24)((int)dir * 3);

        [Pure] public static Dir4 ToDir4(this Dir24 dir)
            => (Dir4)((((int)dir + 2) / 6) & 3);
        [Pure] public static Dir8 ToDir8(this Dir24 dir)
            => (Dir8)((((int)dir + 1) / 3) & 7);

    }
}
