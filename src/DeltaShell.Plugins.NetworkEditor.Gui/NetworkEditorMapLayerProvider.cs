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
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
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
            return data is DrainageBasin
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
                   || data is IEnumerable<Orifice>
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
                       parentObject is HydroArea) // dry area & enclosures
                   || (data is IEventedList<GroupablePointFeature> && parentObject is HydroArea) // dry points, 
                   || (data is IEventedList<FixedWeir> && parentObject is HydroArea) //fixed weirs
                   || (data is IEventedList<Embankment> && parentObject is HydroArea)
                   || (data is IEventedList<BridgePillar> && parentObject is HydroArea)
                   || (data is IEventedList<LeveeBreach> && parentObject is HydroArea) //levee breach
                   || (data is IEventedList<Gully> && parentObject is HydroArea) //gullies
                   || (data is IEventedList<RoofArea> && parentObject is HydroArea) //roofareas;
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

                var drainageBasin = hydroRegion as DrainageBasin;
                if (drainageBasin != null)
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

        public ILayer CreateLayer(object data, object parentData)
        {

            var area2D = data as HydroArea;
            if (area2D != null)
            {
                return new AreaLayer { HydroArea = area2D, NameIsReadOnly = true };
            }

            var hydroRegion = data as IHydroRegion;
            if (hydroRegion != null)
            {
                return new HydroRegionMapLayer { Name = hydroRegion.Name, Region = hydroRegion, LayersReadOnly = true };
            }

            var drainageBasin = parentData as DrainageBasin;
            var hydroNetwork = parentData as IHydroNetwork;

            var manholeNodes = data as IEnumerable<IManhole>;
            if (hydroNetwork != null && manholeNodes != null)
            {
                return CreateNetworkVisibilityVectorLayer<Manhole>(manholeNodes, "Manholes", hydroNetwork, MaxVisibilityLayerValue); ;
            }

            var outletCompartments = data as IEnumerable<OutletCompartment>;
            if (hydroNetwork != null && outletCompartments != null)
            {
                return CreateNetworkVisibilityVectorLayer<OutletCompartment>(outletCompartments, "Outlets", hydroNetwork, MaxVisibilityLayerValue);
            }

            var compartments = data as IEnumerable<Compartment>;
            if (hydroNetwork != null && compartments != null)
            {
                return CreateNetworkVisibilityVectorLayer<Compartment>(compartments, "Compartments", hydroNetwork, MaxVisibilityLayerValue);
            }

            var orifices = data as IEnumerable<Orifice>;
            if (orifices != null && hydroNetwork != null)
            {
                return CreateNetworkVisibilityVectorLayer<Orifice>(orifices, "Orifices", hydroNetwork, MaxVisibilityLayerValue);
            }

            var pipes = data as IEnumerable<IPipe>;
            if (hydroNetwork != null && pipes != null)
            {
                return CreateNetworkVectorLayer<Pipe>(pipes, "Pipes", hydroNetwork);
            }

            var sewerConnections = data as IEnumerable<ISewerConnection>;
            if (hydroNetwork != null && sewerConnections != null)
            {
                return CreateNetworkVectorLayer<SewerConnection>(sewerConnections, "Sewer Connections", hydroNetwork);
            }

            var links = data as IEventedList<HydroLink>;
            if (links != null && parentData is IHydroRegion)
            {
                var region = (IHydroRegion) parentData;

                return new VectorLayer("Links " + region.Name)
                           {
                               Visible = true,
                               Style = NetworkLayerStyleFactory.CreateStyle(links, drainageBasin!=null),
                               NameIsReadOnly = true,
                               DataSource = new FeatureCollection { Features = (IList)region.Links, FeatureType = typeof(HydroLink), CoordinateSystem = region.CoordinateSystem },
                               FeatureEditor = new HydroLinkFeatureEditor
                                                   {
                                                       SnapRules = {new HydroLinkSnapRule {Obligatory = true, PixelGravity = 40}},
                                                       Region = region
                                                   }
                           };
            }

            if (parentData is IHydroRegion && parentData as HydroArea == null)
            {
                var wasteWaterTreatmentPlants = data as IEventedList<WasteWaterTreatmentPlant>;
                if (wasteWaterTreatmentPlants != null && parentData is DrainageBasin)
                {
                    return new VectorLayer("Wastewater Treatment Plants")
                        {
                            Style = NetworkLayerStyleFactory.CreateStyle(wasteWaterTreatmentPlants),
                            NameIsReadOnly = true,
                            DataSource =
                                new FeatureCollection((IList) wasteWaterTreatmentPlants,
                                                      typeof (WasteWaterTreatmentPlant))
                                    {
                                        CoordinateSystem = drainageBasin.CoordinateSystem
                                    },
                            FeatureEditor = new WasteWaterTreatmentPlantFeatureEditor {DrainageBasin = drainageBasin}
                        };
                }

                var runoffBoundaries = data as IEventedList<RunoffBoundary>;
                if (runoffBoundaries != null && parentData is DrainageBasin)
                {
                    return new VectorLayer("Runoff Boundaries")
                        {
                            Style = NetworkLayerStyleFactory.CreateStyle(runoffBoundaries),
                            NameIsReadOnly = true,
                            DataSource =
                                new FeatureCollection((IList) runoffBoundaries, typeof (RunoffBoundary))
                                    {
                                        CoordinateSystem = drainageBasin.CoordinateSystem
                                    },
                            FeatureEditor = new RunoffBoundaryFeatureEditor {DrainageBasin = drainageBasin}
                        };
                }

                var catchments = data as IEventedList<Catchment>;
                if (catchments != null)
                {
                    var flattenedCatchments = catchments.SelectMany(c => new[] {c}.Concat(c.SubCatchments));

                    var centerLayers = new VectorLayer
                        {
                            Name = "Catchments (centers)",
                            DataSource =
                                new FeatureCollection(
                                    new WrappedEnumerableList<Catchment>(flattenedCatchments, catchments),
                                    typeof (Catchment)) {CoordinateSystem = drainageBasin.CoordinateSystem},
                            FeatureEditor = new CatchmentFeatureEditor {DrainageBasin = drainageBasin},
                            CustomRenderers = {new CatchmentAnchorPointRenderer()},
                            NameIsReadOnly = true,
                            Selectable = false
                        };
                    var catchmentLayer = new VectorLayer
                        {
                            Name = "Catchments (Polygons)",
                            Style = NetworkLayerStyleFactory.CreateStyle(catchments),
                            DataSource =
                                new FeatureCollection((IList) catchments, typeof (Catchment))
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

                var hydroNodes = data as IEnumerable<IHydroNode>;
                if (hydroNodes != null)
                {
                    return CreateNetworkVectorLayer<HydroNode>(hydroNodes, "Nodes", hydroNetwork);
                }

                var channels = data as IEnumerable<IChannel>;
                if (channels != null)
                {
                    return CreateNetworkVectorLayer<Channel>(channels, "Branches", hydroNetwork);
                }

                var pumps = data as IEnumerable<IPump>;
                if (pumps != null)
                {
                    return CreateNetworkVectorLayer<Pump>(pumps, "Pumps", hydroNetwork,
                                                          o => o is Channel && ((Channel) o).Pumps.Any());
                }

                var lateralSources = data as IEnumerable<ILateralSource>;
                if (lateralSources != null)
                {
                    return CreateNetworkVectorLayer<LateralSource>(lateralSources, "Lateral Sources", hydroNetwork,
                                                                   o =>
                                                                   o is Channel &&
                                                                   ((Channel) o).BranchFeatures.OfType<LateralSource>()
                                                                                .Any());
                }

                var retentions = data as IEnumerable<IRetention>;
                if (retentions != null)
                {
                    return CreateNetworkVectorLayer<Retention>(retentions, "Retentions", hydroNetwork,
                                                               o =>
                                                               o is Channel &&
                                                               ((Channel) o).BranchFeatures.OfType<Retention>().Any());
                }

                var observationPoints = data as IEnumerable<IObservationPoint>;
                if (observationPoints != null)
                {
                    return CreateNetworkVectorLayer<ObservationPoint>(observationPoints, "Observation Points",
                                                                      hydroNetwork,
                                                                      o =>
                                                                      o is Channel &&
                                                                      ((Channel) o).ObservationPoints.Any());
                }

                var weirs = data as IEnumerable<IWeir>;
                if (weirs != null)
                {
                    return CreateNetworkVisibilityVectorLayer<Weir>(weirs, "Weirs", hydroNetwork, MaxVisibilityLayerValue,
                                                          o => o is Channel && ((Channel) o).Weirs.Any());
                }

                var gates = data as IEnumerable<IGate>;
                if (gates != null)
                {
                    return CreateNetworkVectorLayer<Gate>(gates, "Gates", hydroNetwork,
                                                          o => o is Channel && ((Channel) o).Gates.Any());
                }

                var culverts = data as IEnumerable<ICulvert>;
                if (culverts != null)
                {
                    return CreateNetworkVectorLayer<Culvert>(culverts, "Culverts", hydroNetwork,
                                                             o => o is Channel && ((Channel) o).Culverts.Any());
                }

                var bridges = data as IEnumerable<IBridge>;
                if (bridges != null)
                {
                    return CreateNetworkVectorLayer<Bridge>(bridges, "Bridges", hydroNetwork,
                                                            o => o is Channel && ((Channel) o).Bridges.Any());
                }

                var extraResistances = data as IEnumerable<IExtraResistance>;
                if (extraResistances != null)
                {
                    return CreateNetworkVectorLayer<ExtraResistance>(extraResistances, "Extra Resistances", hydroNetwork,
                                                                     o =>
                                                                     o is Channel &&
                                                                     ((Channel) o).BranchFeatures
                                                                                  .OfType<ExtraResistance>().Any());
                }

                var compositeBranchStructures = data as IEnumerable<ICompositeBranchStructure>;
                if (compositeBranchStructures != null)
                {
                    return CreateNetworkVectorLayer<CompositeBranchStructure>(compositeBranchStructures,
                                                                              "Composite Structure",
                                                                              hydroNetwork,
                                                                              o =>
                                                                              o is Channel &&
                                                                              ((Channel) o).CrossSections.Any());
                }

                var crossSections = data as IEnumerable<ICrossSection>;
                if (crossSections != null)
                {
                    return CreateNetworkVectorLayer<CrossSection>(crossSections, "Cross Sections", hydroNetwork,
                                                                  o => o is Channel && ((Channel) o).CrossSections.Any());
                }

                var routes = data as IEnumerable<Route>;
                if (routes != null)
                {
                    return new GroupLayer("Routes")
                        {
                            Selectable = false,
                            NameIsReadOnly = true,
                            LayersReadOnly = true
                        };
                }
            }

            const string modelName = "NetworkEditorModelName";
            var features = data as IEnumerable<IFeature>;
            var area2DParent = parentData as HydroArea; 
            if (features != null && area2DParent != null)
            {
                if (Equals(features, area2DParent.ObservationPoints))
                {
                    return new VectorLayer(HydroArea.ObservationPointsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.ObservationPointStyle,
                        DataSource = new HydroAreaFeature2DCollection (area2DParent).Init(area2DParent.ObservationPoints, "ObservationPoint_2D_", modelName, area2DParent.CoordinateSystem)
                    };
                }

                if (Equals(features, area2DParent.DryPoints))
                {
                    return new VectorLayer(HydroArea.DryPointsPluralName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        Style = AreaLayerStyles.DryPointStyle,
                        DataSource =
                            new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.DryPoints, "DryPoint", modelName, area2DParent.CoordinateSystem)
                    };
                }
                if (Equals(features, area2DParent.DryAreas))
                {
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.DryAreas, "DryArea", modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        if (!(geometry is IPolygon))
                        {
                            if (geometry.Coordinates.Count() < 4) return null;
                            geometry = new Polygon(new LinearRing(geometry.Coordinates));
                        }
                        var newFeature = new GroupableFeature2DPolygon { Geometry = geometry };
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

                if (Equals(features, area2DParent.Enclosures))
                {
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Enclosures, "Enclosure", modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        if (!(geometry is IPolygon))
                        {
                            if (geometry.Coordinates.Count() < 4) return null;
                            geometry = new Polygon(new LinearRing(geometry.Coordinates));
                        }
                        var newFeature = new GroupableFeature2DPolygon() { Geometry = geometry };
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };

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
                if (Equals(features, area2DParent.RoofAreas))
                {
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.RoofAreas, "RoofAreas", modelName, area2DParent.CoordinateSystem);
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
                        var newFeature = new RoofArea { Geometry = geometry };
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

                if (Equals(features, area2DParent.Gullies))
                {
                    var ds = new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Gullies, "Gullies", modelName, area2DParent.CoordinateSystem);
                    ds.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
                    {
                        var newFeature = new Gully() { Geometry = geometry };
                        ds.Features.Add(newFeature);

                        return newFeature;
                    };
                    return new VisibilityVectorLayer(HydroArea.GullyName)
                    {
                        Style = AreaLayerStyles.Gulliestyle,
                        NameIsReadOnly = true,
                        DataSource = ds,
                        FeatureEditor = new Feature2DEditor(area2DParent),
                        CanBeRemovedByUser = true,
                        Selectable = true,
                        MaxVisible = MaxVisibilityLayerValue
                    }; ; 
                }
            }

            var obsCrossSections2d = data as IEventedList<ObservationCrossSection2D>;
            if (obsCrossSections2d != null && area2DParent != null && Equals(obsCrossSections2d, area2DParent.ObservationCrossSections))
            {
                return new VectorLayer(HydroArea.ObservationCrossSectionsPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    Style = AreaLayerStyles.ObsCrossSectionStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.ObservationCrossSections, "ObservationCrossSection_2D_",
                                                       modelName, area2DParent.CoordinateSystem),
                    CustomRenderers = new[] { new ArrowLineStringAdornerRenderer() }
                };
            }

            var pumps2d = data as IEventedList<Pump2D>;
            if (pumps2d != null && area2DParent != null && Equals(pumps2d, area2DParent.Pumps))
            {
                var areaFeature2DCollection = new HydroAreaFeature2DCollection(area2DParent).Init(pumps2d, "Pump_2D_", modelName,
                    area2DParent.CoordinateSystem);
                areaFeature2DCollection.FeatureType = typeof(Pump2D); // Override so we can use FeatureAttributes!
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
                    MaxVisible = MaxVisibilityLayerValue
                };
            }

            var weirs2d = data as IEventedList<Weir2D>;
            if (weirs2d != null && area2DParent != null && Equals(weirs2d, area2DParent.Weirs))
            {
                var feature2DCollection = new HydroAreaFeature2DCollection(area2DParent).Init(weirs2d, "Weir_2D_", modelName,
                                                                         area2DParent.CoordinateSystem);
                feature2DCollection.FeatureType = typeof(Weir2D); // Override so we can use FeatureAttributes!
                return new VisibilityVectorLayer(HydroArea.WeirsPluralName)
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
                    MaxVisible = MaxVisibilityLayerValue
                };
            }

            var gates2d = data as IEventedList<Gate2D>;
            if (gates2d != null && area2DParent != null && Equals(gates2d, area2DParent.Gates))
            {
                var feature2DCollection = new HydroAreaFeature2DCollection (area2DParent).Init(gates2d, "Gate_2D_", modelName,
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


            var thinDams = data as IEventedList<ThinDam2D>;
            if (thinDams != null && area2DParent != null && Equals(thinDams, area2DParent.ThinDams))
            {
                return new VectorLayer(HydroArea.ThinDamsPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    Style = AreaLayerStyles.ThinDamStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.ThinDams, "ThinDam_2D_", modelName,
                                                       area2DParent.CoordinateSystem)
                };
            }

            var landBoundaries = data as IEventedList<LandBoundary2D>;
            if (landBoundaries != null && area2DParent != null && Equals(landBoundaries, area2DParent.LandBoundaries))
            {
                return new VectorLayer(HydroArea.LandBoundariesPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    Style = AreaLayerStyles.LandBoundaryStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.LandBoundaries, "LandBoundary_2D_", modelName,
                            area2DParent.CoordinateSystem)
                };
            }
            var embankments = data as IEventedList<Embankment>;
            if (embankments != null && area2DParent != null && Equals(embankments, area2DParent.Embankments))
            {
                return new VectorLayer(HydroArea.EmbankmentsPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor =
                        new HydroAreaFeatureEditor(area2DParent) {CreateNewFeature = l => new Embankment {Region = area2DParent}},
                    Style = AreaLayerStyles.EmbankmentStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.Embankments, "Embankment_2D_", modelName,
                            area2DParent.CoordinateSystem),
                    CustomRenderers = new List<IFeatureRenderer>(new [] {new EmbankmentRenderer()})
                };
            }

            
            var fixedWeirs2D = data as IEventedList<FixedWeir>;
            if (fixedWeirs2D != null && area2DParent != null && Equals(fixedWeirs2D, area2DParent.FixedWeirs))
            {
                return new VisibilityVectorLayer(HydroArea.FixedWeirsPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    Style = AreaLayerStyles.FixedWeirStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.FixedWeirs, "FixedWeir_2D_", modelName,
                            area2DParent.CoordinateSystem),
                    MaxVisible = MaxVisibilityLayerValue
                };
            }

            var bridgePillars = data as IEventedList<BridgePillar>;
            if (bridgePillars != null && area2DParent != null && Equals(bridgePillars, area2DParent.BridgePillars))
            {
                return new VectorLayer(HydroArea.BridgePillarsPluralName)
                {
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    Style = AreaLayerStyles.BridgePillarStyle,
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.BridgePillars, "BridgePillar_2D_", modelName,
                            area2DParent.CoordinateSystem)
                };
            }

            var damBreaks = data as IEventedList<LeveeBreach>;
            if (damBreaks != null && area2DParent != null && Equals(damBreaks, area2DParent.LeveeBreaches))
            {
                return new VectorLayer(HydroArea.LeveeBreachName)
                {
                    NameIsReadOnly = true,
                    CanBeRemovedByUser = true,
                    FeatureEditor = new Feature2DEditor(area2DParent),
                    DataSource =
                        new HydroAreaFeature2DCollection(area2DParent).Init(area2DParent.LeveeBreaches, "LeveeBreach_2D_", modelName,
                                                       area2DParent.CoordinateSystem),
                    CustomRenderers = new List<IFeatureRenderer>(new[] { new LeveeBreachRenderer(AreaLayerStyles.LeveeStyle, AreaLayerStyles.BreachStyle) })
                };
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
                return new GroupLayer("Roughness data")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }
            return null;
        }

        private static VectorLayer CreateNetworkVectorLayer<TFeature>(IEnumerable<IFeature> networkItems, string name, IHydroNetwork hydroNetwork, Func<object, bool> refreshForChangedItem = null)
        {
            var layer = new VectorLayer(name)
                            {
                                Style = NetworkLayerStyleFactory.CreateStyle(networkItems),
                                Theme = NetworkLayerStyleFactory.CreateTheme(networkItems),
                                NameIsReadOnly = true,
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
        
        private static VisibilityVectorLayer CreateNetworkVisibilityVectorLayer<TFeature>(IEnumerable<IFeature> networkItems, string name, IHydroNetwork hydroNetwork, double maxVisible, Func<object, bool> refreshForChangedItem = null)
        {
            var layer = new VisibilityVectorLayer(name)
            {
                Style = NetworkLayerStyleFactory.CreateStyle(networkItems),
                Theme = NetworkLayerStyleFactory.CreateTheme(networkItems),
                NameIsReadOnly = true,
                DataSource = new HydroNetworkFeatureCollection
                {
                    FeatureType = typeof(TFeature),
                    Network = hydroNetwork,
                    RefreshForChangedItem = refreshForChangedItem,
                    CoordinateSystem = hydroNetwork.CoordinateSystem
                },
                FeatureEditor = new HydroNetworkFeatureEditor(hydroNetwork)
                {
                    CreateNewFeature = NewNetworkFeature<TFeature>()
                },
                CustomRenderers = GetCustomRenderer<TFeature>(),
                MaxVisible = maxVisible
            };
            
                

            return (VisibilityVectorLayer)AddSnappingRulesToLayer<TFeature>(layer);
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

            if (type == typeof(Weir) || type == typeof(Pump) || type == typeof(Culvert) || type == typeof(Bridge) || type == typeof(ExtraResistance))
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
                || type == typeof(OutletCompartment) || type == typeof(Orifice))
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

            return new List<IFeatureRenderer>();
        }

        private static Func<ILayer, IFeature> NewNetworkFeature<T>()
        {
            var type = typeof (T);

            if (type == typeof(Pump))
            {
                return l => new Pump(false);
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