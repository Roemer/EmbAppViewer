using System.Collections.Generic;
using EmbAppViewer.Core;

namespace EmbAppViewer.Presentation
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            ApplicationItems = new List<ApplicationItem>();
        }

        public List<ApplicationItem> ApplicationItems { get; }
    }
}
