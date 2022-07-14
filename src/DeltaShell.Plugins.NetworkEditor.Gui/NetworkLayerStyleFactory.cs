using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public static class NetworkLayerStyleFactory
    {
        public static VectorStyle CreateStyle(IEnumerable networkObjects, bool alternativeStyle=false)
        {
            if (networkObjects is IEventedList<HydroLink>)
            {
                var linkEndCap = new AdjustableArrowCap(5, 5, true) { BaseCap = LineCap.Triangle };
                return new VectorStyle
                    {
                        GeometryType = typeof (ILineString),
                        EnableOutline = false,
                        Line = new Pen(new SolidBrush(Color.FromArgb(80,
                                                              alternativeStyle
                                                                  ? Color.FromArgb(80, Color.DarkCyan)
                                                                  : Color.Chocolate)), 4f)
                                {
                                    CustomEndCap = linkEndCap,
                                    DashStyle = DashStyle.Dash
                                }
                    };
            }

            if (networkObjects is IEventedList<WasteWaterTreatmentPlant>)
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (IPoint),
                               Symbol = (Properties.Resources.wwtp)
                           };
            }

            if (networkObjects is IEventedList<RunoffBoundary>)
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (IPoint),
                               Symbol = (Properties.Resources.runoff)
                           };
            }

            if (networkObjects is IEventedList<Catchment>)
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (IPolygon),
                               Fill = new SolidBrush(Color.FromArgb(50, Color.LightSkyBlue)),
                               Outline = new Pen(Color.FromArgb(100, Color.DarkBlue), 2f),
                           };
            }

            if (networkObjects is IEnumerable<ILateralSource>)
            {
                return CreatePointStyle(Properties.Resources.LateralSourceMap);
            }

            if (networkObjects is IEnumerable<IManhole>)
            {
                return new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Shape = ShapeType.Ellipse,
                    ShapeSize = 11,
                    Fill = new SolidBrush(Color.Orange),
                    Outline = new Pen(Color.FromArgb(255, Color.Black), 1f),
                };
            }
            
            if (networkObjects is IEnumerable<IRetention>)
            {
                return CreatePointStyle(Properties.Resources.Retention);
            }

            if (networkObjects is IEnumerable<IObservationPoint>)
            {
                return CreatePointStyle(Properties.Resources.Observation);
            }

            if (networkObjects is IEnumerable<IPump>)
            {
                return CreatePointStyle(Properties.Resources.pump);
            }

            if (networkObjects is IEnumerable<IWeir>)
            {
                var weirBitmap = networkObjects is IEnumerable<Orifice> ? Properties.Resources.Gate : Properties.Resources.WeirSmall;
                return CreatePointStyle(weirBitmap);
            }

            if (networkObjects is IEnumerable<OutletCompartment>)
            {
                return CreatePointStyle(Properties.Resources.Outlet);
            }

            if (networkObjects is IEnumerable<ICulvert>)
            {
                return CreatePointStyle(Properties.Resources.CulvertSmall);
            }

            if (networkObjects is IEnumerable<IBridge>)
            {
                return CreatePointStyle(Properties.Resources.BridgeSmall);
            }

            if (networkObjects is IEnumerable<ICompositeBranchStructure>)
            {
                return new VectorStyle
                    {
                        Fill = new SolidBrush(Color.SpringGreen),
                        Line = new Pen(Color.Black, 1),
                        GeometryType = typeof (IPolygon)
                    };
            }

            if (networkObjects is IEnumerable<ICrossSection>)
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (ILineString),
                               Fill = new SolidBrush(Color.Tomato),
                               Line = new Pen(Color.Indigo, 2)
                           };
            }

            if (networkObjects is IEnumerable<IChannel>)
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (ILineString),
                               Line = new Pen(Color.SteelBlue, 3)
                                          {
                                              CustomEndCap = new AdjustableArrowCap(5, 5, true)
                                                                 {
                                                                     BaseCap = LineCap.Triangle
                                                                 }
                                          },
                               EnableOutline = false
                           };
            }

            if (networkObjects is IEnumerable<IPipe>)
            {
                return new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.Black, 2)
                };
            }

            if (networkObjects is IEnumerable<ISewerConnection>)
            {
                return new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.CadetBlue, 3),
                    EnableOutline = false
                };
            }

            return null;
        }

        public static ITheme CreateTheme(IEnumerable networkObjects)
        {
            var nodes = networkObjects as IEnumerable<IHydroNode>;
            if (nodes != null)
            {
                var onSingleBranchesStyle = CreatePointStyle(Properties.Resources.NodeOnSingleBranch);
                var onMultipleBranchesStyle = CreatePointStyle(Properties.Resources.NodeOnMultipleBranches);
                return new CategorialTheme
                           {
                               AttributeName = "IsOnSingleBranch",
                               DefaultStyle = onSingleBranchesStyle,
                               ThemeItems = new EventedList<IThemeItem>
                                                {
                                                    new CategorialThemeItem("Boundary node", onSingleBranchesStyle, onSingleBranchesStyle.Symbol, true),
                                                    new CategorialThemeItem("Connection node", onMultipleBranchesStyle, onMultipleBranchesStyle.Symbol, false)
                                                }
                           };
            }


            if (networkObjects is IEnumerable<IChannel>)
            {
                var branchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.FromArgb(255,0,0,128), 3)
                               {
                                   CustomEndCap = new AdjustableArrowCap(5, 5, true)
                                                      {
                                                          BaseCap = LineCap.Triangle
                                                      }
                               },
                    EnableOutline = false
                };

                return new CategorialTheme
                           {
                               AttributeName = "IsLengthCustom",
                               DefaultStyle = branchStyle
                           };
            }

            const int lineWidth = 3;
            if (networkObjects is IEnumerable<IPipe>)
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
                    AttributeName = "WaterType",
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

            if (networkObjects is IEnumerable<ISewerConnection>)
            {
                var branchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.Pink, lineWidth),
                };

                var pumpStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.Red, lineWidth)
                    {
                        DashStyle = DashStyle.Dash
                    },
                };

                var weirStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.LimeGreen, lineWidth)
                    {
                        DashStyle = DashStyle.Dash
                    },
                };
                
                return new CategorialTheme
                {
                    AttributeName = nameof(SewerConnection.SpecialConnectionType),
                    DefaultStyle = branchStyle,
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        new CategorialThemeItem(SewerConnectionSpecialConnectionType.Pump.GetDescription(), pumpStyle, null, SewerConnectionSpecialConnectionType.Pump),
                        new CategorialThemeItem(SewerConnectionSpecialConnectionType.Weir.GetDescription(), weirStyle, null, SewerConnectionSpecialConnectionType.Weir),
                        new CategorialThemeItem(SewerConnectionSpecialConnectionType.None.GetDescription(), branchStyle, null, SewerConnectionSpecialConnectionType.None),
                    }
                };
            }

            return null;
        }

        private static VectorStyle CreatePointStyle(Bitmap bitmap)
        {
            return new VectorStyle
                       {
                           GeometryType = typeof (IPoint),
                           Symbol = bitmap
                       };
        }
    }
}