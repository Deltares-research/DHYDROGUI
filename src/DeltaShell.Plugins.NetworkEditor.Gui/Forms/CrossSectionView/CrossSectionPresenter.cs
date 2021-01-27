using System;
using DelftTools.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    internal class CrossSectionPresenter : ICanvasEditor
    {
        public bool CanSelectItem
        {
            get { return false; }
        }

        public bool IsSelectItemActive
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public bool CanMoveItem
        {
            get { return false; }
        }

        public bool IsMoveItemActive
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public bool CanMoveItemLinear
        {
            get { return false; }
        }

        public bool IsMoveItemLinearActive
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public bool CanDeleteItem
        {
            get { return false; }
        }

        public bool IsDeleteItemActive
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public bool CanAddPoint
        {
            get { return true; }
        }

        public bool IsAddPointActive
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsRemovePointActive { get; set; }

        public bool CanRemovePoint { get { return false; } }
    }
}