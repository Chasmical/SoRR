using UnityEngine;

namespace SoRR
{
    public static class RectTransformExtensions
    {
        private static readonly Vector2[] minAnchors = [
            new Vector2(0f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
        ];

        public static void Position(this RectTransform rect, Anchor anchor, Vector2 offset, Vector2 size)
        {
            Vector2 pos = minAnchors[(int)anchor];
            rect.anchorMin = pos;
            rect.anchorMax = pos;
            rect.pivot = pos;

        }

    }
    public enum Anchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        LeftCenter,
        Center,
        RightCenter,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }
}
