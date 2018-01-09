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

            _mainViewModel = new MainViewModel();

            _mainViewModel.ApplicationItems.Add(new ApplicationItem("Notepad", "notepad.exe"));
            _mainViewModel.ApplicationItems.Add(new ApplicationItem("Calc", "calc.exe") { Resize = false });

            DataContext = _mainViewModel;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            // Get the item that is selected in the tree
            if (!(treeView?.SelectedItem is ApplicationItem selectedItem))
            {
                // No item was selected
                return;
            }
            // TODO: Search for an existing tab item and select that one

            // Create a new hosting-panel for the application to embedd

            Panel containerPanel;
            Panel mainPanel;
            if (selectedItem.Resize)
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

            // Create a new tab item with this panel
            var newTabItem = new TabItem
            {
                Header = selectedItem.Name,
                Content = winFormsHost,
                Tag = selectedItem
            };
            // Add the new tab item
            MyTab.Items.Add(newTabItem);
            // Select the new tab
            MyTab.SelectedIndex = MyTab.Items.Count - 1;
            // Start and embedd the application
            selectedItem.ContainerPanel = containerPanel;
            // Start and embedd the app
            selectedItem.StartAndEmbedd();
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
            if (selectedTabItem?.Tag is ApplicationItem appItem)
            {
                appItem.QueueResize();
            }
        }

        private async void MyTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(200);
            ResizeCurrentSelectedApp();
        }
    }
}
