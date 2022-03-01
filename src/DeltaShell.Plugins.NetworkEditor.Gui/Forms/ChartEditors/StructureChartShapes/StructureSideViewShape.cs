using System.Collections.Generic;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{

    /// <summary>
    /// Class updates shape features just before paint and contains. Descended classes need to have a CalculatedShapeFeatures
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StructureSideViewShape<T>: CompositeShapeFeature where  T :IStructure1D
    {
        protected StructureSideViewShape(IChart chart, double offsetInSideView, T structure):base(chart)
        {
            Structure = structure;
            OffsetInSideView = offsetInSideView;

            CreateStyles();
        }

        protected T Structure { get; }

        protected double OffsetInSideView { get; }

        /// <summary>
        /// Style initialization. Called at constructor. 
        /// TODO: Make it virtual??
        /// </summary>
        protected abstract void CreateStyles();

        /// <summary>
        /// Update shape features before paint and contains.
        /// </summary>
        private void CalculateShapeFeatures()
        {
            var wasSelected = Selected;
            var wasActive = Active;
            ShapeFeatures.Clear();
            foreach (IShapeFeature feature in GetShapeFeatures())
            {
                ShapeFeatures.Add(feature);    
            }
            //set it again to update the child shapes.
            Selected = wasSelected;
            Active = wasActive;
        }

        protected abstract IEnumerable<IShapeFeature> GetShapeFeatures();

        /// <summary>
        /// Custom paint method since x of level lines is dependend of zoom-level
        /// </summary>
        /// <param name="vectorStyle"></param>
        public override void Paint(VectorStyle vectorStyle)
        {
            //custom paint logic :)
            CalculateShapeFeatures();
            base.Paint(vectorStyle);
        }

        public override bool Contains(int x, int y)
        {
            CalculateShapeFeatures();
            return base.Contains(x, y);
            //get current shapes
        }

        protected double GetWorldWidth(int deviceWidth)
        {
            return ChartCoordinateService.ToWorldWidth(Chart, deviceWidth);
        }

        protected double GetWorldHeigth(int deviceHeight)
        {
            return ChartCoordinateService.ToWorldHeight(Chart, deviceHeight);
        }
    }
}