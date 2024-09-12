using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static partial class AssetUtility
    {
        // Order of bytes in an int32/int64 may differ depending on the machine's endianness
        private static readonly int OggHeader = ByteSequenceToInt32(+'O', +'g', +'g', +'S');
        private static readonly int RiffHeader = ByteSequenceToInt32(+'R', +'I', +'F', +'F');
        private static readonly int WaveHeader = ByteSequenceToInt32(+'W', +'A', +'V', +'E');
        private static readonly long PngHeader = ByteSequenceToInt64(0x89, +'P', +'N', +'G', +'\r', +'\n', 0x1A, +'\n');

        [Pure] private static int ByteSequenceToInt32(params Span<byte> span)
        {
            Debug.Assert(span.Length == 4);
            return Unsafe.As<byte, int>(ref span[0]);
        }
        [Pure] private static long ByteSequenceToInt64(params Span<byte> span)
        {
            Debug.Assert(span.Length == 8);
            return Unsafe.As<byte, long>(ref span[0]);
        }

        public const int MinBytesToDetectAudioFormat = 12; // wav header length
        public const int MinBytesToDetectImageFormat = 8; // png header length

        [Pure] public static AudioType DetectAudioFormat(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length < MinBytesToDetectAudioFormat) return AudioType.UNKNOWN;

            ref int int32 = ref Unsafe.As<byte, int>(ref Unsafe.AsRef(in rawData[0]));

            if (int32 == OggHeader)
                return AudioType.OGGVORBIS;

            if (int32 == RiffHeader && Unsafe.Add(ref int32, 2) == WaveHeader)
                return AudioType.WAV;

            if (rawData[0] is +'I' && rawData[1] is +'D' && rawData[2] is +'3' || rawData[0] is 0xFF && (rawData[1] | 1) is 0xFB)
                return AudioType.MPEG;

            return AudioType.UNKNOWN;
        }

        [Pure] public static ImageType DetectImageFormat(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length < MinBytesToDetectImageFormat) return ImageType.UNKNOWN;

            ref long int64 = ref Unsafe.As<byte, long>(ref Unsafe.AsRef(in rawData[0]));

            if (int64 == PngHeader)
                return ImageType.PNG;

            // 0xFF - marker, 0xD8 - Start of Image, 0xFF - marker
            if (rawData[0] is 0xFF && rawData[1] is 0xD8 && rawData[2] is 0xFF)
                return ImageType.JPEG;

            return ImageType.UNKNOWN;
        }

        [Pure] public static AssetFormat DetectFormat(ReadOnlySpan<char> pathOrExtension)
        {
            pathOrExtension = Path.GetExtension(pathOrExtension);
            return pathOrExtension switch
            {
                ".mp3" => AssetFormat.Mp3,
                ".ogg" => AssetFormat.Ogg,
                ".wav" => AssetFormat.Wav,

                ".png" => AssetFormat.Png,
                ".jpg" or ".jpeg" => AssetFormat.Jpeg,

                ".mp4" => AssetFormat.Mp4,

                ".txt" => AssetFormat.Txt,
                ".csv" => AssetFormat.Csv,
                ".json" => AssetFormat.Json,
                ".yaml" => AssetFormat.Yaml,
                ".xml" => AssetFormat.Xml,

                ".bin" or ".bytes" => AssetFormat.Bin,

                _ => AssetFormat.Unknown,
            };
        }

        [Pure] public static AssetType ToType(this AssetFormat format)
            => (AssetType)((int)(format + 31) >> 5);

    }
    public enum ImageType
    {
        // ReSharper disable InconsistentNaming
        UNKNOWN = 0,
        PNG = 33,
        JPEG = 34,
        // ReSharper restore InconsistentNaming
    }
    public enum AssetType
    {
        // ReSharper disable InconsistentNaming
        UNKNOWN = 0,
        AUDIO = 1,
        IMAGE = 2,
        VIDEO = 3,
        TEXT = 4,
        BINARY = 5,
        // ReSharper restore InconsistentNaming
    }
    public enum AssetFormat
    {
        Unknown = 0,

        // [1, 32] - reserved for direct mapping to and from AudioType
        Mp3 = AudioType.MPEG,
        Ogg = AudioType.OGGVORBIS,
        Wav = AudioType.WAV,

        // [33, 64] - reserved for direct mapping to and from ImageType
        Png = ImageType.PNG,
        Jpeg = ImageType.JPEG,

        // [65, 96] - video formats
        Mp4 = 65,

        // [97, 128] - text formats
        Txt = 97,
        Csv = 98,
        Json = 99,
        Yaml = 100,
        Xml = 101,

        // [129, 160] - binary format
        Bin = 129,

    }
}
