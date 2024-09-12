using System;
using System.IO;
using JetBrains.Annotations;

namespace SoRR
{
    public static class StreamExtensions
    {
        [MustUseReturnValue] public static byte[] ToByteArray(this Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            if (stream.TryGetLength(out int byteLength))
            {
                // If the stream's size is known, allocate and populate an array
                byte[] array = new byte[byteLength];
                if (stream.Read(array) != byteLength) throw new InvalidOperationException();
                return array;
            }
            // If the stream's size cannot be determined, use a MemoryStream
            MemoryStream temp = new();
            stream.CopyTo(temp);
            return temp.ToArray();
        }
        [MustUseReturnValue] public static byte[] ToByteArrayDangerous(this Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            // If the stream is an exposed MemoryStream, try to use its buffer directly without copying
            if (stream is MemoryStream memory && memory.TryGetBuffer(out ArraySegment<byte> segment))
            {
                byte[] array = segment.Array!;
                if (segment.Offset == 0 && segment.Count == array.Length)
                    return array;
            }
            // Use the safe way with copying instead
            return ToByteArray(stream);
        }

        [Pure] public static bool TryGetLength(this Stream stream, out int byteLength)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            try
            {
                byteLength = checked((int)stream.Length);
                return true;
            }
            catch (NotSupportedException)
            {
                byteLength = -1;
                return false;
            }
        }

    }
}
