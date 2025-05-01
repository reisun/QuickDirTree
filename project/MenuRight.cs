namespace QuickDirTree;

public class MenuRight
{
    private ContextMenuStrip _menu;
    public MenuRight(Control parent)
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;

        var menuTargetDirectries = new ToolStripMenuItem();
        menuTargetDirectries.Text = Path.GetFileName(Texts.Get().TargetDirectryList);
        RefreshTargetDirectriesMenu(menuTargetDirectries);
        Settings.Get().TargetDirectries.Subscribe(v => RefreshTargetDirectriesMenu(menuTargetDirectries));

        this._menu.Items.Add(menuTargetDirectries);
        this._menu.Items.Add(Texts.Get().Exit, null, (s, e) => Application.Exit());

        parent.ContextMenuStrip = this._menu;
    }

    public void Show(Point showPoint)
    {
        this._menu.Show(showPoint);
    }
    public void Hide()
    {
        if (this._menu != null)
        {
            this._menu.Hide();
        }
    }

    private static ToolStripMenuItem RefreshTargetDirectriesMenu(ToolStripMenuItem subMenu)
    {
        subMenu.DropDownItems.Clear();

        var dirList = Settings.Get().TargetDirectries.Value;
        foreach (var dir in dirList)
        {
            subMenu.DropDownItems.Add(dir, null, (s, e) =>
            {
                MyMsgBox.ShowConfirm(Texts.Get().ConfirmDeleteTargetDirectry).ThenIfOk(v =>
                {
                    var newList = Settings.Get().TargetDirectries.Value.ToList();
                    newList.Remove(dir);
                    return Settings.Get().TargetDirectries.Value = newList;
                });
            });
        }
        subMenu.DropDownItems.Add(Texts.Get().AddTargetDirectry, null, (s, e) =>
        {
            Utils.SelectFolder("").ThenIfOk(v =>
            {
                var newList = Settings.Get().TargetDirectries.Value.ToList();
                newList.Add(v);
                return Settings.Get().TargetDirectries.Value = newList;
            });
        });
        return subMenu;
    }
}
