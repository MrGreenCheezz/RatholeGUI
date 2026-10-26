using System.Text;
using System.Text.Json;

namespace PortsAppGui
{
    public class JsonDataClass
    {
        public ConfigStore Configs { get; set; } = new();
        public List<Service> Services { get; set; } = new();
    }

    public static class DataFile
    {
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        /// <summary>
        /// Returns null when the file does not exist yet. A file that is not valid JSON is moved
        /// aside to <paramref name="brokenCopyPath"/> and an empty configuration is returned, so a
        /// damaged data.json never stops the app from starting.
        /// </summary>
        public static JsonDataClass? Load(string path, out string? brokenCopyPath)
        {
            brokenCopyPath = null;
            if (!File.Exists(path))
                return null;

            try
            {
                var data = JsonSerializer.Deserialize<JsonDataClass>(File.ReadAllText(path))
                           ?? throw new JsonException("The file contains no configuration.");
                data.Services ??= new List<Service>();
                data.Configs ??= new ConfigStore();
                data.Services.RemoveAll(service => service == null);
                return data;
            }
            catch (JsonException)
            {
                brokenCopyPath = $"{path}.broken-{DateTime.Now:yyyyMMdd-HHmmss}";
                File.Move(path, brokenCopyPath, overwrite: true);
                return new JsonDataClass();
            }
        }

        public static void Save(string path, JsonDataClass data)
        {
            WriteAllTextAtomic(path, JsonSerializer.Serialize(data, WriteOptions));
        }

        /// <summary>
        /// Writes to a temporary file first and then swaps it in, so a crash or power loss in the
        /// middle of a write leaves the previous version instead of a truncated file.
        /// </summary>
        public static void WriteAllTextAtomic(string path, string contents)
        {
            var tempPath = path + ".tmp";
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(contents);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(tempPath, path, overwrite: true);
        }
    }
}
