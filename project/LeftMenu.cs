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
        var dirListList = Settings.Get().TargetDirectries.Value;
        if (!dirListList.Any())
        {
            MyMsgBox.ShowWarn(Texts.Get().WarnTargetDirectryEmpty);
            if (!Utils.SelectFolder("").Bind(v => Ok(dirListList = [v])).IsOk)
            {
                return;
            }
        }
        Settings.Get().TargetDirectries.Value = dirListList;

        this._menu.Items.Clear();
        if (!dirListList.Any())
        {   // 0ヶ
            this._menu.Items.Add(Texts.Get().DirectoryEmpty);
        }
        else if (!dirListList.Skip(1).Any())
        {   // 1ヶ
            UpdateSubMenuItems(this._menu.Items, dirListList.First());
        }
        else
        {   // N
            this._menu.Items.AddRange(dirListList.Select(CreateSubMenu).ToArray());
        }

        this._dummyForm.Show();
        this._dummyForm.Activate();

        // カーソルの横中心に表示
        showPonint.Offset(-this._menu.Width / 2, 0);

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
