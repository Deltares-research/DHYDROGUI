using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class RemoveableItemsListBox : ListBox
    {
        private const int itemMargin = 5;

        private const int deleteButtonLeftStartCoordinate = 0;
        
        private readonly Bitmap deleteIcon;

        private bool deleteIconClicked;

        public event EventHandler<ListBoxItemRemovedEventArgs> OnItemRemoved;
        public event EventHandler<ListBoxItemRemovingEventArgs> OnItemRemoving;

        private readonly MaxNameableWidthCalculator maxNameableWidthCalculator;

        public RemoveableItemsListBox()
        {
            InitializeComponent();
            DrawMode = DrawMode.OwnerDrawFixed;
            IntegralHeight = false;
            ResizeRedraw = true;
            ItemHeight = FontHeight+itemMargin;
            int iconSize = ItemHeight;
            deleteIcon = new Bitmap(Resources.Delete, iconSize, iconSize);
            maxNameableWidthCalculator = new MaxNameableWidthCalculator();

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

            if (e.X > deleteButtonLeftStartCoordinate && e.X < ClientRectangle.Width && deleteIconClicked && SelectedItem != null)
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
            
            SetScrollBarWidth();
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
                e.Graphics.DrawString(name, e.Font, solidBrush, GetTextLeftStartCoordinate(), e.Bounds.Y);
            }

            e.DrawFocusRectangle();
            if (AllowItemDelete)
            {
                e.Graphics.DrawImageUnscaled(deleteIcon, deleteButtonLeftStartCoordinate, e.Bounds.Y);
            }
            
            SetScrollBarWidth();
        }

        /// <remark>
        /// The HorizontalExtent is the maxItemWidth plus an itemMargin.
        /// <br></br><br></br>
        /// This is done to create some extra scroll space in the UI to make the items better visible.
        /// </remark>
        private void SetScrollBarWidth()
        {
            int maxItemWidth = CalculateMaxItemWidth();
            HorizontalExtent = maxItemWidth + itemMargin;
            HorizontalScrollbar = maxItemWidth >= Width;
        }

        private int CalculateMaxItemWidth()
        {
            int additionalMargin = deleteIcon.Width + itemMargin;
            IEnumerable<INameable> nameableItems = Items.OfType<INameable>();
            int maxNameableWidth = maxNameableWidthCalculator.GetMaxNameableWidth(nameableItems, Font);
            return maxNameableWidth + additionalMargin;
        }

        private bool IsWithinBounds(Point point)
        {
            int x = point.X;

            if (x <= deleteButtonLeftStartCoordinate || x >= deleteIcon.Width)
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

        private int GetTextLeftStartCoordinate()
        {
            return deleteButtonLeftStartCoordinate+deleteIcon.Width+itemMargin;
        }
    }
}