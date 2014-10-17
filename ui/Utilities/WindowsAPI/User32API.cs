
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/******************************************************************************************************************************************

	Copyright Nero AG. (Ltd.) 2009-2010


	Changes:
	--------
	__date__________name________remarks_________________________________________________
	01.SEP.2008                 Created
******************************************************************************************************************************************/
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text;

namespace Utilities.WindowsAPI
{
    /// <summary>
    /// User 32 api class
    /// </summary>
    public static class User32API
    {
        #region Constants
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_RESTORE = 0xF120;
        public const int WM_GETMINMAXINFO = 0x0024;

        public const int WM_MOVING = 0x0216;
        public const int WM_MOVE = 0x0003;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
		public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_NCACTIVATE = 0x0086;

        public const int GWL_STYLE = (-16);

        public const int GWL_EXSTYLE = (-20);

        public const int WS_SYSMENU = 0x00080000;

        //Start by Afzaal: Added to allow transparency on WebBrowser
        public const long WS_POPUP = 0x80000000L;
        public const long WS_CLIPCHILDREN = 0x02000000L;
        //End 

        public const int WM_NCHITTEST = 0x0084;
        public const int HTCAPTION = 2;

        // winerror.h   
        public const long ERROR_CANCELLED = 1223L;
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        // Max Min information
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            public RECT rcMonitor = new RECT();

            public RECT rcWork = new RECT();

            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            /// <summary>
            /// Initializes a new instance of the <see cref="RECT"/> struct.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="top">The top.</param>
            /// <param name="right">The right.</param>
            /// <param name="bottom">The bottom.</param>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }
        #endregion

        #region Functions
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("user32.dll")]
        public static extern void DisableProcessWindowsGhosting();

        [DllImport("user32.dll")]
        public static extern sbyte GetMessage(out MSG msg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern sbyte TranslateMessage(ref MSG msg);

        [DllImport("user32.dll")]
        public static extern sbyte DispatchMessage(ref MSG msg);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string classname, string title);

        #endregion
    }
}
