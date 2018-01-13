using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EmbAppViewer.Core
{
    /// <summary>
    /// Extension methods for Win32 structs.
    /// </summary>
    public static class Win32Extensions
    {
        public static int Width(this Win32.RECT rect)
        {
            return rect.Right - rect.Left;
        }

        public static int Height(this Win32.RECT rect)
        {
            return rect.Bottom - rect.Top;
        }
    }

    /// <summary>
    /// Various things from the Win32 API.
    /// </summary>
    public static class Win32
    {
        // Windows Styles
        public const int GWL_STYLE = -16;
        public const uint WS_TILED = 0x00000000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_SIZEBOX = 0x00040000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_TABSTOP = WS_MAXIMIZEBOX;
        public const uint WS_GROUP = WS_MINIMIZEBOX;
        public const uint WS_THICKFRAME = WS_SIZEBOX;
        public const uint WS_CHILDWINDOW = WS_CHILD;
        public const uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);
        public const uint WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public const uint WS_OVERLAPPED = WS_TILED;
        public const uint WS_OVERLAPPEDWINDOW = WS_TILEDWINDOW;

        //Extended Window Styles
        public const uint WS_EX_DLGMODALFRAME = 0x00000001;
        public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_ACCEPTFILES = 0x00000010;
        public const uint WS_EX_TRANSPARENT = 0x00000020;

        // SetWindowPosFlags
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        public const int SWP_NOSENDCHANGING = 0x0400;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;

        // ShowWindowTypes
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;

        // Get Window values
        public const uint GW_CHILD = 5;
        public const uint GW_ENABLEDPOPUP = 6;
        public const uint GW_HWNDFIRST = 0;
        public const uint GW_HWNDLAST = 1;
        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;
        public const uint GW_OWNER = 4;

        // Events
        public const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;
        public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;
        public const uint EVENT_OBJECT_STATECHANGE = 0x800A;
        public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        // Flags
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint WINEVENT_SKIPOWNTHREAD = 0x000;
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        public const uint WINEVENT_INCONTEXT = 0x0004;

        // Imports
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32")]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        // Delegates
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    }
}
