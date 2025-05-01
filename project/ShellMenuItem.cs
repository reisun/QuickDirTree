

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QuickDirTree;

public sealed class ShellMenuItem: IDisposable
{
    private List<ShellMenuItem> _items = new List<ShellMenuItem>();

    private ShellMenuItem()
    {
    }

    public IReadOnlyList<ShellMenuItem> Items => _items;

    public void Dispose()
    {
        if (OwnerMenu != IntPtr.Zero)
            DestroyMenu(OwnerMenu);
        if (FolderPidl != 0)
            Marshal.FreeCoTaskMem(FolderPidl);
        if (ItemPidl != 0)
            Marshal.FreeCoTaskMem(ItemPidl);
    }

    public IntPtr OwnerMenu;
    public nint FolderPidl;
    public nint ItemPidl;

    public ShellMenuItem? Parent { get; set; }

    public uint Id { get; private set; }
    public string Text { get; private set; }
    public string Verb { get; private set; }
    public MFS State { get; private set; }
    public MFT Type { get; private set; }
    public string path { get; private set; }
    private IContextMenu2 cm { get; set; }
    private nint hSubMenu { get; set; }

    public bool IsSeparator => Type.HasFlag(MFT.MFT_SEPARATOR);
    public bool HasUnderMenu => this.hSubMenu != IntPtr.Zero;

    public static ShellMenuItem ExtractMenu(string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (path == null)
            throw new ArgumentNullException(nameof(path));

        int hr = SHCreateItemFromParsingName(path, IntPtr.Zero, typeof(IShellItem).GUID, out var item);
        if (hr < 0)
            throw new Win32Exception(hr);

        var pai = (IParentAndItem)item;

        hr = pai.GetParentAndItem(out var folderPidl, out var folder, out var itemPidl);
        if (hr < 0)
            throw new Win32Exception(hr);

        hr = folder.GetUIObjectOf(IntPtr.Zero, 1, new[] { itemPidl }, typeof(IContextMenu).GUID, IntPtr.Zero, out var obj);
        if (hr < 0)
            throw new Win32Exception(hr);

        var list = new List<ShellMenuItem>();

        var shellItem = new ShellMenuItem();
        // --- Disposeで解放される
        shellItem.OwnerMenu = CreateMenu();
        shellItem.FolderPidl = folderPidl;
        shellItem.ItemPidl = itemPidl;
        // ---

        var cm = (IContextMenu2)obj;
        hr = cm.QueryContextMenu(shellItem.OwnerMenu, 0, 0, 0x7FFF, CMF.CMF_NORMAL);
        if (hr < 0)
            throw new Win32Exception(hr);

        ExtractMenu(path, cm, shellItem.OwnerMenu, shellItem);

        return shellItem;
    }

    private static void ExtractMenu(
        string path,
        IContextMenu2 cm,
        IntPtr menuHandle,
        ShellMenuItem parent)
    {
        if (menuHandle == IntPtr.Zero)
        {
            return;
        }

        int count = GetMenuItemCount(menuHandle);
        for (int i = 0; i < count; i++)
        {
            var mii = new MENUITEMINFO();
            mii.cbSize = Marshal.SizeOf(typeof(MENUITEMINFO));
            mii.fMask = MIIM.MIIM_FTYPE | MIIM.MIIM_ID | MIIM.MIIM_STATE | MIIM.MIIM_STRING | MIIM.MIIM_SUBMENU | MIIM.MIIM_DATA;
            if (!GetMenuItemInfo(menuHandle, i, true, ref mii))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (mii.fType == MFT.MFT_STRING)
            {
                mii.dwTypeData = new string('\0', (mii.cch + 1) * 2);
                mii.cch++;
                if (!GetMenuItemInfo(menuHandle, i, true, ref mii))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var item = new ShellMenuItem();
            item.Text = mii.dwTypeData;
            item.Id = mii.wID;
            item.Type = mii.fType;
            item.State = mii.fState;
            item.path = path;
            item.hSubMenu = mii.hSubMenu;
            item.cm = cm;

            parent._items.Add(item);
        }
    }
 
    public IReadOnlyList<ShellMenuItem> LoadUnderMenuList()
    {
        this._items.Clear();
        ExtractMenu(this.path, this.cm, this.hSubMenu, this);
        return this._items;
    }

    public void InvokeCommand()
    {
        var info = new CMINVOKECOMMANDINFOEX();
        info.cbSize = Marshal.SizeOf(info);
        info.hwnd = IntPtr.Zero;
        info.lpVerb = new IntPtr(this.Id);
        int hr = this.cm.InvokeCommand(ref info);
        if (hr < 0)
            throw new Win32Exception(hr);
    }

    private const int GCS_VERBA = 0x00000000;
    private const int GCS_VERBW = 0x00000004;
    private const int GCS_VERBICONW = 0x00000014;

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    private static extern int SHCreateItemFromParsingName(string path, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

    [DllImport("user32")]
    private static extern IntPtr CreateMenu();

    [DllImport("user32")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32")]
    private static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

    [DllImport("user32")]
    private static extern int GetMenuItemCount(IntPtr hMenu);

    [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem, bool fByPosition, ref MENUITEMINFO pmii);

    [Flags]
    private enum MIIM
    {
        MIIM_STATE = 0x00000001,
        MIIM_ID = 0x00000002,
        MIIM_SUBMENU = 0x00000004,
        MIIM_CHECKMARKS = 0x00000008,
        MIIM_TYPE = 0x00000010,
        MIIM_DATA = 0x00000020,
        MIIM_STRING = 0x00000040,
        MIIM_BITMAP = 0x00000080,
        MIIM_FTYPE = 0x00000100,
    }

    [Flags]
    private enum CMF
    {
        CMF_NORMAL = 0x00000000,
        CMF_DEFAULTONLY = 0x00000001,
        CMF_VERBSONLY = 0x00000002,
        CMF_EXPLORE = 0x00000004,
        CMF_NOVERBS = 0x00000008,
        CMF_CANRENAME = 0x00000010,
        CMF_NODEFAULT = 0x00000020,
        CMF_INCLUDESTATIC = 0x00000040,
        CMF_ITEMMENU = 0x00000080,
        CMF_EXTENDEDVERBS = 0x00000100,
        CMF_DISABLEDVERBS = 0x00000200,
        CMF_ASYNCVERBSTATE = 0x00000400,
        CMF_OPTIMIZEFORINVOKE = 0x00000800,
        CMF_SYNCCASCADEMENU = 0x00001000,
        CMF_DONOTPICKDEFAULT = 0x00002000,
        CMF_UNDOCUMENTED1 = 0x00004000,
        CMF_DVFILE = 0x10000,
        CMF_UNDOCUMENTED2 = 0x20000,
        CMF_RESERVED = unchecked((int)0xffff0000)
    }

    [Flags]
    public enum CMIC_MASK
    {
        CMIC_MASK_ASYNCOK = 0x00100000,
        CMIC_MASK_HOTKEY = 0x00000020,
        CMIC_MASK_FLAG_NO_UI = 0x00000400,
        CMIC_MASK_UNICODE = 0x00004000,
        CMIC_MASK_NO_CONSOLE = 0x00008000,
        CMIC_MASK_NOASYNC = 0x00000100,
        CMIC_MASK_SHIFT_DOWN = 0x10000000,
        CMIC_MASK_CONTROL_DOWN = 0x40000000,
        CMIC_MASK_FLAG_LOG_USAGE = 0x04000000,
        CMIC_MASK_NOZONECHECKS = 0x00800000,
        CMIC_MASK_PTINVOKE = 0x20000000,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MENUITEMINFO
    {
        public int cbSize;
        public MIIM fMask;
        public MFT fType;
        public MFS fState;
        public uint wID;
        public IntPtr hSubMenu;
        public IntPtr hbmpChecked;
        public IntPtr hbmpUnchecked;
        public IntPtr dwItemData;
        public string dwTypeData;
        public int cch;
        public IntPtr hbmpItem;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CMINVOKECOMMANDINFOEX
    {
        public int cbSize;
        public CMIC_MASK fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpParameters;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpDirectory;
        public int nShow;
        public int dwHotKey;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpTitle;
        public IntPtr lpVerbW;
        public string lpParametersW;
        public string lpDirectoryW;
        public string lpTitleW;
        public long ptInvoke;
    }

    [Guid("000214e4-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IContextMenu
    {
        // we don't need anything from this, all is in IContextMenu2
    }


    [Guid("000214f4-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IContextMenu2
    {
        // IContextMenu
        [PreserveSig]
        int QueryContextMenu(IntPtr hmenu, int indexMenu, int idCmdFirst, int idCmdLast, CMF uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);

        [PreserveSig]
        int GetCommandString(IntPtr idCmd, int uType, IntPtr pReserved, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMax);
        [PreserveSig]
        int GetCommandStringW(IntPtr idCmd, int uType, IntPtr pReserved, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMax);

        // IContextMenu2
        [PreserveSig]
        int HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);
    }

    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        // we don't need anything from this
    }

    [Guid("000214e6-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellFolder
    {
        void _VtblGap1_7(); // skip 7 methods we don't need

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, int cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr rgfReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    [Guid("b3a4b685-b685-4805-99d9-5dead2873236"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IParentAndItem
    {
        void _VtblGap1_1(); // skip 1 method we don't need

        [PreserveSig]
        int GetParentAndItem(out IntPtr ppidlParent, out IShellFolder ppsf, out IntPtr ppidlChild);
    }
}

[Flags]
public enum MFS
{
    MFS_GRAYED = 3,
    MFS_CHECKED = 8,
    MFS_HILITE = 128,
    MFS_ENABLED = 0,
    MFS_UNCHECKED = 0,
    MFS_UNHILITE = 0,
    MFS_DEFAULT = 4096,
}

[Flags]
public enum MFT
{
    MFT_STRING = 0,
    MFT_BITMAP = 4,
    MFT_MENUBARBREAK = 32,
    MFT_MENUBREAK = 64,
    MFT_RADIOCHECK = 512,
    MFT_SEPARATOR = 2048,
    MFT_RIGHTORDER = 8192,
    MFT_RIGHTJUSTIFY = 16384,
}