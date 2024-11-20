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
    public class CulvertInSideViewShape : StructureSideViewShape<ICulvert>
    {
        private readonly bool horizontalAxisIsReversed;
        private int LevelCircleFeatureRadius = 5;

        private static readonly Bitmap culvertSmallIcon = Properties.Resources.CulvertSmall;
        private readonly double iconLocationY;


        public CulvertInSideViewShape(IChart chart, 
                                      double offset, 
                                      double iconLocationY,
                                      ICulvert culvert,
                                      bool horizontalAxisIsReversed)
            : base(chart,offset,culvert)
        {
            this.horizontalAxisIsReversed = horizontalAxisIsReversed;
            this.iconLocationY = iconLocationY;
        }

        protected override void CreateStyles()
        {
            DisabledStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(20, Color.Black)),
                Line = new Pen(Color.FromArgb(20, Color.Black))
            };
        }

        /// <summary>
        /// delivery is left if we bridge along the branch and the axis is reversed
        /// or we bridge against the branch and the axis was not reversed
        /// </summary>
        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            //keep selection
            IList<IShapeFeature> features = new List<IShapeFeature>();
            PolygonShapeFeature tube = GetTube();
            if (tube != null)
            {
                features.Add(tube);    
            }
            
            features.Add(GetInletFeature());
            features.Add(GetOutletFeature());
            
            if (Structure.GroundLayerEnabled)
            {
                features.Add(GetGroundLayerLine());
            }

            //set a disabled style to use in structureview when the culvert is not 'active'
            foreach (var feature in features)
            {
                feature.DisabledStyle = DisabledStyle;
            }

            features.Add(GetIcon());

            return features;
        }

        private IShapeFeature GetOutletFeature()
        {
            double x = horizontalAxisIsReversed ?
                OffsetInSideView - Structure.Length / 2 :
                OffsetInSideView + Structure.Length / 2;

            double y = Structure.OutletLevel;
            
            var xRadius = GetWorldWidth(LevelCircleFeatureRadius);
            var yRadius = GetWorldHeigth(LevelCircleFeatureRadius);
            var feature = new CircleShapeFeature(Chart, new Coordinate(x, y), xRadius, yRadius)
                              {
                                  NormalStyle = CulvertStyling.NormalOutletStyle,
                                  SelectedStyle = CulvertStyling.SelectedOutletStyle
                              };
            return feature;
        }

        private IShapeFeature GetInletFeature()
        {
            double x = horizontalAxisIsReversed
                           ?
                               OffsetInSideView + Structure.Length/2
                           :
                               OffsetInSideView - Structure.Length/2;

            double y = Structure.InletLevel;

            var xRadius = GetWorldWidth(LevelCircleFeatureRadius);
            var yRadius = GetWorldHeigth(LevelCircleFeatureRadius);
            var feature = new CircleShapeFeature(Chart, new Coordinate(x, y), xRadius, yRadius)
                              {
                                  NormalStyle = CulvertStyling.NormalInletStyle,
                                  SelectedStyle = CulvertStyling.SelectedInletStyle
                              };
            return feature;
        }

        private IShapeFeature GetGroundLayerLine()
        {
            VectorStyle normalStyle = CulvertStyling.NormalInletStyle;
            VectorStyle selectedStyle = CulvertStyling.SelectedInletStyle;

            var level = Structure.GroundLayerThickness + Structure.BottomLevel;
            var x = OffsetInSideView - Structure.Length / 2;
            var width = Structure.Length;

            var thickness = Math.Max(0.1, Structure.GroundLayerThickness); //always show something when its enabled

            var feature = new FixedRectangleShapeFeature(Chart, x, level, width, thickness, true, true)
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
        private PolygonShapeFeature GetTube()
        {
            var yzvalues = Structure.CrossSectionDefinitionAtInletAbsolute.GetProfile();
            if (yzvalues.Count() == 0)
                return null;

            //unless the axis is reversed the inlet has a smaller chainage.
            var leftPointZ = horizontalAxisIsReversed?Structure.OutletLevel : Structure.InletLevel;
            var rightPointZ = horizontalAxisIsReversed ? Structure.InletLevel: Structure.OutletLevel;
            //the height of the culvert is defined by the range of the crossection.
            var height = Structure.CrossSectionDefinitionAtInletAbsolute.HighestPoint -
                         Structure.CrossSectionDefinitionAtInletAbsolute.LowestPoint;

            var minX = OffsetInSideView - Structure.Length / 2;
            var maxX = OffsetInSideView + Structure.Length / 2;
            
            var vertices = new List<Coordinate>();
            vertices.Add(new Coordinate(minX, leftPointZ));
            vertices.Add(new Coordinate(maxX, rightPointZ));
            vertices.Add(new Coordinate(maxX, rightPointZ+height));
            vertices.Add(new Coordinate(minX, leftPointZ+height));
            //close the polygon
            vertices.Add(new Coordinate(minX, leftPointZ));
            
            ILinearRing newLinearRing =  new LinearRing(vertices.ToArray());
            IPolygon polygon =  new Polygon(newLinearRing, (ILinearRing[])null);


            var feature = new PolygonShapeFeature(Chart, polygon);
            feature.NormalStyle = CulvertStyling.NormalStyle;
            feature.SelectedStyle = CulvertStyling.SelectedStyle;
            return feature;
        }

        private IShapeFeature GetIcon()
        { 
            return new SymbolShapeFeature(Chart, 
                                          OffsetInSideView,
                                          iconLocationY,
                                          SymbolShapeFeatureHorizontalAlignment.Center,
                                          SymbolShapeFeatureVerticalAlignment.Center)
            { 
                Image = culvertSmallIcon, 
            };
        }
    }
}
