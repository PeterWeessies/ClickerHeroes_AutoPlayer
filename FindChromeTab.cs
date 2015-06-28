using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

//http://stackoverflow.com/questions/16958051/get-chrome-browser-title-using-c-sharp?answertab=votes#tab-top
//Found this code to return the title of Chrome browser windows
//Modified to find Clicker Heroes tab
//Clicker Heroes must be the active tab

namespace clickerheroes.autoplayer
{
    class FindChromeTab
    {
        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr windowHandle, StringBuilder stringBuilder, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hwnd);


        /// <summary>Find the windows matching the specified class name.</summary>

        public static IEnumerable<IntPtr> WindowsMatching(string className)
        {
            return new FindChromeTab(className)._result;
        }

        private FindChromeTab(string className)
        {
            _className = className;
            EnumWindows(callback, IntPtr.Zero);
        }

        private bool callback(IntPtr hWnd, IntPtr lparam)
        {
            if (GetClassName(hWnd, _apiResult, _apiResult.Capacity) != 0)
            {
                if (string.CompareOrdinal(_apiResult.ToString(), _className) == 0)
                {
                    _result.Add(hWnd);
                }
            }

            return true; // Keep enumerating.
        }

        public static IEnumerable<IntPtr> WindowsMatchingClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentOutOfRangeException("className", className, "className can't be null or blank.");

            return WindowsMatching(className);
        }

		//Return handle to Clicker Heroes Chrome Window
		public static IntPtr ChromeWindow()
        {
			foreach(var windowHandle in WindowsMatchingClassName("Chrome_WidgetWin_1"))
            {
                int length = GetWindowTextLength(windowHandle);
                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(windowHandle, sb, sb.Capacity);

				//If Title of text contains Clicke Heroes return handle
                if (sb.ToString().Contains("Clicker Heroes"))
					return windowHandle;
            }

			//No active Clicker Heroes window
            return IntPtr.Zero;
        }

        private readonly string _className;
        private readonly List<IntPtr> _result = new List<IntPtr>();
        private readonly StringBuilder _apiResult = new StringBuilder(1024);
    }
}