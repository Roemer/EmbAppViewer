﻿using System.Windows.Forms;

namespace EmbAppViewer.Core.Overlay
{
    public class OverlayRectangleForm : Form
    {
        public OverlayRectangleForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            Left = 0;
            Top = 0;
            Width = 1;
            Height = 1;
            Visible = false;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.ExStyle |= (int)Win32.WS_EX_TOPMOST;
                return createParams;
            }
        }
    }
}
