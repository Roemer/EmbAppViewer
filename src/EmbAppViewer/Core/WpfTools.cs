using System;
using System.Windows;
using System.Windows.Threading;

namespace EmbAppViewer.Core
{
    public static class WpfTools
    {
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
