using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.Plugins.NetworkEditor.Gui.LayerGenerators;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public class NetworkEditorMapLayerProvider : MapLayerCreationInfoMapLayerProvider
    {
        private static readonly IMapLayerCreationInfo[] layerCreators = GetLayerCreators();

        public NetworkEditorMapLayerProvider() : base(layerCreators)
        {
        }

        public override IEnumerable<object> ChildLayerObjects(object data)
        {
            foreach (var childLayerObject in base.ChildLayerObjects(data))
            {
                yield return childLayerObject;
            }

            if (data is IModelWithRoughnessSections modelWithRoughnessSections)
            {
                yield return modelWithRoughnessSections.RoughnessSections;
            }
        }

        private static IMapLayerCreationInfo[] GetLayerCreators()
        {
            return GetNetworkLayerCreators()
                   .Concat(GetAreaLayerCreators())
                   .Concat(GetBasinLayerCreators())
                   .Concat(GetOtherLayerCreators())
                   .ToArray();
        }

        /// <summary>
        /// Creates <see cref="IMapLayerCreationInfo">MapLayerCreationInfos</see> for other objects (HydroRegion, Roughness)
        /// </summary>
        private static IEnumerable<IMapLayerCreationInfo> GetOtherLayerCreators()
        {
            yield return new MapLayerCreationInfo<HydroRegion>
            {
                CreateLayerFunc = (region, o) => new HydroRegionMapLayer
                {
                    Name = region.Name,
                    Region = region,
                    LayersReadOnly = true
                },
                ChildLayerObjectsFunc = region => region.SubRegions.OfType<object>().Plus(region.Links)
            };

            yield return new MapLayerCreationInfo<IEventedList<HydroLink>, IHydroModel>
            {
                CreateLayerFunc = (links, model) => CreateLinksLayer(links, model.Region, "Model links"),
                CanBuildWithParentFunc = model => model.Region is HydroRegion
            };

            yield return new MapLayerCreationInfo<IEventedList<HydroLink>, IHydroRegion>
            {
                CreateLayerFunc = (links, region) => CreateLinksLayer(links, region, $"{region.Name} links"),
                CanBuildWithParentFunc = region => !(region is IHydroNetwork || region is HydroArea)
            };

            yield return new MapLayerCreationInfo<IEventedList<RoughnessSection>>
            {
                CreateLayerFunc = (sections, parentObject) => new GroupLayer("Lanes")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                },
                ChildLayerObjectsFunc = sections => sections.Where(s => !(s is ReverseRoughnessSection reverseRoughnessSection && reverseRoughnessSection.UseNormalRoughness))
            };

            yield return new MapLayerCreationInfo<RoughnessSection>
            {
                CreateLayerFunc = (section, parentObject) =>
                {
                    var coverageLayer = SharpMapLayerFactory.CreateMapLayerForCoverage(section.RoughnessNetworkCoverage, null);
                    coverageLayer.Visible = false;
                    return coverageLayer;
                }
            };
        }

        /// <summary>
        /// Creates <see cref="IMapLayerCreationInfo">MapLayerCreationInfos</see> for <see cref="IDrainageBasin"/>
        /// </summary>
        private static IEnumerable<IMapLayerCreationInfo> GetBasinLayerCreators()
        {
            yield return new MapLayerCreationInfo<IDrainageBasin>
            {
                CreateLayerFunc = (basin, parentObject) => new HydroRegionMapLayer
                {
                    Name = basin.Name,
                    Region = basin,
                    LayersReadOnly = true
                },
                ChildLayerObjectsFunc = basin =>
                {
                    return new object[]
                    {
                        basin.Boundaries,
                        basin.Catchments,
                        basin.Links,
                        basin.WasteWaterTreatmentPlants
                    };
                }
            };
            yield return GetDrainageBasinLayerCreator<WasteWaterTreatmentPlant>();
            yield return GetDrainageBasinLayerCreator<RunoffBoundary>();
            yield return GetDrainageBasinLayerCreator<Catchment>();
        }

        /// <summary>
        /// Creates <see cref="IMapLayerCreationInfo">MapLayerCreationInfos</see> for <see cref="HydroArea"/>
        /// </summary>
        private static IEnumerable<IMapLayerCreationInfo> GetAreaLayerCreators()
        {
            yield return new MapLayerCreationInfo<HydroArea>
            {
                CreateLayerFunc = (area, o) => new AreaLayer
                {
                    HydroArea = area,
                    NameIsReadOnly = true
                },
                ChildLayerObjectsFunc = area =>
                {
                    return new object[]
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
                    };
                },
                AfterCreateFunc = (layer, area, parentData, objectsLookup) =>
                {
                    var objectsInRenderOrder = new object[]
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
                    };

                    if (!(layer is IGroupLayer groupLayer)) return;
                    groupLayer.SetRenderOrderByObjectOrder(objectsInRenderOrder, objectsLookup);
                }
            };

            yield return GetHydroAreaLayerCreator<Feature2D>();
            yield return GetHydroAreaLayerCreator<GroupableFeature2D>();
            yield return GetHydroAreaLayerCreator<LandBoundary2D>();
            yield return GetHydroAreaLayerCreator<ThinDam2D>();
            yield return GetHydroAreaLayerCreator<ObservationCrossSection2D>();
            yield return GetHydroAreaLayerCreator<Pump2D>();
            yield return GetHydroAreaLayerCreator<Weir2D>();
            yield return GetHydroAreaLayerCreator<Gate2D>();
            yield return GetHydroAreaLayerCreator<Feature2DPoint>();
            yield return GetHydroAreaLayerCreator<GroupableFeature2DPoint>();
            yield return GetHydroAreaLayerCreator<GroupableFeature2DPolygon>();
            yield return GetHydroAreaLayerCreator<GroupablePointFeature>();
            yield return GetHydroAreaLayerCreator<FixedWeir>();
            yield return GetHydroAreaLayerCreator<Embankment>();
            yield return GetHydroAreaLayerCreator<BridgePillar>();
            yield return GetHydroAreaLayerCreator<Gully>();
        }

        /// <summary>
        /// Creates <see cref="IMapLayerCreationInfo">MapLayerCreationInfos</see> for <see cref="HydroNetwork"/>
        /// </summary>
        private static IEnumerable<IMapLayerCreationInfo> GetNetworkLayerCreators()
        {
            yield return new MapLayerCreationInfo<HydroNetwork>
            {
                CreateLayerFunc = (network, parentObject) => new HydroRegionMapLayer
                {
                    Name = network.Name,
                    Region = network,
                    LayersReadOnly = true
                },
                ChildLayerObjectsFunc = network =>
                {
                    return new object[]
                    {
                        network.HydroNodes,
                        network.Channels,
                        network.Bridges,
                        network.CompositeBranchStructures,
                        network.CrossSections,
                        network.Culverts,
                        network.ExtraResistances,
                        network.Gates,
                        network.LateralSources,
                        network.Compartments,
                        network.Manholes,
                        network.ObservationPoints,
                        network.Orifices,
                        network.OutletCompartments,
                        network.Pipes,
                        network.Pumps,
                        network.Retentions,
                        network.Routes,
                        network.SewerConnections,
                        network.Weirs
                    };
                },
                AfterCreateFunc = (layer, network, parentData, objectsLookup) =>
                {
                    var objectsInRenderOrder = new object[]
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
                    };

                    if (!(layer is IGroupLayer groupLayer)) return;
                    groupLayer.SetRenderOrderByObjectOrder(objectsInRenderOrder, objectsLookup);
                }
            };

            yield return GetHydroNetworkLayerCreator<IHydroNode>();
            yield return GetHydroNetworkLayerCreator<IChannel>();
            yield return GetHydroNetworkLayerCreator<IManhole>();
            yield return GetHydroNetworkLayerCreator<OutletCompartment>();
            yield return GetHydroNetworkLayerCreator<Compartment>();
            yield return GetHydroNetworkLayerCreator<IOrifice>();
            yield return GetHydroNetworkLayerCreator<IPipe>();
            yield return GetHydroNetworkLayerCreator<ISewerConnection>();
            yield return GetHydroNetworkLayerCreator<IPump>();
            yield return GetHydroNetworkLayerCreator<ILateralSource>();
            yield return GetHydroNetworkLayerCreator<IRetention>();
            yield return GetHydroNetworkLayerCreator<IObservationPoint>();
            yield return GetHydroNetworkLayerCreator<IWeir>();
            yield return GetHydroNetworkLayerCreator<ICulvert>();
            yield return GetHydroNetworkLayerCreator<IBridge>();
            yield return GetHydroNetworkLayerCreator<IExtraResistance>();
            yield return GetHydroNetworkLayerCreator<ICompositeBranchStructure>();
            yield return GetHydroNetworkLayerCreator<ICrossSection>();

            yield return new MapLayerCreationInfo<IEventedList<Route>, HydroNetwork>
            {
                CreateLayerFunc = (routes, parentObject) => new GroupLayer("Routes")
                {
                    Selectable = false,
                    NameIsReadOnly = true,
                    LayersReadOnly = true
                },
                ChildLayerObjectsFunc = routes => routes
            };
        }

        private static MapLayerCreationInfo<IEnumerable<TFeature>, HydroNetwork> GetHydroNetworkLayerCreator<TFeature>()
        {
            return new MapLayerCreationInfo<IEnumerable<TFeature>, HydroNetwork>
            {
                CreateLayerFunc = (list, hydroNetwork) => hydroNetwork.GenerateHydroNetworkLayer(list)
            };
        }

        private static MapLayerCreationInfo<IEnumerable<TFeature>, HydroArea> GetHydroAreaLayerCreator<TFeature>()
        {
            return new MapLayerCreationInfo<IEnumerable<TFeature>, HydroArea>
            {
                CreateLayerFunc = (list, area) => area.GenerateArea2DLayer(list)
            };
        }

        private static MapLayerCreationInfo<IEnumerable<TFeature>, IDrainageBasin> GetDrainageBasinLayerCreator<TFeature>()
        {
            return new MapLayerCreationInfo<IEnumerable<TFeature>, IDrainageBasin>
            {
                CreateLayerFunc = (list, drainageBasin) => drainageBasin.GenerateDrainageBasinLayer(list)
            };
        }

        private static ILayer CreateLinksLayer(IEventedList<HydroLink> links, IHydroRegion region, string layerName)
        {
            return new VectorLayer(layerName)
            {
                Visible = true,
                Style = NetworkLayerStyleFactory.CreateStyle(links, region is IDrainageBasin),
                NameIsReadOnly = true,
                DataSource = new ComplexFeatureCollection(region, (IList)region.Links, typeof(HydroLink)),
                FeatureEditor = new HydroLinkFeatureEditor
                {
                    SnapRules =
                    {
                        new HydroLinkSnapRule
                        {
                            Obligatory = true,
                            PixelGravity = 40
                        }
                    },
                    Region = region
                }
            };
        }
    }
}