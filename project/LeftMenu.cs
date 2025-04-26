namespace QuickDirTree;

using System.Diagnostics;
using static Results;

public class LeftMenu
{
    private Form _dummyForm;
    private ContextMenuStrip _menu;
    public LeftMenu()
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;

        this._dummyForm = new Form();
        this._dummyForm.ShowInTaskbar = false; // タスクバーに出さない
        this._dummyForm.FormBorderStyle = FormBorderStyle.None; // 枠なし
        this._dummyForm.StartPosition = FormStartPosition.Manual;
        this._dummyForm.Size = new Size(0, 0); // サイズゼロ
        this._dummyForm.Location = new Point(-2000, -2000); // 画面外に飛ばす
        this._dummyForm.ContextMenuStrip = this._menu;
    }

    public void Show(Point showPonint)
    {
        Hide();
        string dir = Settings.Get().TargetDirectry;
        if (string.IsNullOrEmpty(dir))
        {
            MessageBox.Show(Texts.Get().TargetDirectryEmpty);
            if (!Utils.SelectFolder(dir).Bind(v => Ok(dir = v)).IsOk)
            {
                return;
            }
        }
        if (!Directory.Exists(dir))
        {
            MessageBox.Show(Texts.Get().TargetDirectryNotFound);
            if (!Utils.SelectFolder(dir).Bind(v => Ok(dir = v)).IsOk)
            {
                return;
            }
        }
        Settings.Get().TargetDirectry = dir;

        SetSubMenu(this._menu.Items, dir);

        this._dummyForm.Show();
        this._dummyForm.Activate();

        this._menu.Show(showPonint);
    }
    public void Hide()
    {
        if (this._menu != null)
        {
            this._menu.Hide();
        }
    }

    public static void SetSubMenu(ToolStripItemCollection collection, string parentMenuPath)
    {
        collection.Clear();
        if (Directory.Exists(parentMenuPath))
        {
            foreach (var dir in Directory.GetDirectories(parentMenuPath))
            {
                var folderName = Path.GetFileName(dir);
                var subMenu = new ToolStripMenuItem()
                {
                    Name = folderName,
                    Text = folderName,
                };
                subMenu.DropDownOpening += (s, e) => SetSubMenu(subMenu.DropDownItems, dir);
                collection.Add(subMenu);
            }
            return;
        }
        if (File.Exists(parentMenuPath))
        {
            // OSに任せて開く
            Process.Start(new ProcessStartInfo
            {
                FileName = parentMenuPath,
                UseShellExecute = true
            });
        }
    }
}
