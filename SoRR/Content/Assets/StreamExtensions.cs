using System;
using System.IO;

namespace SoRR
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            if (!stream.TryGetLength(out int byteLength))
            {
                // If the stream's size is known, allocate and populate an array
                byte[] array = new byte[byteLength];
                if (stream.Read(array) != byteLength) throw new InvalidOperationException();
                return array;
            }

            // If the stream's size can't be determined, use a MemoryStream
            MemoryStream memory = new();
            stream.CopyTo(memory);
            return memory.ToArray();
        }

        public static bool TryGetLength(this Stream stream, out int byteLength)
        {
            // Try to determine the stream's length in bytes
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
