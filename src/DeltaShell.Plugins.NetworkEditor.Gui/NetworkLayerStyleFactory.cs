using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
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

            if (networkObjects is IEnumerable<IRetention>)
            {
                return CreatePointStyle(Properties.Resources.Retention);
            }

            if (networkObjects is IEnumerable<IObservationPoint>)
            {
                return CreatePointStyle(Properties.Resources.Observation);
            }

            if (networkObjects is IEnumerable<IWeir>)
            {
                return CreatePointStyle(Properties.Resources.WeirSmall);
            }

            if (networkObjects is IEnumerable<ICulvert>)
            {
                return CreatePointStyle(Properties.Resources.CulvertSmall);
            }

            if (networkObjects is IEnumerable<IBridge>)
            {
                return CreatePointStyle(Properties.Resources.BridgeSmall);
            }

            if (networkObjects is IEnumerable<IExtraResistance>)
            {
                return CreatePointStyle(Properties.Resources.ExtraResistanceSmall);
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

            var channels = networkObjects as IEnumerable<IChannel>;
            if (channels != null)
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

            var sewerConnections = networkObjects as IEnumerable<ISewerConnection>;
            if (sewerConnections != null)
            {
                return new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.CadetBlue, 3)
                    {
                        CustomEndCap = new AdjustableArrowCap(5, 5, true)
                        {
                            BaseCap = LineCap.Triangle
                        }
                    },
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
                                                    new CategorialThemeItem("True", onSingleBranchesStyle, onSingleBranchesStyle.Symbol, true),
                                                    new CategorialThemeItem("False", onMultipleBranchesStyle, onMultipleBranchesStyle.Symbol, false)
                                                }
                           };
            }


            var channels = networkObjects as IEnumerable<IChannel>;
            if (channels != null)
            {
                var branchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.SteelBlue, 3)
                               {
                                   CustomEndCap = new AdjustableArrowCap(5, 5, true)
                                                      {
                                                          BaseCap = LineCap.Triangle
                                                      }
                               },
                    EnableOutline = false
                };

                var customBranchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.PowderBlue, 5)
                               {
                                   CustomEndCap = new AdjustableArrowCap(4, 4, true)
                                                      {
                                                          BaseCap = LineCap.Triangle
                                                      }
                               },
                    EnableOutline = false
                };

                return new CategorialTheme
                           {
                               AttributeName = "IsLengthCustom",
                               DefaultStyle = branchStyle,
                               ThemeItems = new EventedList<IThemeItem>
                                                {
                                                    new CategorialThemeItem("True", customBranchStyle, null, true),
                                                    new CategorialThemeItem("False", branchStyle, null, false)
                                                }
                           };
            }

            var manholes = networkObjects as IEnumerable<IManhole>;
            if (manholes != null)
            {
                var onSingleBranchesStyle = new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Shape = ShapeType.Ellipse,
                    Fill = new SolidBrush(Color.Orange),
                    Outline = new Pen(Color.FromArgb(100, Color.DarkSlateGray), 2f),
                };
                var onMultipleBranchesStyle = new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Shape = ShapeType.Ellipse,
                    Fill = new SolidBrush(Color.DarkOrange),
                    Outline = new Pen(Color.FromArgb(100, Color.DimGray), 2f),
                };
                return new CategorialTheme
                {
                    AttributeName = "IsOnSingleBranch",
                    DefaultStyle = onSingleBranchesStyle,
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        new CategorialThemeItem("True", onSingleBranchesStyle, onSingleBranchesStyle.Symbol, true),
                        new CategorialThemeItem("False", onMultipleBranchesStyle, onMultipleBranchesStyle.Symbol, false)
                    }
                };
            }

            var connections = networkObjects as IEnumerable<ISewerConnection>;
            if (connections != null)
            {
                var branchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.DarkSlateGray, 3)
                    {
                        CustomEndCap = new AdjustableArrowCap(5, 5, true)
                        {
                            BaseCap = LineCap.Triangle
                        }
                    },
                    EnableOutline = false
                };

                var customBranchStyle = new VectorStyle
                {
                    GeometryType = typeof(ILineString),
                    Line = new Pen(Color.DimGray, 5)
                    {
                        CustomEndCap = new AdjustableArrowCap(4, 4, true)
                        {
                            BaseCap = LineCap.Triangle
                        }
                    },
                    EnableOutline = false
                };

                return new CategorialTheme
                {
                    AttributeName = "IsLengthCustom",
                    DefaultStyle = branchStyle,
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        new CategorialThemeItem("True", customBranchStyle, null, true),
                        new CategorialThemeItem("False", branchStyle, null, false)
                    }
                };
            }

            var pumps = networkObjects as IEnumerable<IPump>;
            if (pumps != null)
            {
                var pumpPositiveStyle = CreatePointStyle(Properties.Resources.PumpSmallPositive);
                var pumpNegativeStyle = CreatePointStyle(Properties.Resources.PumpSmallNegative);

                return new CategorialTheme
                           {
                               AttributeName = "DirectionIsPositive", 
                               DefaultStyle = pumpNegativeStyle,
                               ThemeItems = new EventedList<IThemeItem>
                                                {
                                                    new CategorialThemeItem("True", pumpPositiveStyle, pumpPositiveStyle.Symbol, true),
                                                    new CategorialThemeItem("False", pumpNegativeStyle, pumpNegativeStyle.Symbol, false)
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