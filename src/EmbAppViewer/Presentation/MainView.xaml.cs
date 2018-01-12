using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using EmbAppViewer.Core;
using EmbAppViewer.Core.Overlay;
using Image = System.Windows.Controls.Image;
using Panel = System.Windows.Forms.Panel;
using Size = System.Windows.Size;
using TreeView = System.Windows.Controls.TreeView;

namespace EmbAppViewer.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Clear the design-time items
            MyTab.Items.Clear();

            _mainViewModel = new MainViewModel();
            var localConfig = ConfigLoader.LoadConfig();
            _mainViewModel.Items.AddRange(localConfig.Items);

            DataContext = _mainViewModel;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            // Get the item that is selected in the tree
            if (!(treeView?.SelectedItem is Item item))
            {
                // No item was selected
                return;
            }
            if (item.IsFolder)
            {
                return;
            }
            if (!item.Multiple)
            {
                // TODO: Search for an existing tab item and select that one if it exists, continue otherwise
            }

            var appInstance = new ApplicationInstance(item);
            var startSuccessfull = appInstance.Start();
            if (!startSuccessfull)
            {
                return;
            }
            EmbeddAppInTab(appInstance);
        }

        private void EmbeddAppInTab(ApplicationInstance appInstance)
        {
            // Create a new hosting-panel for the application to embedd
            Panel containerPanel;
            Panel mainPanel;
            if (appInstance.Item.Resize)
            {
                // Default mode, just have one panel which resizes to the available space
                mainPanel = new Panel();
                containerPanel = mainPanel;
            }
            else
            {
                // Mode where the application has a fixed size.
                // Here we need two panels, where the outer one can scroll
                // and the inner one does not resize (will be the size of the app to embedd).
                var winFormsPanel = new Panel
                {
                    AutoScroll = true
                };
                var innerPanel = new Panel
                {
                    AutoSize = false
                };
                winFormsPanel.Controls.Add(innerPanel);
                mainPanel = winFormsPanel;
                containerPanel = innerPanel;
            }
            // Create the win forms hosting control and add the panel
            var winFormsHost = new WindowsFormsHost { Child = mainPanel };

            // Create the application instance
            appInstance.ContainerPanel = containerPanel;
            appInstance.Removed += AppInstance_Removed;

            // Create a new tab item with this panel
            var newTabItem = new TabItem
            {
                Header = appInstance,
                Content = winFormsHost
            };
            // Associate the tab item with the app instance
            appInstance.TabItem = newTabItem;
            // Add the new tab item
            MyTab.Items.Add(newTabItem);
            // Select the new tab
            MyTab.SelectedIndex = MyTab.Items.Count - 1;

            // Wait until the tab is correctly added
            WpfTools.DoEvents();

            // Start and embedd the app
            appInstance.Embedd();
        }

        private void AppInstance_Removed(ApplicationInstance obj)
        {
            obj.Removed -= AppInstance_Removed;
            // Fix the "binding error" when the last tab is removed and it tries to find the tab control which is then removed as well.
            // See: https://stackoverflow.com/questions/14419248/cannot-find-source-for-binding
            obj.TabItem.Template = null;
            // Effectively remove the item
            MyTab.Items.Remove(obj.TabItem);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            ResizeCurrentSelectedApp();
            return size;
        }

        private void ResizeCurrentSelectedApp()
        {
            var selectedTabItem = MyTab.SelectedItem as TabItem;
            if (selectedTabItem?.Header is ApplicationInstance appInstance)
            {
                appInstance.QueueResize();
            }
        }

        private async void MyTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(200);
            ResizeCurrentSelectedApp();
        }

        private void DragImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (e.ChangedButton == MouseButton.Left)
            {
                Mouse.OverrideCursor = Cursors.Cross;
                Mouse.Capture(image);
                e.Handled = true;
            }
        }

        private void DragImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Mouse.OverrideCursor = null;
                Mouse.Capture(null);

                _lastOverlay?.Dispose();

                if (_lastWindowHandle != IntPtr.Zero)
                {
                    var item = new Item();
                    item.Name = "Test";
                    var appInstance = new ApplicationInstance(item);
                    appInstance.InitFromHwnd(_lastWindowHandle);
                    EmbeddAppInTab(appInstance);

                    _lastWindowHandle = IntPtr.Zero;
                }

                e.Handled = true;
            }
        }

        private IntPtr _lastWindowHandle = IntPtr.Zero;
        private OverlayRectangle _lastOverlay = null;

        private void DragImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the control under the mouse
                var point = System.Windows.Forms.Cursor.Position;
                var window = Win32.WindowFromPoint(point);

                // Go down the parents
                IntPtr parent;
                while ((parent = Win32.GetParent(window)) != IntPtr.Zero)
                {
                    window = parent;
                }

                Win32.GetWindowRect(window, out var rect);
                var rectangle = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                if (_lastWindowHandle == IntPtr.Zero)
                {
                    _lastOverlay = new OverlayRectangle(rectangle);
                }
                else if (window != _lastWindowHandle)
                {
                    _lastOverlay.Dispose();
                    _lastOverlay = new OverlayRectangle(rectangle);
                }

                _lastWindowHandle = window;
            }
        }
    }
}
