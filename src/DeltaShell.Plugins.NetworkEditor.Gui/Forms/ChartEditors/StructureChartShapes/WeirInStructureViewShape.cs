using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    internal class WeirInStructureViewShape : CompositeShapeFeature
    {
        private static readonly StructureShapeStyleProvider StructureShapeStyleProvider =
            new StructureShapeStyleProvider();
        
        private readonly IChart chart;
        private readonly IWeir weir;

        public WeirInStructureViewShape(IChart chart, IWeir weir)
            : base(chart)
        {
            this.weir = weir;
            this.chart = chart;

            CalculateShapeFeatures();
        }

        public RectangleShapeFeature WeirShape
        {
            get { return (RectangleShapeFeature) ShapeFeatures[0]; }
        }

        private void CalculateShapeFeatures()
        {
            var oldStatus = Selected;
            ShapeFeatures.Clear();

            IShapeFeature weirShape = GetWeirShape();

            ShapeFeatures.Add(weirShape);

            Selected = oldStatus;
        }

        private IShapeFeature GetWeirShape()
        {
            double minZValue = ChartCoordinateService.ToWorldY(chart, chart.ChartBounds.Bottom);
            double maxZValue = ChartCoordinateService.ToWorldY(chart, chart.ChartBounds.Top);

            IShapeFeature weirShape;
            if (weir.IsGated)
            {
                var gatedWeirFormula = (IGatedWeirFormula) weir.WeirFormula;
                var gatedWeirShape = new GatedWeirShape(chart, weir.OffsetY, weir.CrestLevel,
                                               weir.CrestWidth, weir.CrestLevel + gatedWeirFormula.GateOpening,
                                               minZValue, maxZValue, false,true)
                                {
                                    NormalStyle = StructureShapeStyleProvider.GetNormalStyleForStructure(weir),
                                    SelectedStyle = StructureShapeStyleProvider.GetSelectedStyleForStructure(weir)
                                };
                gatedWeirShape.WaterStyle = new VectorStyle
                                                              {
                                                                  Line = Pens.Transparent,
                                                                  Fill = new SolidBrush(Color.FromArgb(100, Color.LightCyan)),
                                                              };
                gatedWeirShape.AddHover(new HoverText("Crest level", string.Format("{0:f2}m.", weir.CrestLevel),
                                                      gatedWeirShape.WeirShape, Color.Black, HoverPosition.Left,
                                                      ArrowHeadPosition.Top));
                gatedWeirShape.AddHover(new HoverText("Crest width", string.Format("{0:f2}m.", weir.CrestWidth),
                                                      gatedWeirShape.WeirShape, Color.Black, HoverPosition.Bottom,
                                                      ArrowHeadPosition.LeftRight));
                gatedWeirShape.AddHover(new HoverText("Gate opening",
                                                      string.Format("{0:f2}m.", gatedWeirFormula.GateOpening),
                                                      gatedWeirShape.WaterShape, Color.Black, HoverPosition.Left,
                                                      ArrowHeadPosition.TopDown));
                weirShape = gatedWeirShape;
            }
            else
            {
                if (weir.IsRectangle)
                {
                    var weirShapeFeature = new WeirShapeFeature(chart,
                                                                weir.OffsetY,
                                                                weir.CrestLevel,
                                                                weir.OffsetY + weir.CrestWidth,
                                                                minZValue, maxZValue)
                                               {
                                                   WeirShape =
                                                       {
                                                           NormalStyle = StructureShapeStyleProvider.GetNormalStyleForStructure(weir),
                                                           SelectedStyle = StructureShapeStyleProvider.GetSelectedStyleForStructure(weir)
                                                       },
                                                   WaterStyle = new VectorStyle
                                                                    {
                                                                        Line = Pens.Transparent,
                                                                        Fill = new SolidBrush(Color.FromArgb(100, Color.LightCyan)),
                                                                    }
                                               };

                    weirShapeFeature.AddHover(new HoverText("Crest width", string.Format("{0:f2}m.", weir.CrestWidth),
                                                            weirShapeFeature.WaterShape, Color.Black, HoverPosition.Top,
                                                            ArrowHeadPosition.LeftRight));
                    weirShapeFeature.AddHover(new HoverText("Crest level", string.Format("{0:f2}m.", weir.CrestLevel),
                                                            weirShapeFeature.WeirShape, Color.Black, HoverPosition.Left,
                                                            ArrowHeadPosition.Top));
                    weirShape = weirShapeFeature;
                }
                else
                {
                    var freeFormWeirFormula = (FreeFormWeirFormula)weir.WeirFormula;
                    var freeFormatWeirShapeFeature = new FreeFormatWeirShapeFeature(chart, weir, freeFormWeirFormula.Shape, minZValue, maxZValue);
                    freeFormatWeirShapeFeature.PolygonShapeFeature.NormalStyle = StructureShapeStyleProvider.GetNormalStyleForStructure(weir);
                    freeFormatWeirShapeFeature.PolygonShapeFeature.SelectedStyle = StructureShapeStyleProvider.GetSelectedStyleForStructure(weir);
                    freeFormatWeirShapeFeature.WaterStyle = new VectorStyle
                    {
                        Fill = new SolidBrush(Color.FromArgb(100, Color.LightCyan)),
                    };
                    freeFormatWeirShapeFeature.AddHover(new HoverText("Crest width",
                                                                      string.Format("{0:f2}m.", weir.CrestWidth),
                                                                      freeFormatWeirShapeFeature.WaterShape,
                                                                      Color.Black, HoverPosition.Top,
                                                                      ArrowHeadPosition.LeftRight));
                    weirShape = freeFormatWeirShapeFeature;

                }
            }
            return weirShape;
        }

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

        public override void Hover(List<Rectangle> usedSpace, VectorStyle style, Graphics graphics)
        {
            IShapeFeature shapeFeature = GetWeirShape();
            IHover hover = shapeFeature as IHover;
            if (hover != null)
            {
                hover.Hover(usedSpace, style, graphics);
            }
        }
    }
}