using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    [Flags]
    public enum ShapeEditMode
    {
        ShapeReadOnly = 0,
        ShapeResize = 1,
        ShapeMove = 2,
        ShapeSelect = 4
    }

    public class ShapeModifyTool : ShapeTool, IChartViewTool
    {
        readonly List<IShapeLayerTool> tools = new List<IShapeLayerTool>();
        public ShapeEditMode ShapeEditMode;
        private IShapeFeature selectedShape;

        public ShapeModifyTool(IChart chart) : base(chart)
        {
            ShapeSelectTool = new ShapeSelectTool();
            ShapeHoverTool = new ShapeHoverTool();
            ShapeMoveTool = new ShapeMoveTool(ShapeSelectTool);

            ShapeInsertPointTool = new ShapeInsertPointTool {ShapeMoveTool = ShapeMoveTool};

            ShapeDeletePointTool = new ShapeDeletePointTool();

            SelectStyle = new VectorStyle
                              {
                                  Fill = new SolidBrush(Color.FromArgb(150, Color.Magenta)),
                                  Line = new Pen(Color.Black)
                              };
            DefaultStyle = new VectorStyle
                               {
                                   Fill = new SolidBrush(Color.FromArgb(100, Color.Gold)),
                                   Line = new Pen(Color.Black)
                               };

            tools.Add(ShapeSelectTool);
            tools.Add(ShapeMoveTool);
            tools.Add(ShapeInsertPointTool);
            tools.Add(ShapeDeletePointTool);
            tools.Add(ShapeHoverTool);
            tools.ForEach(t => t.ShapeModifyTool = this);
            ShapeSelectTool.IsActive = true;
            OnMouseEvent = MouseEvent;
        }

        public ShapeMoveTool ShapeMoveTool { get; private set; }
        public ShapeSelectTool ShapeSelectTool { get; private set; }
        public ShapeInsertPointTool ShapeInsertPointTool { get; private set; }
        public ShapeDeletePointTool ShapeDeletePointTool { get; private set; }
        public ShapeHoverTool ShapeHoverTool { get; private set; }

        public IShapeFeature SelectedShape
        {
            get { return selectedShape; }
            set
            {
                //we have already updated selection
                if (selectedShape == value)
                    return;
                shapeFeatures.ForEach(
                    sf =>
                        {
                            sf.Selected = false;
                        });
                selectedShape = value;
                if (null != selectedShape)
                {
                    selectedShape.Selected = true;
                }
                
                if (null != SelectionChanged)
                {
                    SelectionChanged(this, new ShapeEventArgs(selectedShape));
                }
            }
        }

        public IChartView ChartView { get; set; }

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public new bool Active
        {
            get { return base.Active; }
            set
            {
                base.Active = value;
                if (ActiveChanged != null)
                {
                    ActiveChanged(this, null);
                }
            }
        }

        public event EventHandler<EventArgs> ActiveChanged;

        private IShapeLayerTool ActiveTool
        {
            get
            {
                foreach (var tool in tools)
                {
                    if (tool.IsActive)
                    {
                        return tool;   
                    }
                }
                return null;
            }
        }

        public void ActivateTool(IShapeLayerTool tool)
        {
            tools.ForEach(t => t.IsActive = t == tool);
        }

        public void Clear()
        {
            selectedShape = null;
            shapeFeatures.Clear();
            ShapeSelectTool.Clear();
            ShapeHoverTool.Clear();
        }

        public void AddShape(IShapeFeature shapeFeature)
        {
            shapeFeatures.Add(shapeFeature);
        }

        public void RemoveShape(IShapeFeature shapeFeature)
        {
            if ((null != ShapeFeatureEditor) && (ShapeFeatureEditor.ShapeFeature == shapeFeature))
            {
                ShapeFeatureEditor = null;
            }
            shapeFeatures.Remove(shapeFeature);
        }

        protected override void AfterDraw(EventArgs e)
        {
            base.AfterDraw(e);
            if (null != BeforeDraw)
            {
                BeforeDraw(this, new EventArgs());
            }

            // let the tools (selecttool, movetools, etc.) inside the ShapeModifyTool do their drawing.
            ShapeHoverTool.ClearBuffer();
            tools.ForEach(t => t.Paint());
        }

        private void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            ShapeHoverTool.MouseEvent(kind, e, c);
            ActiveTool.MouseEvent(kind, e, c);
        }

        public event EventHandler BeforeDraw;

        public event SelectionChangedEventHandler SelectionChanged;
    }
}
