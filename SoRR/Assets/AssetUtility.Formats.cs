using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        private static readonly int FirstThreeBytesMask = ByteSequenceToInt32(255, 255, 255, 0);

        // ID3 header used for metadata in MPEG files (fourth byte is irrelevant)
        private static readonly int Id3Header = ByteSequenceToInt32(+'I', +'D', +'3', 0);
        // 0xFF - marker, 0xD8 - Start of Image, 0xFF - marker (fourth byte is irrelevant)
        private static readonly int JpegHeader = ByteSequenceToInt32(0xFF, 0xD8, 0xFF, 0);

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

        /// <summary>
        ///   <para>The minimum amount of bytes required to detect a supported audio format.</para>
        /// </summary>
        public const int MinBytesToDetectAudioFormat = 12; // wav header length
        /// <summary>
        ///   <para>The minimum amount of bytes required to detect a supported image format.</para>
        /// </summary>
        public const int MinBytesToDetectImageFormat = 8; // png header length

        /// <summary>
        ///   <para>Attempts to identify the audio format of the specified <paramref name="rawData"/>.</para>
        /// </summary>
        /// <param name="rawData">A read-only span of bytes to identify.</param>
        /// <returns>The identified audio format, or <see cref="AudioType.UNKNOWN"/> if it could not be identified.</returns>
        [Pure] public static AudioType DetectAudioFormat(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length < MinBytesToDetectAudioFormat) return AudioType.UNKNOWN;

            ref int int32 = ref Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(rawData));

            if (int32 == OggHeader)
                return AudioType.OGGVORBIS;

            if (int32 == RiffHeader && Unsafe.Add(ref int32, 2) == WaveHeader)
                return AudioType.WAV;

            if ((int32 & FirstThreeBytesMask) == Id3Header)
                return AudioType.MPEG;

            if (rawData[0] == 0xFF && (rawData[1] | 1) == 0xFB)
                return AudioType.MPEG;

            return AudioType.UNKNOWN;
        }

        /// <summary>
        ///   <para>Attempts to identify the image format of the specified <paramref name="rawData"/>.</para>
        /// </summary>
        /// <param name="rawData">A read-only span of bytes to identify.</param>
        /// <returns>The identified image format, or <see cref="ImageType.UNKNOWN"/> if it could not be identified.</returns>
        [Pure] public static ImageType DetectImageFormat(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length < MinBytesToDetectImageFormat) return ImageType.UNKNOWN;

            ref long int64 = ref Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(rawData));

            if (int64 == PngHeader)
                return ImageType.PNG;

            ref int int32 = ref Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(rawData));

            if ((int32 & FirstThreeBytesMask) == JpegHeader)
                return ImageType.JPEG;

            return ImageType.UNKNOWN;
        }

        /// <summary>
        ///   <para>Returns the asset format associated with the specified file extension.</para>
        /// </summary>
        /// <param name="pathOrExtension">The file's extension, or a path to the file.</param>
        /// <returns>The identified asset format, or <see cref="AssetFormat.Unknown"/> if it could not be identified.</returns>
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

        /// <summary>
        ///   <para>Returns the asset type of the specified asset <paramref name="format"/>.</para>
        /// </summary>
        /// <param name="format">The asset format to get the type of.</param>
        /// <returns>The asset type of the specified asset <paramref name="format"/>.</returns>
        [Pure] public static AssetType ToType(this AssetFormat format)
            => (AssetType)((int)(format + 31) >> 5);

    }
}
