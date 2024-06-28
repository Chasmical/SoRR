using UnityEngine;

namespace SoRR
{
    internal static class Assets
    {
        public static UnityAssetManager Manager { get; } = new(ResourcesAPI.ActiveAPI);

        public static Sprite LoadSprite(string path)
            => Manager.LoadSprite(path);
        public static Texture2D LoadTexture(string path)
            => Manager.LoadTexture(path);
        public static AudioClip LoadAudio(string path)
            => Manager.LoadAudio(path);
        public static string LoadText(string path)
            => Manager.LoadText(path);
        public static byte[] LoadBinary(string path)
            => Manager.LoadBinary(path);

    }
}
