using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class WeirInSideViewShape : StructureSideViewShape<IWeir>
    {
        private static readonly StructureShapeStyleProvider StructureShapeStyleProvider =
            new StructureShapeStyleProvider();

        public WeirInSideViewShape(IChart chart, double offsetInSideView, IWeir structure)
            : base(chart, offsetInSideView, structure) {}

        protected override void CreateStyles()
        {
            NormalStyle = StructureShapeStyleProvider.GetNormalStyleForStructure(Structure);
            SelectedStyle = StructureShapeStyleProvider.GetSelectedStyleForStructure(Structure);
            DisabledStyle = StructureShapeStyleProvider.GetDisabledStyleForStructure(Structure);
        }

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            IShapeFeature weirShape = GetWeirShape();

            weirShape.NormalStyle = NormalStyle;
            weirShape.SelectedStyle = SelectedStyle;
            weirShape.DisabledStyle = DisabledStyle;

            yield return weirShape;
        }

        private IShapeFeature GetWeirShape()
        {
            //const double zMinValue = -1000;
            //const double zMaxValue = 1000;
            double zMinValue = ChartCoordinateService.ToWorldY(Chart, Chart.ChartBounds.Bottom);
            double zMaxValue = ChartCoordinateService.ToWorldY(Chart, Chart.ChartBounds.Top);
            double minX = Chart.BottomAxis.Minimum;
            double maxX = Chart.BottomAxis.Maximum;

            if (Structure.WeirFormula is IGatedWeirFormula)
            {
                var formula = (IGatedWeirFormula) Structure.WeirFormula;
                var gatedWeirShape = new GatedWeirShape(Chart, OffsetInSideView, Structure.CrestLevel,
                                                        16, Structure.CrestLevel + formula.GateOpening,
                                                        zMinValue,
                                                        zMaxValue, true, false);

                gatedWeirShape.WaterStyle = new VectorStyle
                {
                    Fill = new SolidBrush(Color.LightCyan)
                };
                return gatedWeirShape;
            }
            // todo add support for sharp crested
            // Broad crested == user defined ? as current default

            var crestShape = CrestShape.Sharp;
            if (Structure.WeirFormula is RiverWeirFormula)
            {
                // only river wier supports crest shape
                crestShape = Structure.CrestShape;
            }

            if (crestShape == CrestShape.Round)
            {
                double height = Structure.CrestLevel - zMinValue;
                double archHeight = Math.Min(5, height / 3);
                double crestOffset = Structure.CrestLevel - zMinValue - archHeight;

                return new RoundCrestShapeFeature(Chart, OffsetInSideView,
                                                  Structure.CrestLevel,
                                                  16,
                                                  zMinValue, crestOffset);
            }

            if (crestShape == CrestShape.Triangular)
            {
                double height = Structure.CrestLevel - zMinValue;
                double triangleHeight = Math.Min(5, height / 3);
                double crestOffset = Structure.CrestLevel - zMinValue - triangleHeight;

                return new TriangularCrestShapeFeature(Chart, OffsetInSideView,
                                                       Structure.CrestLevel,
                                                       16,
                                                       zMinValue, crestOffset);
            }

            if (crestShape == CrestShape.Broad)
            {
                return new BroadCrestShapeFeature(Chart, OffsetInSideView,
                                                  Structure.CrestLevel,
                                                  16,
                                                  zMinValue);
            }

            //it must be sharp
            if (crestShape == CrestShape.Sharp)
            {
                double weirHeight = Math.Max(0, Structure.CrestLevel - zMinValue);
                return new FixedRectangleShapeFeature(Chart, OffsetInSideView, Structure.CrestLevel, 16,
                                                      weirHeight, false, true) {HorizontalShapeAlignment = HorizontalShapeAlignment.Center};
            }

            return null;
        }
    }
}