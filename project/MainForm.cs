namespace QuickDirTree;


public class MainForm : HideForm
{
    private FolderMenu _leftMenu;
    private MenuRight _rightMenu;
    private ShellMenu _shellMenu;

    public MainForm() : base()
    {

        // var trayIcon = new NotifyIcon();
        // trayIcon.Visible = true;
        // trayIcon.Text = "QuickDirTree";
        // trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);

        // this._leftMenu = new FolderMenu(this);
        // this._rightMenu = new MenuRight(this);
        // this._shellMenu = new MenuShell(this);

        // this.Load += (s, e) =>
        // {
        //     // イベント
        //     Settings.Get().TargetDirectries.Subscribe(vlist =>
        //     {
        //         trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
        //     });
        //     trayIcon.MouseUp += (s, e) =>
        //     {
        //         if (e.Button == MouseButtons.Left)
        //         {
        //             this._leftMenu.Show(Cursor.Position);
        //         }
        //         else if (e.Button == MouseButtons.Right)
        //         {
        //             this._rightMenu.Show(Cursor.Position);
        //         }
        //     };

        //     // this._leftMenu.ContextMenuShowwing += (s, e) => this._shellMenu.Show(e.Path, Cursor.Position);
        //     // this._shellMenu._menu.Opening += (s, e) => this._leftMenu.InSuspend = true;
        //     // this._shellMenu._menu.Closing += (s,e) => this._leftMenu.InSuspend = false;

        //     this.Deactivate += (s, e) =>
        //     {
        //         this._leftMenu.Hide();
        //         this._rightMenu.Hide();
        //         this._shellMenu.Hide();
        //     };
        // };
    }
}
