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
    public class CompositeShapeFeature : IShapeFeature, ICloneable, IHover
    {
        private readonly List<IShapeFeature> shapeFeatures = new List<IShapeFeature>();
        private VectorStyle normalStyle;
        private VectorStyle selectedStyle;
        private IChart chart;
        private bool selected;
        private bool active;

        protected readonly IList<IHoverFeature> hovers = new List<IHoverFeature>();

        public CompositeShapeFeature(IChart chart)
        {
            this.chart = chart;
            Active = true;
        }

        public virtual bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                shapeFeatures.ForEach(cs => cs.Selected = value);
            }
        }

        public IChart Chart
        {
            get
            {
                return chart;
            }
            set
            {
                chart = value;
                //propagate in the children
                shapeFeatures.ForEach(cs => cs.Chart = value);
            }
        }

        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                shapeFeatures.ForEach(cs=> cs.Active = value);
            }
        }

        public VectorStyle NormalStyle
        {
            get { return normalStyle; }
            set
            {
                normalStyle = value;
                foreach (var feature in ShapeFeatures)
                {
                    feature.NormalStyle = value;
                }
            }
        }

        public VectorStyle SelectedStyle
        {
            get { return selectedStyle; }
            set
            {
                selectedStyle = value;
                foreach (var feature in ShapeFeatures)
                {
                    feature.SelectedStyle = value;
                }
            }
        }

        public VectorStyle DisabledStyle
        {
            get; set;
        }

        public object Tag { get; set; }
        
        public string Label { get; set; }

        public IGeometry Geometry
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IList<IShapeFeature> ShapeFeatures
        {
            get { return shapeFeatures; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">X in world coordinates</param>
        /// <param name="y">Y in world coordinates</param>
        /// <returns></returns>
        public virtual bool Contains(double x, double y)
        {
            bool contains = false;
            shapeFeatures.ForEach(cs => contains |= cs.Contains(x, y));
            return contains;
        }

        /// <summary>
        /// Contains 
        /// </summary>
        /// <param name="x">X in device coordinates</param>
        /// <param name="y">Y in device coordinates</param>
        /// <returns></returns>
        public virtual bool Contains(int x, int y)
        {
            double worldX = ChartCoordinateService.ToWorldX(Chart, x);
            double worldY = ChartCoordinateService.ToWorldY(Chart, y);
            return Contains(worldX, worldY);
        }

        public virtual void Paint(VectorStyle style)
        {
            shapeFeatures.ForEach(cs => cs.Paint(style));
        }

        public void Invalidate()
        {
            shapeFeatures.ForEach(cs => cs.Invalidate());
        }

        public virtual IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new CompositeShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }

        public virtual Rectangle GetBounds()
        {
            Rectangle rectangle = Rectangle.Empty;
            shapeFeatures.ForEach(
                cs => rectangle = rectangle.IsEmpty ? cs.GetBounds() : Rectangle.Union(rectangle, cs.GetBounds()));
            return rectangle;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

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
            foreach (IHoverFeature hoverFeature in hovers)
            {
                if ((hoverFeature.HoverType == HoverType.Selected) && (!Selected))
                {
                    continue;
                }
                hoverFeature.Render(usedSpace, Chart, graphics);
            }
        }
    }
}