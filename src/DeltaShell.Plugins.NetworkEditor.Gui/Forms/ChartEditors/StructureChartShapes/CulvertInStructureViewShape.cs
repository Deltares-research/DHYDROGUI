using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class CulvertInStructureViewShape : CompositeShapeFeature
    {
        private readonly ICulvert culvert;

        private VectorStyle normalGateStyle;
        private VectorStyle selectedGateStyle;


        public CulvertInStructureViewShape(IChart chart, ICulvert culvert)
            : base(chart)
        {
            this.culvert = culvert;

            CreateStyles();


            CalculateShapeFeatures();
        }

        private void CreateStyles()
        {
            var alpha = 40;

            normalGateStyle = new VectorStyle
                               {
                                   Fill = new SolidBrush(Color.FromArgb(alpha, Color.Black)),
                                   Line = new Pen(Color.FromArgb(alpha, Color.Black))
                               };

            selectedGateStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.Black),
                Line = new Pen(Color.Black)
            };
        }


        /// <summary>
        /// Custom paint method since x of level lines is dependent of zoom-level
        /// </summary>
        /// <param name="vectorStyle"></param>
        public override void Paint(VectorStyle vectorStyle)
        {
            if (culvert.TabulatedCrossSectionDefinition.ZWDataTable.HasErrors)
            {
                return;
            }
            CalculateShapeFeatures();
            base.Paint(vectorStyle);
        }

        public override bool Contains(int x, int y)
        {
            CalculateShapeFeatures();
            return base.Contains(x, y);
            //get current shapes
        }

        private double GetWorldWidth(int deviceWidth)
        {
            return ChartCoordinateService.ToWorldWidth(Chart, deviceWidth);
        }

        private double GetWorldHeigth(int deviceHeight)
        {
            return ChartCoordinateService.ToWorldHeight(Chart, deviceHeight);
        }

        /// <summary>
        /// delivery is left if we culvert along the branch and the axis is reversed
        /// or we culvert against the branch and the axis was not reversed
        /// </summary>

        private void CalculateShapeFeatures()
        {
            //keep selection
            var oldStatus = Selected;
            ShapeFeatures.Clear();
            
            var crossSectionFeature = GetCrossSectionFeature();
            if (crossSectionFeature != null)
            {
                ShapeFeatures.Add(crossSectionFeature);
            }
            
            if (culvert.GroundLayerEnabled)
            {
                ShapeFeatures.Add(GetGroundLayerLine());
            }

            if (culvert.IsGated)
            {
                var gate = GetGateFeature();
                if (gate != null)
                {
                    ShapeFeatures.Add(gate);
                }
            }

            Selected = oldStatus;
        }

        private IShapeFeature GetGroundLayerLine()
        {
            VectorStyle normalStyle = CulvertStyling.NormalInletStyle;
            VectorStyle selectedStyle = CulvertStyling.SelectedInletStyle;

            var level = culvert.GroundLayerThickness + culvert.BottomLevel;

            var worldWidth = culvert.CrossSectionDefinitionForCalculation.Width;
            var x = culvert.OffsetY;
            
            var feature = new FixedRectangleShapeFeature(Chart, x, level, worldWidth, 3, true, false)
            {
                NormalStyle = normalStyle,
                SelectedStyle = selectedStyle
            };
            
            return feature;
        }

        /// <summary>
        /// Defines a horizontal line representing the gate level.
        /// </summary>
        /// <returns>a shape representing the gate level, may return null if there are no coordinates on which to base the shape of the gate level on</returns>
        private IShapeFeature GetGateFeature()
        {
            if (!culvert.IsGated)
                throw new ArgumentException("No gate defined.");

            IList<Coordinate> coordinates = culvert.CrossSectionDefinitionAtInletAbsolute.GetProfile().ToList();
            if (!coordinates.Any())
                return null;

            //take the leftmost and rightmost coordinate..add a little margin and draw a big line a the lower edge level
            var left = coordinates.Min(c => c.X + culvert.OffsetY) - GetWorldWidth(3);
            var right = coordinates.Max(c => c.X + culvert.OffsetY) + GetWorldWidth(3);
            var bottom = culvert.GateLowerEdgeLevel;
            var top = culvert.GateLowerEdgeLevel + GetWorldHeigth(3);

            var feature = new RectangleShapeFeature(Chart, left, top, right, bottom);
            feature.NormalStyle = normalGateStyle;
            feature.SelectedStyle = selectedGateStyle;
            return feature;
        }


        private PolygonShapeFeature GetCrossSectionFeature()
        {
            IList<Coordinate> coordinates = culvert.CrossSectionDefinitionAtInletAbsolute.GetProfile().ToList();

            if (coordinates.Count <= 2)
                return null;

            double offsetLeftIsZero = culvert.CrossSectionDefinitionAtInletAbsolute.Left;
            var drawCoordinates =
                coordinates.Select(c => new Coordinate(c.X + culvert.OffsetY - offsetLeftIsZero, c.Y)).ToList();
            //add the first as the last so we get a closed ring
            drawCoordinates.Add(drawCoordinates.First());

            ILinearRing newLinearRing = new LinearRing(drawCoordinates.ToArray());
            IPolygon polygon = new Polygon(newLinearRing, (ILinearRing[])null);
            var feature = new PolygonShapeFeature(Chart, polygon);

            feature.NormalStyle = CulvertStyling.NormalStyle;
            feature.SelectedStyle = CulvertStyling.SelectedStyle;
            return feature;
        }
    }
}
