using System;
using DelftTools.Controls;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public class StructurePresenter : ICanvasEditor
    {
        ShapeModifyTool ShapeModifyTool { get; set; }

        internal StructurePresenter(ShapeModifyTool shapeModifyTool)
        {
            ShapeModifyTool = shapeModifyTool;
        }

        public bool CanSelectItem
        {
            get { return true; }
        }

        public bool IsSelectItemActive
        {
            get { return ShapeModifyTool.ShapeSelectTool.IsActive; }
            set
            {
                if (value)
                {
                    ShapeModifyTool.ActivateTool(ShapeModifyTool.ShapeSelectTool);
                }
            }
        }

        public bool CanMoveItem
        {
            get { return true; }
        }

        public bool IsMoveItemActive
        {
            get { return ShapeModifyTool.ShapeMoveTool.IsActive; }
            set
            {
                if (value)
                {
                    ShapeModifyTool.ActivateTool(ShapeModifyTool.ShapeMoveTool);
                }
            }
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
            get { return true; }
        }

        public bool IsDeleteItemActive
        {
            get { return ShapeModifyTool.ShapeDeletePointTool.IsActive; }
            set
            {
                if (value)
                {
                    ShapeModifyTool.ActivateTool(ShapeModifyTool.ShapeDeletePointTool);
                }
            }
        }

        public bool CanAddPoint
        {
            get { return true; }
        }

        public bool IsAddPointActive
        {
            get { return ShapeModifyTool.ShapeInsertPointTool.IsActive; }
            set
            {
                if (value)
                {
                    ShapeModifyTool.ActivateTool(ShapeModifyTool.ShapeInsertPointTool);
                }
            }
        }

        public bool IsRemovePointActive { get; set; }
        public bool CanRemovePoint { get { return false; } }
    }
}