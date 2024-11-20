using System;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class GatedWeirShape : CompositeShapeFeature
    {
        private readonly bool hasEditor;

        public ShapeFeatureBase WeirShape { get; set; }
        private ShapeFeatureBase GateShape { get; set; }
        public ShapeFeatureBase WaterShape { get; set; }
        public VectorStyle WaterStyle
        {
            set
            {
                VectorStyle transparentStyle = (VectorStyle) value.Clone();
                transparentStyle.Fill = Brushes.Transparent;
                WaterShape.NormalStyle = value;
                WaterShape.DisabledStyle = value;
                WaterShape.SelectedStyle = value;
            }
        }

        public GatedWeirShape(IChart chart, double offsetY, double crestLevel, double width, double gateLevel,
                              double minZValue, double maxZValue, bool widthInPixels,bool hasEditor) : base(chart)
        {
            this.hasEditor = hasEditor;
            if (widthInPixels && hasEditor)
                throw new InvalidOperationException("Currently editing is not supported for shapes with widthInPixels");
                
            if (widthInPixels)
            {
                WeirShape = new FixedRectangleShapeFeature(chart,
                                                           offsetY,
                                                           crestLevel,
                                                           width,
                                                           crestLevel - minZValue,
                                                           false, true)
                                {HorizontalShapeAlignment = HorizontalShapeAlignment.Center};
                GateShape = new FixedRectangleShapeFeature(chart,
                                                           offsetY,
                                                           maxZValue,
                                                           width,
                                                           maxZValue - gateLevel,
                                                           false, true)
                                {HorizontalShapeAlignment = HorizontalShapeAlignment.Center};
                WaterShape = new FixedRectangleShapeFeature(chart,
                                                            offsetY,
                                                            gateLevel,
                                                            width,
                                                            gateLevel - crestLevel,
                                                            false, true)
                                 {HorizontalShapeAlignment = HorizontalShapeAlignment.Center};
             
            }
            else
            {
                WeirShape = new RectangleShapeFeature(chart,
                                          offsetY,
                                          crestLevel,
                                          offsetY + width,
                                          minZValue);
                GateShape = new RectangleShapeFeature(chart,
                                                          offsetY,
                                                          maxZValue,
                                                          offsetY + width,
                                                          gateLevel);
                WaterShape = new RectangleShapeFeature(chart,
                                                          offsetY,
                                                          gateLevel,
                                                          offsetY + width,
                                                          crestLevel);
            }

            ShapeFeatures.Add(WeirShape);
            ShapeFeatures.Add(GateShape);
            ShapeFeatures.Add(WaterShape);
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return hasEditor ? new GatedWeirShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode) : null;
        }

        public override void Paint(VectorStyle style)
        {
            WeirShape.Paint(style);
            if (GateShape.GetBounds().Height < 0)
            {
                return;
            }
            GateShape.Paint(style);
            WaterShape.Paint(style);
        }
    }
}