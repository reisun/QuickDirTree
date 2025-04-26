using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

namespace QuickDirTree;
using static Results;

public static class Utils
{
    public static string GetAppVertion()
    {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return versionInfo?.FileVersion ?? "0.0.0.0";
    }

    public static Icon? GetTrayIcon(List<string> targetDirecties) {
        var iconPath = 
            !targetDirecties.Any() ? Path.GetTempPath() // 0ヶ
            : !targetDirecties.Skip(1).Any() ? targetDirecties.First() // 1ヶのみ
            : Path.GetTempPath();
        return SystemIconManager.GetIconFromPath(iconPath);
    }

    public static Result<string> SelectFolder(string prev)
    {
        var dialog = new FolderBrowserDialog();
        dialog.SelectedPath = prev;
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return Cancel<string>();
        }
        return Ok(dialog.SelectedPath);
    }

    public static Lazy<T> GetLazy<T>(string fileName) where T : class, new ()
    {
        return new Lazy<T>(() =>
        {
            T v = null!;
            try
            {
                var json = File.ReadAllText(fileName);
                v = JsonConvert.DeserializeObject<T>(json) ?? new T();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return v ?? new T();
        });
    }
}