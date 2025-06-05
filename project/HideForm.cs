namespace QuickDirTree;

public class HideForm : Form
{

    public HideForm()
    {
        this.Load += (s, e) =>
        {
            this.ShowInTaskbar = false; // タスクバーに出さない
            this.FormBorderStyle = FormBorderStyle.None; // 枠なし
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(0, 0); // サイズゼロ
            this.Location = new Point(-2000, -2000); // 画面外に飛ばす
        };
    }
}