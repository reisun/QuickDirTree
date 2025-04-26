using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuickDirTree;

public class BaseResourcesReader<T> where T : class, new()
{
    private static string g_fileName;
    private static Lazy<T> g_instance;
    public static void Initialize(string fileName)
    {
        g_fileName = fileName;
        g_instance = new Lazy<T>(() =>
        {
            var json = File.ReadAllText(g_fileName);
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        });
    }
    public static T Get() => g_instance.Value;

    public void Save()
    {
        string outputJson = JsonConvert.SerializeObject(g_instance, Formatting.Indented);
        File.WriteAllText(g_fileName, outputJson);
    }
}