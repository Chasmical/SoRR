using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of static methods for working with files.</para>
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        ///   <para>Opens the specified file and deserializes its contents as a JSON of the specified <typeparamref name="T"/> type.</para>
        /// </summary>
        /// <typeparam name="T">The type of the serialized data.</typeparam>
        /// <param name="filePath">A path to the file to open.</param>
        /// <returns>The deserialized contents of the specified file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// TODO: path, file and json exceptions
        [MustUseReturnValue] public static T? ReadJson<T>(string filePath)
        {
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));
            return ReadJson<T>(File.OpenRead(filePath));
        }
        /// <summary>
        ///   <para>Deserializes the contents of the specified <paramref name="stream"/> as a JSON of the specified <typeparamref name="T"/> type.</para>
        /// </summary>
        /// <typeparam name="T">The type of the serialized data.</typeparam>
        /// <param name="stream">A stream containing the serialized data.</param>
        /// <returns>The deserialized contents of the specified <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support reading.</exception>
        /// TODO: json exceptions
        [MustUseReturnValue] public static T? ReadJson<T>([HandlesResourceDisposal] Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using (stream)
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
                return new JsonSerializer().Deserialize<T>(jsonReader);
        }

        /// <summary>
        ///   <para>Creates or overwrites the specified file and serializes the specified <paramref name="value"/> as JSON to it.</para>
        /// </summary>
        /// <typeparam name="T">The type of the data to serialize.</typeparam>
        /// <param name="filePath">A path to the file to create or overwrite.</param>
        /// <param name="value">The value to serialize as JSON.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// TODO: path, file and json exceptions
        public static void WriteJson<T>(string filePath, T? value)
        {
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));
            WriteJson(File.Create(filePath), value);
        }
        /// <summary>
        ///   <para>Serializes the specified <paramref name="value"/> as JSON to the specified <paramref name="stream"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the data to serialize.</typeparam>
        /// <param name="stream">A stream to serialize the specified <paramref name="value"/> as JSON to.</param>
        /// <param name="value">The value to serialize as JSON.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not writable.</exception>
        /// TODO: json exceptions
        public static void WriteJson<T>([HandlesResourceDisposal] Stream stream, T? value)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using (stream)
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                new JsonSerializer().Serialize(jsonWriter, value, typeof(T));
        }

        /// <summary>
        ///   <para>Enumerates through all files in the specified <paramref name="directoryPath"/> matching the specified <paramref name="pathWithoutExtension"/>.</para>
        /// </summary>
        /// <param name="directoryPath">A path to the directory to search in.</param>
        /// <param name="pathWithoutExtension">A file sub-path, without the extension, relative to the specified directory.</param>
        /// <returns>An enumerable collection of the full paths for the files in the specified directory matching the specified <paramref name="pathWithoutExtension"/>.</returns>
        /// TODO: path exceptions
        [Pure] public static IEnumerable<string> SearchFiles(string directoryPath, string pathWithoutExtension)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            if (pathWithoutExtension is null) throw new ArgumentNullException(nameof(pathWithoutExtension));
            if (!Directory.Exists(directoryPath)) yield break;

            string nameWithoutExtension = Path.GetFileName(pathWithoutExtension);

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, pathWithoutExtension + ".*"))
            {
                // filter out false positives, e.g. "123.45.txt" when searching for "123.*"
                ReadOnlySpan<char> name = Path.GetFileNameWithoutExtension(filePath.AsSpan());
                if (!name.SequenceEqual(nameWithoutExtension)) continue;

                yield return filePath;
            }
        }

    }
}
