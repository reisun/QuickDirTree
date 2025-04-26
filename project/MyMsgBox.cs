
namespace QuickDirTree;
using static Results;

public static class MyMsgBox
{
    public static DialogResult ShowWarn(string msg)
    {
        return MessageBox.Show(msg, Texts.Get().Confirm, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static Result<Unit> ShowConfirm(string msg)
    {
        var res = MessageBox.Show(msg, Texts.Get().Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (res == DialogResult.Yes) {
            return Ok(Unit.Value);
        }
        return Cancel<Unit>();
    }
}