namespace QuickDirTree;

public partial class DummyForm : Form
{
    private MenuLeft _leftMenu;
    private MenuRight _rightMenu;
    private MenuShell _shellMenu;

    public DummyForm()
    {
        InitializeComponent();

        this.Load += (s, e) =>
        {
            this.ShowInTaskbar = false; // タスクバーに出さない
            this.FormBorderStyle = FormBorderStyle.None; // 枠なし
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(0, 0); // サイズゼロ
            this.Location = new Point(-2000, -2000); // 画面外に飛ばす

            this.label1.Visible = false;
            this.label2.Visible = false;
            this.label3.Visible = false;

            this._leftMenu = new MenuLeft(this.label1);
            this._rightMenu = new MenuRight(this.label2);
            this._shellMenu = new MenuShell(this.label3);

            var trayIcon = new NotifyIcon();
            trayIcon.Visible = true;
            trayIcon.Text = "QuickDirTree";
            trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
            Settings.Get().TargetDirectries.Subscribe(vlist =>
            {
                trayIcon.Icon = Utils.GetTrayIcon(Settings.Get().TargetDirectries.Value);
            });
            trayIcon.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this._leftMenu.Show(Cursor.Position);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    this._rightMenu.Show(Cursor.Position);
                }
            };
            this._leftMenu.ContextMenuShowwing += (s, e) => this._shellMenu.Show(e.Path, Cursor.Position);
            this._shellMenu._menu.Opening += (s, e) => this._leftMenu.InSuspend = true;
            this._shellMenu._menu.Closing += (s,e) => this._leftMenu.InSuspend = false;

            this.Deactivate += (s, e) =>
            {
                this._leftMenu.Hide();
                this._rightMenu.Hide();
                this._shellMenu.Hide();
            };
        };
    }

    private void _menu_Opened(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}