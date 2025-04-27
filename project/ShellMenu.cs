namespace QuickDirTree;

using System.Collections.Generic;
using Reactive.Bindings.Extensions;
using static Results;

public class ShellMenu
{
    private interface IVerbs
    {
        string Name { get; }
        void DoIt();
    }

    public Form _dummyForm;
    public ContextMenuStrip _menu;
    public ShellMenu()
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

    public void Show(string path, Point showPonint)
    {
        Hide();

        var itemList = GetVerbListVer2(path);
        this._menu.Items.Clear();
        this._menu.Items.AddRange(itemList.ToArray());

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

    public static List<ToolStripMenuItem> GetVerbList(string path)
    {
        dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application")!)!;
        dynamic item = shell.NameSpace(0).ParseName(path);
        dynamic verbs = item.Verbs();
        var itemList = new List<ToolStripMenuItem>();
        var dic = new Dictionary<string, dynamic>();
        foreach (var verb in verbs)
        {
            if (!dic.ContainsKey(verb.Name))
            {
                dic.Add(verb.Name, verb);
                itemList.Add(new ToolStripMenuItem());
                itemList.Last().Name = verb.Name;
                itemList.Last().Text = verb.Name;
                itemList.Last().Click += (s, e) =>
                {
                    try
                    {
                        verb.DoIt();
                    }
                    catch (Exception ex)
                    {
                        MyMsgBox.ShowWarn(Texts.Get().WarnNotSupport);
                    }
                };
            }
        }
        return itemList;
    }

    public static List<ToolStripItem> GetVerbListVer2(string path)
    {
        var menuList = ShellMenuItem.ExtractMenu(path);
        return menuList.Select(item => Dump(0, item, path)).ToList();
    }

    static ToolStripItem Dump(int indent, ShellMenuItem item, string path)
    {
        if (item.IsSeparator)
        {
            return new ToolStripSeparator();
        }
        var childItem = new ToolStripMenuItem()
        {
            Text = item.Text,
        };
        childItem.Click += (s, e) =>
        {
            try
            {
                ShellMenuItem.InvokeMenuItem(path, (searchItem) => item.Id == searchItem.Id);
            }
            catch (Exception ex)
            {
                MyMsgBox.ShowWarn(Texts.Get().WarnNotSupport);
            }
        };
        foreach (var itr in item.Items.Select(child => Dump(indent + 1, child, path)))
        {
            childItem.DropDown.Items.Add(itr);
        }
        return childItem;
    }

}
