using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SoRR
{
    public static class FileUtility
    {
        [MustUseReturnValue] public static T? ReadJson<T>(string filePath)
            => ReadJson<T>(File.OpenRead(filePath));
        [MustUseReturnValue] public static T? ReadJson<T>([HandlesResourceDisposal] Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using (stream)
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
                return new JsonSerializer().Deserialize<T>(jsonReader);
        }

        public static void WriteJson<T>(string filePath, T? value)
            => WriteJson(File.Create(filePath), value);
        public static void WriteJson<T>([HandlesResourceDisposal] Stream stream, T? value)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using (stream)
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                new JsonSerializer().Serialize(jsonWriter, value, typeof(T));
        }

    }
}
