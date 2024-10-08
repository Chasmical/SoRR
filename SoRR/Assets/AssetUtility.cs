using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of static methods for creating and manipulating assets and detecting various file formats.</para>
    /// </summary>
    public static partial class AssetUtility
    {
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with the specified texture <paramref name="rawData"/> and <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="rawData">A byte array containing a PNG- or JPEG-encoded image.</param>
        /// <param name="ppu">The pixels-per-unit measure of the sprite to create.</param>
        /// <returns>A <see cref="Sprite"/> created with the specified texture <paramref name="rawData"/> and <paramref name="ppu"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        [Pure] public static Sprite CreateSprite(byte[] rawData, float ppu = 64f)
            => CreateSprite(rawData, Rect.zero, ppu);
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with the specified texture <paramref name="rawData"/>, <paramref name="region"/> and <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="rawData">A byte array containing a PNG- or JPEG-encoded image.</param>
        /// <param name="region">The region of the texture to use for the sprite, or <see langword="default"/> to use the entire texture.</param>
        /// <param name="ppu">The pixels-per-unit measure of the sprite to create.</param>
        /// <returns>A <see cref="Sprite"/> created with the specified texture <paramref name="rawData"/>, <paramref name="region"/> and <paramref name="ppu"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
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

        /// <summary>
        ///   <para>Creates an <see cref="AudioClip"/> from the specified <paramref name="rawData"/>.</para>
        /// </summary>
        /// <param name="rawData">A byte array containing an MP3-, WAV- or Ogg-encoded audio.</param>
        /// <returns>An <see cref="AudioClip"/> created from the specified <paramref name="rawData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        ///   <para>Creates an <see cref="AudioClip"/> from a file at the specified path, auto-detecting the format.</para>
        /// </summary>
        /// <param name="filePath">A path to the MP3-, WAV- or Ogg-encoded audio file.</param>
        /// <returns>An <see cref="AudioClip"/> created from a file at the specified path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
        [Pure] public static Task<AudioClip> CreateAudioClipAsync(string filePath)
            => CreateAudioClipAsync(filePath, AudioType.UNKNOWN);
        /// <summary>
        ///   <para>Creates an <see cref="AudioClip"/> from a file at the specified path, assuming the specified <paramref name="format"/>.</para>
        /// </summary>
        /// <param name="filePath">A path to the MP3-, WAV- or Ogg-encoded audio file.</param>
        /// <param name="format">The audio format to assume the audio file is in.</param>
        /// <returns>An <see cref="AudioClip"/> created from a file at the specified path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
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
