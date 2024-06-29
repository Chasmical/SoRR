using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoRR
{
    public static partial class AssetUtility
    {
        public static Sprite CreateSprite(byte[] rawData, Rect? region = null, float ppu = 64f)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData));
            if (ppu <= 0f) throw new ArgumentOutOfRangeException(nameof(ppu), ppu, $"{nameof(ppu)} is less than or equal to 0.");

            Texture2D texture = new Texture2D(15, 9);
            texture.LoadImage(rawData);
            texture.filterMode = FilterMode.Point;
            Rect rect = region ?? new Rect(0f, 0f, texture.width, texture.height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), ppu, 1u, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        public static Task<AudioClip> CreateAudioClipAsync(string filePath)
            => CreateAudioClipAsync(filePath, AudioType.UNKNOWN);
        public static Task<AudioClip> CreateAudioClipAsync(byte[] rawData)
        {
            AudioType format = DetectAudioFormat(rawData);
            using (TempFile temp = RentTempFile(rawData))
                return CreateAudioClipAsync(temp.Path, format);
        }
        public static Task<AudioClip> CreateAudioClipAsync(string filePath, AudioType format)
        {
            if (format == AudioType.UNKNOWN)
                format = DetectAudioFormat(ReadFixedBytesFromFile(filePath, 12));

            TaskCompletionSource<AudioClip> source = new();

            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, format);
            Awaitable.Awaiter awaiter = uwr.SendWebRequest().GetAwaiter();
            awaiter.OnCompleted(() => source.SetResult(DownloadHandlerAudioClip.GetContent(uwr)));

            return source.Task;
        }

        private static byte[] ReadFixedBytesFromFile(string fileName, int count)
        {
            byte[] buffer = new byte[count];
            using (BinaryReader reader = new BinaryReader(File.OpenRead(fileName)))
                _ = reader.Read(buffer);
            return buffer;
        }
        private static TempFile RentTempFile(byte[]? data = null)
        {
            string tempPath = Path.GetTempFileName();
            if (data is not null) File.WriteAllBytes(tempPath, data);
            return new TempFile(tempPath);
        }

        private sealed record TempFile(string Path) : IDisposable
        {
            public void Dispose() => File.Delete(Path);
        }
    }
}
