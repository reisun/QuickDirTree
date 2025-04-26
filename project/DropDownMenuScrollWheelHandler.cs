public class DropDownMenuScrollWheelHandler : IMessageFilter
{
    private static DropDownMenuScrollWheelHandler Instance = null!;
    public static void Enable(bool enabled)
    {
        if (enabled)
        {
            if (Instance == null)
            {
                Instance = new DropDownMenuScrollWheelHandler();
                Application.AddMessageFilter(Instance);
            }
        }
        else
        {
            if (Instance != null)
            {
                Application.RemoveMessageFilter(Instance);
                Instance = null!;
            }
        }
    }

    private IntPtr activeHwnd;
    private ToolStripDropDown activeMenu = null!;
    private int wheelDeltaAccumulator = 0;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == 0x200 && activeHwnd != m.HWnd) // WM_MOUSEMOVE
        {
            activeHwnd = m.HWnd;
            this.activeMenu = (Control.FromHandle(m.HWnd) as ToolStripDropDown)!;
        }
        else if (m.Msg == 0x20A && this.activeMenu != null) // WM_MOUSEWHEEL
        {
            int delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);

            wheelDeltaAccumulator += delta;
            while (Math.Abs(wheelDeltaAccumulator) >= 120)
            {
                if (wheelDeltaAccumulator > 0)
                {
                    // 上方向へのスクロール処理
                    handleDelta(this.activeMenu, 120);
                    wheelDeltaAccumulator -= 120;
                }
                else
                {
                    handleDelta(this.activeMenu, -120);
                    wheelDeltaAccumulator += 120;
                }
            }
            return true;
        }
        return false;
    }

    private static readonly Action<ToolStrip, int> ScrollInternal
        = (Action<ToolStrip, int>)Delegate.CreateDelegate(typeof(Action<ToolStrip, int>),
            typeof(ToolStrip).GetMethod("ScrollInternal",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance)!);

    private void handleDelta(ToolStripDropDown ts, int delta)
    {
        if (ts.Items.Count == 0)
            return;
        var firstItem = ts.Items[0];
        var lastItem = ts.Items[ts.Items.Count - 1];
        const int TOP_BOTTOM_MARGIN = 28;
        if (delta > 0 && TOP_BOTTOM_MARGIN <= firstItem.Bounds.Top)
            return;
        if (delta < 0 && lastItem.Bounds.Bottom <= (ts.Height - TOP_BOTTOM_MARGIN))
            return;
        int height = (lastItem.Bounds.Top - firstItem.Bounds.Top) / (ts.Items.Count - 1);
        // delta = 120 が 標準の１ノック
        delta = height * (-delta / 120);
        if (delta != 0)
            ScrollInternal(ts, delta);
    }
}