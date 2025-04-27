using System.Configuration;
using System.Diagnostics;
using static QuickDirTree.Results;

namespace QuickDirTree;

public class LeftMenu
{
    public Form _dummyForm;
    public ContextMenuStrip _menu;

    private bool _InSuspend = false;
    public bool InSuspend
    {
        get => this._InSuspend;
        set
        {
            this._menu.AutoClose = !value;
            this._InSuspend = value;
        }
    }

    public event EventHandler<ContextMenuShowwingEventArgs> ContextMenuShowwing;
    public class ContextMenuShowwingEventArgs : EventArgs
    {
        public string Path { get; set; }
        public ContextMenuShowwingEventArgs(string path)
        {
            this.Path = path;
        }
    }

    public LeftMenu()
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;
        this._menu.Closing += (s, e) =>
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        };

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
        var dirListList = Settings.Get().TargetDirectries.Value!;
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
            UpdateSubMenuItems(this, this._menu.Items, dirListList.First());
        }
        else
        {   // N
            this._menu.Items.AddRange(dirListList.Select(v => CreateSubMenu(this, v)).ToArray());
        }

        // カーソルの横中心に表示
        showPonint.Offset(-this._menu.Width / 2, 0);

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

    public static void UpdateSubMenuItems(LeftMenu parent, ToolStripItemCollection collection, string parentMenuPath)
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
            collection.AddRange(pathList.Select(v => CreateSubMenu(parent, v)).ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            collection.Add(Texts.Get().DirectoryOpenFailed);
        }
    }

    private static ToolStripMenuItem CreateSubMenu(LeftMenu parent, string path)
    {
        var subMenuItem = new ToolStripMenuItem()
        {
            Text = Path.GetFileName(path),
            Image = SystemIconManager.GetIconFromPath(path)?.ToBitmap(),
            Tag = path,
        };
        if (Directory.Exists(path))
        {
            // 中身は実際に開くときに調べるが、ディレクトリなので”(空)”でItemを入れておく
            subMenuItem.DropDownItems.Add(Texts.Get().DirectoryEmpty);
        }
        subMenuItem.DropDownOpening += (s, e) =>
        {
            if (parent.InSuspend)
            {
                subMenuItem.DropDown.Close();
            }
        };
        subMenuItem.DropDownOpening += (s, e) => UpdateSubMenuItems(parent, subMenuItem.DropDownItems, path);
        subMenuItem.DropDown.Closing += (s, e) =>
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        };

        DateTime lastClickTime = DateTime.Now;
        int DoubleClickThreshold = 300; // ダブルクリックと見なす間隔（ミリ秒）
        subMenuItem.MouseUp += (s, e) =>
        {
            if (e.Button != MouseButtons.Left)
                return;

            DateTime now = DateTime.Now;
            if ((now - lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                // 開く前に念のためチェック
                if (!File.Exists(path) && !Directory.Exists(path))
                    return;
                try
                {
                    // OSに任せる
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true,
                        //Verb = "open"
                    });
                }
                catch (Exception ex)
                {
                    ; // ユーザーが起動をキャンセルした時に例外に入る場合がある
                }
            }
            lastClickTime = now;
        };
        subMenuItem.MouseUp += (s, e) =>
        {
            if (e.Button != MouseButtons.Right)
                return;

            // 開く前に念のためチェック
            if (!File.Exists(path) && !Directory.Exists(path))
                return;
            parent.ContextMenuShowwing(subMenuItem, new ContextMenuShowwingEventArgs(path));
        };

        return subMenuItem;
    }
}
