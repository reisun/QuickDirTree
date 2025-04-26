using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[StructLayout(LayoutKind.Sequential)]
file struct SHFILEINFO
{
    public IntPtr hIcon;
    public int iIcon;
    public uint dwAttributes;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
}

file class NativeMethods
{
    public const uint SHGFI_ICON = 0x000000100;
    public const uint SHGFI_SMALLICON = 0x000000001;

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SHGetFileInfo(string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}

public class SystemIconManager
{
    public static Icon? GetIconFromPath(string folderPath)
    {
        SHFILEINFO shinfo = new SHFILEINFO();
        IntPtr hImg = NativeMethods.SHGetFileInfo(folderPath,
            0,
            ref shinfo,
            (uint)Marshal.SizeOf(shinfo),
            NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON);

        if (shinfo.hIcon != IntPtr.Zero)
        {
            Icon icon = (Icon.FromHandle(shinfo.hIcon).Clone() as Icon)!;
            NativeMethods.DestroyIcon(shinfo.hIcon); // リソースの解放
            return icon;
        }
        return null;
    }
}