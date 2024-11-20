using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class BridgeInStructureViewShape : CompositeShapeFeature
    {
        private readonly IBridge bridge;

        private VectorStyle selectedCrossSectionStyle;
        private VectorStyle normalCrossSectionStyle;

        private VectorStyle normalSurfaceStyle;
        private VectorStyle selectedSurfaceStyle;
        private const int SurfaceHeight = 3;
        private const int SurfaceOverhang = 3;

        public BridgeInStructureViewShape(IChart chart, IBridge bridge):base(chart)
        {
            this.bridge = bridge;

            CreateStyles();
            
            CalculateShapeFeatures();
        }

        private void CreateStyles()
        {
            var alpha = 40;

            normalCrossSectionStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, Color.LightBlue)),
                Line = new Pen(Color.FromArgb(alpha, Color.Black))
            };

            selectedCrossSectionStyle = new VectorStyle
                                          {
                                              //solid black
                                              Fill = new SolidBrush(Color.LightBlue),
                                              Line = new Pen(Color.Black)
                                          };

            normalSurfaceStyle = new VectorStyle
                               {
                                   Fill = new SolidBrush(Color.FromArgb(alpha, Color.Black)),
                                   Line = new Pen(Color.FromArgb(alpha, Color.Black))
                               };
            selectedSurfaceStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.Black),
                Line = new Pen(Color.Black)
            };

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

        private double GetWorldWidth(int deviceWidth)
        {
            return ChartCoordinateService.ToWorldWidth(Chart, deviceWidth);
        }

        private double GetWorldHeigth(int deviceHeight)
        {
            return ChartCoordinateService.ToWorldHeight(Chart, deviceHeight);
        }

        /// <summary>
        /// delivery is left if we bridge along the branch and the axis is reversed
        /// or we bridge against the branch and the axis was not reversed
        /// </summary>

        private void CalculateShapeFeatures()
        {
            //keep selection
            var oldStatus = Selected;
            ShapeFeatures.Clear();
            PolygonShapeFeature crossSectionFeature = GetCrossSectionFeature();
            var surfaceFeature = GetSurfaceFeature();
            if (surfaceFeature != null)
            {
                ShapeFeatures.Add(surfaceFeature);
            }
            if (crossSectionFeature != null)
            {
                ShapeFeatures.Add(crossSectionFeature);
            }

            if (bridge.GroundLayerEnabled)
            {
                ShapeFeatures.Add(GetGroundLayerLine());
            }

            Selected = oldStatus;
        }

        private IShapeFeature GetGroundLayerLine()
        {
            VectorStyle normalStyle = CulvertStyling.NormalInletStyle;
            VectorStyle selectedStyle = CulvertStyling.SelectedInletStyle;

            ICrossSectionDefinition crossSectionDefinition = bridge.GetShiftedCrossSectionDefinition();

            var level = bridge.GroundLayerThickness + crossSectionDefinition.LowestPoint;
            var worldWidth = crossSectionDefinition.Width;
            var x = bridge.OffsetY;

            var feature = new FixedRectangleShapeFeature(Chart, x, level, worldWidth, 3, true, false)
            {
                NormalStyle = normalStyle,
                SelectedStyle = selectedStyle
            };

            return feature;
        }

        /// <summary>
        /// Defines the top of the bridge as defined by the point of the top section of the cross-section
        /// </summary>
        /// <returns></returns>
        private FixedRectangleShapeFeature GetSurfaceFeature()
        {
            if (bridge.IsPillar)
                return null;

            ICrossSectionDefinition crossSectionDefinition = bridge.GetShiftedCrossSectionDefinition();
            IList<Coordinate> yzValues = crossSectionDefinition.FlowProfile.ToList();

            if (yzValues.Count <= 2)
                return null;

            var centerY = bridge.OffsetY + crossSectionDefinition.Width / 2.0;
            var maxY = yzValues.Select(x => x.X).Max() + GetWorldWidth(SurfaceOverhang);
            var minY = yzValues.Select(x => x.X).Min() - GetWorldWidth(SurfaceOverhang);
            var minZ = yzValues.Select(x => x.Y).Max();

            //draw a shape of 3 pixels height over spanning the points above 3 pixels left and right

            var feature = new FixedRectangleShapeFeature(Chart, minY + centerY - GetWorldWidth(SurfaceOverhang),
                                                         minZ + GetWorldHeigth(SurfaceHeight),
                                                         (maxY - minY) + GetWorldWidth(SurfaceOverhang*2), SurfaceHeight,
                                                         true, false)
                              {
                                  NormalStyle = normalSurfaceStyle,
                                  SelectedStyle = selectedSurfaceStyle
                              };

            return feature;
        }

        private PolygonShapeFeature GetCrossSectionFeature()
        {
            if (bridge.IsPillar)
                return null;

            ICrossSectionDefinition crossSectionDefinition = bridge.GetShiftedCrossSectionDefinition();
            IList<Coordinate> coordinates = crossSectionDefinition.FlowProfile.ToList();

            if (coordinates.Count <= 2)
                return null;
            
            double min = coordinates.Min(c => c.X);
            // calculation of offset in profile is done in structure view; bridge.OffsetY is the left most absolute value
            var drawCoordinates = coordinates.Select(c => new Coordinate(c.X + bridge.OffsetY - min, c.Y)).ToList();
            //add the first as the last so we get a closed ring
            drawCoordinates.Add(drawCoordinates.First());

            ILinearRing newLinearRing = new LinearRing(drawCoordinates.ToArray());
            IPolygon polygon = new Polygon(newLinearRing, (ILinearRing[])null);
            var feature = new PolygonShapeFeature(Chart, polygon);
            
            feature.NormalStyle = normalCrossSectionStyle;
            feature.SelectedStyle = selectedCrossSectionStyle;
            return feature;
        }
    }
}
