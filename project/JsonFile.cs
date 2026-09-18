using Newtonsoft.Json;
using System.Text;

namespace QuickDirTree;

public static class JsonFile
{
    public static T ReadOrDefault<T>(string path) where T : new()
    {
        string json;
        try { json = File.ReadAllText(path); }
        catch (FileNotFoundException) { return new T(); }
        return JsonConvert.DeserializeObject<T>(json)
            ?? throw new JsonSerializationException($"設定ファイルが空、または null です: {path}");
    }

    public static void Write<T>(string path, T value)
    {
        path = Path.GetFullPath(path);
        var json = JsonConvert.SerializeObject(value, Formatting.Indented);
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            // 同じディレクトリに書き込み、完全に書き終えてから置き換える。
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(path))
                File.Replace(temporaryPath, path, null);
            else
                File.Move(temporaryPath, path);
        }
        finally
        {
            // 清掃の失敗で本来の保存エラーを隠さない。
            try { File.Delete(temporaryPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
