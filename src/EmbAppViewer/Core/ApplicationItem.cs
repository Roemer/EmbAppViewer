using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace EmbAppViewer.Core
{
    public class ApplicationItem : IDisposable
    {
        /// <summary>
        /// Callback method when the embedded app changes the size, needs to be a field so that GC does not collect it too early.
        /// </summary>
        private Win32.WinEventDelegate _callback;
        /// <summary>
        /// Hook for detecting application resizes.
        /// </summary>
        private IntPtr _hook;
        /// <summary>
        /// Timer object to limit the number of resizes.
        /// </summary>
        private readonly Timer _resizeTimer;

        public ApplicationItem(string name, string executablePath)
        {
            Name = name;
            ExecutablePath = executablePath;
            _resizeTimer = new Timer();
            _resizeTimer.AutoReset = false;
            _resizeTimer.Interval = 500;
            _resizeTimer.Elapsed += ResizeTimer_Elapsed;
        }

        /// <summary>
        /// The display name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path of the executable of the application.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The command line arguments used when starting the application.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Flag to indicate if the embedded application should resize to the containers size or not.
        /// </summary>
        public bool Resize { get; set; } = true;

        public Panel ContainerPanel { get; set; }

        public Process Process { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void StartAndEmbedd()
        {
            // Start the VNC viewer
            var psi = new ProcessStartInfo(ExecutablePath, Arguments);
            try
            {
                Process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting '{ExecutablePath}':{Environment.NewLine}{ex.Message}", "Error starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            while (Process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(10);
            }
            Process.WaitForInputIdle();

            // Restyle the window (remove the control box)
            var style = Win32.GetWindowLong(Process.MainWindowHandle, Win32.GWL_STYLE);
            style = style & ~Win32.WS_CAPTION & ~Win32.WS_THICKFRAME;
            Win32.SetWindowLong(Process.MainWindowHandle, Win32.GWL_STYLE, style);

            // Set the parent of the window
            Win32.SetParent(Process.MainWindowHandle, ContainerPanel.Handle);

            if (!Resize)
            {
                // Calculate the original size of the window
                Win32.GetWindowRect(new HandleRef(this, Process.MainWindowHandle), out var rct);
                // Move the original window into the panel
                Win32.SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, 0, 0, rct.Right - rct.Left, rct.Bottom - rct.Top, Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                // Resize the panel to match the original window size
                Win32.SetWindowPos(ContainerPanel.Handle, IntPtr.Zero, 0, 0, rct.Right - rct.Left, rct.Bottom - rct.Top, Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
            }
            else
            {
                // Resize the embedded application
                ResizeEmbeddedApp();

                // Set the hook to resize the embedded application if it changes it's size
                SetUpHook(Process.MainWindowHandle);
            }
        }

        public void QueueResize()
        {
            if (Resize && !_resizeTimer.Enabled)
            {
                _resizeTimer.Start();
            }
        }

        private void ResizeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ResizeEmbeddedApp();
        }

        private void ResizeEmbeddedApp()
        {
            if (Process == null || !Resize)
            {
                return;
            }
            Win32.SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, 0, 0, ContainerPanel.ClientSize.Width,
                ContainerPanel.ClientSize.Height, Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
        }

        private void SetUpHook(IntPtr target)
        {
            _callback = WinEventHookCallback;
            var threadId = Win32.GetWindowThreadProcessId(target, out var processId);
            var flags = Win32.WINEVENT_OUTOFCONTEXT;

            // TODO: Currently does not work with the specific process.
            processId = 0;
            threadId = 0;

            _hook = Win32.SetWinEventHook(Win32.EVENT_OBJECT_LOCATIONCHANGE, Win32.EVENT_OBJECT_LOCATIONCHANGE, target, _callback, processId, threadId, flags);
        }

        private void WinEventHookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd != IntPtr.Zero && hwnd == Process.MainWindowHandle)
            {
                if (eventType == Win32.EVENT_OBJECT_LOCATIONCHANGE)
                {
                    ResizeEmbeddedApp();
                }
            }
        }

        public void Dispose()
        {
            Win32.UnhookWinEvent(_hook);
            Process?.Dispose();
        }
    }
}
