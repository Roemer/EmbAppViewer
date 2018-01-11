using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using EmbAppViewer.Core;

namespace EmbAppViewer.Presentation
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            ExitCommand = new RelayCommand(o =>
            {
                Application.Current.Shutdown();
            });    
        }

        public List<Item> Items { get; } = new List<Item>();

        public ICommand ExitCommand { get; }
    }
}
