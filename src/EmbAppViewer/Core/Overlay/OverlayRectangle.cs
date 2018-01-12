using System;
using System.Collections.Generic;
using System.Drawing;

namespace EmbAppViewer.Core.Overlay
{
    public class OverlayRectangle : IDisposable
    {
        private readonly List<OverlayRectangleForm> _borderForms = new List<OverlayRectangleForm>();

        public OverlayRectangle(Rectangle rectangle)
        {
            const int margin = 0;
            const int size = 3;

            var leftBorder = new Rectangle(rectangle.Left - margin, rectangle.Y - margin, size, rectangle.Height + 2 * margin);
            var topBorder = new Rectangle(rectangle.Left - margin, rectangle.Y - margin, rectangle.Width + 2 * margin, size);
            var rightBorder = new Rectangle(rectangle.Left + rectangle.Width - size + margin, rectangle.Y - margin, size, rectangle.Height + 2 * margin);
            var bottomBorder = new Rectangle(rectangle.Left - margin, rectangle.Y + rectangle.Height - size + margin, rectangle.Width + 2 * margin, size);
            var allBorders = new[] { leftBorder, topBorder, rightBorder, bottomBorder };

            foreach (var border in allBorders)
            {
                var form = new OverlayRectangleForm { BackColor = Color.Red };
                _borderForms.Add(form);
                // Position the window
                Win32.SetWindowPos(form.Handle, new IntPtr(-1), border.X, border.Y,
                    border.Width, border.Height, Win32.SWP_NOACTIVATE);
                // Show the window
                Win32.ShowWindow(form.Handle, Win32.SW_SHOWNA);
            }
        }

        public void Dispose()
        {
            foreach (var form in _borderForms)
            {
                // Cleanup
                form.Hide();
                form.Close();
                form.Dispose();
            }
            _borderForms.Clear();
        }
    }
}
