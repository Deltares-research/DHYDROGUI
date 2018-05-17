using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public class PopupNotOnTop : Popup
    {
        private enum SWPFlags
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOACTIVATE = 0x0010
        }

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy,
            int wFlags);

        protected override void OnOpened(EventArgs e)
        {
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            SetWindowPos(hwnd, -2, 0, 0, (int)this.Width, (int)this.Height, (int)SWPFlags.SWP_NOMOVE | (int)SWPFlags.SWP_NOSIZE | (int)SWPFlags.SWP_NOACTIVATE);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var isOpen = this.IsOpen;
            base.OnPreviewMouseLeftButtonDown(e);

            if (isOpen && !this.IsOpen)
            {
                e.Handled = true;
            }
        }
    }
}