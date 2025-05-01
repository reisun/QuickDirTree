using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;

namespace QuickDirTree;

public class SuspendableToolStripDropDownMenu : ToolStripDropDownMenu
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool InSuspend { get; set; } = false;

    protected override void WndProc(ref Message m)
    {
        if (InSuspend && !IsAllowMessage(m))
        {
            m.Result = IntPtr.Zero;
            return;
        }

        base.WndProc(ref m);
    }
    private enum ALLOW_MSGS: int
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_MOUSELEAVE = 0x02A3,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,

        WM_ACTIVATE = 0x0006,
        WM_ACTIVATEAPP = 0x001C,
        WM_NCACTIVATE = 0x0086,

        WM_NCHITTEST = 0x84,
        WM_SETCURSOR = 0x20,
    }
    private bool IsAllowMessage(Message msg)
    {
        bool allow = Enum.GetValues<ALLOW_MSGS>().Any(v => msg.Msg == (int)v);
        //Debug.WriteLine(allow + " " + msg.ToString());
        return allow;
    }
}