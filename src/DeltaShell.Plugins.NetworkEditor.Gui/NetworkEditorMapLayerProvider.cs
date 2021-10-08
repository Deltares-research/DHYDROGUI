using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public class NetworkEditorMapLayerProvider : IMapLayerProvider
    {
        private const double MaxVisibilityLayerValue = Double.MaxValue;

        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is IDrainageBasin
                   || data is HydroNetwork
                   || data is HydroArea
                   || data is HydroRegion
                   || data is IEventedList<HydroLink>
                   || data is IEventedList<WasteWaterTreatmentPlant>
                   || data is IEventedList<RunoffBoundary>
                   || data is IEventedList<Catchment>
                   || data is IEnumerable<IHydroNode>
                   || data is IEnumerable<IChannel>
                   || data is IEnumerable<IManhole>
                   || data is IEnumerable<OutletCompartment>
                   || data is IEnumerable<Compartment>
                   || data is IEnumerable<IOrifice>
                   || data is IEnumerable<IPipe>
                   || data is IEnumerable<ISewerConnection>
                   || (data is IEventedList<Pump2D> && parentObject is HydroArea)
                   || (data is IEventedList<Weir2D> && parentObject is HydroArea)
                   || (data is IEventedList<Gate2D> && parentObject is HydroArea)
                   || data is IEnumerable<IPump>
                   || data is IEnumerable<ILateralSource>
                   || data is IEnumerable<IRetention>
                   || data is IEnumerable<IObservationPoint>
                   || data is IEnumerable<IWeir>
                   || data is IEnumerable<ICulvert>
                   || data is IEnumerable<IBridge>
                   || data is IEnumerable<IExtraResistance>
                   || data is IEnumerable<ICompositeBranchStructure>
                   || data is IEnumerable<ICrossSection>
                   || data is IEventedList<Route>
                   || data is IEventedList<Feature2D> //area2d features & boundaries
                   || data is IEventedList<GroupableFeature2D> //area2d features & boundaries
                   || data is IEventedList<LandBoundary2D>
                   || data is IEventedList<ThinDam2D>
                   || data is IEventedList<ObservationCrossSection2D>
                   || (data is IEventedList<Feature2DPoint> && parentObject is HydroArea) //obs points
                   || (data is IEventedList<GroupableFeature2DPoint> && parentObject is HydroArea) //obs points
                   || (data is IEventedList<GroupableFeature2DPolygon> &&
                       parentObject is HydroArea) // dry area & enclosures & roof areas
                   || (data is IEventedList<GroupablePointFeature> && parentObject is HydroArea) // dry points, 
                   || (data is IEventedList<FixedWeir> && parentObject is HydroArea) //fixed weirs
                   || (data is IEventedList<Embankment> && parentObject is HydroArea)
                   || (data is IEventedList<BridgePillar> && parentObject is HydroArea)
                   || (data is IEventedList<Gully> && parentObject is HydroArea)//gullies
                   || data is IEventedList<RoughnessSection>
                   || data is RoughnessSection;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var hydroRegion = data as IHydroRegion;
            if (hydroRegion != null)
            {
                foreach (var subRegion in hydroRegion.SubRegions)
                {
                    yield return subRegion;
                }

                if (hydroRegion is HydroRegion)
                {
                    // Two layers are created for HydroLinks: one is Basin and one in the HydroRegion itself. This is the latter. 
                    yield return hydroRegion.Links; 
                }

                var network = hydroRegion as IHydroNetwork;
                if (network != null)
                {
                    // The order below is also the order in which the layers will be stacked (layer order)
                    yield return network.HydroNodes;
                    yield return network.Channels;
                    yield return network.Bridges;
                    yield return network.CompositeBranchStructures;
                    yield return network.CrossSections;
                    yield return network.Culverts;
                    yield return network.ExtraResistances;
                    yield return network.Gates;
                    yield return network.LateralSources;
                    yield return network.Compartments;
                    yield return network.Manholes;
                    yield return network.ObservationPoints;
                    yield return network.Orifices;
                    yield return network.OutletCompartments;
                    yield return network.Pipes;
                    yield return network.Pumps;
                    yield return network.Retentions;
                    yield return network.Routes;
                    yield return network.SewerConnections;
                    yield return network.Weirs;
                }

                if (hydroRegion is IDrainageBasin drainageBasin)
                {
                    yield return drainageBasin.Boundaries;
                    yield return drainageBasin.Catchments;
                    yield return drainageBasin.Links;
                    yield return drainageBasin.WasteWaterTreatmentPlants;
                }

                var area2D = hydroRegion as HydroArea;
                if (area2D != null)
                {
                    yield return area2D.BridgePillars;
                    yield return area2D.DryPoints;
                    yield return area2D.DryAreas;
                    yield return area2D.Embankments;
                    yield return area2D.Enclosures;
                    yield return area2D.FixedWeirs;
                    yield return area2D.Gates;
                    yield return area2D.Gullies;
                    yield return area2D.LandBoundaries;
                    yield return area2D.LeveeBreaches;
                    yield return area2D.ObservationCrossSections;
                    yield return area2D.ObservationPoints;
                    yield return area2D.Pumps;
                    yield return area2D.RoofAreas;
                    yield return area2D.ThinDams;
                    yield return area2D.Weirs;
                }
            }

            var routes = data as IEventedList<Route>;
            if (routes != null)
            {
                foreach (var route in routes)
                {
                    yield return route;
                }
            }
            var modelWithRoughnessSections = data as IModelWithRoughnessSections;
            if (modelWithRoughnessSections != null)
            {
                yield return modelWithRoughnessSections.RoughnessSections;
            }
            var roughnessSections = data as IEventedList<RoughnessSection>;
            if (roughnessSections != null)
            {
                foreach (var roughnessSection in roughnessSections)
                {
                    var reverseRoughnessSection = roughnessSection as ReverseRoughnessSection;
                    if (reverseRoughnessSection != null && reverseRoughnessSection.UseNormalRoughness)
                    {
                        continue;
                    }

                    yield return roughnessSection;
                }
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            if (!(layer is IGroupLayer groupLayer)) return;

            var objectsInRenderOrder = new List<object>();

            if (layerObject is IHydroNetwork network)
            {
                objectsInRenderOrder.AddRange(new object[]
                {
                    network.ObservationPoints,
                    network.LateralSources,
                    network.Bridges,
                    network.Orifices,
                    network.Weirs,
                    network.Pumps,
                    network.ExtraResistances,
                    network.Culverts,
                    network.Gates,

                    network.CrossSections,
                    network.OutletCompartments,
                    network.CompositeBranchStructures,
                    network.Compartments,

                    network.SewerConnections,
                    network.Manholes,
                    network.HydroNodes,
                    network.Retentions,
                    network.Routes,

                    network.Pipes,
                    network.Channels
                });
            }

            if (layerObject is HydroArea area)
            {
                objectsInRenderOrder.AddRange(new object[]
                {
                    area.BridgePillars,
                    area.DryPoints,
                    area.DryAreas,

                    area.Embankments,
                    area.Enclosures,
                    area.FixedWeirs,
                    area.Gates,
                    area.Gullies,
                    area.LandBoundaries,
                    area.LeveeBreaches,
                    area.ObservationCrossSections,
                    area.ObservationPoints,
                    area.Pumps,
                    area.RoofAreas,
                    area.ThinDams,
                    area.Weirs
                });
            }

            if (objectsInRenderOrder.Count == 0) return;
            groupLayer.SetRenderOrderByObjectOrder(objectsInRenderOrder, objectsLookup);
        }

        public ILayer CreateLayer(object data, object parentData)
        {
            var area2D = data as HydroArea;
            if (area2D != null)
            {
                return new AreaLayer { HydroArea = area2D, NameIsReadOnly = true };
            }

            if (data is IHydroRegion region)
            {
                return new HydroRegionMapLayer { Name = region.Name, Region = region, LayersReadOnly = true };
            }

            if (parentData is IHydroNetwork hydroNetwork)
            {
                return GenerateHydroNetworkLayer(hydroNetwork, data);
            }

            if (parentData is IHydroRegion hydroRegion 
                && parentData as HydroArea == null)
            {
                return GenerateDrainageBasinLayer(hydroRegion, data);
            }

            
            if (parentData is HydroArea area2DParent)
            {
                return GenerateArea2DLayer(area2DParent, data);
            }

            var roughnessSection = data as RoughnessSection;
            if (roughnessSection != null)
            {
                var coverageLayer = SharpMapLayerFactory.CreateMapLayerForCoverage(roughnessSection.RoughnessNetworkCoverage, null);
                coverageLayer.Visible = false;
                return coverageLayer;
            }

            if (data is IEventedList<RoughnessSection>)
            {
                return new GroupLayer("Lanes")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }
            return null;
        }

        private static ILayer GenerateArea2DLayer(HydroArea area2DParent, object data)
        {
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
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.DryAreas, "DryArea",
                        modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        if (!(geometry is IPolygon))
                        {
                            if (geometry.Coordinates.Count() < 4) return null;
                            geometry = new Polygon(new LinearRing(geometry.Coordinates));
                        }

                        var newFeature = new GroupableFeature2DPolygon {Geometry = geometry};
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };

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
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Enclosures,
                        "Enclosure", modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        if (!(geometry is IPolygon))
                        {
                            if (geometry.Coordinates.Count() < 4) return null;
                            geometry = new Polygon(new LinearRing(geometry.Coordinates));
                        }

                        var newFeature = new GroupableFeature2DPolygon() {Geometry = geometry};
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };

                    return new VectorLayer(HydroArea.EnclosureName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Opacity = (float) 0.25,
                        Style = AreaLayerStyles.EnclosureStyle,
                        DataSource = ds,
                        CustomRenderers = new List<IFeatureRenderer>(new[] {new EnclosureRenderer()})
                    };
                }
                case IEnumerable<IFeature> features
                    when Equals(features, area2DParent.RoofAreas):
                {
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.RoofAreas,
                        "RoofAreas", modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        if (!(geometry is IPolygon))
                        {
                            var coordinates = geometry.Coordinates.ToList();
                            if (coordinates.Count < 3) return null;
                            if (!coordinates.First().Equals(coordinates.Last()))
                            {
                                coordinates.Add(coordinates.First());
                            }

                            geometry = new Polygon(new LinearRing(coordinates.ToArray()));
                        }

                        var newFeature = new GroupableFeature2DPolygon {Geometry = geometry};
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };

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
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Gullies, "Gullies",
                        modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        var newFeature = new Gully() {Geometry = geometry};
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };
                    return new VectorLayer(HydroArea.GullyName)
                    {
                        Style = AreaLayerStyles.Gulliestyle,
                        NameIsReadOnly = true,
                        DataSource = ds,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        CanBeRemovedByUser = true,
                        Selectable = true,
                        MaxVisible = MaxVisibilityLayerValue
                    };
                    ;
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
                        CustomRenderers = new[] {new ArrowLineStringAdornerRenderer()}
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
                        CustomRenderers = new[] {new ArrowLineStringAdornerRenderer()},
                        MaxVisible = MaxVisibilityLayerValue
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
                        CustomRenderers = new[] {new ArrowLineStringAdornerRenderer()},
                        MaxVisible = MaxVisibilityLayerValue
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
                        CustomRenderers = new[] {new ArrowLineStringAdornerRenderer()}
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
                                {CreateNewFeature = l => new Embankment {Region = area2DParent}},
                        Style = AreaLayerStyles.EmbankmentStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Embankments,
                                "Embankment_2D_", modelName,
                                area2DParent.CoordinateSystem),
                        CustomRenderers = new List<IFeatureRenderer>(new[] {new EmbankmentRenderer()})
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
                        MaxVisible = MaxVisibilityLayerValue
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
                        FeatureEditor = new HydroAreaFeatureEditor(area2DParent) { CreateNewFeature = layer => new LeveeBreach()},
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.LeveeBreaches,
                                "LeveeBreach_2D_", modelName,
                                area2DParent.CoordinateSystem),
                        
                        CustomRenderers = new List<IFeatureRenderer>(new[]
                            {new LeveeBreachRenderer(AreaLayerStyles.LeveeStyle, AreaLayerStyles.BreachStyle, AreaLayerStyles.WaterLevelStreamSnappedStyle)})
                    };
                    return (VectorLayer)AddSnappingRulesToLayer<ILeveeBreach>(leveeBreachLayer);
                default:
                    return null;
            }
        }

        private static ILayer GenerateDrainageBasinLayer(IHydroRegion hydroRegion, object data)
        {
            var drainageBasin = hydroRegion as IDrainageBasin;
            switch (data)
            {
                case IEventedList<HydroLink> links:
                    return new VectorLayer("Links " + hydroRegion.Name)
                    {
                        Visible = true,
                        Style = NetworkLayerStyleFactory.CreateStyle(links, drainageBasin != null),
                        NameIsReadOnly = true,
                        DataSource = new ComplexFeatureCollection(hydroRegion, (IList)hydroRegion.Links, typeof(HydroLink)),
                        FeatureEditor = new HydroLinkFeatureEditor
                        {
                            SnapRules = { new HydroLinkSnapRule { Obligatory = true, PixelGravity = 40 } },
                            Region = hydroRegion
                        }
                    };
                case IEventedList<WasteWaterTreatmentPlant> wasteWaterTreatmentPlants when drainageBasin != null:
                    return new VectorLayer("Wastewater Treatment Plants")
                    {
                        Style = NetworkLayerStyleFactory.CreateStyle(wasteWaterTreatmentPlants),
                        NameIsReadOnly = true,
                        DataSource = new ComplexFeatureCollection(drainageBasin, (IList) wasteWaterTreatmentPlants, typeof(WasteWaterTreatmentPlant)),
                        FeatureEditor = new WasteWaterTreatmentPlantFeatureEditor {DrainageBasin = drainageBasin}
                    };
                case IEventedList<RunoffBoundary> runoffBoundaries when drainageBasin != null:
                    return new VectorLayer("Runoff Boundaries")
                    {
                        Style = NetworkLayerStyleFactory.CreateStyle(runoffBoundaries),
                        NameIsReadOnly = true,
                        DataSource = new ComplexFeatureCollection(drainageBasin, (IList) runoffBoundaries, typeof(RunoffBoundary)),
                        FeatureEditor = new RunoffBoundaryFeatureEditor {DrainageBasin = drainageBasin}
                    };
                case IEventedList<Catchment> catchments when drainageBasin != null:
                {
                    var flattenedCatchments = catchments.Select(c => c).Concat(catchments.Flatten(c => c.SubCatchments));

                    var centerLayers = new VectorLayer
                    {
                        Name = "Catchments (centers)",
                        DataSource =
                            new ComplexFeatureCollection(drainageBasin,
                                new WrappedEnumerableList<Catchment>(flattenedCatchments, catchments),
                                typeof(Catchment)),
                        FeatureEditor = new CatchmentFeatureEditor(true) {DrainageBasin = drainageBasin },
                        CustomRenderers = {new CatchmentAnchorPointRenderer()},
                        NameIsReadOnly = true
                    };
                    var catchmentLayer = new VectorLayer
                    {
                        Name = "Catchments (Polygons)",
                        Style = NetworkLayerStyleFactory.CreateStyle(catchments),
                        DataSource =
                            new ComplexFeatureCollection(drainageBasin,
                                (IList) catchments, typeof(Catchment))
                            {
                                CoordinateSystem = drainageBasin.CoordinateSystem
                            },
                        FeatureEditor = new CatchmentFeatureEditor {DrainageBasin = drainageBasin},
                        NameIsReadOnly = true
                    };

                    var groupLayer = new GroupLayer("Catchments") {NameIsReadOnly = true};
                    groupLayer.Layers.AddRange(new[] {centerLayers, catchmentLayer});
                    groupLayer.LayersReadOnly = true;

                    return groupLayer;
                }
                default:
                    return null;
            }
        }

        private static ILayer GenerateHydroNetworkLayer(IHydroNetwork hydroNetwork, object data)
        {
            switch (data)
            {
                case IEnumerable<IManhole> manholeNodes:
                    return CreateNetworkVectorLayer<Manhole>(manholeNodes, "Manholes", hydroNetwork,MaxVisibilityLayerValue);
                case IEnumerable<OutletCompartment> outletCompartments:
                    return CreateNetworkVectorLayer<OutletCompartment>(outletCompartments, "Outlets",
                        hydroNetwork, MaxVisibilityLayerValue);
                case IEnumerable<Compartment> compartments:
                    var compartmentLayer = CreateNetworkVectorLayer<Compartment>(compartments, "Compartments", hydroNetwork,
                        MaxVisibilityLayerValue);
                    return compartmentLayer;
                case IEnumerable<IPipe> pipes:
                    return CreateNetworkVectorLayer<Pipe>(pipes, "Pipes", hydroNetwork);
                case IEnumerable<ISewerConnection> sewerConnections:
                    return CreateNetworkVectorLayer<SewerConnection>(sewerConnections, "Sewer Connections",
                        hydroNetwork, refreshForChangedItem:o => !(o is IPipe) && o is SewerConnection sewerConnection && sewerConnection.BranchFeatures.Any() );
                case IEnumerable<IHydroNode> hydroNodes:
                    return CreateNetworkVectorLayer<HydroNode>(hydroNodes, "Nodes", hydroNetwork);
                case IEnumerable<IChannel> channels:
                    return CreateNetworkVectorLayer<Channel>(channels, "Branches", hydroNetwork);
                case IEnumerable<IPump> pumps:
                    return CreateNetworkVectorLayer<Pump>(pumps, "Pumps", hydroNetwork,
                         refreshForChangedItem:o => o is Channel channel && channel.Pumps.Any());
                case IEnumerable<ILateralSource> lateralSources:
                    return CreateNetworkVectorLayer<LateralSource>(lateralSources, "Lateral Sources", hydroNetwork,
                        refreshForChangedItem: o =>
                            o is Channel channel &&
                            channel.BranchFeatures.OfType<LateralSource>()
                            .Any());
                case IEnumerable<IRetention> retentions:
                    return CreateNetworkVectorLayer<Retention>(retentions, "Retentions", hydroNetwork,
                        refreshForChangedItem: o =>
                            o is Channel channel &&
                            channel.BranchFeatures.OfType<Retention>().Any());
                case IEnumerable<IObservationPoint> observationPoints:
                    return CreateNetworkVectorLayer<ObservationPoint>(observationPoints, "Observation Points",
                        hydroNetwork,
                        refreshForChangedItem: o =>
                            o is Channel channel &&
                            channel.ObservationPoints.Any());
                case IEnumerable<IOrifice> orifices:
                    return CreateNetworkVectorLayer<IOrifice>(orifices, "Orifices", hydroNetwork,
                        MaxVisibilityLayerValue, o => o is Channel channel && channel.Weirs.OfType<IOrifice>().Any());
                case IEnumerable<IWeir> weirs:
                    return CreateNetworkVectorLayer<Weir>(weirs, "Weirs", hydroNetwork, MaxVisibilityLayerValue,
                        o => o is Channel channel && channel.Weirs.OfType<Weir>().Any());
                case IEnumerable<IGate> gates:
                    return CreateNetworkVectorLayer<Gate>(gates, "Gates", hydroNetwork,
                        refreshForChangedItem: o => o is Channel channel && channel.Gates.Any());
                case IEnumerable<ICulvert> culverts:
                    return CreateNetworkVectorLayer<Culvert>(culverts, "Culverts", hydroNetwork,
                        refreshForChangedItem: o => o is Channel channel && channel.Culverts.Any());
                case IEnumerable<IBridge> bridges:
                    return CreateNetworkVectorLayer<Bridge>(bridges, "Bridges", hydroNetwork,
                        refreshForChangedItem: o => o is Channel channel && channel.Bridges.Any());
                case IEnumerable<IExtraResistance> extraResistances:
                    return CreateNetworkVectorLayer<ExtraResistance>(extraResistances, "Extra Resistances", hydroNetwork,
                        refreshForChangedItem: o =>
                            o is Channel channel &&
                            channel.BranchFeatures
                            .OfType<ExtraResistance>().Any());
                case IEnumerable<ICompositeBranchStructure> compositeBranchStructures:
                    return CreateNetworkVectorLayer<CompositeBranchStructure>(compositeBranchStructures,
                        "Compound Structures",
                        hydroNetwork,
                        refreshForChangedItem: o =>
                            o is Channel channel &&
                            channel.CrossSections.Any());
                case IEnumerable<ICrossSection> crossSections:
                    return CreateNetworkVectorLayer<CrossSection>(crossSections, "Cross Sections", hydroNetwork,
                        refreshForChangedItem: o => o is Channel channel && channel.CrossSections.Any());
                case IEnumerable<Route> routes:
                    return new GroupLayer("Routes")
                        {
                            Selectable = false,
                            NameIsReadOnly = true,
                            LayersReadOnly = true
                        };
                default:
                    return null;
            }
        }

        private static VectorLayer CreateNetworkVectorLayer<TFeature>(IEnumerable<IFeature> networkItems, string name, IHydroNetwork hydroNetwork, double maxVisible = double.MaxValue, Func<object, bool> refreshForChangedItem = null)
        {
            var layer = new VectorLayer(name)
                            {
                                Style = NetworkLayerStyleFactory.CreateStyle(networkItems),
                                Theme = NetworkLayerStyleFactory.CreateTheme(networkItems),
                                NameIsReadOnly = true,
                                MaxVisible = maxVisible,
                                DataSource = new HydroNetworkFeatureCollection
                                                 {
                                                     FeatureType = typeof (TFeature),
                                                     Network = hydroNetwork,
                                                     RefreshForChangedItem = refreshForChangedItem,
                                                     CoordinateSystem = hydroNetwork.CoordinateSystem
                                                 },
                                FeatureEditor = new HydroNetworkFeatureEditor(hydroNetwork)
                                                    {
                                                        CreateNewFeature = NewNetworkFeature<TFeature>()
                                                    },
                                CustomRenderers = GetCustomRenderer<TFeature>()
                            };
            return (VectorLayer)AddSnappingRulesToLayer<TFeature>(layer);
        }

        private static ILayer AddSnappingRulesToLayer<TFeature>(ILayer layer)
        {
            var snapRules = GetSnapRule<TFeature>(layer);
            if (snapRules != null)
            {
                foreach (var snapRule in snapRules)
                {
                    layer.FeatureEditor.SnapRules.Add(snapRule);
                }
            }

            return layer;
        }

        private static IEnumerable<ISnapRule> GetSnapRule<T>(ILayer vectorLayer)
        {
            var type = typeof(T);
            if (type == typeof (Channel))
            {
                return new List<ISnapRule>
                           {
                               new BranchSnapRule
                                   {
                                       Criteria = (layer, feature) => feature is IHydroNode && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.AllTrackers,
                                       Obligatory = false,
                                       PixelGravity = 40
                                   }
                           };
            }

            if (type == typeof(Pipe))
            {
                return new List<ISnapRule>
                {
                    new BranchSnapRule
                    {
                        Criteria = (layer, feature) => feature is IManhole && layer.DataSource is HydroNetworkFeatureCollection &&
                                                       ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                       ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                        NewFeatureLayer = vectorLayer,
                        SnapRole = SnapRole.AllTrackers,
                        Obligatory = false,
                        PixelGravity = 40
                    },
                    new BranchSnapRule
                    {
                        Criteria = (layer, feature) => feature is IHydroNode && layer.DataSource is HydroNetworkFeatureCollection &&
                                                       ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                       ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                        NewFeatureLayer = vectorLayer,
                        SnapRole = SnapRole.AllTrackers,
                        Obligatory = false,
                        PixelGravity = 40
                    },
                    new BranchSnapRule
                    {
                    Criteria = (layer, feature) => feature is Compartment && layer.DataSource is HydroNetworkFeatureCollection &&
                    ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                    ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                    NewFeatureLayer = vectorLayer,
                    SnapRole = SnapRole.AllTrackers,
                    Obligatory = false,
                    PixelGravity = 40
                }
                };
            }
            if (type == typeof(SewerConnection) && type != typeof(Pipe))
            {
                return new List<ISnapRule>
                {
                    new BranchSnapRule
                    {
                        Criteria = (layer, feature) => feature is IManhole && layer.DataSource is HydroNetworkFeatureCollection &&
                                                       ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                       ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                        NewFeatureLayer = vectorLayer,
                        SnapRole = SnapRole.AllTrackers,
                        Obligatory = false,
                        PixelGravity = 40
                    },
                    new BranchSnapRule
                    {
                    Criteria = (layer, feature) => feature is Compartment && layer.DataSource is HydroNetworkFeatureCollection &&
                    ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                    ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                    NewFeatureLayer = vectorLayer,
                    SnapRole = SnapRole.AllTrackers,
                    Obligatory = false,
                    PixelGravity = 40
                }
                };
            }

            if (type == typeof (CrossSection))
            {
                return new List<ISnapRule>
                           {
                               new CrossSectionSnapRule
                                   {
                                       Criteria = (layer, feature) => feature is Channel && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }

            if (type == typeof (HydroNode))
            {
                return new List<ISnapRule>
                           {
                               new HydroNodeSnapRule
                                   {
                                       Criteria = (layer, feature) => feature is Channel && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }

            if (type == typeof(Manhole))
            {
                return new List<ISnapRule>
                {
                    new HydroNodeSnapRule
                    {
                        Criteria = (layer, feature) => feature is Pipe && layer.DataSource is HydroNetworkFeatureCollection &&
                                                       ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                       ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                        NewFeatureLayer = vectorLayer,
                        SnapRole = SnapRole.FreeAtObject,
                        Obligatory = true,
                        PixelGravity = 40
                    }
                };
            }
            

            if (type == typeof(LateralSource))
            {
                return new List<ISnapRule>
                           {
                               new SnapRule
                                   {
                                       Criteria = (layer, feature) => (feature is Channel || feature is INode) && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }

            if (type == typeof(CompositeBranchStructure) || type == typeof(Retention) || type == typeof(ObservationPoint))
            {
                return new List<ISnapRule>
                           {
                               new SnapRule
                                   {
                                       Criteria = (layer, feature) => feature is Channel && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }

            if (type == typeof(Culvert) || 
                type == typeof(Bridge) || 
                type == typeof(ExtraResistance))
            {
                return new List<ISnapRule>
                           {
                               new StructureSnapRule
                                   {
                                       // StructureSnapRule needs all structure layer for the custom geometry
                                       NewFeatureLayer = vectorLayer,
                                       PixelGravity = 40
                                   },
                               new SnapRule
                                   {
                                       Criteria = (layer, feature) => feature is Channel && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }
            if (type == typeof(Weir) || 
                type == typeof(IOrifice) ||
                type == typeof(Pump))
            {
                return new List<ISnapRule>
                           {
                               new StructureSnapRule
                                   {
                                       // StructureSnapRule needs all structure layer for the custom geometry
                                       NewFeatureLayer = vectorLayer,
                                       PixelGravity = 40
                                   },
                               new SnapRule
                               {
                                   Criteria = (layer, feature) => feature is Channel && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                  ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                  ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                   NewFeatureLayer = vectorLayer,
                                   SnapRole = SnapRole.FreeAtObject,
                                   Obligatory = true,
                                   PixelGravity = 40
                               },
                               new SnapRule
                                   {
                                       Criteria = (layer, feature) => feature is SewerConnection && !(feature is IPipe) && layer.DataSource is HydroNetworkFeatureCollection &&
                                                                      ((HydroNetworkFeatureCollection) layer.DataSource).Network ==
                                                                      ((HydroNetworkFeatureCollection) vectorLayer.DataSource).Network,
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }
            if (type == typeof(ILeveeBreach))
            {
                return new List<ISnapRule>
                           {
                               new LeveeBreachSnapRule
                                   {
                                       /*Criteria = (layer, feature) => feature is LeveeBreach leveeBreach && 
                                                                      GeometryHelper.PointIsOnLineBetweenPreviousAndNext(
                                                                          leveeBreach.Geometry.Coordinates.First(),
                                                                          leveeBreach.BreachLocation.Coordinate, 
                                                                          leveeBreach.Geometry.Coordinates.Last()),*/
                                       NewFeatureLayer = vectorLayer,
                                       SnapRole = SnapRole.FreeAtObject,
                                       Obligatory = true,
                                       PixelGravity = 40
                                   }
                           };
            }

            return Enumerable.Empty<ISnapRule>();
        }

        private static IList<IFeatureRenderer> GetCustomRenderer<T>()
        {
            var type = typeof(T);

            if (type == typeof(CompositeBranchStructure))
            {
                return new List<IFeatureRenderer>{new CompositeStructureRenderer(new StructureRenderer())};
            }

            if (type == typeof(Weir) || type == typeof(Pump) || type == typeof(Culvert) || 
                type == typeof(Bridge) || type == typeof(ExtraResistance) 
                || type == typeof(OutletCompartment) || type == typeof(IOrifice))
            {
                return new List<IFeatureRenderer> { new StructureRenderer() };
            }

            if (type == typeof(CrossSection))
            {
                 return new List<IFeatureRenderer> {new CrossSectionRenderer()};
            }

            if (type == typeof(LateralSource))
            {
                return new List<IFeatureRenderer> {new DiffuseLateralSourceRenderer()};
            }

            if (type.Implements(typeof(IPipe)))
            {
                return new List<IFeatureRenderer>{new ArrowLineStringAdornerRenderer{ Orientation = Orientation.Forward , Opacity = 1, Size = 3}};
            }

            if (type == typeof(Compartment))
            {
                return new List<IFeatureRenderer> { new CompartmentRenderer() };
            }

            return new List<IFeatureRenderer>();
        }
        
        private static Func<ILayer, IFeature> NewNetworkFeature<T>()
        {
            var type = typeof (T);

            if (type == typeof(Pump))
            {
                return l => new Pump(false);
            }

            if (type == typeof(IOrifice))
            {
                return l => new Orifice(true);
            }

            if (type == typeof(Weir))
            {
                return l => new Weir(true);
            }

            if (type == typeof(Culvert))
            {
                return l => Culvert.CreateDefault();
            }

            if (type == typeof(Bridge))
            {
                return l => Bridge.CreateDefault();
            }

            if (type == typeof(ExtraResistance))
            {
                return l => ExtraResistance.CreateDefault();
            }

            if (type == typeof(CrossSection))
            {
                return l => CrossSection.CreateDefault();
            }

            return null;
        }
    }
}