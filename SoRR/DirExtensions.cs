using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static class DirExtensions
    {
        // Note: Dir4 and Dir8 are normalized by &'ing with 3 (0b11) and 7 (0b111) respectively

        [Pure] public static Dir4 ToDir4(this Dir8 dir)
            => (Dir4)((int)dir / 2);
        [Pure] public static Dir4 ToDir4(this Dir24 dir)
            => (Dir4)((((int)dir + 2) / 3) & 7);

        [Pure] public static Dir8 ToDir8(this Dir4 dir)
            => (Dir8)((int)dir * 2);
        [Pure] public static Dir8 ToDir8(this Dir24 dir)
            => (Dir8)((((int)dir + 1) / 3) & 7);

        [Pure] public static Dir24 ToDir24(this Dir4 dir)
            => (Dir24)((int)dir * 6);
        [Pure] public static Dir24 ToDir24(this Dir8 dir)
            => (Dir24)((int)dir * 3);

        [Pure] public static int ToDegrees(this Dir4 dir)
            => (int)dir * 90;
        [Pure] public static int ToDegrees(this Dir8 dir)
            => (int)dir * 45;
        [Pure] public static int ToDegrees(this Dir24 dir)
            => (int)dir * 15;

        [Pure] public static float ToRadians(this Dir4 dir)
            => (int)dir * (Mathf.PI / 2);
        [Pure] public static float ToRadians(this Dir8 dir)
            => (int)dir * (Mathf.PI / 4);
        [Pure] public static float ToRadians(this Dir24 dir)
            => (int)dir * (Mathf.PI / 12);

        private static readonly string[] directions8 = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

        [Pure] public static string ToLetters(this Dir4 dir)
            => directions8[(int)dir * 2];
        [Pure] public static string ToLetters(this Dir8 dir)
            => directions8[(int)dir];

        [Pure] public static Dir4 Clockwise(this Dir4 dir)
            => (Dir4)(((int)dir + 1) & 3);
        [Pure] public static Dir8 Clockwise(this Dir8 dir)
            => (Dir8)(((int)dir + 1) & 7);
        [Pure] public static Dir24 Clockwise(this Dir24 dir)
            => dir == (Dir24)23 ? 0 : dir + 1;

        [Pure] public static Dir4 Rotate(this Dir4 dir, int turns)
            => (Dir4)(((int)dir + turns) & 3);
        [Pure] public static Dir8 Rotate(this Dir8 dir, int turns)
            => (Dir8)(((int)dir + turns) & 7);
        [Pure] public static Dir24 Rotate(this Dir24 dir, int turns)
        {
            int result = ((int)dir + turns) % 24;
            return (Dir24)(result < 0 ? result + 24 : result);
        }

    }
}
