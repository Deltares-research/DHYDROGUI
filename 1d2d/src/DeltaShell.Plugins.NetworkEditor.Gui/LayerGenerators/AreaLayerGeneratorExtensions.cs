using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Gui.LayerGenerators
{
    internal static class AreaLayerGeneratorExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AreaLayerGeneratorExtensions));
        internal static ILayer GenerateArea2DLayer(this HydroArea area2DParent, object data)
        {
            const double maxVisibilityLayerValue = Double.MaxValue;
            const string modelName = "NetworkEditorModelName";

            switch (data)
            {
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.ObservationPoints):
                    return new VectorLayer(HydroArea.ObservationPointsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.ObservationPointStyle,
                        DataSource = new HydroAreaFeature2DCollection(area2DParent).Init(
                            area2DParent.ObservationPoints, "ObservationPoint_2D_", modelName,
                            area2DParent.CoordinateSystem)
                    };
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.DryPoints):
                    return new VectorLayer(HydroArea.DryPointsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.DryPointStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.DryPoints, "DryPoint",
                                modelName, area2DParent.CoordinateSystem)
                    };
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.DryAreas):
                    {
                        var ds = new HydroAreaFeature2DCollection(area2DParent)
                        {
                            AddNewFeatureFromGeometryDelegate = (provider, geometry) => AddNewPolygonFeature<GroupableFeature2DPolygon>(geometry, provider)
                        }.Init(area2DParent.DryAreas, "DryArea",
                               modelName, area2DParent.CoordinateSystem);

                        return new VectorLayer(HydroArea.DryAreasPluralName)
                        {
                            NameIsReadOnly = true,
                            FeatureEditor = new Feature2DEditor(area2DParent),
                            Style = AreaLayerStyles.DryAreaStyle,
                            DataSource = ds
                        };
                    }
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.Enclosures):
                    {
                        var ds = new HydroAreaFeature2DCollection(area2DParent)
                        {
                            AddNewFeatureFromGeometryDelegate = (provider, geometry) => AddNewPolygonFeature<GroupableFeature2DPolygon>(geometry, provider)
                        }.Init(area2DParent.Enclosures,
                               "Enclosure", modelName, area2DParent.CoordinateSystem);

                        return new VectorLayer(HydroArea.EnclosureName)
                        {
                            NameIsReadOnly = true,
                            FeatureEditor = new Feature2DEditor(area2DParent),
                            Opacity = (float)0.25,
                            Style = AreaLayerStyles.EnclosureStyle,
                            DataSource = ds,
                            CustomRenderers = new List<IFeatureRenderer>(new[] { new EnclosureRenderer() })
                        };
                    }
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.RoofAreas):
                    {
                        var ds = new HydroAreaFeature2DCollection(area2DParent)
                        {
                            AddNewFeatureFromGeometryDelegate = (provider, geometry) => AddNewPolygonFeature<GroupableFeature2DPolygon>(geometry, provider)
                        }.Init(area2DParent.RoofAreas, "RoofAreas", modelName, area2DParent.CoordinateSystem);

                        return new VectorLayer(HydroArea.RoofAreaName)
                        {
                            NameIsReadOnly = true,
                            Style = AreaLayerStyles.RoofAreaStyle,
                            FeatureEditor = new Feature2DEditor(area2DParent),
                            DataSource = ds,
                            CanBeRemovedByUser = true,
                            Selectable = true
                        };
                    }
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.Gullies):
                    {
                        return new VectorLayer(HydroArea.GullyName)
                        {
                            Style = AreaLayerStyles.Gulliestyle,
                            NameIsReadOnly = true,
                            DataSource = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Gullies, "Gullies",
                                                                                             modelName, area2DParent.CoordinateSystem),
                            FeatureEditor = new Feature2DEditor(area2DParent),
                            CanBeRemovedByUser = true,
                            Selectable = true,
                            MaxVisible = maxVisibilityLayerValue
                        };
                    }

                case IEventedList<ObservationCrossSection2D> obsCrossSections2d
                    when Equals(obsCrossSections2d, area2DParent.ObservationCrossSections):
                    return new VectorLayer(HydroArea.ObservationCrossSectionsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.ObsCrossSectionStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(
                                area2DParent.ObservationCrossSections,
                                "ObservationCrossSection_2D_",
                                modelName, area2DParent.CoordinateSystem),
                        CustomRenderers = new[] { new ArrowLineStringAdornerRenderer() }
                    };
                case IEventedList<Pump2D> pumps2d
                    when Equals(pumps2d, area2DParent.Pumps):
                    {
                        var areaFeature2DCollection = new HydroAreaFeature2DCollection(area2DParent).Init(pumps2d,
                            "Pump_2D_", modelName,
                            area2DParent.CoordinateSystem);
                        areaFeature2DCollection.FeatureType =
                            typeof(Pump2D); // Override so we can use FeatureAttributes!
                        return new VectorLayer(HydroArea.PumpsPluralName)
                        {
                            NameIsReadOnly = true,
                            Style = AreaLayerStyles.PumpStyle,
                            DataSource = areaFeature2DCollection,
                            FeatureEditor = new Feature2DEditor(area2DParent)
                            {
                                CreateNewFeature = layer => new Pump2D(true)
                            },
                            CustomRenderers = new[] { new ArrowLineStringAdornerRenderer() },
                            MaxVisible = maxVisibilityLayerValue
                        };
                    }
                case IEventedList<Weir2D> weirs2d
                    when Equals(weirs2d, area2DParent.Weirs):
                    {
                        var feature2DCollection = new HydroAreaFeature2DCollection(area2DParent).Init(weirs2d,
                            "Weir_2D_",
                            modelName,
                            area2DParent.CoordinateSystem);
                        feature2DCollection.FeatureType = typeof(Weir2D); // Override so we can use FeatureAttributes!
                        return new VectorLayer(HydroArea.WeirsPluralName)
                        {
                            NameIsReadOnly = true,
                            Style = AreaLayerStyles.WeirStyle,
                            DataSource = feature2DCollection,
                            FeatureEditor = new Feature2DEditor(area2DParent)
                            {
                                CreateNewFeature = layer =>
                                {
                                    var weir = new Weir2D(true);
                                    weir.CrestWidth = 0.0;
                                    return weir;
                                }
                            },
                            CustomRenderers = new[] { new ArrowLineStringAdornerRenderer() },
                            MaxVisible = maxVisibilityLayerValue
                        };
                    }
                case IEventedList<Gate2D> gates2d
                    when Equals(gates2d, area2DParent.Gates):
                    {
                        var feature2DCollection = new HydroAreaFeature2DCollection(area2DParent).Init(gates2d,
                            "Gate_2D_",
                            modelName,
                            area2DParent.CoordinateSystem);
                        feature2DCollection.FeatureType = typeof(Gate2D); // Override so we can use FeatureAttributes!
                        return new VectorLayer(HydroArea.GatesPluralName)
                        {
                            NameIsReadOnly = true,
                            Style = AreaLayerStyles.GateStyle,
                            DataSource = feature2DCollection,
                            FeatureEditor = new Feature2DEditor(area2DParent)
                            {
                                CreateNewFeature = layer => new Gate2D()
                            },
                            CustomRenderers = new[] { new ArrowLineStringAdornerRenderer() }
                        };
                    }
                case IEventedList<ThinDam2D> thinDams
                    when Equals(thinDams, area2DParent.ThinDams):
                    return new VectorLayer(HydroArea.ThinDamsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.ThinDamStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.ThinDams,
                                "ThinDam_2D_",
                                modelName,
                                area2DParent.CoordinateSystem)
                    };
                case IEventedList<LandBoundary2D> landBoundaries
                    when Equals(landBoundaries, area2DParent.LandBoundaries):
                    return new VectorLayer(HydroArea.LandBoundariesPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.LandBoundaryStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.LandBoundaries,
                                "LandBoundary_2D_", modelName,
                                area2DParent.CoordinateSystem)
                    };
                case IEventedList<Embankment> embankments
                    when Equals(embankments, area2DParent.Embankments):
                    return new VectorLayer(HydroArea.EmbankmentsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor =
                            new HydroAreaFeatureEditor(area2DParent)
                            { CreateNewFeature = l => new Embankment { Region = area2DParent } },
                        Style = AreaLayerStyles.EmbankmentStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Embankments,
                                "Embankment_2D_", modelName,
                                area2DParent.CoordinateSystem),
                        CustomRenderers = new List<IFeatureRenderer>(new[] { new EmbankmentRenderer() })
                    };
                case IEventedList<FixedWeir> fixedWeirs2D
                    when Equals(fixedWeirs2D, area2DParent.FixedWeirs):
                    return new VectorLayer(HydroArea.FixedWeirsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.FixedWeirStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.FixedWeirs,
                                "FixedWeir_2D_", modelName,
                                area2DParent.CoordinateSystem),
                        MaxVisible = maxVisibilityLayerValue
                    };
                case IEventedList<BridgePillar> bridgePillars
                    when Equals(bridgePillars, area2DParent.BridgePillars):
                    return new VectorLayer(HydroArea.BridgePillarsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.BridgePillarStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.BridgePillars,
                                "BridgePillar_2D_", modelName,
                                area2DParent.CoordinateSystem)
                    };
                case IEventedList<Feature2D> damBreaks
                    when Equals(damBreaks, area2DParent.LeveeBreaches):
                    var leveeBreachLayer = new VectorLayer(HydroArea.LeveeBreachName)
                    {
                        NameIsReadOnly = true,
                        CanBeRemovedByUser = true,
                        DataSource = new HydroAreaFeature2DCollection(area2DParent)
                            .Init(area2DParent.LeveeBreaches,
                                  "LeveeBreach_2D_", modelName,
                                  area2DParent.CoordinateSystem),
                        CustomRenderers = new List<IFeatureRenderer>(new[]
                        {
                            new LeveeBreachRenderer(AreaLayerStyles.LeveeStyle, AreaLayerStyles.BreachStyle, AreaLayerStyles.WaterLevelStreamSnappedStyle)
                        })
                    };

                    leveeBreachLayer.FeatureEditor = new HydroAreaFeatureEditor(area2DParent)
                    {
                        CreateNewFeature = layer => new LeveeBreach(),
                        SnapRules = new List<ISnapRule>
                        {
                            new LeveeBreachSnapRule
                            {
                                Criteria = (layer, feature) => feature is Channel
                                                               && layer.DataSource is HydroNetworkFeatureCollection featureCollection
                                                               && featureCollection.Network == ((HydroNetworkFeatureCollection)leveeBreachLayer.DataSource).Network,
                                SnapRole = SnapRole.FreeAtObject,
                                NewFeatureLayer = leveeBreachLayer,
                                Obligatory = true,
                                PixelGravity = 40
                            }
                        }
                    };

                    return leveeBreachLayer;
                default:
                    return null;
            }
        }

        private static IFeature AddNewPolygonFeature<TFeature>(IGeometry geometry, IFeatureProvider ds) where TFeature : IFeature, new()
        {
            var newFeature = CreatePolygonFeatureFromLineString<TFeature>(geometry);
            if (newFeature == null)
            {
                return null;
            }

            ds.Features.Add(newFeature);

            return newFeature;
        }

        private static IFeature CreatePolygonFeatureFromLineString<TFeature>(IGeometry geometry) where TFeature: IFeature, new()
        {
            if (geometry is IPolygon)
            {
                return new TFeature { Geometry = geometry };
            }

            var coordinates = geometry.Coordinates;
            if (!coordinates[0].Equals2D(coordinates[coordinates.Length -1]))
            {
                coordinates = coordinates.Plus(coordinates[0]).ToArray();
            }
            
            if (coordinates.Length <= 3)
            {
                log.Warn(Resources.Polygon_drawn_with_3_or_less_points_but_a_valid_polygon_needs_at_least_3_points);
                return null;
            }

            return new TFeature
            {
                Geometry = new Polygon(new LinearRing(coordinates))
            };
        }
    }
}