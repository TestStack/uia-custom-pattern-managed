using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;

namespace UiaControlsTest
{
    // System.Diagnostic.Process's CloseMainWindow and MainWindowHandle are very slow,
    // this is faster for the simple single-main-window case.
    internal class TargetApp : IDisposable
    {
        private Process _p;
        private readonly string _cmdLine;
        private readonly string _args;
        private IntPtr _hwnd;
        private bool _closed;
        private bool _ownProcess;

        private static bool _debug;

        public TargetApp(string cmdLine)
        {
            _cmdLine = cmdLine;
            _args = null;
            _closed = false;
        }

        public TargetApp(string cmdLine, string args)
        {
            _cmdLine = cmdLine;
            _args = args;
        }

        public void Start()
        {
            if (TryOpenExisting())
            {
                _ownProcess = false;
                WaitForWindow();
                return;
            }
            if (_debug)
            {
                Console.WriteLine("Debug mode:");
                Console.WriteLine("Start the following process manually, and enter the PID when done,");
                Console.WriteLine("or hit Enter to start the process automatically");
                Console.WriteLine("  " + _cmdLine + " " + _args);

                for (;;)
                {
                    var str = Console.ReadLine();

                    if (str == "")
                    {
                        _p = Process.Start(_cmdLine, _args);
                        Console.WriteLine("App started - pause to attach debugger - hit Enter to continue");
                        Console.ReadLine();
                        break;
                    }
                    try
                    {
                        var id = Int32.Parse(str);

                        _p = Process.GetProcessById(id);
                        _ownProcess = false;
                        break;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid int - try again");
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Bad Process ID - try again");
                    }
                }
            }
            else
            {
                _p = Process.Start(_cmdLine, _args);
                _ownProcess = true;
            }

            // _p.WaitForInputIdle(); - doesn't work for non-GUI apps - Instead, wait for the window to appear, then use this.

            WaitForWindow();
        }

        public bool TryOpenExisting()
        {
            var processName = Path.GetFileNameWithoutExtension(_cmdLine);
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();
            if (proc != null)
            {
                _p = proc;
                return true;
            }
            return false;
        }

        private void WaitForWindow()
        {
            _hwnd = IntPtr.Zero;

            var pid = _p.Id;

            _hwnd = WaitForWindow(pid, null, null);

            if (_hwnd == IntPtr.Zero)
            {
                throw new Exception("Couldn't start or find window of " + _cmdLine);
            }
        }

        public IntPtr MainWindow
        {
            get { return _hwnd; }
        }

        public int ProcessId
        {
            get { return _p.Id; }
        }

        public static bool Debug
        {
            get { return _debug; }

            set { _debug = value; }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hwnd, int dir);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int pid);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder str, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder str, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hwnd);


        private const int GW_HWNDNEXT = 2;
        private const int GW_CHILD = 5;
        private const int WM_CLOSE = 0x0010;

        public static IntPtr WaitForWindow(int pid, string className, string windowTitle)
        {
            // Try up to 80 times, 1/2 sec apart (40 secs max)
            // Extended this from 8 secs, since sometimes WCP can take ~35 secs to start
            // cold on a slow machine or on ia64.
            // Bump this again because ia64 machine is doing software acceleration so it takes a very 
            // long time to come up.   Try 240 times for ~2 minutes.
            for (var attempt = 0; attempt < 240; attempt++)
            {
                var hwnd = TryFindWindowNow(pid, className, windowTitle);
                if (hwnd != IntPtr.Zero)
                {
                    return hwnd;
                }
                Thread.Sleep(500);
            }
            return IntPtr.Zero;
        }

        private static IntPtr TryFindWindowNow(int pid, string className, string windowTitle)
        {
            var hwnd = GetDesktopWindow();
            var hwndChild = GetWindow(hwnd, GW_CHILD);
            for (; hwndChild != IntPtr.Zero; hwndChild = GetWindow(hwndChild, GW_HWNDNEXT))
            {
                // Check for PID, if specfied (do first because it's quick)...
                if (pid != 0)
                {
                    int id;
                    GetWindowThreadProcessId(hwndChild, out id);
                    if (id != pid)
                    {
                        continue;
                    }
                }

                // ignore invisible worker windows...
                if (! IsWindowVisible(hwndChild))
                {
                    continue;
                }

                // ignore console windows...
                var testClassName = new StringBuilder(64);
                GetClassName(hwndChild, testClassName, 64);
                if (String.Compare(testClassName.ToString(), "ConsoleWindowClass", true) == 0)
                {
                    // It's a console window - ignore it
                    continue;
                }

                // Check classname, if specified...
                if (className != null)
                {
                    if (String.Compare(className, testClassName.ToString(), true, CultureInfo.InvariantCulture) != 0)
                        continue;
                }

                // Check title, if specified...
                if (windowTitle != null)
                {
                    var testWindowTitle = new StringBuilder(64);
                    GetWindowText(hwndChild, testWindowTitle, 64);
                    if (String.Compare(windowTitle, testWindowTitle.ToString(), true, CultureInfo.InvariantCulture) != 0)
                        continue;
                }

                // Matches all criteria - return it!
                return hwndChild;
            }
            return IntPtr.Zero;
        }


        public void Close()
        {
            if (_closed || !_ownProcess)
                return;

            if (_debug)
            {
                Console.WriteLine("Debug mode: Closing app " + _cmdLine + " - hit Enter to continue");
                Console.ReadLine();
            }

            // Process.CloseMainWindow is too slow
            PostMessage(_hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

            if (! _p.WaitForExit(2000) && !_debug)
            {
                try
                {
                    _p.Kill();
                }
                catch (InvalidOperationException)
                {
                    // process may have already died in the meantime
                }
            }
            _closed = true;
        }

        public void Dispose()
        {
            Close();
        }
    }
}