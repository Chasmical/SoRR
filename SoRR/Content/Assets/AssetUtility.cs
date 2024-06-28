using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SoRR
{
    public static class AssetUtility
    {
        // Order of bytes in an int32 may differ depending on the machine's endianness
        private static readonly int OggHeader = ByteSequenceToInt32(+'O', +'g', +'g', +'S');
        private static readonly int RiffHeader = ByteSequenceToInt32(+'R', +'I', +'F', +'F');
        private static readonly int WaveHeader = ByteSequenceToInt32(+'W', +'A', +'V', +'E');
        private static readonly long PngHeader = ByteSequenceToInt64(0x89, +'P', +'N', +'G', +'\r', +'\n', 0x1A, +'\n');

        private static int ByteSequenceToInt32(byte a, byte b, byte c, byte d)
        {
            Span<byte> bytes = stackalloc byte[4] { a, b, c, d };
            return Unsafe.As<byte, int>(ref bytes[0]);
        }
        private static long ByteSequenceToInt64(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
        {
            Span<byte> bytes = stackalloc byte[8] { a, b, c, d, e, f, g, h };
            return Unsafe.As<byte, long>(ref bytes[0]);
        }

        public static AudioType DetectAudioFormat(ReadOnlySpan<byte> rawData)
        {
            // Need at least 12 bytes (WAV) to detect a format
            if (rawData.Length < 12) return AudioType.UNKNOWN;

            ref int int32 = ref Unsafe.As<byte, int>(ref Unsafe.AsRef(in rawData[0]));

            if (int32 == OggHeader)
                return AudioType.OGGVORBIS;

            if (int32 == RiffHeader && Unsafe.Add(ref int32, 2) == WaveHeader)
                return AudioType.WAV;

            if (rawData[0] is +'I' && rawData[1] is +'D' && rawData[2] is +'3' || rawData[0] is 0xFF && (rawData[1] | 1) is 0xFB)
                return AudioType.MPEG;

            return AudioType.UNKNOWN;
        }

        public static ImageType DetectImageFormat(ReadOnlySpan<byte> rawData)
        {
            // Need at least 8 bytes (PNG) to detect a format
            if (rawData.Length < 8) return ImageType.UNKNOWN;

            ref long int64 = ref Unsafe.As<byte, long>(ref Unsafe.AsRef(in rawData[0]));

            if (int64 == PngHeader)
                return ImageType.PNG;

            // 0xFF - marker, 0xD8 - Start of Image, 0xFF - marker
            if (rawData[0] is 0xFF && rawData[1] is 0xD8 && rawData[2] is 0xFF)
                return ImageType.JPEG;

            return ImageType.UNKNOWN;
        }

        public static AssetType DetectAssetType(ReadOnlySpan<char> pathOrExtension)
        {
            ReadOnlySpan<char> extension = pathOrExtension.IsEmpty || pathOrExtension[0] == '.'
                ? pathOrExtension
                : Path.GetExtension(pathOrExtension);

            return extension switch
            {
                ".png" or ".jpg" or ".jpeg" => AssetType.IMAGE,
                ".ogg" or ".mp3" or ".wav" => AssetType.AUDIO,
                ".mp4" => AssetType.VIDEO,
                ".txt" or ".json" or ".xml" => AssetType.TEXT,
                ".bin" or ".data" => AssetType.BINARY,
                _ => AssetType.UNKNOWN,
            };
        }

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

        public static AudioClip CreateAudioClip(byte[] rawData)
        {
            AudioType format = DetectAudioFormat(rawData);
            throw new NotImplementedException("AudioClip loading not implemented yet.");
        }

    }
    public enum ImageType
    {
        // ReSharper disable InconsistentNaming
        UNKNOWN,
        PNG,
        JPEG,
        // ReSharper restore InconsistentNaming
    }
    public enum AssetType
    {
        // ReSharper disable InconsistentNaming
        UNKNOWN,
        IMAGE,
        AUDIO,
        VIDEO,
        TEXT,
        BINARY,
        // ReSharper restore InconsistentNaming
    }
}
