using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace EasySnippets.Utils
{
    public class MessageBoxCentered
    {
        private static IntPtr _owner;
        private static readonly HookProcDelegate HookProc;
        private static IntPtr _hHook;

        public static MessageBoxResult Show(string text)
        {
            Initialize();
            return MessageBox.Show(text);
        }

        public static MessageBoxResult Show(string text, string caption)
        {
            Initialize();
            return MessageBox.Show(text, caption);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defResult);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defResult, options);
        }

        public static MessageBoxResult Show(Window owner, string text)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defResult);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon,
                defResult, options);
        }

        public delegate IntPtr HookProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIdEvent, uint dwTime);

        public const int WhCallwndprocret = 12;

        public enum CbtHookAction
        {
            HcbtMovesize = 0,
            HcbtMinmax = 1,
            HcbtQs = 2,
            HcbtCreatewnd = 3,
            HcbtDestroywnd = 4,
            HcbtActivate = 5,
            HcbtClickskipped = 6,
            HcbtKeyskipped = 7,
            HcbtSyscommand = 8,
            HcbtSetfocus = 9
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("User32.dll")]
        public static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIdEvent, uint uElapse, TimerProc lpTimerFunc);

        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProcDelegate lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        public static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        public struct Cwpretstruct
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        };

        static MessageBoxCentered()
        {
            HookProc = MessageBoxHookProc;
            _hHook = IntPtr.Zero;
        }

        private static void Initialize()
        {
            if (_hHook != IntPtr.Zero)
            {
                throw new NotSupportedException("multiple calls are not supported");
            }

            _hHook = SetWindowsHookEx(WhCallwndprocret, HookProc, IntPtr.Zero, (int) GetCurrentThreadId());
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            Cwpretstruct msg = (Cwpretstruct)Marshal.PtrToStructure(lParam, typeof(Cwpretstruct));
            IntPtr hook = _hHook;

            if (msg.message != (int) CbtHookAction.HcbtActivate)
            {
                return CallNextHookEx(hook, nCode, wParam, lParam);
            }
            try
            {
                CenterWindow(msg.hwnd);
            }
            finally
            {
                UnhookWindowsHookEx(_hHook);
                _hHook = IntPtr.Zero;
            }

            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static void CenterWindow(IntPtr hChildWnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            GetWindowRect(hChildWnd, ref recChild);

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Rectangle recParent = new Rectangle(0, 0, 0, 0);
            GetWindowRect(_owner, ref recParent);

            System.Drawing.Point ptCenter =
                new System.Drawing.Point(0, 0)
                {
                    X = recParent.X + (recParent.Width - recParent.X) / 2,
                    Y = recParent.Y + (recParent.Height - recParent.Y) / 2
                };


            System.Drawing.Point ptStart = new System.Drawing.Point(0, 0)
            {
                X = ptCenter.X - width / 2,
                Y = ptCenter.Y - height / 2
            };

            System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;
            ptStart.X = allScreens.All(a => a.WorkingArea.Left > ptStart.X) ? allScreens.Min(a => a.WorkingArea.Left) : ptStart.X;
            ptStart.X = allScreens.All(a => a.WorkingArea.Right - width < ptStart.X) ? allScreens.Max(a => a.WorkingArea.Right) - width : ptStart.X;
            ptStart.Y = allScreens.All(a => a.WorkingArea.Top > ptStart.Y) ? allScreens.Min(a => a.WorkingArea.Top) : ptStart.Y;
            ptStart.Y = allScreens.All(a => a.WorkingArea.Bottom - height < ptStart.Y) ? allScreens.Max(a => a.WorkingArea.Bottom) - height : ptStart.Y;

            MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width,
                height, false);

        }
    }
}