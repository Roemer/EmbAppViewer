using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using EmbAppViewer.Core;
using Panel = System.Windows.Forms.Panel;
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
            _mainViewModel.ApplicationItems.Add(new ApplicationItem("Notepad", "notepad.exe") { AllowMultiple = true });
            _mainViewModel.ApplicationItems.Add(new ApplicationItem("Calc", "calc.exe") { Resize = false });

            DataContext = _mainViewModel;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            // Get the item that is selected in the tree
            if (!(treeView?.SelectedItem is ApplicationItem applicationItem))
            {
                // No item was selected
                return;
            }
            if (!applicationItem.AllowMultiple)
            {
                // TODO: Search for an existing tab item and select that one if it exists, continue otherwise
            }

            // Create a new hosting-panel for the application to embedd
            Panel containerPanel;
            Panel mainPanel;
            if (applicationItem.Resize)
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
            var appInstance = new ApplicationInstance(applicationItem);
            // Start and embedd the application
            appInstance.ContainerPanel = containerPanel;

            // Create a new tab item with this panel
            var newTabItem = new TabItem
            {
                Header = appInstance,
                Content = winFormsHost
            };
            // Add the new tab item
            MyTab.Items.Add(newTabItem);
            // Select the new tab
            MyTab.SelectedIndex = MyTab.Items.Count - 1;

            // Start and embedd the app
            appInstance.StartAndEmbedd();
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
    }
}
