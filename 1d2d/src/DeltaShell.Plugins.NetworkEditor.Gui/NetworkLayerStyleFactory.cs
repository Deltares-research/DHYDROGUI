using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public static class NetworkLayerStyleFactory
    {
        public static VectorStyle CreateStyle(IEnumerable networkObjects, bool alternativeStyle = false)
        {
            switch (networkObjects)
            {
                case IEnumerable<HydroLink> _:
                    return GenerateHydroLinkStyle(alternativeStyle);
                case IEnumerable< WasteWaterTreatmentPlant > _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.wwtp);
                case IEnumerable<RunoffBoundary> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.runoff);
                case IEnumerable<Catchment> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(IPolygon),
                        Fill = new SolidBrush(Color.FromArgb(50, Color.LightSkyBlue)),
                        Outline = new Pen(Color.FromArgb(100, Color.DarkBlue), 2f),
                    };
                case IEnumerable<ILateralSource> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.LateralSourceMap);
                case IEnumerable<IManhole> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Shape = ShapeType.Ellipse,
                        ShapeSize = 11,
                        Fill = new SolidBrush(Color.Orange),
                        Outline = new Pen(Color.FromArgb(255, Color.Black), 1f),
                    };
                case IEnumerable<IRetention> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.Retention);
                case IEnumerable<IObservationPoint> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.Observation);
                case IEnumerable<IPump> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.pump);
                case IEnumerable<IWeir> _:
                {
                    var weirBitmap = networkObjects is IEnumerable<IOrifice> ? Properties.Resources.Gate : Properties.Resources.WeirSmall;
                    return NetworkLayerFactory.CreatePointStyle(weirBitmap);
                }
                case IEnumerable<OutletCompartment> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.Outlet);
                case IEnumerable<ICulvert> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.CulvertSmall);
                case IEnumerable<IBridge> _:
                    return NetworkLayerFactory.CreatePointStyle(Properties.Resources.BridgeSmall);
                case IEnumerable<ICompositeBranchStructure> _:
                    return new VectorStyle
                    {
                        Fill = new SolidBrush(Color.SpringGreen),
                        Line = new Pen(Color.Black, 1),
                        GeometryType = typeof(IPolygon)
                    };
                case IEnumerable<ICrossSection> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(ILineString),
                        Fill = new SolidBrush(Color.Tomato),
                        Line = new Pen(Color.Indigo, 2)
                    };
                case IEnumerable<IChannel> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(ILineString),
                        Line = new Pen(Color.SteelBlue, 3) { CustomEndCap = new AdjustableArrowCap(5, 5, true) { BaseCap = LineCap.Triangle } },
                        EnableOutline = false
                    };
                case IEnumerable<IPipe> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(ILineString),
                        Line = new Pen(Color.Black, 2)
                    };
                case IEnumerable<ISewerConnection> _:
                    return new VectorStyle
                    {
                        GeometryType = typeof(ILineString),
                        Line = new Pen(Color.CadetBlue, 3),
                        EnableOutline = false
                    };
                default:
                    return null;
            }
        }

        private static VectorStyle GenerateHydroLinkStyle(bool alternativeStyle)
        {
            var linkEndCap = new AdjustableArrowCap(5, 5, true) { BaseCap = LineCap.Triangle };
            return new VectorStyle
            {
                GeometryType = typeof(ILineString),
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
    }
}