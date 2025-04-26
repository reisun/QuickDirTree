namespace QuickDirTree;

using Reactive.Bindings.Extensions;
using static Results;

public class RightMenu
{
    private Form _dummyForm;
    private ContextMenuStrip _menu;
    private ToolStripMenuItem _menuTargetDirectries;
    public RightMenu()
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;

        this._menuTargetDirectries = new ToolStripMenuItem()
        {
            Text = Path.GetFileName(Texts.Get().TargetDirectryList),
        };
        this._menu.Items.Add(this._menuTargetDirectries);
        this._menu.Items.Add(Texts.Get().Exit, null, (s, e) => Application.Exit());

        this._dummyForm = new Form();
        this._dummyForm.ShowInTaskbar = false; // タスクバーに出さない
        this._dummyForm.FormBorderStyle = FormBorderStyle.None; // 枠なし
        this._dummyForm.StartPosition = FormStartPosition.Manual;
        this._dummyForm.Size = new Size(0, 0); // サイズゼロ
        this._dummyForm.Location = new Point(-2000, -2000); // 画面外に飛ばす
        this._dummyForm.ContextMenuStrip = this._menu;

        RefreshTargetDirectriesMenu();
        Settings.Get().TargetDirectries.Subscribe(v => {
            RefreshTargetDirectriesMenu();
        });

    }

    public void Show(Point showPonint)
    {
        Hide();
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

    private ToolStripMenuItem RefreshTargetDirectriesMenu()
    {
        var subMenu = this._menuTargetDirectries;
        subMenu.DropDownItems.Clear();

        var dirList = Settings.Get().TargetDirectries.Value;
        foreach (var dir in dirList)
        {
            subMenu.DropDownItems.Add(dir, null, (s, e) =>
            {
                MyMsgBox.ShowConfirm(Texts.Get().ConfirmDeleteTargetDirectry).Bind(v => {
                    var newList = Settings.Get().TargetDirectries.Value.ToList();
                    newList.Remove(dir);
                    return Ok(Settings.Get().TargetDirectries.Value = newList);
                });
            });
        }
        subMenu.DropDownItems.Add(Texts.Get().AddTargetDirectry, null, (s, e) =>
        {
            Utils.SelectFolder("").Bind(v =>
            {
                var newList = Settings.Get().TargetDirectries.Value.ToList();
                newList.Add(v);
                return Ok(Settings.Get().TargetDirectries.Value = newList);
            });
        });
        return subMenu;
    }
}
