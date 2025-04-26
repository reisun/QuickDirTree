using Newtonsoft.Json;

namespace QuickDirTree;

public class Texts
{
    public string Wirnning { get; set; } = "警告";
    public string WarnTargetDirectryEmpty { get; set; } = "フォルダを設定してください。";
    public string WarnTargetDirectryNotFound { get; set; } = "フォルダが見つかりませんでした。";
    public string Confirm { get; set; } = "確認";
    public string ConfirmDeleteTargetDirectry { get; set; } = "登録フォルダから削除しますか？";
    public string DirectoryEmpty { get; set; } = "(空)";
    public string DirectoryOpenFailed { get; set; } = "(フォルダを開けませんでした)";
    public string TargetDirectryList { get; set; } = "登録フォルダ";
    public string AddTargetDirectry { get; set; } = "(追加)";
    public string Exit { get; set; } = "終了";


    private static string g_fileName = null!;
    private static Lazy<Texts> g_instance = null!;
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