using System.Configuration;
using System.Diagnostics;
using static QuickDirTree.Results;

namespace QuickDirTree;

public class MenuLeft
{
    public SuspendableContextMenuStrip _menu;

    public bool InSuspend
    {
        get => this._menu.InSuspend;
        set
        {
            this._menu.AutoClose = !value;
            this._menu.InSuspend = value;
            ProcessMenuItems(this._menu.Items, value);
        }
    }

    void ProcessMenuItems(ToolStripItemCollection items, bool inSuspend)
    {
        foreach (ToolStripItem item in items)
        {
            // サブメニューがある場合は再帰的に処理
            if (item is ToolStripMenuItem menuItem 
                && menuItem.HasDropDownItems 
                && menuItem.DropDown is SuspendableToolStripDropDownMenu suspendableToolStripDropDown)
            {
                suspendableToolStripDropDown.InSuspend = inSuspend;
                ProcessMenuItems(menuItem.DropDownItems, inSuspend);
            }
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

    public MenuLeft(Control parent)
    {
        this._menu = new SuspendableContextMenuStrip();
        this._menu.AutoClose = true;
        this._menu.Closing += (s, e) =>
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
            //if (this.InSuspend && e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange)
            //{
            //    e.Cancel = true;
            //}
        };

        parent.ContextMenuStrip = this._menu;
    }

    public void Show(Point showPonint)
    {
        Hide();
        var dirListList = Settings.Get().TargetDirectries.Value!;
        if (!dirListList.Any())
        {
            MyMsgBox.ShowWarn(Texts.Get().WarnTargetDirectryEmpty);
            if (!Utils.SelectFolder("").ThenIfOk(v => Ok(dirListList = [v])).IsOk)
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

        this._menu.Show(showPonint);
    }
    public void Hide()
    {
        this._menu.Hide();
    }

    public static void UpdateSubMenuItems(MenuLeft parent, ToolStripItemCollection collection, string parentMenuPath)
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

    private static ToolStripMenuItem CreateSubMenu(MenuLeft parent, string path)
    {
        var subMenuItem = new ToolStripMenuItem()
        {
            Text = Path.GetFileName(path),
            Image = SystemIconManager.GetIconFromPath(path)?.ToBitmap(),
            Tag = path,
            DropDown = new SuspendableToolStripDropDownMenu(),
        };
        if (Directory.Exists(path))
        {
            // 中身は実際に開くときに調べるが、ディレクトリなので”(空)”でItemを入れておく
            subMenuItem.DropDownItems.Add(Texts.Get().DirectoryEmpty);
        }
        //subMenuItem.DropDown.Opening += (s, e) =>
        //{
        //    if (parent.InSuspend)
        //    {
        //        e.Cancel = true;
        //    }
        //};
        subMenuItem.DropDown.Opening += (s, e) => {
            UpdateSubMenuItems(parent, subMenuItem.DropDown.Items, path);
        };
        subMenuItem.DropDown.Closing += (s, e) =>
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
            if (parent.InSuspend && e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange)
            {
                e.Cancel = true;
            }
        };
        // 左シングルクリック
        subMenuItem.MouseUp += (s, e) =>
        {
            if (e.Button != MouseButtons.Left)
                return;
            // ファイルのみ可能
            if (Directory.Exists(path) || !File.Exists(path))
                return;
            parent.Hide();
            FileExec(path);
        };
        // 左ダブルクリック
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
                FileExec(path);
            }
            lastClickTime = now;
        };
        // 右クリック
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

    public static void FileExec(string path)
    {
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
}
