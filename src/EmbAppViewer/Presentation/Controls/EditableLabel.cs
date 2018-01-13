using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EmbAppViewer.Presentation.Controls
{
    public class EditableLabel : TextBox
    {
        private string _previousValue;

        public EditableLabel()
        {
            DisableEditing();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            _previousValue = Text;
            EnableEditing();
            Focus();
            SelectAll();
            e.Handled = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            StopEditing();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Escape)
            {
                if (e.Key == Key.Escape)
                {
                    Text = _previousValue;
                }
                StopEditing();
                e.Handled = true;
                return;
            }
            base.OnKeyUp(e);
        }

        private void StopEditing()
        {
            DisableEditing();
            Keyboard.ClearFocus();
        }

        private void DisableEditing()
        {
            Focusable = false;
            BorderThickness = new Thickness(0);
            Background = Brushes.Transparent;
            CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            SelectionLength = 0;
        }

        private void EnableEditing()
        {
            Focusable = true;
            BorderThickness = new Thickness(1);
            Background = Brushes.White;
            CaretBrush = null;
        }
    }
}
