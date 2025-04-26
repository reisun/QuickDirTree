using Newtonsoft.Json;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace QuickDirTree;

public class SettingsModel
{
    public List<string> TargetDirectries { get; set; } = [];
}

public class Settings
{
    private readonly SettingsModel _model;
    public ReactiveProperty<List<string>> TargetDirectries { get; }

    private Settings(SettingsModel model) {
        this._model = model;
        this.TargetDirectries = new ReactiveProperty<List<string>>(this._model.TargetDirectries);
        this.TargetDirectries.Subscribe(v => model.TargetDirectries = v);
    }

    private static string g_fileName = null!;
    private static Lazy<Settings> g_instance = null!;
    public static void Initialize(string fileName)
    {
        g_fileName = fileName;
        g_instance = new Lazy<Settings>(() => {
            return new Settings(Utils.GetLazy<SettingsModel>(fileName).Value);
        });
    }
    public static Settings Get() => g_instance.Value;
    public void Save()
    {
        string outputJson = JsonConvert.SerializeObject(g_instance.Value._model, Formatting.Indented);
        File.WriteAllText(g_fileName, outputJson);
    }
}