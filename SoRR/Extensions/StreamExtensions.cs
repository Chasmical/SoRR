using System;
using System.IO;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of extension methods for the <see cref="Stream"/> class.</para>
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        ///   <para>Writes the specified <paramref name="stream"/>'s content to an array and returns it.</para>
        /// </summary>
        /// <param name="stream">The stream to read content from.</param>
        /// <returns>A byte array containing the specified <paramref name="stream"/>'s content.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="stream"/>'s content length did not match the retrieved length.</exception>
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
        /// <summary>
        ///   <para>Retrieves a byte array containing the specified <paramref name="stream"/>'s content. May return an exposed <see cref="MemoryStream"/>'s buffer, avoiding an allocation.</para>
        /// </summary>
        /// <param name="stream">The stream to read content from.</param>
        /// <returns>A byte array containing the specified <paramref name="stream"/>'s content.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="stream"/>'s content length did not match the retrieved length.</exception>
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

        /// <summary>
        ///   <para>Attempts to retrieve the specified <paramref name="stream"/>'s <see cref="Stream.Length"/>, and returns a value indicating whether the operation was successful.</para>
        /// </summary>
        /// <param name="stream">The stream to determine the byte length of.</param>
        /// <param name="byteLength">When this method returns, contains the specified <paramref name="stream"/>'s <see cref="Stream.Length"/>, if the operation was successful, or -1 if the operation failed.</param>
        /// <returns><see langword="true"/>, if the specified <paramref name="stream"/>'s <see cref="Stream.Length"/> was successfully retrieved; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
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
