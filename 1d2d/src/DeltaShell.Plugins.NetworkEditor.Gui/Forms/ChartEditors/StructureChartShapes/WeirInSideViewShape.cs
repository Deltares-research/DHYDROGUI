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
        private static readonly Bitmap weirSmallIcon = Properties.Resources.WeirSmall;
        private static readonly Bitmap gateIcon = Properties.Resources.Gate;
        private readonly double iconLocationY;

        private static readonly StructureShapeStyleProvider structureShapeStyleProvider =
            new StructureShapeStyleProvider();

        public WeirInSideViewShape(IChart chart, 
                                   double offsetInSideView, 
                                   double iconLocationY,
                                   IWeir structure)
            : base(chart, offsetInSideView, structure)
        {
            this.iconLocationY = iconLocationY;
        }

        protected override void CreateStyles()
        {
            NormalStyle = structureShapeStyleProvider.GetNormalStyleForStructure(Structure);
            SelectedStyle = structureShapeStyleProvider.GetSelectedStyleForStructure(Structure);
            DisabledStyle = structureShapeStyleProvider.GetDisabledStyleForStructure(Structure);
        }

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            IShapeFeature weirShape = GetWeirShape();

            weirShape.NormalStyle = NormalStyle;
            weirShape.SelectedStyle = SelectedStyle;
            weirShape.DisabledStyle = DisabledStyle;

            yield return weirShape;

            var symbolShapeFeature = new SymbolShapeFeature(Chart, OffsetInSideView, iconLocationY,
                                                            SymbolShapeFeatureHorizontalAlignment.Center,
                                                            SymbolShapeFeatureVerticalAlignment.Center)
                                         {Image = Structure.WeirFormula is IGatedWeirFormula ? gateIcon : weirSmallIcon};

            yield return symbolShapeFeature;
        }

        private IShapeFeature GetWeirShape()
        {
            double zMinValue = ChartCoordinateService.ToWorldY(Chart, Chart.ChartBounds.Bottom);
            double zMaxValue = ChartCoordinateService.ToWorldY(Chart, Chart.ChartBounds.Top);
            
            if (Structure.WeirFormula is IGatedWeirFormula formula)
            {
                var gatedWeirShape = new GatedWeirShape(Chart, OffsetInSideView, Structure.CrestLevel,
                                                        16, formula.LowerEdgeLevel,
                                                        zMinValue,
                                                        zMaxValue, true, false);
                
                gatedWeirShape.WaterStyle = new VectorStyle
                {
                    Fill = new SolidBrush(Color.LightCyan),
                };
                return gatedWeirShape;
            }

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
                var weirHeight = Math.Max(0, Structure.CrestLevel - zMinValue);
                return new FixedRectangleShapeFeature(Chart, OffsetInSideView, Structure.CrestLevel, 16,
                                                      weirHeight, false, true)
                           {
                               HorizontalShapeAlignment = HorizontalShapeAlignment.Center
                           };    
            }
            return null;
        }
    }
}
