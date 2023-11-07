using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public sealed partial class RemoveableItemsListBox : ListBox
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
            
            deleteIcon = new Bitmap(Properties.Resources.Delete);
            ScaleBitmapLogicalToDevice(ref deleteIcon);

            AllowItemDelete = true;
        }

        public bool AllowItemDelete { private get; set; }

        public override int ItemHeight
        {
            get => base.ItemHeight;
            set => base.ItemHeight = LogicalToDeviceUnits(value);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            deleteIconClicked = SelectedItem != null && IsWithinBounds(e.Location);
            base.OnMouseDown(e);
        }

        private bool IsWithinBounds(Point point)
        {
            var x = point.X;

            if (x <= GetDeleteButtonLeft() || x >= ClientRectangle.Width) return false;
            
            var index = IndexFromPoint(point);

            if (index == NoMatches) return false;

            return index == SelectedIndex;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!AllowItemDelete)
            {
                base.OnMouseUp(e);
                return;
            }

            if (e.X <= GetDeleteButtonLeft() || e.X >= ClientRectangle.Width || !deleteIconClicked ||
                SelectedItem == null) return;

            var removedItem = SelectedItem;
            var removedIndex = SelectedIndex;
            var removingEvent = new ListBoxItemRemovingEventArgs(removedItem, removedIndex);
            
            OnItemRemoving?.Invoke(this, removingEvent);

            if (!removingEvent.Cancel)
            {
                Items.Remove(SelectedItem);
                OnItemRemoved?.Invoke(this, new ListBoxItemRemovedEventArgs(removedItem, removedIndex));
            }

            deleteIconClicked = false;
        }

        private int GetDeleteButtonLeft()
        {
            return ClientRectangle.Width - ItemHeight;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0 || Items.Count == 0)
                return;

            e.Graphics.SetClip(e.Bounds);
            e.Graphics.Clear(BackColor);

            string name = Items[e.Index].ToString();
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                deleteIcon.Dispose();
            }
            
            base.Dispose(disposing);
        }
    }
}
