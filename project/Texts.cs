using Newtonsoft.Json;

namespace QuickDirTree;

public class Texts
{
    public string TargetDirectryEmpty { get; set; } = "フォルダを設定してください。";
    public string TargetDirectryNotFound { get; set; }= "フォルダが見つかりませんでした。";
    public string DirectoryEmpty { get; set; }= "(空)";
    public string DirectoryOpenFailed { get; set; }= "(フォルダを開けませんでした)";
    public string ChangeDirectry { get; set; }= "フォルダ変更";
    public string Exit { get; set; }= "終了";


    private static string g_fileName;
    private static Lazy<Texts> g_instance;
    public static void Initialize(string fileName)
    {
        g_fileName = fileName;
        g_instance = Utils.GetLazy<Texts>(fileName);
    }
    public static Texts Get() => g_instance.Value;
    public void Save()
    {
        string outputJson = JsonConvert.SerializeObject(g_instance.Value, Formatting.Indented);
        File.WriteAllText(g_fileName, outputJson);
    }
}