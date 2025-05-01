namespace QuickDirTree;

using System.Collections.Generic;
using Reactive.Bindings.Extensions;
using static Results;

public class MenuShell
{
    public ContextMenuStrip _menu;

    public MenuShell(Control parent)
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;

        parent.ContextMenuStrip = this._menu;
    }

    public void Show(string path, Point showPoint)
    {
        // 開く前に念のためチェック
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return;
        }

        var itemList = GetVerbListVer2(path);
        this._menu.Items.Clear();
        this._menu.Items.AddRange(itemList.ToArray());

        this._menu.Show(showPoint);
    }
    public void Hide()
    {
        this._menu.Hide();
    }

    public static List<ToolStripItem> GetVerbListVer2(string path)
    {
        var menu = ShellMenuItem.ExtractMenu(path);
        return menu.Items.Select(item => Dump(0, item, path)).ToList();
    }

    static ToolStripItem Dump(int indent, ShellMenuItem shellItem, string path)
    {
        if (shellItem.IsSeparator)
        {
            return new ToolStripSeparator();
        }
        var item = new ToolStripMenuItem()
        {
            Text = shellItem.Text,
        };
        if (shellItem.HasUnderMenu)
        {
            item.DropDown.Items.Add(Texts.Get().DirectoryEmpty);
        }
        item.DropDown.Opening += (s, e) =>
        {
            item.DropDown.Items.Clear();
            item.DropDown.Items.AddRange(
                shellItem.LoadUnderMenuList().Select(v => {
                    return Dump(indent + 1, v, path);
                }).ToArray()
            );
        };
        item.Click += (s, e) =>
        {
            try
            {
                shellItem.InvokeCommand();
            }
            catch (Exception ex)
            {
                MyMsgBox.ShowWarn(Texts.Get().WarnNotSupport);
            }
        };
        return item;
    }

}
