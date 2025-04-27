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

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // 実験的（標準ダークテーマ対応）
        Application.SetColorMode(SystemColorMode.System);
        // ドロップダウンのホイールスクロールを可能にする
        DropDownMenuScrollWheelHandler.Enable(true);

        Texts.Initialize("lang.json");
        Settings.Initialize("appsettings.json");
        Application.ApplicationExit += (s, e) => {
            Texts.Get().Save();
            Settings.Get().Save();
        };

        var trayIcon = new NotifyIcon();
        var leftMenu = new LeftMenu();
        var rightMenu = new RightMenu();
        var shellMenu = new ShellMenu();

        trayIcon.Visible = true;
        trayIcon.Text = "QuickDirTree";
        trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
        Settings.Get().TargetDirectries.Subscribe(vlist => {
            trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
        });
        trayIcon.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMenu.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMenu.Show(Cursor.Position);
            }
        };
        leftMenu.ContextMenuShowwing += (s, e) => {
            leftMenu.InSuspend = true;
            shellMenu._menu.Closed += (s, e) => {
                leftMenu.InSuspend = false;
            };
            shellMenu._dummyForm = leftMenu._dummyForm;
            shellMenu.Show(e.Path, Cursor.Position);
        };

        Application.Run();
    }
}