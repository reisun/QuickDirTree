using System.IO;
using System.Windows.Forms;
using static QuickDirTree.Results;

namespace QuickDirTree;

public class FolderMenu
{
    private ToolStripDropDownMenu _rootMenu;

    public event EventHandler<ContextMenuShowwingEventArgs> ContextMenuShowwing;

    public void SetAutoClose(bool enable)
    {
        if (_rootMenu != null)
            SetAutoCloseRecursive(_rootMenu, enable);
    }

    private void SetAutoCloseRecursive(ToolStripDropDown menu, bool enable)
    {
        menu.AutoClose = enable;
        foreach (ToolStripItem item in menu.Items)
        {
            if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
            {
                SetAutoCloseRecursive(mi.DropDown, enable);
            }
        }
    }

    public void Show(Control parent, Point showPoint)
    {
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

        // 既存メニューがあれば閉じる
        _rootMenu?.Close();
        _rootMenu?.Dispose();
        _rootMenu = new ToolStripDropDownMenu();
        _rootMenu.AutoClose = true;
        _rootMenu.Closed += (s, e) => { _rootMenu = null; };

        if (dirListList.Count == 1)
        {
            // 1件ならそのフォルダの内容を表示
            AddFolderItems(_rootMenu, dirListList[0]);
        }
        else
        {
            // 複数なら各登録フォルダをルートとして表示
            foreach (var dir in dirListList)
            {
                var item = CreateFolderMenuItem(dir, dir);
                _rootMenu.Items.Add(item);
            }
        }

        // showPoint（スクリーン座標）→親コントロールのクライアント座標へ変換して表示
        var clientPoint = parent.PointToClient(showPoint);
        _rootMenu.Show(parent, clientPoint);
    }
    public void Hide()
    {
        if (_rootMenu != null)
        {
            _rootMenu.Hide();
        }
    }

    private void AttachContextMenuMouseUp(ToolStripMenuItem item, string path)
    {
        item.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var owner = item.Owner as ToolStripDropDown;
                // このアイテムが属するDropDownが表示中、かつマウス座標がこのアイテム上にある場合のみ発火
                if (owner != null && owner.Visible && item.Bounds.Contains(owner.PointToClient(Cursor.Position)))
                {
                    if (_rootMenu != null)
                        SetAutoCloseRecursive(_rootMenu, false);
                    ContextMenuShowwing?.Invoke(this, new ContextMenuShowwingEventArgs(path));
                }
            }
        };
    }

    // 指定フォルダの内容をメニューに追加
    private void AddFolderItems(ToolStripDropDown menu, string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            var errItem = new ToolStripMenuItem("(存在しません)") { Enabled = false };
            menu.Items.Add(errItem);
            return;
        }

        var dirs = Directory.GetDirectories(folderPath);
        var files = Directory.GetFiles(folderPath);

        if (dirs.Length == 0 && files.Length == 0)
        {
            var emptyItem = new ToolStripMenuItem("(空)") { Enabled = false };
            menu.Items.Add(emptyItem);
            return;
        }

        foreach (var dir in dirs)
        {
            var item = CreateFolderMenuItem(Path.GetFileName(dir), dir);
            AttachContextMenuMouseUp(item, dir);
            menu.Items.Add(item);
        }
        foreach (var file in files)
        {
            var item = CreateFileMenuItem(Path.GetFileName(file), file);
            AttachContextMenuMouseUp(item, file);
            menu.Items.Add(item);
        }
    }

    // フォルダ用メニューアイテム生成（サブメニューで再帰）
    private ToolStripMenuItem CreateFolderMenuItem(string displayName, string folderPath)
    {
        var item = new ToolStripMenuItem(displayName);
        item.Tag = folderPath;
        item.Image = Utils.GetTrayIcon([folderPath])?.ToBitmap();
        item.DropDownItems.Add(new ToolStripMenuItem("(空)") { Enabled = false });
        item.DropDownOpening += (s, e) =>
        {
            var mi = (ToolStripMenuItem)s;
            mi.DropDownItems.Clear();
            AddFolderItems(mi.DropDown, folderPath);
        };
        return item;
    }

    // ファイル用メニューアイテム生成
    private ToolStripMenuItem CreateFileMenuItem(string displayName, string filePath)
    {
        var item = new ToolStripMenuItem(displayName);
        item.Tag = filePath;
        item.Image = Utils.GetTrayIcon([filePath])?.ToBitmap();
        item.Click += (s, e) =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch
            {
                MyMsgBox.ShowWarn("ファイルを開けませんでした: " + filePath);
            }
        };
        return item;
    }
}

// 右クリックで発火するイベント用
public class ContextMenuShowwingEventArgs : EventArgs
{
    public string Path { get; }
    public ContextMenuShowwingEventArgs(string path)
    {
        Path = path;
    }
}