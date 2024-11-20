using System;
using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public class SupportPointListBox : CheckedListBox
    {
        private static readonly SolidBrush TransparentOverlay = new SolidBrush(Color.FromArgb(200, Color.White));

        private bool ignoreCheck;
        private SolidBrush backColorBrush;
        private Bitmap itemRenderBuffer;
        private ToolStripItem[] contextMenuItems;

        public SupportPointListBox()
        {
            ItemCheck += SupportPointListBoxItemCheck;
            MouseClick += SupportPointListBoxMouseClick;
            MouseDown += SupportPointListBoxMouseDown;
            MouseUp += SupportPointListBoxMouseUp;
            Resize += SupportPointListBoxResize;
            backColorBrush = new SolidBrush(BackColor);
            itemRenderBuffer = new Bitmap(Width, ItemHeight);
            CheckOnClick = false;
        }

        public ToolStripItem[] ContextMenuItems
        {
            get
            {
                return contextMenuItems;
            }
            set
            {
                contextMenuItems = value;
                ContextMenuStrip = contextMenuItems != null ? new ContextMenuStrip() : null;
                if (ContextMenuStrip != null)
                {
                    ContextMenuStrip.Items.AddRange(contextMenuItems);
                }
            }
        }

        public new void SetItemChecked(int index, bool value)
        {
            ignoreCheck = false;
            base.SetItemChecked(index, value);
        }

        public new void SetItemCheckState(int index, CheckState checkState)
        {
            ignoreCheck = false;
            base.SetItemCheckState(index, checkState);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (backColorBrush != null)
            {
                backColorBrush.Dispose();
            }

            backColorBrush = new SolidBrush(BackColor);
        }

        /// <summary>
        /// Draw with double buffering here to prevent flicker. Customize the look & feel a bit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            bool isItemChecked = DesignMode || GetItemChecked(e.Index); // breaks in designer?!
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (Graphics bufferGraphics = Graphics.FromImage(itemRenderBuffer))
            {
                var newBounds = new Rectangle(0, 0, e.Bounds.Width, e.Bounds.Height);
                int x = newBounds.X;
                int y = newBounds.Y;
                int dim = ItemHeight;

                var args = new DrawItemEventArgs(bufferGraphics, e.Font, newBounds,
                                                 e.Index, e.State,
                                                 isItemChecked
                                                     ? e.ForeColor
                                                     : Color.LightGray, // draw text in lighter color if not checked
                                                 e.BackColor);

                base.OnDrawItem(args);

                bufferGraphics.FillRectangle(backColorBrush, x, y, dim, dim); // first remove checkbox

                if (isItemChecked)
                {
                    bufferGraphics.DrawImage(Resources.arrow, x, y, dim, dim); // overlay image
                }

                bufferGraphics.DrawImage(isItemChecked ? Resources.Delete : Resources.add,
                                         newBounds.Width - dim, y,
                                         dim, dim); // overlay image

                if (!selected) //semi-transparent overlay to make icon less visible
                {
                    bufferGraphics.FillRectangle(TransparentOverlay,
                                                 newBounds.Width - dim, y, dim, dim);
                }

                e.Graphics.DrawImageUnscaled(itemRenderBuffer, e.Bounds.X, e.Bounds.Y);
            }
        }

        private void SupportPointListBoxResize(object sender, EventArgs e)
        {
            if (itemRenderBuffer != null)
            {
                itemRenderBuffer.Dispose();
            }

            itemRenderBuffer = Width == 0 ? null : new Bitmap(Width, ItemHeight);
        }

        private void SupportPointListBoxMouseUp(object sender, MouseEventArgs e)
        {
            CheckIgnoreOrNot(e);
            if (ContextMenuItems != null && e.Button == MouseButtons.Right)
            {
                int index = IndexFromPoint(e.Location);
                ContextMenuStrip.Visible = index == SelectedIndex && index != NoMatches;
            }
        }

        private void SupportPointListBoxMouseClick(object sender, MouseEventArgs e)
        {
            CheckIgnoreOrNot(e);
        }

        private void SupportPointListBoxMouseDown(object sender, MouseEventArgs e)
        {
            CheckIgnoreOrNot(e);
        }

        private void CheckIgnoreOrNot(MouseEventArgs e)
        {
            ignoreCheck = e.X < ClientSize.Width - ItemHeight;
        }

        private void SupportPointListBoxItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ignoreCheck)
            {
                e.NewValue = e.CurrentValue;
            }
        }
    }
}