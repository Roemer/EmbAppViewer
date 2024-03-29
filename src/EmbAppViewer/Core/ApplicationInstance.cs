﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Panel = System.Windows.Forms.Panel;
using Timer = System.Timers.Timer;

namespace EmbAppViewer.Core
{
    /// <summary>
    /// Represents a launched instance of an application.
    /// </summary>
    public class ApplicationInstance : IDisposable
    {
        /// <summary>
        /// The window handle of the applications main window.
        /// </summary>
        private IntPtr _windowHandle;
        /// <summary>
        /// Variable containing the original window position and size before embedding.
        /// </summary>
        private Win32.RECT _originalWindowRect;
        /// <summary>
        /// Variable containing the original window style to restore it on detach.
        /// </summary>
        private long _originalWindowStyle;
        /// <summary>
        /// Timer object to limit the number of resizes.
        /// </summary>
        private readonly Timer _resizeTimer;
        /// <summary>
        /// Hook for detecting application resizes.
        /// </summary>
        private IntPtr _hook;
        /// <summary>
        /// Callback method when the embedded app changes the size, needs to be a field so that GC does not collect it too early.
        /// </summary>
        private Win32.WinEventDelegate _callback;

        /// <summary>
        /// Create a new instance of the application.
        /// </summary>
        /// <param name="item">The <see cref="Item"/> on which this instance is based on.</param>
        public ApplicationInstance(Item item)
        {
            Item = item;

            _resizeTimer = new Timer
            {
                AutoReset = false,
                Interval = 500
            };
            _resizeTimer.Elapsed += ResizeTimer_Elapsed;

            CloseCommand = new RelayCommand(o =>
            {
                AppProcess?.CloseMainWindow();
                AppProcess?.Close();
                Dispose();
            });

            DetachCommand = new RelayCommand(o =>
            {
                // Unset the parent
                Win32.SetParent(_windowHandle, IntPtr.Zero);
                // Reset the style
                Win32.SetWindowLongPtr(_windowHandle, Win32.GWL_STYLE, new IntPtr(_originalWindowStyle));
                // Reset the position
                Win32.SetWindowPos(_windowHandle, IntPtr.Zero, _originalWindowRect.Left, _originalWindowRect.Top, _originalWindowRect.Width(), _originalWindowRect.Height(), Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                Dispose();
            });
        }

        /// <summary>
        /// The <see cref="Item"/> on which this instance is based on.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// The <see cref="TabItem"/> where this app is displayed on.
        /// </summary>
        public TabItem TabItem { get; set; }

        /// <summary>
        /// The associated process for the application.
        /// </summary>
        public Process AppProcess { get; set; }

        /// <summary>
        /// The WinForms <see cref="System.Windows.Forms.Panel"/> which is the container for the application.
        /// </summary>
        public Panel ContainerPanel { get; set; }

        /// <summary>
        /// The command which is executed when the tab is closed.
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// The command which is executed when the application should be detached.
        /// </summary>
        public ICommand DetachCommand { get; }

        /// <summary>
        /// The display name of the application.
        /// </summary>
        public string Name
        {
            get => Item.Name!;
            set => Item.Name = value;
        }

        public event Action<ApplicationInstance> Removed;

        public bool Start()
        {
            var psi = new ProcessStartInfo(Item.Path!, Item.Arguments!);
            if (String.IsNullOrWhiteSpace(Item.WorkDirectory))
            {
                psi.WorkingDirectory = Path.GetDirectoryName(Item.Path);
            }
            else
            {
                psi.WorkingDirectory = Item.WorkDirectory;
            }
            try
            {
                var process = Process.Start(psi);
                if (process == null)
                {
                    throw new Exception("Application process is null");
                }
                AppProcess = process;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting '{Item.Path}':{Environment.NewLine}{ex.Message}", "Error starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            _windowHandle = IntPtr.Zero;
            var successWaintingForWindow = SpinWait.SpinUntil(() =>
            {
                // Try the main process
                if (AppProcess.MainWindowHandle != IntPtr.Zero)
                {
                    _windowHandle = AppProcess.MainWindowHandle;
                    return true;
                }
                // Try the child processes
                foreach (var child in AppProcess.GetChildProcesses())
                {
                    if (child.MainWindowHandle != IntPtr.Zero)
                    {
                        _windowHandle = child.MainWindowHandle;
                        return true;
                    }
                }
                return false;
            }, Item.MaxLoadTime);
            if (!successWaintingForWindow)
            {
                MessageBox.Show($"Error waiting for MainWindow of '{Item.Path}'", "Error finding MainWindow", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public void StartAndEmbedd()
        {
            if (!Start())
            {
                return;
            }
            Embedd();
        }

        public void Embedd()
        {
            // Get the original size of the window
            Win32.GetWindowRect(_windowHandle, out _originalWindowRect);

            // Restyle the window (remove the control box)
            _originalWindowStyle = Win32.GetWindowLongPtr(_windowHandle, Win32.GWL_STYLE).ToInt64();
            var style = _originalWindowStyle & ~Win32.WS_CAPTION & ~Win32.WS_THICKFRAME & ~Win32.WS_POPUP & ~Win32.WS_SYSMENU & ~Win32.WS_DLGFRAME;
            Win32.SetWindowLongPtr(_windowHandle, Win32.GWL_STYLE, new IntPtr(style));

            // Set the parent of the window
            Win32.SetParent(_windowHandle, ContainerPanel.Handle);

            if (!Item.Resize)
            {
                // Move the original window into the panel
                Win32.SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, _originalWindowRect.Width(), _originalWindowRect.Height(), Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                // Resize the panel to match the original window size
                Win32.SetWindowPos(ContainerPanel.Handle, IntPtr.Zero, 0, 0, _originalWindowRect.Width(), _originalWindowRect.Height(), Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
            }
            else
            {
                // Resize the embedded application
                ResizeEmbeddedApp();

                // Set the hook to resize the embedded application if it changes it's size
                SetUpHook(_windowHandle);
            }
        }

        /// <summary>
        /// Queues a request to resize the application to the containers window size.
        /// </summary>
        public void QueueResize()
        {
            if (Item.Resize && !_resizeTimer.Enabled)
            {
                _resizeTimer.Start();
            }
        }

        private void ResizeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ResizeEmbeddedApp();
        }

        /// <summary>
        /// Resizes the embedded app to fill the entire container size.
        /// </summary>
        private void ResizeEmbeddedApp()
        {
            if (_windowHandle == IntPtr.Zero || !Item.Resize)
            {
                return;
            }
            Win32.SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, ContainerPanel.ClientSize.Width,
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
            if (hwnd != IntPtr.Zero && hwnd == _windowHandle)
            {
                if (eventType == Win32.EVENT_OBJECT_LOCATIONCHANGE)
                {
                    ResizeEmbeddedApp();
                }
            }
        }

        public void Dispose()
        {
            _resizeTimer.Stop();
            _resizeTimer.Elapsed -= ResizeTimer_Elapsed;
            _resizeTimer.Dispose();
            Win32.UnhookWinEvent(_hook);
            AppProcess?.Dispose();

            Removed?.Invoke(this);
        }

        public void InitFromHwnd(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            Win32.GetWindowThreadProcessId(windowHandle, out uint processId);
            var process = Process.GetProcessById((int)processId);
            AppProcess = process;
            Name = process.ProcessName;
        }
    }
}
