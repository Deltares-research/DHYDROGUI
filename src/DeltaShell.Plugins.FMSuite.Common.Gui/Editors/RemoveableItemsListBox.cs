using System;
using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class RemoveableItemsListBox : ListBox
    {
        private readonly Bitmap deleteIcon;

        private bool deleteIconClicked;

        public event EventHandler<ListBoxItemRemovedEventArgs> OnItemRemoved;
        public event EventHandler<ListBoxItemRemovingEventArgs> OnItemRemoving;

        public RemoveableItemsListBox()
        {
            InitializeComponent();
            DrawMode = DrawMode.OwnerDrawFixed;
            IntegralHeight = false;
            ResizeRedraw = true;
            int iconSize = ItemHeight;
            deleteIcon = new Bitmap(Resources.Delete, iconSize, iconSize);

            AllowItemDelete = true;
        }

        public bool AllowItemDelete { private get; set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            deleteIconClicked = SelectedItem != null && IsWithinBounds(e.Location);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!AllowItemDelete)
            {
                base.OnMouseUp(e);
                return;
            }

            if (e.X > GetDeleteButtonLeft() && e.X < ClientRectangle.Width && deleteIconClicked && SelectedItem != null)
            {
                object removedItem = SelectedItem;
                int removedIndex = SelectedIndex;
                var removingEvent = new ListBoxItemRemovingEventArgs(removedItem, removedIndex);
                OnItemRemoving?.Invoke(this, removingEvent);

                if (!removingEvent.Cancel)
                {
                    Items.Remove(SelectedItem);
                    OnItemRemoved?.Invoke(this, new ListBoxItemRemovedEventArgs(removedItem, removedIndex));
                }

                deleteIconClicked = false;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0 || Items.Count == 0)
            {
                return;
            }

            e.Graphics.SetClip(e.Bounds);
            e.Graphics.Clear(BackColor);

            var name = Items[e.Index].ToString();
            if (FormattingEnabled)
            {
                var args = new ListControlConvertEventArgs("", typeof(string), Items[e.Index]);
                OnFormat(args);
                name = (string) args.Value;
            }

            e.DrawBackground();
            using (var solidBrush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(name, e.Font, solidBrush, 0, e.Bounds.Y);
            }

            e.DrawFocusRectangle();
            if (AllowItemDelete)
            {
                e.Graphics.DrawImageUnscaled(deleteIcon, GetDeleteButtonLeft(), e.Bounds.Y);
            }
        }

        private bool IsWithinBounds(Point point)
        {
            int x = point.X;

            if (x <= GetDeleteButtonLeft() || x >= ClientRectangle.Width)
            {
                return false;
            }

            int index = IndexFromPoint(point);

            if (index == NoMatches)
            {
                return false;
            }

            return index == SelectedIndex;
        }

        private int GetDeleteButtonLeft()
        {
            return ClientRectangle.Width - ItemHeight;
        }
    }
}