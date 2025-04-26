namespace QuickDirTree;

using System.Diagnostics;
using static Results;

public class RightMenu
{
    private Form _dummyForm;
    private ContextMenuStrip _menu;
    public RightMenu()
    {
        this._menu = new ContextMenuStrip();
        this._menu.AutoClose = true;
        this._menu.Items.Add("ディレクトリ変更", null, (s, e) => {
            Utils.SelectFolder(Settings.Get().TargetDirectry)
            .Bind(v => Ok(Settings.Get().TargetDirectry = v));
        });
        this._menu.Items.Add("終了", null, (s, e) => Application.Exit());

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
}
