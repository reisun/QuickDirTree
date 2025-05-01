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

        Application.Run(new DummyForm());
    }
}