using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public delegate VectorStyle GetCustomStyleEventHandler(object sender, IShapeFeature shapeFeature);

    public class ShapeTool : ToolBase
    {
        public event GetCustomStyleEventHandler GetCustomStyle;
        public IShapeFeatureEditor ShapeFeatureEditor { get; set; }

        protected List<IShapeFeature> shapeFeatures = new List<IShapeFeature>();
        public IList<IShapeFeature> ShapeFeatures { get { return shapeFeatures; } }

        public VectorStyle DefaultStyle { get; set; }
        public VectorStyle SelectStyle { get; set; }

        protected ShapeTool(DelftTools.Controls.Swf.Charting.IChart chart) : base(chart)
        {
            OnAfterDraw = AfterDraw;
        }

        protected virtual void AfterDraw(EventArgs e)
        {
            foreach (IShapeFeature shapeFeature in shapeFeatures)
            {
                if (null != GetCustomStyle)
                {
                    shapeFeature.Paint(GetCustomStyle(this, shapeFeature));
                }
                else
                {
                    shapeFeature.Paint(shapeFeature.Selected ? SelectStyle : DefaultStyle);
                }
            }

            if (null != ShapeFeatureEditor)
            {
                // if there is an editor active let it do its drawing; eg draw Trackers
                ShapeFeatureEditor.Paint(Chart, Chart.Graphics);
            }
        }

        private readonly Dictionary<IShapeFeature, Rectangle> cache = new Dictionary<IShapeFeature, Rectangle>();

        /// <summary>
        /// Clears the bounding rect cache. The cache should be cleared when dimensions or extent of chart change or 
        /// a property of a shape.
        /// </summary>
        public void ClearChache()
        {
            cache.Clear();
        }

        private int referenceNullValue;

        /// <summary>
        /// Clears the cache if the reference calculation gives a new result. This avoids dependance on teechart
        /// ViewPortChanged event; when this event is fired axes are not updated.
        /// </summary>
        private void ClearCacheIfDirty()
        {
            int newNullPos = DelftTools.Controls.Swf.Charting.ChartCoordinateService.ToDeviceX(Chart, 0);
            if (referenceNullValue == newNullPos)
            {
                return;
            }
            ClearChache();
            referenceNullValue = newNullPos;
        }

        protected internal IShapeFeature Clicked(int x, int y)
        {
            ClearCacheIfDirty();

            foreach (IShapeFeature shapeFeature in shapeFeatures)
            {
                if (!cache.ContainsKey(shapeFeature))
                {
                    cache[shapeFeature] = shapeFeature.GetBounds();
                }
                if (!cache[shapeFeature].Contains(x, y))
                {
                    continue;
                }
                if (shapeFeature.Contains(x, y))
                {
                    return shapeFeature;
                }
            }
            return null;
        }
    }
}