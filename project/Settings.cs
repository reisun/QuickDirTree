using Newtonsoft.Json;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace QuickDirTree;

public class SettingsModel
{
    [JsonProperty(Required = Required.Always)]
    public List<string> TargetDirectries { get; set; } = [];
}

public class Settings
{
    private readonly string _fileName;
    private readonly Action<string> _reportError;
    private readonly ReactiveProperty<IReadOnlyList<string>> _directories;
    public ReadOnlyReactiveProperty<IReadOnlyList<string>> TargetDirectries { get; }

    public Settings(string fileName, Action<string> reportError)
    {
        _fileName = Path.GetFullPath(fileName);
        _reportError = reportError;
        var model = JsonFile.ReadOrDefault<SettingsModel>(_fileName);
        if (model.TargetDirectries.Any(string.IsNullOrWhiteSpace))
            throw new JsonSerializationException("登録フォルダに空のパスが含まれています。");
        _directories = new ReactiveProperty<IReadOnlyList<string>>(model.TargetDirectries.AsReadOnly());
        TargetDirectries = _directories.ToReadOnlyReactiveProperty(_directories.Value);
    }

    // 保存成功後だけ画面に通知する。失敗時は直前の登録内容を維持する。
    public bool TrySetDirectories(IEnumerable<string> directories)
    {
        var next = directories.ToList();
        if (next.Any(string.IsNullOrWhiteSpace))
        {
            _reportError("登録フォルダに空のパスは指定できません。");
            return false;
        }
        if (_directories.Value.SequenceEqual(next))
            return true;
        try
        {
            JsonFile.Write(_fileName, new SettingsModel { TargetDirectries = next });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _reportError($"フォルダ設定を保存できませんでした。変更は反映されていません。\n{_fileName}\n{ex.Message}");
            return false;
        }
        _directories.Value = next.AsReadOnly();
        return true;
    }

    private static Settings g_instance = null!;
    public static void Initialize(string fileName, Action<string> reportError)
        => g_instance = new Settings(fileName, reportError);
    public static Settings Get() => g_instance;
}
