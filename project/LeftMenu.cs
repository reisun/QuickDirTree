namespace QuickDirTree;

using System.Configuration;
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
        string dir = Settings.Get().TargetDirectry.Value;
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
        Settings.Get().TargetDirectry.Value = dir;

        UpdateSubMenuItems(this._menu.Items, dir);

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

    public static void UpdateSubMenuItems(ToolStripItemCollection collection, string parentMenuPath)
    {
        collection.Clear();
        if (!Directory.Exists(parentMenuPath))
        {
            // フォルダでなければサブメニューは作れない
            return;
        }

        try
        {
            var pathList = Directory.GetFileSystemEntries(parentMenuPath);
            if (!pathList.Any())
            {
                collection.Add(Texts.Get().DirectoryEmpty);
                return;
            }
            collection.AddRange(pathList.Select(CreateSubMenu).ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            collection.Add(Texts.Get().DirectoryOpenFailed);
        }
    }

    private static ToolStripMenuItem CreateSubMenu(string path)
    {
        var subMenu = new ToolStripMenuItem()
        {
            Text = Path.GetFileName(path),
            Image = SystemIconManager.GetIconFromPath(path)?.ToBitmap()
        };
        if (Directory.Exists(path))
        {
            // 中身は実際に開くときに調べるが、ディレクトリなので”(空)”でItemを入れておく
            subMenu.DropDownItems.Add(Texts.Get().DirectoryEmpty);
        }
        subMenu.DropDownOpening += (s, e) => UpdateSubMenuItems(subMenu.DropDownItems, path);
        subMenu.MouseUp += (s, e) =>
        {
            if (e.Button != MouseButtons.Left)
                return;
            // 開く前に念のためチェック
            if (!File.Exists(path) && !Directory.Exists(path))
                return;
            // OSに任せる
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                //Verb = "open"
            });
        };
        return subMenu;
    }
}
