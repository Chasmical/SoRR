using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace SoRR
{
    public static partial class AssetUtility
    {
        [Pure] public static Sprite CreateSprite(byte[] rawData, float ppu = 64f)
            => CreateSprite(rawData, Rect.zero, ppu);
        [Pure] public static Sprite CreateSprite(byte[] rawData, Rect region, float ppu = 64f)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData));
            if (ppu <= 0f) throw new ArgumentOutOfRangeException(nameof(ppu), ppu, $"{nameof(ppu)} is less than or equal to 0.");

            Texture2D texture = new Texture2D(15, 9);
            texture.LoadImage(rawData, false);
            texture.filterMode = FilterMode.Point;
            if (region == Rect.zero) region = new Rect(0f, 0f, texture.width, texture.height);

            return Sprite.Create(texture, region, new Vector2(0.5f, 0.5f), ppu, 1u, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        [Pure] public static async Task<AudioClip> CreateAudioClipAsync(byte[] rawData)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData));

            AudioType format = DetectAudioFormat(rawData);
            string tempFilePath = Path.GetTempFileName();
            try
            {
                await File.WriteAllBytesAsync(tempFilePath, rawData);
                return await CreateAudioClipAsync(tempFilePath, format);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Pure] public static Task<AudioClip> CreateAudioClipAsync(string filePath)
            => CreateAudioClipAsync(filePath, AudioType.UNKNOWN);
        [Pure] public static Task<AudioClip> CreateAudioClipAsync(string filePath, AudioType format)
        {
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));

            if (format == AudioType.UNKNOWN)
                format = DetectAudioFormatFromFile(filePath);

            TaskCompletionSource<AudioClip> source = new();

            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, format);
            var awaiter = uwr.SendWebRequest().GetAwaiter();
            awaiter.OnCompleted(() => source.SetResult(DownloadHandlerAudioClip.GetContent(uwr)));

            return source.Task;
        }

        [Pure] private static AudioType DetectAudioFormatFromFile(string filePath)
        {
            Span<byte> buffer = stackalloc byte[MinBytesToDetectAudioFormat];
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                _ = reader.Read(buffer);
            return DetectAudioFormat(buffer);
        }

    }
}
