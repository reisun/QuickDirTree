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

        Texts.Initialize("lang.json");
        Settings.Initialize("appsettings.json");
        Application.ApplicationExit += (s, e) =>
        {
            Texts.Get().Save();
            Settings.Get().Save();
        };

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