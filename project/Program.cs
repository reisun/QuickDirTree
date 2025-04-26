using System;
using System.Windows.Forms;

namespace QuickDirTree;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Texts.Initialize("lang.json");
        Settings.Initialize("appsettings.json");

        NotifyIcon trayIcon = new NotifyIcon();
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Visible = true;

        ContextMenuStrip leftMenu = new ContextMenuStrip();
        leftMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

        ContextMenuStrip rightMenu = new ContextMenuStrip();
        rightMenu.Items.Add("ディレクトリ変更", null, (s, e) => Dialog);
        rightMenu.Items.Add("終了", null, (s, e) => Application.Exit());

        trayIcon.ContextMenuStrip = leftMenu;
        trayIcon.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                // 左クリック → 左用メニューを手動表示
                leftMenu.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 右クリック → 右用メニューを手動表示
                rightMenu.Show(Cursor.Position);
            }
        };


        Application.Run();
    }
}