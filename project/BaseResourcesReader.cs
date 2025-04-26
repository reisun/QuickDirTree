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
            T v = null!;
            try
            {
                var json = File.ReadAllText(g_fileName);
                v = JsonConvert.DeserializeObject<T>(json) ?? new T();
            }
            catch (Exception ex){
                Console.WriteLine(ex.ToString());
            }
            return v ?? new T();
        });
    }
    public static T Get() => g_instance.Value;

    public void Save()
    {
        string outputJson = JsonConvert.SerializeObject(g_instance.Value, Formatting.Indented);
        File.WriteAllText(g_fileName, outputJson);
    }
}