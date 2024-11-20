using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public static class NetworkLayerThemeFactory
    {
        public static ITheme CreateTheme(IEnumerable networkObjects)
        {
            const int lineWidth = 3;

            switch (networkObjects)
            {
                case IEnumerable<IHydroNode> _:
                {
                    return GenerateNodesTheme();
                }
                case IEnumerable<IChannel> _:
                {
                    return GenerateChannelsTheme();
                }
                case IEnumerable<IPipe> _:
                {
                    return GeneratePipesTheme(lineWidth);
                }
                case IEnumerable<ISewerConnection> _:
                {
                    return GenerateSewerConnectionsTheme(lineWidth);
                }
                case IEnumerable<ICrossSection> _:
                {
                    return GenerateCrossSectionsTheme(lineWidth);
                }
                default:
                    return null;
            }
        }

        private static ITheme GenerateCrossSectionsTheme(int lineWidth)
        {
            var yzStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Silver, lineWidth),
            };

            var zwStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Gray, lineWidth),
            };

            var geometryStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.OrangeRed, lineWidth),
            };

            var standardStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Purple, lineWidth)
            };

            return new CategorialTheme
            {
                AttributeName = nameof(ICrossSection.CrossSectionType),
                DefaultStyle = geometryStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem(CrossSectionType.YZ.GetDescription(), yzStyle, null, CrossSectionType.YZ),
                    new CategorialThemeItem(CrossSectionType.ZW.GetDescription(), zwStyle, null, CrossSectionType.ZW),
                    new CategorialThemeItem(CrossSectionType.GeometryBased.GetDescription(), geometryStyle, null, CrossSectionType.GeometryBased),
                    new CategorialThemeItem(CrossSectionType.Standard.GetDescription(), standardStyle, null, CrossSectionType.Standard),
                }
            };
        }

        private static ITheme GenerateSewerConnectionsTheme(int lineWidth)
        {
            var branchStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Pink, lineWidth),
            };

            var pumpStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Red, lineWidth) { DashStyle = DashStyle.Dash },
            };

            var weirStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.LimeGreen, lineWidth) { DashStyle = DashStyle.Dash },
            };

            return new CategorialTheme
            {
                AttributeName = nameof(ISewerConnection.SpecialConnectionType),
                DefaultStyle = branchStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem(SewerConnectionSpecialConnectionType.Pump.GetDescription(), pumpStyle, null, SewerConnectionSpecialConnectionType.Pump),
                    new CategorialThemeItem(SewerConnectionSpecialConnectionType.Weir.GetDescription(), weirStyle, null, SewerConnectionSpecialConnectionType.Weir),
                    new CategorialThemeItem(SewerConnectionSpecialConnectionType.None.GetDescription(), branchStyle, null, SewerConnectionSpecialConnectionType.None),
                }
            };
        }

        private static ITheme GeneratePipesTheme(int lineWidth)
        {
            var branchStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.SlateGray, lineWidth)
            };

            var stormWaterConnectionStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.RoyalBlue, lineWidth)
            };

            var dryWaterConnectionStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.OrangeRed, lineWidth)
            };

            var combinedWaterConnectionStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Black, lineWidth),
            };

            return new CategorialTheme
            {
                AttributeName = nameof(ISewerConnection.WaterType),
                DefaultStyle = branchStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem("Default", branchStyle, null, SewerConnectionWaterType.None),
                    new CategorialThemeItem("Storm water", stormWaterConnectionStyle, null, SewerConnectionWaterType.StormWater),
                    new CategorialThemeItem("Foul water", dryWaterConnectionStyle, null, SewerConnectionWaterType.DryWater),
                    new CategorialThemeItem("Combined", combinedWaterConnectionStyle, null, SewerConnectionWaterType.Combined),
                }
            };
        }

        private static ITheme GenerateChannelsTheme()
        {
            var branchStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.FromArgb(255, 0, 0, 128), 3) { CustomEndCap = new AdjustableArrowCap(5, 5, true) { BaseCap = LineCap.Triangle } },
                EnableOutline = false
            };

            return new CategorialTheme
            {
                AttributeName = nameof(IBranch.IsLengthCustom),
                DefaultStyle = branchStyle
            };
        }

        private static ITheme GenerateNodesTheme()
        {
            var onSingleBranchesStyle = NetworkLayerFactory.CreatePointStyle(Properties.Resources.NodeOnSingleBranch);
            var onMultipleBranchesStyle = NetworkLayerFactory.CreatePointStyle(Properties.Resources.NodeOnMultipleBranches);
            return new CategorialTheme
            {
                AttributeName = nameof(INode.IsOnSingleBranch),
                DefaultStyle = onSingleBranchesStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem("Boundary node", onSingleBranchesStyle, onSingleBranchesStyle.Symbol, true),
                    new CategorialThemeItem("Connection node", onMultipleBranchesStyle, onMultipleBranchesStyle.Symbol, false)
                }
            };
        }
    }
}