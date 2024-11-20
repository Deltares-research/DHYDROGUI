using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public abstract class ShapeFeatureBase : IShapeFeature, ICloneable, IHover
    {
        protected ShapeFeatureBase(IChart chart)
        {
            Chart = chart;
            Active = true;
        }

        public virtual IGeometry Geometry { get; set; }

        public virtual bool Contains(double x, double y)
        {
            int xDevice = ChartCoordinateService.ToDeviceX(Chart, x);
            int yDevice = ChartCoordinateService.ToDeviceY(Chart, y);
            return GetBounds().Contains(xDevice, yDevice);
        }

        public virtual bool Contains(int x, int y)
        {
            return GetBounds().Contains(x, y);
        }

        public virtual Rectangle GetBounds()
        {
            var envelope = Geometry.EnvelopeInternal;
            int x = ChartCoordinateService.ToDeviceX(Chart, envelope.MinX);
            int y = ChartCoordinateService.ToDeviceY(Chart, envelope.MinY);
            int width = ChartCoordinateService.ToDeviceWidth(Chart, envelope.Width);
            int height = ChartCoordinateService.ToDeviceHeight(Chart, envelope.Height);
            return new Rectangle(x, y - height, width, height);
        }

        public VectorStyle NormalStyle { get; set; }

        public VectorStyle SelectedStyle { get; set; }

        public VectorStyle DisabledStyle {get; set;}

        public IChart Chart { get; set; }

        public virtual void Paint(VectorStyle style)
        {
            VectorStyle vectorStyle = style;

            if ((Selected ) && (SelectedStyle != null))
            {
                vectorStyle = SelectedStyle;
            }
            else if (!Active && DisabledStyle!= null) 
            {
                vectorStyle = DisabledStyle;
            }
            else if (NormalStyle != null)
            {
                vectorStyle = NormalStyle;
            }
            
            var g = Chart.Graphics;
            IChartDrawingContext chartDrawingContext = new ChartDrawingContext(g, vectorStyle);
            Paint(chartDrawingContext);
            chartDrawingContext.Reset();
        }

        protected abstract void Paint(IChartDrawingContext chartDrawingContext);

        public virtual bool Selected { get; set; }

        public bool Active{ get; set;}
        
        public virtual void Invalidate()
        {
            if (Chart.ParentControl != null)
            {
                Chart.ParentControl.Invalidate();
            }
        }

        public virtual IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            // default no Trackers
            return null;
        }

        public virtual object Clone()
        {
            return Activator.CreateInstance(GetType());
        }

        public object Tag { get; set; }

        public string Label { get; set; }

        readonly IList<IHoverFeature> hovers = new List<IHoverFeature>();

        public void AddHover(IHoverFeature hoverText)
        {
            hovers.Add(hoverText);
        }

        public void ClearHovers()
        {
            hovers.Clear();
        }

        public virtual void Hover(List<Rectangle> usedSpace, VectorStyle style, Graphics graphics)
        {
            foreach (IHoverFeature hoverText in hovers)
            {
                if ((hoverText.HoverType == HoverType.Selected) && (!Selected))
                {
                    continue;
                }
                hoverText.Render(usedSpace, Chart, graphics);
            }
        }
    }
}