namespace QuickDirTree;
using static Results;

public static class Utils
{
    public static Result<string> SelectFolder(string dir)
    {
        var dialog = new FolderBrowserDialog();
        dialog.SelectedPath = dir;
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return Cancel<string>();
        }
        return Ok(dialog.SelectedPath);
    }
}