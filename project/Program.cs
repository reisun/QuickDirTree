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

        Texts.Initialize("lang.json");
        Settings.Initialize("appsettings.json");
        Application.ApplicationExit += (s, e) => {
            Texts.Get().Save();
            Settings.Get().Save();
        };

        var trayIcon = new NotifyIcon();
        var leftMenu = new LeftMenu();
        var rightMenu = new RightMenu();
        
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Visible = true;
        trayIcon.Text = Settings.Get().TargetDirectry.Value ?? "";
        Settings.Get().TargetDirectry.Subscribe(v => {
            trayIcon.Text = v ?? "";
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

        Application.Run();
    }
}