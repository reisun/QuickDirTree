using System;
using System.Windows.Forms;

namespace QuickDirTree;
using static Results;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Texts.Initialize("lang.json");
        Settings.Initialize("appsettings.json");
        Application.ApplicationExit += (s, e) => {
            Texts.Get().Save();
            Settings.Get().Save();
        };

        var trayIcon = new NotifyIcon();
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Visible = true;

        var leftMenu = new LeftMenu();
        var rightMenu = new RightMenu();

        trayIcon.MouseUp += (s, e) =>
        {
            // leftMenu.Hide();
            // rightMenu.Hide();
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