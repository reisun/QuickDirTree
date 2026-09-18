using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuickDirTree;

class Program
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiFlag);

    [STAThread]
    static void Main()
    {
        // 高DPI対応設定
        SetProcessDpiAwarenessContext((IntPtr)(-4)); // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // 実験的（標準ダークテーマ対応）
        Application.SetColorMode(SystemColorMode.System);
        // ドロップダウンのホイールスクロールを可能にする
        DropDownMenuScrollWheelHandler.Enable(true);

        // スタートアップ起動時も、作業ディレクトリではなく実行ファイルの場所を基準にする。
        var languagePath = Path.Combine(AppContext.BaseDirectory, "lang.json");
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        void ReportError(string message) => MessageBox.Show(message, "QuickDirTree", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        try
        {
            Texts.Initialize(languagePath);
            _ = Texts.Get();
            Settings.Initialize(settingsPath, ReportError);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Newtonsoft.Json.JsonException)
        {
            ReportError($"設定ファイルを読み込めないため起動を中止します。元のファイルは変更していません。\n{languagePath}\n{settingsPath}\n{ex.Message}");
            return;
        }
        // 言語設定の雛形は初回だけ作成する。終了時に設定を書き戻さない。
        if (!File.Exists(languagePath))
        {
            try { Texts.Get().Save(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Newtonsoft.Json.JsonException)
            {
                ReportError($"言語設定ファイルを作成できませんでした。既定の表示で続行します。\n{languagePath}\n{ex.Message}");
            }
        }

        var hideForm = new HideForm();
        var trayIcon = new NotifyIcon();
        trayIcon.Visible = true;
        trayIcon.Text = "QuickDirTree";
        trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);

        var leftMenu = new FolderMenu();
        var rightMenu = new MenuRight(hideForm);
        var shellMenu = new ShellMenu(hideForm);
        // イベント
        Settings.Get().TargetDirectries.Subscribe(vlist =>
        {
            trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
        });
        trayIcon.MouseUp += (s, e) =>
        {
            hideForm.Show();
            hideForm.Activate();

            if (e.Button == MouseButtons.Left)
            {
                leftMenu.Show(hideForm, Cursor.Position);
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMenu.Show(Cursor.Position);
            }
        };

        leftMenu.ContextMenuShowwing += (s, e) =>
        {
            Debug.WriteLine($"Left menu showing: {e.Path}");
            shellMenu.Show(e.Path, Cursor.Position);
            leftMenu.SetAutoClose(true);
        };

        Application.Run();
    }
}
