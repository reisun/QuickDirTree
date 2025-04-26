namespace QuickDirTree;
using static Results;

public class LeftMenu
{
    public LeftMenu()
    {
    }

    public void Show()
    {
        string dir = Settings.Get().TargetDirectry;
        if (string.IsNullOrEmpty(dir))
        {
            MessageBox.Show(Texts.Get().TargetDirectryEmpty);
            if (!Utils.SelectFolder("").Bind(v => Ok(dir = v)).IsOk){
                return;
            }
        }
        if (!Directory.Exists(dir))
        {
            MessageBox.Show(Texts.Get().TargetDirectryNotFound);
            if(!Utils.SelectFolder("").Bind(v => Ok(dir = v)).IsOk){
                return;
            }
        }

var dynamicParentMenu = new ToolStripMenuItem("フォルダ一覧");

        // DropDownOpening イベントで動的に子メニューを作る
        dynamicParentMenu.DropDownOpening += (s, e) =>
        {
            dynamicParentMenu.DropDownItems.Clear(); // 一旦クリアしてから作り直す

            string targetDir = @"C:\Temp"; // 適当にサンプルパス
            if (Directory.Exists(targetDir))
            {
                foreach (var dir in Directory.GetDirectories(targetDir))
                {
                    var folderName = Path.GetFileName(dir);
                    dynamicParentMenu.DropDownItems.Add(folderName, null, (sender, args) =>
                    {
                        MessageBox.Show($"{folderName} を選びました！");
                    });
                }
            }
            else
            {
                dynamicParentMenu.DropDownItems.Add("(フォルダが見つかりません)");
            }
        };
    }
}
