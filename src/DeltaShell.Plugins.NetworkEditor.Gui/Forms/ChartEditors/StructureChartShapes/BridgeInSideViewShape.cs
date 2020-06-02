using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class BridgeInSideViewShape : StructureSideViewShape<IBridge>
    {
        private const int SurfaceHeight = 3;
        private const int SurfaceOverhang = 3;
        private static readonly Bitmap BridgeSmallIcon = Resources.BridgeSmall;

        private VectorStyle selectedCrossSectionStyle;
        private VectorStyle normalCrossSectionStyle;
        private VectorStyle normalSurfaceStyle;
        private VectorStyle selectedSurfaceStyle;
        private VectorStyle disableSurfaceStyle;
        private VectorStyle disabledCrossSectionStyle;

        public BridgeInSideViewShape(IChart chart, double offset, IBridge bridge)
            : base(chart, offset, bridge) {}

        protected override void CreateStyles()
        {
            const int alpha = 40;

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

            disableSurfaceStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, Color.Black)),
                Line = new Pen(Color.FromArgb(alpha, Color.Black))
            };

            const int disabledAlpa = 20;
            disabledCrossSectionStyle = new VectorStyle
            {
                //solid black
                Fill = new SolidBrush(Color.FromArgb(disabledAlpa, Color.Black)),
                Line = new Pen(Color.FromArgb(disabledAlpa, Color.Black))
            };
        }

        /// <summary>
        /// delivery is left if we bridge along the branch and the axis is reversed
        /// or we bridge against the branch and the axis was not reversed
        /// </summary>
        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            if (Structure.IsPillar)
            {
                //hack:
                double y = ((Chart.LeftAxis.Maximum - Chart.LeftAxis.Minimum) / 2.0) + Chart.LeftAxis.Minimum;

                yield return
                    new SymbolShapeFeature(Chart, OffsetInSideView, y,
                                           SymbolShapeFeatureHorizontalAlignment.Left,
                                           SymbolShapeFeatureVerticalAlignment.Center) {Image = BridgeSmallIcon};
            }
            else
            {
                //keep selection
                ShapeFeatureBase crossSectionFeature = GetCrossSectionFeature();
                FixedRectangleShapeFeature surfaceFeature = GetSurfaceFeature();
                if (surfaceFeature != null)
                {
                    yield return surfaceFeature;
                }

                if (crossSectionFeature != null)
                {
                    yield return crossSectionFeature;
                }

                if (Structure.GroundLayerEnabled)
                {
                    yield return GetGroundLayerLine();
                }
            }
        }

        private IShapeFeature GetGroundLayerLine()
        {
            VectorStyle normalStyle = CulvertStyling.NormalInletStyle;
            VectorStyle selectedStyle = CulvertStyling.SelectedInletStyle;

            double level = Structure.GroundLayerThickness + Structure.EffectiveCrossSectionDefinition.LowestPoint;
            double x = OffsetInSideView - (Structure.Length / 2);
            double width = Structure.Length;

            double thickness = Math.Max(0.1, Structure.GroundLayerThickness); //always show something when its enabled

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
        private FixedRectangleShapeFeature GetSurfaceFeature()
        {
            IBridge bridge = Structure;

            if (bridge.IsPillar)
            {
                return null;
            }

            IList<Coordinate> yzValues = bridge.EffectiveCrossSectionDefinition.FlowProfile.ToList();

            if (yzValues.Count <= 2)
            {
                return null;
            }

            //a rectange defined by min/max height of length around Offset
            double minX = OffsetInSideView - (Structure.Length / 2) - GetWorldWidth(SurfaceOverhang);
            double maxZ = yzValues.Select(x => x.Y).Max() + GetWorldHeigth(SurfaceHeight);

            var feature = new FixedRectangleShapeFeature(Chart, minX, maxZ, Structure.Length + (2 * GetWorldWidth(SurfaceOverhang)), SurfaceHeight, true, false)
            {
                NormalStyle = normalSurfaceStyle,
                SelectedStyle = selectedSurfaceStyle,
                DisabledStyle = disableSurfaceStyle
            };

            return feature;
        }

        private ShapeFeatureBase GetCrossSectionFeature()
        {
            IBridge bridge = Structure;

            if (bridge.IsPillar)
            {
                return null;
            }

            IList<Coordinate> yzValues = bridge.EffectiveCrossSectionDefinition.FlowProfile.ToList();

            if (yzValues.Count <= 2)
            {
                return null;
            }

            //a rectange defined by min/max height of length around Offset
            double minX = OffsetInSideView - (Structure.Length / 2);
            double minZ = yzValues.Select(x => x.Y).Min();
            double maxZ = yzValues.Select(x => x.Y).Max();

            var feature = new FixedRectangleShapeFeature(Chart, minX, maxZ, Structure.Length, maxZ - minZ, true, true)
            {
                NormalStyle = normalCrossSectionStyle,
                SelectedStyle = selectedCrossSectionStyle,
                DisabledStyle = disabledCrossSectionStyle
            };

            return feature;
        }
    }
}